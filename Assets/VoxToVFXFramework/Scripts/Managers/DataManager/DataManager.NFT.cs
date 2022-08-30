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
		public Dictionary<string, MoralisDataCacheDTO> NFTPerContract = new Dictionary<string, MoralisDataCacheDTO>();
		public Dictionary<string, NFTDetailsCacheDTO> NFTDetailsCache = new Dictionary<string, NFTDetailsCacheDTO>();

		//public async UniTask<List<CollectionMintedEvent>> GetNFTForContractWithCache(string creator, string contract)
		//{
		//	if (NFTPerContract.ContainsKey(contract))
		//	{
		//		MoralisDataCacheDTO dto = NFTPerContract[contract];
		//		if ((DateTime.UtcNow - dto.LastTimeUpdated).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
		//		{
		//			return dto.List.Cast<CollectionMintedEvent>().ToList();
		//		}
		//	}
		//	List<CollectionMintedEvent> listNfTsForContract = await NFTManager.Instance.FetchNFTsForContract(creator, contract);
		//	NFTPerContract[contract] = new MoralisDataCacheDTO()
		//	{
		//		LastTimeUpdated = DateTime.UtcNow,
		//		List = listNfTsForContract.Cast<MoralisObject>().ToList()
		//	};

		//	return listNfTsForContract;
		//}

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
			if (NFTPerContract.ContainsKey(collectionMinted.Address))
			{
				MoralisDataCacheDTO dto = NFTPerContract[collectionMinted.Address];
				if (dto.List.Cast<CollectionMintedEvent>().All(t => t.TokenID != collectionMinted.TokenID))
				{
					dto.List.Add(collectionMinted);
					dto.LastTimeUpdated = DateTime.UtcNow;
				}
			}
			else
			{
				MoralisDataCacheDTO cache = new MoralisDataCacheDTO();
				cache.List = new List<MoralisObject>()
				{
					collectionMinted
				};
				cache.LastTimeUpdated = DateTime.UtcNow;
				NFTPerContract[collectionMinted.Address] = cache;
			}
		}
	}
}
