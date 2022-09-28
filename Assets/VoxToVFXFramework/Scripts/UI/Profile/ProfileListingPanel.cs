using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public class ProfileListingPanel : MonoBehaviour, IFilterPanelListener
	{
		#region Enum

		private enum eProfileListingState
		{
			CREATED,
			COLLECTION,
			OWNED,
			LOADING
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

		[SerializeField] private GameObject NoCreatedPanel;
		[SerializeField] private GameObject NoCollectionPanel;
		[SerializeField] private GameObject NoOwnedPanel;

		[SerializeField] private ProfileNFTGridAdaptater CreatedNFTGridAdaptater;
		[SerializeField] private ProfileCollectionGridAdaptater CollectionGridAdaptater;
		[SerializeField] private ProfileNFTGridAdaptater OwnedGridAdaptater;

		[SerializeField] private GameObject LoadingPanel;

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
				CreatedPanel.SetActive(mEProfileListingState == eProfileListingState.CREATED || mEProfileListingState == eProfileListingState.LOADING);
				CollectionPanel.SetActive(mEProfileListingState == eProfileListingState.COLLECTION || mEProfileListingState == eProfileListingState.LOADING);
				OwnedPanel.SetActive(mEProfileListingState == eProfileListingState.OWNED || mEProfileListingState == eProfileListingState.LOADING);

				CreatedButton.transform.GetChild(0).gameObject.SetActive(mEProfileListingState == eProfileListingState.CREATED);
				CollectionButton.transform.GetChild(0).gameObject.SetActive(mEProfileListingState == eProfileListingState.COLLECTION);
				OwnedButton.transform.GetChild(0).gameObject.SetActive(mEProfileListingState == eProfileListingState.OWNED);

				LoadingPanel.gameObject.SetActive(mEProfileListingState == eProfileListingState.LOADING);

				CreatedButton.interactable = mEProfileListingState != eProfileListingState.LOADING;
				CollectionButton.interactable = mEProfileListingState != eProfileListingState.LOADING;
				OwnedButton.interactable = mEProfileListingState != eProfileListingState.LOADING;
			}
		}

		private readonly List<NftWithDetails> mItemCreated = new List<NftWithDetails>();
		private CustomUser mCustomUser;

		public string UserAddress => mCustomUser.EthAddress;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			CreatedButton.onClick.AddListener(() => OnSwitchTabClicked(eProfileListingState.CREATED));
			CollectionButton.onClick.AddListener(() => OnSwitchTabClicked(eProfileListingState.COLLECTION));
			OwnedButton.onClick.AddListener(() => OnSwitchTabClicked(eProfileListingState.OWNED));
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
			ProfileListingState = eProfileListingState.LOADING;
			UniTask task1 = RefreshCreatedTab();
			UniTask task2 = RefreshCollectionTab();
			UniTask task3 = RefreshOwnedTab();

			await (task1, task2, task3);
			ProfileListingState = eProfileListingState.CREATED;
			ProfileFilterPanel.Initialize(this);
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
			List<NftWithDetails> list = new List<NftWithDetails>();

			switch (mFilterOrderBy)
			{
				case eFilterOrderBy.PRICE_HIGHEST_FIRST:
					list = mItemCreated.Where(t => t.BuyPriceInEther != 0).OrderByDescending(item => item.BuyPriceInEther).ToList();
					break;
				case eFilterOrderBy.PRICE_LOWEST_FIRST:
					list = mItemCreated.Where(t => t.BuyPriceInEther != 0).OrderBy(item => item.BuyPriceInEther).ToList();
					break;
				case eFilterOrderBy.NEWEST:
					list = mItemCreated.OrderByDescending(item => item.MintedDate).ToList();
					break;
				case eFilterOrderBy.OLDEST:
					list = mItemCreated.OrderBy(item => item.MintedDate).ToList();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mFilterOrderBy), mFilterOrderBy, null);
			}

			if (!string.IsNullOrEmpty(mCollectionFilterName))
			{
				list = list.Where(t => t.CollectionName == mCollectionFilterName).ToList();
			}

			CreatedNFTGridAdaptater.Initialize(list);
		}

		private void OnSwitchTabClicked(eProfileListingState profileListingState)
		{
			ProfileListingState = profileListingState;
		}

		private async UniTask RefreshCreatedTab()
		{
			List<CollectionCreatedEvent> list = await DataManager.Instance.GetUserListContractWithCache(mCustomUser.EthAddress);
			mItemCreated.Clear();
			foreach (CollectionCreatedEvent collection in list.OrderByDescending(c => c.createdAt))
			{
				List<NftWithDetails> nftCollection = await DataManager.Instance.GetNftCollectionWithCache(collection.CollectionContract);
				mItemCreated.AddRange(nftCollection);
			}

			NoCreatedPanel.gameObject.SetActive(mItemCreated.Count == 0);
			CreatedNFTGridAdaptater.Initialize(mItemCreated);
			CreatedCountText.text = mItemCreated.Count.ToString();
		}

		private async UniTask RefreshCollectionTab()
		{
			List<CollectionCreatedEvent> list = await DataManager.Instance.GetUserListContractWithCache(mCustomUser.EthAddress);

			CollectionGridAdaptater.Initialize(list.OrderByDescending(c => c.createdAt).ToList());
			CollectionCountText.text = list.Count.ToString();
			NoCollectionPanel.SetActive(list.Count == 0);
		}

		private async UniTask RefreshOwnedTab()
		{
			List<NftWithDetails> collection = await DataManager.Instance.GetNFTOwnedByUser(mCustomUser.EthAddress);
			OwnedGridAdaptater.Initialize(collection);
			OwnedCountText.text = collection.Count.ToString();
			NoOwnedPanel.SetActive(collection.Count == 0);
		}

		#endregion


	}
}
