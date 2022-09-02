using Cysharp.Threading.Tasks;
using MoralisUnity.Platform.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, MoralisDataCacheDTO> ContractCreatedPerUsers = new Dictionary<string, MoralisDataCacheDTO>();

		public async UniTask<List<CollectionCreatedEvent>> GetUserListContractWithCache(string userAddress)
		{
			if (string.IsNullOrEmpty(userAddress))
			{
				return new List<CollectionCreatedEvent>();
			}

			if (ContractCreatedPerUsers.ContainsKey(userAddress))
			{
				MoralisDataCacheDTO dto = ContractCreatedPerUsers[userAddress];
				if ((DateTime.UtcNow - dto.LastTimeUpdated).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
				{
					return dto.List.Cast<CollectionCreatedEvent>().ToList();
				}
			}

			List<CollectionCreatedEvent> list = await CollectionFactoryManager.Instance.GetUserListContract(userAddress);
			ContractCreatedPerUsers[userAddress] = new MoralisDataCacheDTO()
			{
				LastTimeUpdated = DateTime.UtcNow,
				List = list.Cast<MoralisObject>().ToList()
			};

			return list;
		}

		public bool IsCollectionCreatedByCurrentUser(string address)
		{
			string currentUser = UserManager.Instance.CurrentUserAddress;
			if (string.IsNullOrEmpty(currentUser))
				return false;

			if (ContractCreatedPerUsers.ContainsKey(currentUser))
			{
				return ContractCreatedPerUsers[currentUser].List.Cast<CollectionCreatedEvent>()
					.Any(t => t.CollectionContract == address);
			}

			return false;
		}

		public async UniTask<CollectionCreatedEvent> GetCollectionWithCache(string address)
		{
			foreach (MoralisDataCacheDTO moralisCache in ContractCreatedPerUsers.Values)
			{
				foreach (CollectionCreatedEvent collection in moralisCache.List.Cast<CollectionCreatedEvent>())
				{
					if (collection.CollectionContract == address)
					{
						return collection;
					}
				}
			}

			CollectionCreatedEvent collectionCreated = await CollectionFactoryManager.Instance.GetCollection(address);
			AddCollectionCreated(collectionCreated);
			return collectionCreated;
		}

		public void AddCollectionCreated(CollectionCreatedEvent collectionCreated)
		{
			if (ContractCreatedPerUsers.ContainsKey(collectionCreated.Creator))
			{
				MoralisDataCacheDTO dto = ContractCreatedPerUsers[collectionCreated.Creator];
				if (dto.List.Cast<CollectionCreatedEvent>()
				    .All(t => t.CollectionContract != collectionCreated.CollectionContract))
				{
					dto.List.Add(collectionCreated);
					dto.LastTimeUpdated = DateTime.UtcNow;
				}
			}
			else
			{
				MoralisDataCacheDTO cache = new MoralisDataCacheDTO();
				cache.List = new List<MoralisObject>()
				{
					collectionCreated
				};
				cache.LastTimeUpdated = DateTime.UtcNow;
				ContractCreatedPerUsers[collectionCreated.Creator] = cache;
			}
		}
	}
}
