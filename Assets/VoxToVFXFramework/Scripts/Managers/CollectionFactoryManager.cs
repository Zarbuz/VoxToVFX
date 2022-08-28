using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Platform.Queries;
using MoralisUnity.Platform.Queries.Live;
using Nethereum.Hex.HexTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.ScriptableObjets;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.Utils.ContractFunction;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class CollectionFactoryManager : ModuleSingleton<CollectionFactoryManager>
	{
		#region Fields

		public SmartContractAddressConfig SmartContractAddressConfig => ConfigManager.Instance.SmartContractAddress;

		//Database Queries
		private MoralisQuery<CollectionCreatedEvent> mCollectionCreatedQuery;
		private MoralisLiveQueryCallbacks<CollectionCreatedEvent> mCollectionCreatedQueryCallbacks;

		private MoralisQuery<CollectionMintedEvent> mCollectionMintedQuery;
		private MoralisLiveQueryCallbacks<CollectionMintedEvent> mCollectionMintedQueryCallbacks;
		#endregion

		#region UnityMethods

		protected override async void OnStart()
		{
			await SubscribeToDatabaseEvents();
		}

		#endregion

		#region PublicMethods

		public async UniTask<string> CreateCollection(string collectionName, string collectionSymbol)
		{
			int nounce = UnityEngine.Random.Range(1, 1000000000);
			Debug.Log("CreateCollection nounce: " + nounce);
			object[] parameters = {
				collectionName,
				collectionSymbol,
				nounce
			};

			// Set gas estimate
			HexBigInteger value = new HexBigInteger(0);
			HexBigInteger gas = new HexBigInteger(100000);
			HexBigInteger gasPrice = new HexBigInteger(0); //useless

			string resp = await ExecuteContractFunctionUtils.ExecuteContractFunction(SmartContractAddressConfig.CollectionFactoryAddress, SmartContractAddressConfig.CollectionFactoryABI, "createCollection", parameters, value, gas, gasPrice);

			Debug.Log("[CollectionFactoryManager] CreateCollection: " + resp);
			return resp;
		}

		public async UniTask<List<CollectionCreatedEvent>> GetUserListContract(CustomUser user)
		{
			MoralisQuery<CollectionCreatedEvent> q = await Moralis.Query<CollectionCreatedEvent>();
			q = q.WhereEqualTo("creator", user.EthAddress);
			IEnumerable<CollectionCreatedEvent> result = await q.FindAsync();
			return result.ToList();
		}

		public async UniTask<CollectionCreatedEvent> GetCollection(string collectionContract)
		{
			MoralisQuery<CollectionCreatedEvent> q = await Moralis.Query<CollectionCreatedEvent>();
			q = q.WhereEqualTo("collectionContract", collectionContract);
			CollectionCreatedEvent collection = await q.FirstOrDefaultAsync();
			return collection;
		}

		public async UniTask WatchMintedEventContract(CollectionCreatedEvent collectionCreated)
		{
			try
			{
				Dictionary<string, object> parameters = new Dictionary<string, object>();
				//parameters.Add("address", collectionCreated.CollectionContract);
				//parameters.Add("chainId", ConfigManager.Instance.ChainListString);
				await Moralis.Cloud.RunAsync<object>("watchMintedEventContract", parameters);
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message + " " + e.StackTrace);
			}
		}

		#endregion

		#region PrivateMethods

		private async UniTask SubscribeToDatabaseEvents()
		{
			await UniTask.WaitWhile(() => UserManager.Instance.CurrentUser == null);
			mCollectionCreatedQuery = await Moralis.GetClient().Query<CollectionCreatedEvent>();
			mCollectionCreatedQueryCallbacks = new MoralisLiveQueryCallbacks<CollectionCreatedEvent>();

			mCollectionMintedQuery = await Moralis.GetClient().Query<CollectionMintedEvent>();
			mCollectionMintedQueryCallbacks = new MoralisLiveQueryCallbacks<CollectionMintedEvent>();

			mCollectionMintedQueryCallbacks.OnUpdateEvent += HandleOnCollectionMintedEvent;
			mCollectionMintedQueryCallbacks.OnErrorEvent += delegate (ErrorMessage evt)
			{
				Debug.LogError("OnErrorEvent: " + evt.error + " " + evt.code);
			};

			mCollectionCreatedQueryCallbacks.OnUpdateEvent += HandleOnCollectionCreatedEvent;
			//mQueryCallbacks.OnCreateEvent += HandleOnCollectionCreatedEvent;
			mCollectionCreatedQueryCallbacks.OnErrorEvent += delegate (ErrorMessage evt)
			{
				Debug.LogError("OnErrorEvent: " + evt.error + " " + evt.code);
			};
			MoralisLiveQueryController.AddSubscription<CollectionCreatedEvent>("CollectionCreatedEvent", mCollectionCreatedQuery, mCollectionCreatedQueryCallbacks);
			MoralisLiveQueryController.AddSubscription<CollectionMintedEvent>("CollectionMintedEvent", mCollectionMintedQuery, mCollectionMintedQueryCallbacks);
		}

		private void HandleOnCollectionCreatedEvent(CollectionCreatedEvent item, int requestid)
		{
			Debug.Log("[CollectionFactoryManager] HandleOnCollectionCreatedEvent: " + item.Creator);

			if (UserManager.Instance.CurrentUser != null && UserManager.Instance.CurrentUser.EthAddress == item.Creator)
			{
				Debug.Log("[CollectionFactoryManager] HandleOnCollectionCreatedEvent is for current user");
				DataManager.Instance.AddCollectionCreated(item);
				EventDatabaseManager.Instance.OnEventReceived(item);
			}
		}

		private void HandleOnCollectionMintedEvent(CollectionMintedEvent item, int requestid)
		{
			Debug.Log("[CollectionFactoryManager] HandleOnCollectionMintedEvent " + item.Creator);
			if (UserManager.Instance.CurrentUser != null && UserManager.Instance.CurrentUser.EthAddress == item.Creator)
			{
				Debug.Log("[CollectionFactoryManager] HandleOnCollectionMintedEvent is for current user");

				DataManager.Instance.AddCollectionMinted(item);
				EventDatabaseManager.Instance.OnEventReceived(item);
			}
		}

		#endregion
	}
}
