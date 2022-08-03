using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Platform.Queries;
using MoralisUnity.Platform.Queries.Live;
using Nethereum.Hex.HexTypes;
using UnityEngine;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.ScriptableObjets;
using VoxToVFXFramework.Scripts.Singleton;
using Random = System.Random;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class CollectionFactoryManager : ModuleSingleton<CollectionFactoryManager>
	{
		#region Fields

		public SmartContractAddressConfig SmartContractAddressConfig => ConfigManager.Instance.SmartContractAddress;
		public event Action<CollectionCreatedEvent> CollectionCreatedEvent; 

		//Database Queries
		private MoralisQuery<CollectionCreatedEvent> mGetEventsQuery;
		private MoralisLiveQueryCallbacks<CollectionCreatedEvent> mQueryCallbacks;
		#endregion

		#region UnityMethods

		protected override void OnAwake()
		{
			Moralis.Start();
		}

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
			string resp = await Moralis.ExecuteContractFunction(SmartContractAddressConfig.CollectionFactoryAddress, SmartContractAddressConfig.CollectionFactoryABI, "createCollection", parameters, value, gas, gasPrice);

			Debug.Log("[CollectionFactoryManager] CreateCollection: " + resp);
			return resp;
		}

		public async UniTask<List<CollectionCreatedEvent>> GetUserListContract()
		{
			MoralisQuery<CollectionCreatedEvent> q = await Moralis.Query<CollectionCreatedEvent>();
			MoralisUser moralisUser = await Moralis.GetUserAsync();
			q = q.WhereEqualTo("address", moralisUser.ethAddress);
			IEnumerable<CollectionCreatedEvent> result = await q.FindAsync();
			return result.ToList();
		}

		#endregion

		#region PrivateMethods

		private async UniTask SubscribeToDatabaseEvents()
		{
			mGetEventsQuery = await Moralis.GetClient().Query<CollectionCreatedEvent>();
			mQueryCallbacks = new MoralisLiveQueryCallbacks<CollectionCreatedEvent>();

			mQueryCallbacks.OnUpdateEvent += HandleOnCollectionCreatedEvent;
			//mQueryCallbacks.OnCreateEvent += HandleOnCollectionCreatedEvent;
			mQueryCallbacks.OnErrorEvent += delegate(ErrorMessage evt)
			{
				Debug.LogError("OnErrorEvent: " + evt.error + " " + evt.code);
			};
			MoralisLiveQueryController.AddSubscription<CollectionCreatedEvent>("CollectionCreatedEvent", mGetEventsQuery, mQueryCallbacks);
		}

		private async void HandleOnCollectionCreatedEvent(CollectionCreatedEvent item, int requestid)
		{
			Debug.Log("CollectionFactoryManager] HandleOnCollectionCreatedEvent: " + item.Creator);
			MoralisUser user = await Moralis.GetUserAsync();
			if (user != null)
			{
				if (user.ethAddress == item.Creator)
				{
					await UnityMainThreadManager.Instance.EnqueueAsync(() =>
					{
						CollectionCreatedEvent?.Invoke(item);
					});
				}
			}
		}

		#endregion
	}
}
