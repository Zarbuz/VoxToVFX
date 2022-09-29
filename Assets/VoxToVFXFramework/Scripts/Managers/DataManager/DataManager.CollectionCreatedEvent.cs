using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using VoxToVFXFramework.Scripts.Models.ContractEvent;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, CollectionCreatedEventCache> ContractCreatedPerUsers = new Dictionary<string, CollectionCreatedEventCache>();

		public async UniTask<List<CollectionCreatedEvent>> GetUserListContractWithCache(string userAddress)
		{
			if (string.IsNullOrEmpty(userAddress))
			{
				return new List<CollectionCreatedEvent>();
			}

			if (ContractCreatedPerUsers.ContainsKey(userAddress))
			{
				CollectionCreatedEventCache dto = ContractCreatedPerUsers[userAddress];
				if ((DateTime.UtcNow - dto.LastTimeUpdated).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
				{
					return dto.List;
				}
			}

			List<CollectionCreatedEvent> list = await CollectionFactoryManager.Instance.GetUserListContract(userAddress);
			ContractCreatedPerUsers[userAddress] = new CollectionCreatedEventCache()
			{
				LastTimeUpdated = DateTime.UtcNow,
				List = list
			};

			return list;
		}

		public async UniTask<CollectionCreatedEvent> GetCollectionCreatedEventWithCache(string address)
		{
			foreach (CollectionCreatedEvent collection in ContractCreatedPerUsers.Values.SelectMany(moralisCache => moralisCache.List.Where(collection => collection.CollectionContract == address)))
			{
				return collection;
			}

			CollectionCreatedEvent collectionCreated = await CollectionFactoryManager.Instance.GetCollection(address);
			AddCollectionCreated(collectionCreated);
			return collectionCreated;
		}


		public async UniTask<string> GetCreatorOfCollection(string address)
		{
			foreach (KeyValuePair<string, CollectionCreatedEventCache> item in ContractCreatedPerUsers)
			{
				if (item.Value.List.Any(t => t.CollectionContract == address))
				{
					return item.Key;
				}
			}

			CollectionCreatedEvent collection = await GetCollectionCreatedEventWithCache(address);
			return collection.Creator;
		}

		public void AddCollectionCreated(CollectionCreatedEvent collectionCreated)
		{
			if (ContractCreatedPerUsers.ContainsKey(collectionCreated.Creator))
			{
				CollectionCreatedEventCache dto = ContractCreatedPerUsers[collectionCreated.Creator];
				if (dto.List
				    .All(t => t.CollectionContract != collectionCreated.CollectionContract))
				{
					dto.List.Add(collectionCreated);
					dto.LastTimeUpdated = DateTime.UtcNow;
				}
			}
			else
			{
				CollectionCreatedEventCache cache = new CollectionCreatedEventCache();
				cache.List = new List<CollectionCreatedEvent>()
				{
					collectionCreated
				};
				cache.LastTimeUpdated = DateTime.UtcNow;
				ContractCreatedPerUsers[collectionCreated.Creator] = cache;
			}
		}

		public class CollectionCreatedEventCache
		{
			public List<CollectionCreatedEvent> List { get; set; }
			public DateTime LastTimeUpdated { get; set; }
		}
	}
}
