using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoxToVFXFramework.Scripts.UI.Popups;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, Nft> NftMetadataPerAddressAndTokenId = new Dictionary<string, Nft>();

		public async UniTask<Nft> GetTokenIdMetadataWithCache(string address, string tokenId)
		{
			string key = address + "_" + tokenId;
			if (NftMetadataPerAddressAndTokenId.TryGetValue(key, out Nft nft))
			{
				return nft;
			}

			try
			{
				Nft tokenIdMetadata = await Moralis.Web3Api.Token.GetTokenIdMetadata(address: address, tokenId: tokenId, ConfigManager.Instance.ChainList);
				NftMetadataPerAddressAndTokenId[key] = tokenIdMetadata;
				return tokenIdMetadata;
			}
			catch (Exception e)
			{
				Debug.LogError("[DataManager] Failed to get NFT metadata: " + e.Message);
				MessagePopup.Show(e.Message, LogType.Error);
			}

			return null;
		}
	}
}
