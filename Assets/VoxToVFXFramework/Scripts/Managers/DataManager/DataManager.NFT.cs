using Cysharp.Threading.Tasks;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Web3Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.Models.ContractEvent;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, CollectionMintedEvent> CollectionMintedCache = new Dictionary<string, CollectionMintedEvent>();
		public Dictionary<string, NFTDetailsCacheDTO> NFTDetailsCache = new Dictionary<string, NFTDetailsCacheDTO>();

		public async UniTask<CollectionMintedEvent> GetCollectionMintedWithCache(string contract, string tokenId)
		{
			string key = contract + "_" + tokenId;

			if (CollectionMintedCache.ContainsKey(key))
			{
				return CollectionMintedCache[key];
			}

			CollectionMintedEvent collectionMinted = await NFTManager.Instance.GetCollectionMintedItem(contract, tokenId);
			if (collectionMinted != null)
			{
				CollectionMintedCache[key] = collectionMinted;
				return collectionMinted;
			}

			return null;
		}

		public async UniTask<NFTDetailsContractType> GetNFTDetailsWithCache(string address, string tokenId)
		{
			string key = address + "_" + tokenId;
			if (NFTDetailsCache.ContainsKey(key))
			{
				NFTDetailsCacheDTO details = NFTDetailsCache[key];
				if ((DateTime.UtcNow - details.LastTimeUpdated).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
				{
					return details.ContractType;
				}
			}
			NFTDetailsContractType nftDetails = await MiddlewareManager.Instance.GetNFTDetails(address, tokenId);
			if (nftDetails != null)
			{
				NFTDetailsCache[key] = new NFTDetailsCacheDTO()
				{
					LastTimeUpdated = DateTime.UtcNow,
					ContractType = nftDetails
				};
				return nftDetails;
			}

			return null;
		}

		public void AddOrUpdateCollectionItem(CollectionMintedEvent collectionMinted)
		{
			string key = collectionMinted.Address + "_" + collectionMinted.TokenID;
			CollectionMintedCache[key] = collectionMinted;
		}
	}
}
