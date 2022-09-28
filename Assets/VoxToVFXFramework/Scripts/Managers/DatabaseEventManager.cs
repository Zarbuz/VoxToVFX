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

		#region ConstStatic

		public const string NULL_ADDRESS = "0x0000000000000000000000000000000000000000";

		#endregion

		#region UnityMethods

		protected override async void OnStart()
		{
			await SubscribeToDatabaseEvents();
		}

		#endregion

		#region PublicMethods

		public void UpdateCache(AbstractContractEvent item)
		{
			switch (item)
			{
				case BuyPriceAcceptedEvent buyPriceAcceptedEvent:
					DataManager.DataManager.Instance.DeleteCacheForTokenId(buyPriceAcceptedEvent.NFTContract, buyPriceAcceptedEvent.TokenId);
					break;
				case BuyPriceCanceledEvent buyPriceCanceledEvent:
					DataManager.DataManager.Instance.DeleteCacheForTokenId(buyPriceCanceledEvent.NFTContract, buyPriceCanceledEvent.TokenId);
					break;
				case BuyPriceInvalidatedEvent buyPriceInvalidatedEvent:
					DataManager.DataManager.Instance.DeleteCacheForTokenId(buyPriceInvalidatedEvent.NFTContract, buyPriceInvalidatedEvent.TokenId);
					break;
				case BuyPriceSetEvent buyPriceSetEvent:
					DataManager.DataManager.Instance.DeleteCacheForTokenId(buyPriceSetEvent.NFTContract, buyPriceSetEvent.TokenId);
					break;
				case CollectionCreatedEvent collectionCreatedEvent:
					DataManager.DataManager.Instance.AddCollectionCreated(collectionCreatedEvent);
					break;
				case CollectionMintedEvent collectionMintedEvent:
					DataManager.DataManager.Instance.NftCollection.Remove(collectionMintedEvent.Address);
					break;
				case EthNFTTransfers ethNftTransfers:
					if (ethNftTransfers.ToAddress == NULL_ADDRESS)
					{
						Debug.Log("[DatabaseEventManager] ToAddress is null, NFT is burned");
						DataManager.DataManager.Instance.DeleteCacheNFTItemInCollection(ethNftTransfers.TokenAddress, ethNftTransfers.TokenId);
					}
					DataManager.DataManager.Instance.DeleteCacheForTokenId(ethNftTransfers.TokenAddress, ethNftTransfers.TokenId);
					break;
				case OfferAcceptedEvent offerAcceptedEvent:
					DataManager.DataManager.Instance.DeleteCacheForTokenId(offerAcceptedEvent.NFTContract, offerAcceptedEvent.TokenId);
					break;
				case OfferMadeEvent offerMadeEvent:
					DataManager.DataManager.Instance.DeleteCacheForTokenId(offerMadeEvent.NFTContract, offerMadeEvent.TokenId);
					break;
				case SelfDestructEvent selfDestructEvent:
					DataManager.DataManager.Instance.ContractCreatedPerUsers.Remove(selfDestructEvent.Owner);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(item));
			}
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

			MoralisQuery<SelfDestructEvent> selfDestructQuery = await Moralis.GetClient().Query<SelfDestructEvent>();
			MoralisLiveQueryCallbacks<SelfDestructEvent> selfDestructQueryCallbacks = new MoralisLiveQueryCallbacks<SelfDestructEvent>();

			MoralisQuery<BuyPriceAcceptedEvent> buyPriceAcceptedQuery = await Moralis.GetClient().Query<BuyPriceAcceptedEvent>();
			MoralisLiveQueryCallbacks<BuyPriceAcceptedEvent> buyPriceAcceptedQueryCallbacks = new MoralisLiveQueryCallbacks<BuyPriceAcceptedEvent>();

			MoralisQuery<OfferMadeEvent> offerMadeQuery = await Moralis.GetClient().Query<OfferMadeEvent>();
			MoralisLiveQueryCallbacks<OfferMadeEvent> offerMadeQueryCallbacks = new MoralisLiveQueryCallbacks<OfferMadeEvent>();

			MoralisQuery<OfferAcceptedEvent> offerAcceptedQuery = await Moralis.GetClient().Query<OfferAcceptedEvent>();
			MoralisLiveQueryCallbacks<OfferAcceptedEvent> offerAcceptedQueryCallbacks = new MoralisLiveQueryCallbacks<OfferAcceptedEvent>();

			collectionMintedQueryCallbacks.OnUpdateEvent += HandleGenericEvent;
			collectionMintedQueryCallbacks.OnErrorEvent += OnError;

			collectionCreatedQueryCallbacks.OnUpdateEvent += HandleGenericEvent;
			collectionCreatedQueryCallbacks.OnErrorEvent += OnError;

			setBuyPriceQueryCallbacks.OnUpdateEvent += HandleGenericEvent;
			setBuyPriceQueryCallbacks.OnErrorEvent += OnError;

			cancelBuyPriceQueryCallbacks.OnUpdateEvent += HandleGenericEvent;
			cancelBuyPriceQueryCallbacks.OnErrorEvent += OnError;

			transferQueryCallbacks.OnUpdateEvent += HandleGenericEvent;
			transferQueryCallbacks.OnErrorEvent += OnError;

			selfDestructQueryCallbacks.OnUpdateEvent += HandleGenericEvent;
			selfDestructQueryCallbacks.OnErrorEvent += OnError;

			buyPriceAcceptedQueryCallbacks.OnUpdateEvent += HandleGenericEvent;
			buyPriceAcceptedQueryCallbacks.OnErrorEvent += OnError;

			offerMadeQueryCallbacks.OnUpdateEvent += HandleGenericEvent;
			offerMadeQueryCallbacks.OnErrorEvent += OnError;

			offerAcceptedQueryCallbacks.OnUpdateEvent += HandleGenericEvent;
			offerAcceptedQueryCallbacks.OnErrorEvent += OnError;

			MoralisLiveQueryController.AddSubscription("CollectionCreatedEvent", collectionCreatedQuery, collectionCreatedQueryCallbacks);
			MoralisLiveQueryController.AddSubscription("CollectionMintedEvent", collectionMintedQuery, collectionMintedQueryCallbacks);
			MoralisLiveQueryController.AddSubscription("BuyPriceSetEvent", setBuyPriceQuery, setBuyPriceQueryCallbacks);
			MoralisLiveQueryController.AddSubscription("BuyPriceCanceledEvent", cancelBuyPriceQuery, cancelBuyPriceQueryCallbacks);
			MoralisLiveQueryController.AddSubscription("EthNFTTransfers", transferQuery, transferQueryCallbacks);
			MoralisLiveQueryController.AddSubscription("SelfDestructEvent", selfDestructQuery, selfDestructQueryCallbacks);
			MoralisLiveQueryController.AddSubscription("BuyPriceAcceptedEvent", buyPriceAcceptedQuery, buyPriceAcceptedQueryCallbacks);
			MoralisLiveQueryController.AddSubscription("OfferMadeEvent", offerMadeQuery, offerMadeQueryCallbacks);
			MoralisLiveQueryController.AddSubscription("OfferAcceptedEvent", offerAcceptedQuery, offerAcceptedQueryCallbacks);
		}

		private void OnError(ErrorMessage evt)
		{
			Debug.LogError("OnErrorEvent: " + evt.error + " " + evt.code);
		}

		private void HandleGenericEvent(AbstractContractEvent item, int requestId)
		{
			Debug.Log("[DatabaseEventManager] HandleGenericEvent: " + item.GetType() + " requestId: " + requestId);
			OnEventReceived(item);
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
