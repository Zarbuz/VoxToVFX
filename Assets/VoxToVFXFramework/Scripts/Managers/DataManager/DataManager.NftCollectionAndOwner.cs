using System;
using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using System.Collections.Generic;
using System.Linq;
using VoxToVFXFramework.Scripts.Models;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, NftCollectionCache> NFTCollection = new Dictionary<string, NftCollectionCache>();
		public Dictionary<string, NftCollectionCache> NFTOwnedByUser = new Dictionary<string, NftCollectionCache>();

		public async UniTask<List<NftWithDetails>> GetNftCollectionWithCache(string address)
		{
			if (NFTCollection.TryGetValue(address, out NftCollectionCache collection))
			{
				if ((DateTime.UtcNow - collection.LastUpdate).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
				{
					return collection.NFTCollection;
				}
			}


			NftCollectionCache nftCollectionCache = new NftCollectionCache();
			NftCollection result = await NFTManager.Instance.GetNFTForContract(address);
			List<NftWithDetails> nftCollection = new List<NftWithDetails>();

			if (result != null)
			{
				nftCollection.AddRange(result.Result.Where(t => !string.IsNullOrEmpty(t.Metadata)).Select(t => new NftWithDetails(t)));
				nftCollectionCache.LastUpdate = DateTime.UtcNow;
				nftCollectionCache.NFTCollection = nftCollection;

				NFTCollection[address] = nftCollectionCache;
			}

			return nftCollection;
		}

		public async UniTask<List<NftWithDetails>> GetNFTOwnedByUser(string address)
		{
			if (NFTOwnedByUser.TryGetValue(address, out NftCollectionCache userOwnerCache))
			{
				if ((DateTime.UtcNow - userOwnerCache.LastUpdate).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
				{
					return userOwnerCache.NFTCollection;
				}
			}

			NftOwnerCollection result = await NFTManager.Instance.GetNFTForUser(address);
			NftCollectionCache nftCollectionCache = new NftCollectionCache();
			List<NftWithDetails> nftCollection = new List<NftWithDetails>();

			if (result != null)
			{
				nftCollection.AddRange(result.Result.Where(t => !string.IsNullOrEmpty(t.Metadata)).Select(owner => new NftWithDetails()
				{
					TokenAddress = owner.TokenAddress,
					Metadata = owner.Metadata,
					Name = owner.Name,
					TokenId = owner.TokenId,
					TokenUri = owner.TokenUri,
					Symbol = owner.Symbol,
					Amount = owner.Amount,
					ContractType = owner.ContractType,
					SyncedAt = owner.SyncedAt
				}).ToList());

				nftCollectionCache.LastUpdate = DateTime.UtcNow;
				nftCollectionCache.NFTCollection = nftCollection;

				NFTCollection[address] = nftCollectionCache;
			}

			return nftCollection;
		}

		public void DeleteCacheNFTItemInCollection(string address, string tokenId)
		{
			if (NFTCollection.ContainsKey(address) && NFTCollection[address].NFTCollection != null)
			{
				NFTCollection[address].NFTCollection.RemoveAll(nft => nft.TokenId == tokenId);
			}
		}

		public class NftCollectionCache
		{
			public List<NftWithDetails> NFTCollection { get; set; }
			public DateTime LastUpdate { get; set; }
		}
	
	}

}
