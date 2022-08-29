using Cysharp.Threading.Tasks;
using MoralisUnity.Platform.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using VoxToVFXFramework.Scripts.Models.ContractEvent;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, MoralisDataCacheDTO> NFTPerContract = new Dictionary<string, MoralisDataCacheDTO>();

		public async UniTask<List<CollectionMintedEvent>> GetNFTForContractWithCache(string creator, string contract)
		{
			if (NFTPerContract.ContainsKey(contract))
			{
				MoralisDataCacheDTO dto = NFTPerContract[contract];
				if ((DateTime.UtcNow - dto.LastTimeUpdated).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
				{
					return dto.List.Cast<CollectionMintedEvent>().ToList();
				}
			}
			List<CollectionMintedEvent> listNfTsForContract = await NFTManager.Instance.FetchNFTsForContract(creator, contract);
			NFTPerContract[contract] = new MoralisDataCacheDTO()
			{
				LastTimeUpdated = DateTime.UtcNow,
				List = listNfTsForContract.Cast<MoralisObject>().ToList()
			};

			return listNfTsForContract;
		}

		public void AddCollectionMinted(CollectionMintedEvent collectionMinted)
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
