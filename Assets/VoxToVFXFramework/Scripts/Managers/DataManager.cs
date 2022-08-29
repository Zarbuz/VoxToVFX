using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Web3Api.Models;
using UnityEngine;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI.Popups;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class DataManager : SimpleSingleton<DataManager>
	{
		#region Fields

		public Dictionary<string, MoralisDataCacheDTO> ContractCreatedPerUsers = new Dictionary<string, MoralisDataCacheDTO>();
		public Dictionary<string, MoralisDataCacheDTO> NFTPerContract = new Dictionary<string, MoralisDataCacheDTO>();
		public Dictionary<string, Nft> NftMetadataPerAddressAndTokenId = new Dictionary<string, Nft>();
		public Dictionary<string, CollectionDetails> CollectionDetails = new Dictionary<string, CollectionDetails>();
		public Dictionary<string, CustomUser> Users = new Dictionary<string, CustomUser>();
		public Dictionary<string, AccountInfoContractType> AccountDetails = new Dictionary<string, AccountInfoContractType>();
		#endregion

		#region ConstStatic

		public const int MINUTES_BEFORE_UPDATE_CACHE = 2;

		#endregion

		#region PublicMethods

		public async UniTask<List<CollectionCreatedEvent>> GetUserListContractWithCache(CustomUser user)
		{
			if (ContractCreatedPerUsers.ContainsKey(user.EthAddress))
			{
				MoralisDataCacheDTO dto = ContractCreatedPerUsers[user.EthAddress];
				if ((DateTime.UtcNow - dto.LastTimeUpdated).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
				{
					return dto.List.Cast<CollectionCreatedEvent>().ToList();
				}
			}

			List<CollectionCreatedEvent> list = await CollectionFactoryManager.Instance.GetUserListContract(user);
			ContractCreatedPerUsers[user.EthAddress] = new MoralisDataCacheDTO()
			{
				LastTimeUpdated = DateTime.UtcNow,
				List = list.Cast<MoralisObject>().ToList()
			};

			return list;
		}

		public bool IsCollectionCreatedByCurrentUser(string address)
		{
			CustomUser currentUser = UserManager.Instance.CurrentUser;
			if (currentUser == null)
				return false;

			if (ContractCreatedPerUsers.ContainsKey(currentUser.EthAddress))
			{
				return ContractCreatedPerUsers[currentUser.EthAddress].List.Cast<CollectionCreatedEvent>()
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

		public async UniTask<Nft> GetTokenIdMetadataWithCache(string address, string tokenId)
		{
			string key = address + "_" + tokenId;
			if (NftMetadataPerAddressAndTokenId.TryGetValue(key, out Nft nft))
			{
				return nft;
			}

			try
			{
				Nft tokenIdMetadata = await Moralis.Web3Api.Token.GetTokenIdMetadata(address: address, tokenId: tokenId, ConfigManager.Instance.ChainList);
				NftMetadataPerAddressAndTokenId[key] = tokenIdMetadata;
				return tokenIdMetadata;
			}
			catch (Exception e)
			{
				Debug.LogError("[DataManager] Failed to get NFT metadata: " + e.Message);
				MessagePopup.Show(e.Message, LogType.Error);
			}
			
			return null;
		}

		public async UniTask<CollectionDetails> GetCollectionDetailsWithCache(string collectionContract)
		{
			if (CollectionDetails.TryGetValue(collectionContract, out CollectionDetails collectionDetails))
			{
				return collectionDetails;
			}

			CollectionDetails details = await CollectionDetailsManager.Instance.GetCollectionDetails(collectionContract);
			CollectionDetails[collectionContract] = details;
			return details;
		}

		public async UniTask<CustomUser> GetUserWithCache(string ethAddress)
		{
			if (Users.TryGetValue(ethAddress, out CustomUser user))
			{
				return user;
			}

			CustomUser userFromEth = await UserManager.Instance.LoadUserFromEthAddress(ethAddress);
			if (userFromEth != null)
			{
				Users[ethAddress] = userFromEth;
				return userFromEth;
			}
			return null;
		}

		public void SaveCollectionDetails(CollectionDetails collectionDetails)
		{
			CollectionDetails[collectionDetails.CollectionContract] = collectionDetails;
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

		#endregion
	}

	public class MoralisDataCacheDTO
	{
		public List<MoralisObject> List { get; set; }
		public DateTime LastTimeUpdated { get; set; }
	}
}
