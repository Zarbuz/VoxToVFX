using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Queries;
using MoralisUnity.Platform.Queries.Live;
using System;
using UnityEngine;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class DatabaseEventManager : ModuleSingleton<DatabaseEventManager>
	{
		#region Fields

		public event Action<AbstractContractEvent> OnDatabaseEventReceived;

		#endregion

		#region UnityMethods

		protected override async void OnStart()
		{
			await SubscribeToDatabaseEvents();
		}

		#endregion

		#region PrivateMethods

		private async UniTask SubscribeToDatabaseEvents()
		{
			await UniTask.WaitWhile(() => UserManager.Instance.CurrentUser == null);
			MoralisQuery<CollectionCreatedEvent> collectionCreatedQuery = await Moralis.GetClient().Query<CollectionCreatedEvent>();
			MoralisLiveQueryCallbacks<CollectionCreatedEvent> collectionCreatedQueryCallbacks = new MoralisLiveQueryCallbacks<CollectionCreatedEvent>();

			MoralisQuery<CollectionMintedEvent> collectionMintedQuery = await Moralis.GetClient().Query<CollectionMintedEvent>();
			MoralisLiveQueryCallbacks<CollectionMintedEvent> collectionMintedQueryCallbacks = new MoralisLiveQueryCallbacks<CollectionMintedEvent>();

			MoralisQuery<BuyPriceSetEvent> setBuyPriceQuery = await Moralis.GetClient().Query<BuyPriceSetEvent>();
			MoralisLiveQueryCallbacks<BuyPriceSetEvent> setBuyPriceQueryCallbacks = new MoralisLiveQueryCallbacks<BuyPriceSetEvent>();

			MoralisQuery<BuyPriceCanceledEvent> cancelBuyPriceQuery = await Moralis.GetClient().Query<BuyPriceCanceledEvent>();
			MoralisLiveQueryCallbacks<BuyPriceCanceledEvent> cancelBuyPriceQueryCallbacks = new MoralisLiveQueryCallbacks<BuyPriceCanceledEvent>();

			MoralisQuery<EthNFTTransfers> transferQuery = await Moralis.GetClient().Query<EthNFTTransfers>();
			MoralisLiveQueryCallbacks<EthNFTTransfers> transferQueryCallbacks = new MoralisLiveQueryCallbacks<EthNFTTransfers>();

			collectionMintedQueryCallbacks.OnUpdateEvent += HandleOnCollectionMintedEvent;
			collectionMintedQueryCallbacks.OnErrorEvent += OnError;

			collectionCreatedQueryCallbacks.OnUpdateEvent += HandleOnCollectionCreatedEvent;
			collectionCreatedQueryCallbacks.OnErrorEvent += OnError;

			setBuyPriceQueryCallbacks.OnUpdateEvent += HandleOnBuyPriceSetEvent;
			setBuyPriceQueryCallbacks.OnErrorEvent += OnError;

			cancelBuyPriceQueryCallbacks.OnUpdateEvent += HandleOnBuyPriceCanceledEvent;
			cancelBuyPriceQueryCallbacks.OnErrorEvent += OnError;

			transferQueryCallbacks.OnUpdateEvent += HandleTransferEvent;
			transferQueryCallbacks.OnErrorEvent += OnError;

			MoralisLiveQueryController.AddSubscription("CollectionCreatedEvent", collectionCreatedQuery, collectionCreatedQueryCallbacks);
			MoralisLiveQueryController.AddSubscription("CollectionMintedEvent", collectionMintedQuery, collectionMintedQueryCallbacks);
			MoralisLiveQueryController.AddSubscription("BuyPriceSetEvent", setBuyPriceQuery, setBuyPriceQueryCallbacks);
			MoralisLiveQueryController.AddSubscription("BuyPriceCanceledEvent", cancelBuyPriceQuery, cancelBuyPriceQueryCallbacks);
			MoralisLiveQueryController.AddSubscription("EthNFTTransfers", transferQuery, transferQueryCallbacks);
		}

		

		private void OnError(ErrorMessage evt)
		{
			Debug.LogError("OnErrorEvent: " + evt.error + " " + evt.code);
		}

		private void HandleOnCollectionCreatedEvent(CollectionCreatedEvent item, int requestid)
		{
			Debug.Log("[DatabaseEventManager] HandleOnCollectionCreatedEvent: " + item.Creator);

			if (UserManager.Instance.CurrentUser != null && UserManager.Instance.CurrentUser.EthAddress == item.Creator)
			{
				Debug.Log("[DatabaseEventManager] HandleOnCollectionCreatedEvent is for current user");
				DataManager.DataManager.Instance.AddCollectionCreated(item);
				OnEventReceived(item);
			}
		}

		private void HandleOnCollectionMintedEvent(CollectionMintedEvent item, int requestid)
		{
			Debug.Log("[DatabaseEventManager] HandleOnCollectionMintedEvent " + item.Creator);
			if (UserManager.Instance.CurrentUser != null && UserManager.Instance.CurrentUser.EthAddress == item.Creator)
			{
				Debug.Log("[DatabaseEventManager] HandleOnCollectionMintedEvent is for current user");

				DataManager.DataManager.Instance.NftCollection.Remove(item.Address);
				DataManager.DataManager.Instance.AddOrUpdateCollectionItem(item);
				OnEventReceived(item);
			}
		}

		private void HandleOnBuyPriceSetEvent(BuyPriceSetEvent item, int requestid)
		{
			Debug.Log("[DatabaseEventManager] HandleOnBuyPriceSetEvent " + item.NFTContract);
			if (DataManager.DataManager.Instance.IsCollectionCreatedByCurrentUser(item.NFTContract))
			{
				Debug.Log("[DatabaseEventManager] HandleOnBuyPriceSetEvent is for current user");
				string key = item.NFTContract + "_" + item.TokenId;
				//Will force refresh the next time it's called
				if (DataManager.DataManager.Instance.NFTDetailsCache.ContainsKey(key))
				{
					DataManager.DataManager.Instance.NFTDetailsCache.Remove(key);
				}
				OnEventReceived(item);
			}
		}

		private void HandleOnBuyPriceCanceledEvent(BuyPriceCanceledEvent item, int requestid)
		{
			Debug.Log("[DatabaseEventManager] HandleOnBuyPriceCanceledEvent " + item.NFTContract);
			if (DataManager.DataManager.Instance.IsCollectionCreatedByCurrentUser(item.NFTContract))
			{
				Debug.Log("[DatabaseEventManager] HandleOnBuyPriceCanceledEvent is for current user");
				string key = item.NFTContract + "_" + item.TokenId;
				//Will force refresh the next time it's called
				if (DataManager.DataManager.Instance.NFTDetailsCache.ContainsKey(key))
				{
					DataManager.DataManager.Instance.NFTDetailsCache.Remove(key);
				}
				OnEventReceived(item);
			}
		}

		private void HandleTransferEvent(EthNFTTransfers item, int requestid)
		{
			Debug.Log("[DatabaseEventManager] HandleTransferEvent " + item.TokenAddress);
			if (DataManager.DataManager.Instance.IsCollectionCreatedByCurrentUser(item.TokenAddress))
			{
				Debug.Log("[DatabaseEventManager] HandleTransferEvent is for current user");
				//string key = item.NFTContract + "_" + item.TokenId;
				////Will force refresh the next time it's called
				//if (DataManager.DataManager.Instance.NFTDetailsCache.ContainsKey(key))
				//{
				//	DataManager.DataManager.Instance.NFTDetailsCache.Remove(key);
				//}
				OnEventReceived(item);
			}
		}

		private async void OnEventReceived(AbstractContractEvent contractEvent)
		{
			await UnityMainThreadManager.Instance.EnqueueAsync(() =>
			{
				OnDatabaseEventReceived?.Invoke(contractEvent);
			});
		}

		#endregion
	}
}
