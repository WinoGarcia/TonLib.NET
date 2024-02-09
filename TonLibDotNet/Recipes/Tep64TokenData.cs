using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TonLibDotNet.Cells;

namespace TonLibDotNet.Recipes;

/// <summary>
/// Based on <see href="https://github.com/ton-blockchain/TEPs/blob/master/text/0064-token-data-standard.md">TEP 64: A standard interface for tokens (meta)data</see>
/// </summary>
public class Tep64TokenData
{
    #region Private Fields

    private readonly HttpClient httpClient = new();

    private const string ipfs = "ipfs://";
    private const string ipfsPublicGate = "https://ipfs.io/ipfs/";

    private const string categoryNameImage = "image";
    private const string categoryNameName = "name";
    private const string categoryNameSymbol = "symbol";
    private const string categoryNameDecimals = "decimals";
    private const string categoryNameDescription = "description";
    private const string categoryNameImageData = "image_data";
    private const string categoryNameSocialLinks = "SocialLinks";
    private const string categoryNameMarketplace = "Marketplace";
    private const string categoryNameContentUrl = "content_url";
    private const string categoryNameAttributes = "attributes";

    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
    };

    #endregion

    #region Public Fields

    public static readonly Tep64TokenData Instance = new();

    public static readonly byte[] CategoryBytesImage = EncodeCategory(categoryNameImage);
    public static readonly byte[] CategoryBytesName = EncodeCategory(categoryNameName);
    public static readonly byte[] CategoryBytesSymbol = EncodeCategory(categoryNameSymbol);
    public static readonly byte[] CategoryBytesDecimals = EncodeCategory(categoryNameDecimals);
    public static readonly byte[] CategoryBytesDescription = EncodeCategory(categoryNameDescription);
    public static readonly byte[] CategoryBytesImageData = EncodeCategory(categoryNameImageData);
    public static readonly byte[] CategoryBytesSocialLinks = EncodeCategory(categoryNameSocialLinks);
    public static readonly byte[] CategoryBytesMarketplace = EncodeCategory(categoryNameMarketplace);
    public static readonly byte[] CategoryBytesContentUrl = EncodeCategory(categoryNameContentUrl);
    public static readonly byte[] CategoryBytesAttributes = EncodeCategory(categoryNameAttributes);

    #endregion

    #region Public Methods

    public Cell BuildOnChainJettonContent(JettonEntries entries) =>
        new CellBuilder()
            .StoreUInt(0, 8)
            .StoreDict(
                entries.ToDictionary(),
                256,
                (x, v) => x.StoreBytes(v),
                (x, v) => x.StoreUInt(0, 8).StoreStringSnake(v))
            .Build();


    /// <summary>
    /// Loads the content of an NFT item.
    /// </summary>
    /// <param name="itemContent">The BOC object containing item content.</param>
    /// <param name="individualContent">The BOC object containing individual content.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The loaded NFT item entries.</returns>
    public async Task<NftItemEntries?> LoadNftItemContent(
        Boc itemContent,
        Boc individualContent,
        CancellationToken cancellationToken = default)
    {
        var contentSlice = itemContent.RootCells[0].BeginRead();
        var offChain = contentSlice.LoadUInt(8);
        if (offChain == 1)
        {
            var url = contentSlice.LoadString();

            var individualContentUrl = individualContent.RootCells[0].BeginRead().LoadString();

            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            if (url.Contains(ipfs))
            {
                url = $"{ipfsPublicGate}{url.Replace(ipfs, string.Empty)}";
            }

            return await this.LoadOffChainTokenDataAsync<NftItemEntries>(CombineUrl(url, individualContentUrl), cancellationToken).ConfigureAwait(false);
        }

        var entries = contentSlice.TryLoadAndParseDict(256, k => k.LoadBitsToBytes(256), v => v);
        if (entries != null)
        {
            var result = new NftItemEntries();
            foreach (var entry in entries)
            {
                if (CategoryBytesName.SequenceEqual(entry.Key))
                {
                    result.Name = entry.Value.LoadStringSnake();
                }

                if (CategoryBytesDescription.SequenceEqual(entry.Key))
                {
                    result.Description = entry.Value.LoadStringSnake();
                }

                if (CategoryBytesImageData.SequenceEqual(entry.Key))
                {
                    result.Image = entry.Value.LoadStringChunked();
                }

                if (CategoryBytesContentUrl.SequenceEqual(entry.Key))
                {
                    result.ContentUrl = entry.Value.LoadStringSnake();
                }
                //if (CategoryBytesAttributes.SequenceEqual(entry.Key))
                //{
                //    result.Attributes = entry.Value.LoadList(AttributeItem.Load);
                //}
            }

            return result;
        }

        return null;
    }

    /// <summary>
    /// Loads the content of an NFT collection.
    /// </summary>
    /// <param name="collectionContent">The collection content to load.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The loaded NFT collection entries.</returns>
    public async Task<NftCollectionEntries?> LoadNftCollectionContent(
        Boc collectionContent,
        CancellationToken cancellationToken = default)
    {
        var contentSlice = collectionContent.RootCells[0].BeginRead();
        var offChain = contentSlice.LoadUInt(8);

        if (offChain == 1)
        {
            var url = contentSlice.LoadString();

            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            if (url.Contains(ipfs))
            {
                url = $"{ipfsPublicGate}{url.Replace(ipfs, "")}";
            }

            return await this.LoadOffChainTokenDataAsync<NftCollectionEntries>(url, cancellationToken).ConfigureAwait(false);
        }

        var entries = contentSlice.TryLoadAndParseDict(256, k => k.LoadBitsToBytes(256), v => v);
        if (entries != null)
        {
            var result = new NftCollectionEntries();
            foreach (var entry in entries)
            {
                if (CategoryBytesImage.SequenceEqual(entry.Key))
                {
                    result.Image = entry.Value.LoadString();
                }

                if (CategoryBytesName.SequenceEqual(entry.Key))
                {
                    result.Name = entry.Value.LoadString();
                }

                if (CategoryBytesDescription.SequenceEqual(entry.Key))
                {
                    result.Description = entry.Value.LoadString();
                }

                //if (CategoryBytesSocialLinks.SequenceEqual(entry.Key))
                //{
                //    result.SocialLinks = entry.Value.LoadList(s => s.LoadString());
                //}
                if (CategoryBytesMarketplace.SequenceEqual(entry.Key))
                {
                    result.Marketplace = entry.Value.LoadString();
                }
            }

            return result;
        }

        return null;
    }

    /// <summary>
    /// Loads the jetton content.
    /// </summary>
    /// <param name="jettonContent">The Boc object containing the jetton content.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// Returns the JettonEntries object or null if the jetton content is invalid or empty.
    /// </returns>
    public async Task<JettonEntries?> LoadJettonContent(
        Boc jettonContent,
        CancellationToken cancellationToken = default)
    {
        var contentSlice = jettonContent.RootCells[0].BeginRead();
        var isOffChain = contentSlice.LoadUInt(8);

        if (isOffChain == 1)
        {
            var url = contentSlice.LoadString();

            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            if (url.Contains(ipfs))
            {
                url = $"{ipfsPublicGate}{url.Replace(ipfs, "")}";
            }

            return await this.LoadOffChainTokenDataAsync<JettonEntries>(url, cancellationToken).ConfigureAwait(false);
        }

        var entries = contentSlice.TryLoadAndParseDict(256, k => k.LoadBitsToBytes(256), v => v);
        if (entries != null)
        {
            var result = new JettonEntries();
            foreach (var entry in entries)
            {
                if (CategoryBytesImage.SequenceEqual(entry.Key))
                {
                    result.Image = entry.Value.LoadStringSnake();
                }

                if (CategoryBytesName.SequenceEqual(entry.Key))
                {
                    result.Name = entry.Value.LoadStringSnake();
                }

                if (CategoryBytesSymbol.SequenceEqual(entry.Key))
                {
                    result.Symbol = entry.Value.LoadStringSnake();
                }

                if (CategoryBytesDecimals.SequenceEqual(entry.Key))
                {
                    result.Decimals = entry.Value.LoadUShort();
                }

                if (CategoryBytesDescription.SequenceEqual(entry.Key))
                {
                    result.Description = entry.Value.LoadStringSnake();
                }

                if (CategoryBytesImageData.SequenceEqual(entry.Key))
                {
                    result.ImageData = entry.Value.LoadStringSnake();
                }
            }

            return result;
        }

        return null;
    }

    #endregion

    #region Private Methods

    private static byte[] EncodeCategory(string categoryName) =>
        System.Security.Cryptography.SHA256.HashData(Encoding.ASCII.GetBytes(categoryName));


    //System.Security.Cryptography.SHA256.HashData(Encoding.ASCII.GetBytes(categoryName));

    private static string CombineUrl(string url, string? individualContentUrl) =>
        url.LastIndexOf(".json", StringComparison.Ordinal) != -1
            ? url
            : $"{url}{individualContentUrl}";

    private async Task<T?> LoadOffChainTokenDataAsync<T>(
        string url,
        CancellationToken cancellationToken = default)
    {
        var response = await this.httpClient.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        var collectionEntries = await response.Content.ReadFromJsonAsync<T>(
            jsonSerializerOptions,
            cancellationToken);

        return collectionEntries;
    }

    #endregion
}

