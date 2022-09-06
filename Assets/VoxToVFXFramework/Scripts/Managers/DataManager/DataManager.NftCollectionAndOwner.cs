﻿using System;
using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, NftCollectionAndOwner> NftCollection = new Dictionary<string, NftCollectionAndOwner>();
		public Dictionary<string, UserOwnerCache> UserOwner = new Dictionary<string, UserOwnerCache>();
		public async UniTask<NftCollectionAndOwner> GetNftCollectionWithCache(string address)
		{
			if (NftCollection.TryGetValue(address, out NftCollectionAndOwner collection))
			{
				if ((DateTime.UtcNow - collection.LastUpdate).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
				{
					return collection;
				}
			}

			NftCollectionAndOwner collectionAndOwner = new NftCollectionAndOwner();
			NftCollection result = await NFTManager.Instance.GetAllTokenIds(address);

			if (result != null)
			{
				collectionAndOwner.NftCollection = result;
			}

			NftOwnerCollection result2 = await NFTManager.Instance.GetNFTOwners(address);
			if (result2 != null)
			{
				collectionAndOwner.NftOwnerCollection = result2;
			}

			collectionAndOwner.LastUpdate = DateTime.UtcNow;
			NftCollection[address] = collectionAndOwner;

			return collectionAndOwner;
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

			NftOwnerCollection result = await NFTManager.Instance.GetNfts(address);
			UserOwner[address] = new UserOwnerCache()
			{
				LastUpdate = DateTime.UtcNow,
				NftOwnerCollection = result
			};
			return result;
		}

		public async UniTask<NftOwner> GetOwnerOfNft(Nft nft)
		{
			NftCollectionAndOwner collection = await GetNftCollectionWithCache(nft.TokenAddress);
			return collection is { NftOwnerCollection: { } } ? collection.NftOwnerCollection.Result.FirstOrDefault(nftOwner => nftOwner.TokenId == nft.TokenId) : null;
		}

		public class NftCollectionAndOwner
		{
			public NftCollection NftCollection { get; set; }
			public NftOwnerCollection NftOwnerCollection { get; set; }
			public DateTime LastUpdate { get; set; }

		}

		public class UserOwnerCache
		{
			public NftOwnerCollection NftOwnerCollection { get; set; }
			public DateTime LastUpdate { get; set; }

		}
	}

}
