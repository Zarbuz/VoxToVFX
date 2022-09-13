using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MoralisUnity.Web3Api.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.Utils.Extensions;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public class ProfileListingPanel : MonoBehaviour, IFilterPanelListener
	{
		#region Enum

		private enum eProfileListingState
		{
			CREATED,
			COLLECTION,
			OWNED
		}

		#endregion

		#region ScriptParameters

		[Header("Tabs")]
		[SerializeField] private Button CreatedButton;
		[SerializeField] private Button CollectionButton;
		[SerializeField] private Button OwnedButton;

		[SerializeField] private TextMeshProUGUI CreatedCountText;
		[SerializeField] private TextMeshProUGUI CollectionCountText;
		[SerializeField] private TextMeshProUGUI OwnedCountText;

		[Header("Panels")]
		[SerializeField] private GameObject CreatedPanel;
		[SerializeField] private GameObject CollectionPanel;
		[SerializeField] private GameObject OwnedPanel;

		[SerializeField] private Transform CreatedGridTransform;
		[SerializeField] private Transform CollectionGridTransform;
		[SerializeField] private Transform OwnedGridTransform;

		[Header("ProfileListNFTItem")]
		[SerializeField] private ProfileListNFTItem ProfileListNftItemPrefab;
		[SerializeField] private ProfileCollectionItem ProfileCollectionItemPrefab;

		[SerializeField] private Image LoadingSpinner;

		[Header("Created")]
		[SerializeField] private ProfileFilterPanel ProfileFilterPanel;

		#endregion

		#region Fields

		private eProfileListingState mEProfileListingState;
		private eFilterOrderBy mFilterOrderBy;
		private string mCollectionFilterName;

		private eProfileListingState ProfileListingState
		{
			get => mEProfileListingState;
			set
			{
				mEProfileListingState = value;
				CreatedPanel.SetActive(mEProfileListingState == eProfileListingState.CREATED);
				CollectionPanel.SetActive(mEProfileListingState == eProfileListingState.COLLECTION);
				OwnedPanel.SetActive(mEProfileListingState == eProfileListingState.OWNED);

				CreatedButton.transform.GetChild(0).gameObject.SetActive(mEProfileListingState == eProfileListingState.CREATED);
				CollectionButton.transform.GetChild(0).gameObject.SetActive(mEProfileListingState == eProfileListingState.COLLECTION);
				OwnedButton.transform.GetChild(0).gameObject.SetActive(mEProfileListingState == eProfileListingState.OWNED);
			}
		}

		private List<ProfileListNFTItem> mItemsCreated = new List<ProfileListNFTItem>();
		private readonly List<ProfileListNFTItem> mItemsOwned = new List<ProfileListNFTItem>();
		private CustomUser mCustomUser;

		public string UserAddress => mCustomUser.EthAddress;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			CreatedButton.onClick.AddListener(() => OnSwitchTabClicked(eProfileListingState.CREATED));
			CollectionButton.onClick.AddListener(() => OnSwitchTabClicked(eProfileListingState.COLLECTION));
			OwnedButton.onClick.AddListener(() => OnSwitchTabClicked(eProfileListingState.OWNED));
			ProfileListingState = eProfileListingState.CREATED;
		}


		private void OnDisable()
		{
			CreatedButton.onClick.RemoveAllListeners();
			CollectionButton.onClick.RemoveAllListeners();
			OwnedButton.onClick.RemoveAllListeners();
		}

		#endregion

		#region PublicMethods

		public async void Initialize(CustomUser user)
		{
			mCustomUser = user;
			ShowSpinnerImage(true);
			UniTask task1 = RefreshCreatedTab();
			UniTask task2 = RefreshCollectionTab();
			UniTask task3 = RefreshOwnedTab();

			await (task1, task2, task3);
			ProfileFilterPanel.Initialize(this);
			ShowSpinnerImage(false);
		}

		public void OnFilterOrderByChanged(eFilterOrderBy orderBy)
		{
			mFilterOrderBy = orderBy;
			RefreshData();
		}

		public void OnCollectionFilterChanged(string collectionName)
		{
			mCollectionFilterName = collectionName;
			RefreshData();
		}



		#endregion

		#region PrivateMethods

		private void RefreshData()
		{
			List<ProfileListNFTItem> list = new List<ProfileListNFTItem>();

			switch (mFilterOrderBy)
			{
				case eFilterOrderBy.PRICE_HIGHEST_FIRST:
					list = mItemsCreated.OrderByDescending(item => item.BuyPriceInEther).ToList();
					break;
				case eFilterOrderBy.PRICE_LOWEST_FIRST:
					list = mItemsCreated.OrderBy(item => item.BuyPriceInEther).ToList();
					break;
				case eFilterOrderBy.NEWEST:
					list = mItemsCreated.OrderByDescending(item => item.MintedDate).ToList();
					break;
				case eFilterOrderBy.OLDEST:
					list = mItemsCreated.OrderBy(item => item.MintedDate).ToList();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mFilterOrderBy), mFilterOrderBy, null);
			}

			if (!string.IsNullOrEmpty(mCollectionFilterName))
			{
				list = list.Where(t => t.CollectionName == mCollectionFilterName).ToList();
			}

			mItemsCreated.ForEach(t => t.gameObject.SetActive(list.Contains(t)));

			for (int index = 0; index < list.Count; index++)
			{
				ProfileListNFTItem item = list[index];
				item.transform.SetSiblingIndex(index);
			}
		}

		private void OnSwitchTabClicked(eProfileListingState profileListingState)
		{
			ProfileListingState = profileListingState;
		}

		private void ShowSpinnerImage(bool showSpinner)
		{
			LoadingSpinner.gameObject.SetActive(showSpinner);
			CollectionPanel.SetActive(!showSpinner && ProfileListingState == eProfileListingState.COLLECTION);
			CreatedPanel.SetActive(!showSpinner && ProfileListingState == eProfileListingState.CREATED);
			OwnedPanel.SetActive(!showSpinner && ProfileListingState == eProfileListingState.OWNED);

			CreatedButton.interactable = !showSpinner;
			CollectionButton.interactable = !showSpinner;
			OwnedButton.interactable = !showSpinner;
		}

		private async UniTask RefreshCreatedTab()
		{
			CreatedGridTransform.DestroyAllChildren();

			mItemsCreated.Clear();
			List<UniTask> tasks = new List<UniTask>();
			List<CollectionCreatedEvent> list = await DataManager.Instance.GetUserListContractWithCache(mCustomUser.EthAddress);
			foreach (CollectionCreatedEvent collection in list.OrderByDescending(c => c.createdAt))
			{
				var nftCollection = await DataManager.Instance.GetNftCollectionWithCache(collection.CollectionContract);

				//List<CollectionMintedEvent> listNfTsForContract = await DataManager.Instance.GetNFTForContractWithCache(mCustomUser.EthAddress, collection.CollectionContract);
				foreach (Nft nft in nftCollection.NftCollection.Result.Where(t => !string.IsNullOrEmpty(t.Metadata)))
				{
					ProfileListNFTItem item = Instantiate(ProfileListNftItemPrefab, CreatedGridTransform, false);
					NftOwner nftOwner = nftCollection.NftOwnerCollection.Result.FirstOrDefault(t => t.TokenId == nft.TokenId);
					tasks.Add(item.Initialize(nft, nftOwner));
					mItemsCreated.Add(item);
				}
			}

			await UniTask.WhenAll(tasks);
			CreatedCountText.text = mItemsCreated.Count(i => i.InitSuccess).ToString();
		}

		private async UniTask RefreshCollectionTab()
		{
			CollectionGridTransform.DestroyAllChildren();
			List<CollectionCreatedEvent> list = await DataManager.Instance.GetUserListContractWithCache(mCustomUser.EthAddress);

			List<UniTask> tasks = new List<UniTask>();
			foreach (CollectionCreatedEvent collection in list.OrderByDescending(c => c.createdAt))
			{
				ProfileCollectionItem item = Instantiate(ProfileCollectionItemPrefab, CollectionGridTransform, false);
				tasks.Add(item.Initialize(collection));
			}
			await UniTask.WhenAll(tasks);
			CollectionCountText.text = list.Count.ToString();
		}

		private async UniTask RefreshOwnedTab()
		{
			OwnedGridTransform.DestroyAllChildren();
			NftOwnerCollection ownerCollection = await DataManager.Instance.GetNFTOwnedByUser(mCustomUser.EthAddress);

			mItemsOwned.Clear();
			List<UniTask> tasks = new List<UniTask>();

			//List<CollectionMintedEvent> listNfTsForContract = await DataManager.Instance.GetNFTForContractWithCache(mCustomUser.EthAddress, collection.CollectionContract);
			foreach (NftOwner nftOwner in ownerCollection.Result.Where(t => !string.IsNullOrEmpty(t.Metadata)))
			{
				ProfileListNFTItem item = Instantiate(ProfileListNftItemPrefab, OwnedGridTransform, false);

				Nft nft = new Nft()
				{
					Name = nftOwner.Name,
					Metadata = nftOwner.Metadata,
					Symbol = nftOwner.Symbol,
					TokenAddress = nftOwner.TokenAddress,
					TokenId = nftOwner.TokenId,
					SyncedAt = nftOwner.SyncedAt,
				};

				tasks.Add(item.Initialize(nft, nftOwner));
				mItemsOwned.Add(item);
			}

			await UniTask.WhenAll(tasks);
			OwnedCountText.text = mItemsOwned.Count(i => i.InitSuccess).ToString();
		}

		#endregion



		
	}
}
