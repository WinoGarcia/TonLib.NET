﻿using TonLibDotNet.Recipes;

namespace TonLibDotNet
{
    public static class TonRecipes
    {
        /// <summary>
        /// Functions to work with DNS contracts (*.ton domains).
        /// </summary>
        /// <remarks>
        /// Based on <see href="https://github.com/ton-blockchain/TEPs/blob/master/text/0081-dns-standard.md">TEP 81: TON DNS Standard</see>
        ///   and <see href="https://github.com/ton-blockchain/dns-contract">TON DNS Contracts</see>.
        /// </remarks>
        public static RootDnsRecipes RootDns { get; } = RootDnsRecipes.Instance;

        /// <summary>
        /// Functions to work with Telegram username NFTs (*.t.me).
        /// </summary>
        /// <remarks>
        /// Based on <see href="https://github.com/ton-blockchain/TEPs/blob/master/text/0081-dns-standard.md">TEP 81: TON DNS Standard</see>
        ///   and <see href="https://github.com/TelegramMessenger/telemint">Telemint Contracts</see>.
        /// </remarks>
        public static TelegramUsernamesRecipes TelegramUsernames { get; } = TelegramUsernamesRecipes.Instance;

        /// <summary>
        /// Functions to work with Telegram anonymous number NFTs (+888...).
        /// </summary>
        /// <remarks>
        /// Based on <see href="https://github.com/ton-blockchain/TEPs/blob/master/text/0081-dns-standard.md">TEP 81: TON DNS Standard</see>
        ///   and <see href="https://github.com/TelegramMessenger/telemint">Telemint Contracts</see>.
        /// </remarks>
        public static TelegramNumbersRecipes TelegramNumbers { get; } = TelegramNumbersRecipes.Instance;

        /// <summary>
        /// Functions to work with Jettons.
        /// </summary>
        /// <remarks>
        /// Based on <see href="https://github.com/ton-blockchain/TEPs/blob/master/text/0074-jettons-standard.md">TEP 74: Fungible tokens (Jettons) standard</see>
        ///   and <see href="https://github.com/ton-blockchain/token-contract/">Tokens Smart Contracts</see>.
        /// </remarks>
        public static Tep74Jettons Jettons { get; } = Tep74Jettons.Instance;

        /// <summary>
        /// Functions to work with NFT.
        /// </summary>
        /// <remarks>
        /// Based on <see href="https://github.com/ton-blockchain/TEPs/blob/master/text/0062-nft-standard.md">TEP 62: NFT Standard</see>
        ///   and <see href="https://github.com/ton-blockchain/token-contract/tree/main/nft/">Reference NFT implementation</see>.
        /// </remarks>
        public static Tep62Nft NFTs { get; } = Tep62Nft.Instance;

        /// <summary>
        /// Functions to work with Jettons and Nft metadata.
        /// </summary>
        /// <remarks>
        /// Based on <see href="https://github.com/ton-blockchain/TEPs/blob/master/text/0064-token-data-standard.md">TEP 64: A standard interface for tokens (meta)data</see>
        /// </remarks>
        public static Tep64TokenData TokenData { get; } = Tep64TokenData.Instance;
    }
}
