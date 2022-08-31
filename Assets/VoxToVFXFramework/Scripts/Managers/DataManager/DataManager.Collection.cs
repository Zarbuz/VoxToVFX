using System;
using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using System.Collections.Generic;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, NftCollectionCache> NftCollection = new Dictionary<string, NftCollectionCache>();

		public async UniTask<NftCollection> GetNftCollectionWithCache(string address)
		{
			if (NftCollection.TryGetValue(address, out NftCollectionCache collection))
			{
				if ((DateTime.UtcNow - collection.LastUpdate).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
				{
					return collection.NftCollection;
				}
			}

			NftCollection result = await NFTManager.Instance.GetAllTokenIds(address);
			if (result != null)
			{
				NftCollectionCache collectionCache = new NftCollectionCache()
				{
					LastUpdate = DateTime.UtcNow,
					NftCollection = result
				};
				NftCollection[address] = collectionCache;
				return result;
			}

			return null;
		}

		public class NftCollectionCache
		{
			public NftCollection NftCollection { get; set; }
			public DateTime LastUpdate { get; set; }
		}
	}

}
