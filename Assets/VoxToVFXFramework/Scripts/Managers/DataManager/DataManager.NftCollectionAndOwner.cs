using System;
using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, NftCollectionCache> NftCollection = new Dictionary<string, NftCollectionCache>();
		public Dictionary<string, UserOwnerCache> UserOwner = new Dictionary<string, UserOwnerCache>();

		public async UniTask<NftCollectionCache> GetNftCollectionWithCache(string address)
		{
			if (NftCollection.TryGetValue(address, out NftCollectionCache collection))
			{
				if ((DateTime.UtcNow - collection.LastUpdate).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
				{
					return collection;
				}
			}

			NftCollectionCache collectionCache = new NftCollectionCache
			{
				NftOwnerCollection = new NftOwnerCollection()
			};

			NftCollection result = await NFTManager.Instance.GetNFTForContract(address);
			NftOwnerCollection ownerCollection = await NFTManager.Instance.GetNFTOwners(address);

			if (result != null)
			{
				collectionCache.NftOwnerCollection.Result = new List<NftOwner>();
				foreach (Nft nft in result.Result)
				{
					NftOwner owner = ownerCollection?.Result.FirstOrDefault(t => t.TokenId == nft.TokenId);
					collectionCache.NftOwnerCollection.Result.Add(new NftOwner()
					{
						Name = nft.Name,
						Amount = nft.Amount,
						TokenId = nft.TokenId,
						Symbol = nft.Symbol,
						TokenAddress = nft.TokenAddress,
						TokenUri = nft.TokenUri,
						SyncedAt = nft.SyncedAt,
						ContractType = nft.ContractType,
						Metadata = nft.Metadata,
						OwnerOf = owner != null ? owner.OwnerOf : string.Empty,
						BlockNumber = owner != null ? owner.BlockNumber : string.Empty,
						BlockNumberMinted = owner != null ? owner.BlockNumberMinted : string.Empty
					});
				}

				collectionCache.LastUpdate = DateTime.UtcNow;
				NftCollection[address] = collectionCache;
			}

			return collectionCache;
		}

		public async UniTask<NftOwnerCollection> GetNFTOwnedByUser(string address)
		{
			if (UserOwner.TryGetValue(address, out UserOwnerCache userOwnerCache))
			{
				if ((DateTime.UtcNow - userOwnerCache.LastUpdate).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
				{
					return userOwnerCache.NftOwnerCollection;
				}
			}

			NftOwnerCollection result = await NFTManager.Instance.GetNFTForUser(address);
			if (result != null)
			{
				UserOwner[address] = new UserOwnerCache()
				{
					LastUpdate = DateTime.UtcNow,
					NftOwnerCollection = result
				};
			}

			return result;
		}

		public class NftCollectionCache
		{
			public NftOwnerCollection NftOwnerCollection { get; set; }
			public DateTime LastUpdate { get; set; }

			public int TotalItems
			{
				get
				{
					if (NftOwnerCollection != null && NftOwnerCollection.Total.HasValue)
					{
						return NftOwnerCollection.Total.Value;
					}

					return 0;
				}
			}

		}

		public class UserOwnerCache
		{
			public NftOwnerCollection NftOwnerCollection { get; set; }
			public DateTime LastUpdate { get; set; }

		}

	}

}