/// <summary>
/// Represents a jetton TEP-64 standard entry.
/// </summary>
public class JettonEntries
{
    /// <summary>
    /// The name of the token.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Describes the token.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The symbol of the token. UTF8 string.
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// The number of decimals the token uses. 
    /// </summary>
    /// <value>
    /// The default value is 9.
    /// </value>
    public uint Decimals { get; set; } = 9;

    /// <summary>
    /// Either binary representation of the image for onchain layout or base64 for offchain layout.
    /// </summary>
    public string? ImageData { get; set; }

    /// <summary>
    /// A URI pointing to a jetton icon with mime type image. ASCII string.
    /// </summary>
    public string? Image { get; set; }

    public Dictionary<byte[], string> ToDictionary()
    {
        var result = new Dictionary<byte[], string>();

        if (this.Decimals != 9)
        {
            result.Add(Tep64TokenData.CategoryBytesDecimals, this.Decimals.ToString());
        }

        if (!string.IsNullOrEmpty(this.Name))
        {
            result.Add(Tep64TokenData.CategoryBytesName, this.Name);
        }

        if (!string.IsNullOrEmpty(this.Description))
        {
            result.Add(Tep64TokenData.CategoryBytesDescription, this.Description);
        }

        if (!string.IsNullOrEmpty(this.Symbol))
        {
            result.Add(Tep64TokenData.CategoryBytesSymbol, this.Symbol);
        }

        if (!string.IsNullOrEmpty(this.ImageData))
        {
            result.Add(Tep64TokenData.CategoryBytesImageData, this.ImageData);
        }

        if (!string.IsNullOrEmpty(this.Image))
        {
            result.Add(Tep64TokenData.CategoryBytesImage, this.Image);
        }

        return result;
    }
}

/// <summary>
/// Represents a collection of NFT TEP-64 standard entry.
/// </summary>
public class NftCollectionEntries
{
    /// <summary>
    /// Collection's image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Collection's name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Collection's description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Collection's social links.
    /// </summary>
    public IEnumerable<string> SocialLinks { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Marketplace for this collection.
    /// </summary>
    public string? Marketplace { get; set; }
}

/// <summary>
/// Represents a NFT item TEP-64 standard entry.
/// </summary>
public class NftItemEntries
{
    /// <summary>
    /// Name the asset.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Describes the asset.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URI pointing to a resource with mime type image.
    /// </summary>
    public string? Image { get; set; }

    public string? ContentUrl { get; set; }
    public IEnumerable<AttributeItem> Attributes { get; set; } = Enumerable.Empty<AttributeItem>();
}

public class AttributeItem
{
    public string? TraitType { get; set; }
    public string? Value { get; set; }
}