using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.Models.ContractEvent;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, List<AbstractContractEvent>> NFTEventsCache = new Dictionary<string, List<AbstractContractEvent>>();
		public Dictionary<string, NFTDetailsCacheDTO> NFTDetailsCache = new Dictionary<string, NFTDetailsCacheDTO>();

		public async UniTask<List<AbstractContractEvent>> GetAllEventsForNFT(string contract, string tokenId)
		{
			string key = contract + "_" + tokenId;
			if (NFTEventsCache.TryGetValue(key, out List<AbstractContractEvent> events))
			{
				return events;
			}

			List<AbstractContractEvent> result = await NFTManager.Instance.GetAllEventsForNFT(contract, tokenId);
			NFTEventsCache[key] = result;
			return result;
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
		

		public void DeleteCacheForTokenId(string address, string tokenId)
		{
			string key = address + "_" + tokenId;
			NFTDetailsCache.Remove(key);
			NFTEventsCache.Remove(key);
		}
	}
}
