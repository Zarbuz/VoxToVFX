using System;
using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using System.Collections.Generic;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, NftCollectionAndOwner> NftCollection = new Dictionary<string, NftCollectionAndOwner>();

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

		public class NftCollectionAndOwner
		{
			public NftCollection NftCollection { get; set; }
			public NftOwnerCollection NftOwnerCollection { get; set; }
			public DateTime LastUpdate { get; set; }

		}

		
	}

}
