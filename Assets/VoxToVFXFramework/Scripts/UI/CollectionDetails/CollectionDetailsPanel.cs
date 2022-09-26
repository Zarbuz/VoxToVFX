using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.UI.CollectionUpdate;
using VoxToVFXFramework.Scripts.UI.Popups;
using VoxToVFXFramework.Scripts.UI.Profile;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.CollectionDetails
{
	public class CollectionDetailsPanel : AbstractComplexDetailsPanel, IFilterPanelListener
	{
		#region Enum

		private enum eCollectionDetailsState
		{
			NFT,
			DESCRIPTION,
			ACTIVITY,
			LOADING
		}

		#endregion

		#region ScriptParameters

		[Header("General")]

		[Header("MainImage")]
		[SerializeField] private RawImage MainImage;
		[SerializeField] private RawImage LogoImage;
		[SerializeField] private Image LoadingBackgroundImage;
		[SerializeField] private TextMeshProUGUI CollectionNameText;
		[SerializeField] private OpenUserProfileButton OpenUserProfileButton;
		[SerializeField] private Button EditCollectionButton;

		[Header("CollectionInfoPanel")]
		[SerializeField] private TextMeshProUGUI CollectionOfCountText;
		[SerializeField] private Button OpenOwnedByPopupButton;
		[SerializeField] private TextMeshProUGUI OwnedByCountText;
		[SerializeField] private TextMeshProUGUI FloorPriceText;
		[SerializeField] private TextMeshProUGUI TotalSalesText;
		
		[SerializeField] private Button NFTTabButton;
		[SerializeField] private Button ActivityTabButton;
		[SerializeField] private Button DescriptionTabButton;
		[SerializeField] private GameObject NoItemFoundPanel;
		[SerializeField] private GameObject NoItemOwnerFoundPanel;
		[SerializeField] private Button MintNftButton;

		[SerializeField] private Button OpenSymbolButton;
		[SerializeField] private TextMeshProUGUI CollectionSymbolText;

		[SerializeField] private GameObject NFTPanel;
		[SerializeField] private GameObject ActivityPanel;
		[SerializeField] private GameObject DescriptionPanel;

		[Header("Manage")]
		[SerializeField] private Toggle MoreToggle;
		[SerializeField] private Button SelfDestructButton;

		[Header("NFT")]
		[SerializeField] private ProfileNFTGridAdaptater ProfileNftGridAdaptater;
		[SerializeField] private ProfileFilterPanel ProfileFilterPanel;


		[Header("Description")]
		[SerializeField] private TextMeshProUGUI DescriptionText;

		#endregion

		#region Fields

		private eFilterOrderBy mFilterOrderBy;
		private CollectionCreatedEvent mCollectionCreated;
		public string UserAddress => mCreatorUser;

		private string mCreatorUser;
		private Models.CollectionDetails mCollectionDetails;
		private eCollectionDetailsState mCollectionDetailsState;
		private TransparentButton[] mTransparentButtons;
		private DataManager.NftCollectionCache mCollectionCache;
		private readonly List<NftOwnerWithDetails> mItems = new List<NftOwnerWithDetails>();
		private eCollectionDetailsState CollectionDetailsState
		{
			get => mCollectionDetailsState;
			set
			{
				mCollectionDetailsState = value;
				NFTPanel.gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.NFT || mCollectionDetailsState == eCollectionDetailsState.LOADING);
				ActivityPanel.gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.ACTIVITY);
				DescriptionPanel.gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.DESCRIPTION);
				NFTTabButton.transform.GetChild(0).gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.NFT);
				ActivityTabButton.transform.GetChild(0).gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.ACTIVITY);
				DescriptionTabButton.transform.GetChild(0).gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.DESCRIPTION);
				LoadingBackgroundImage.gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.LOADING);
			}
		}

		#endregion

		#region UnityMethods

		protected override void OnEnable()
		{
			base.OnEnable();
			OpenSymbolButton.onClick.AddListener(OnSymbolClicked);
			NFTTabButton.onClick.AddListener(() => OnSwitchTabClicked(eCollectionDetailsState.NFT));
			ActivityTabButton.onClick.AddListener(() => OnSwitchTabClicked(eCollectionDetailsState.ACTIVITY));
			DescriptionTabButton.onClick.AddListener(() => OnSwitchTabClicked(eCollectionDetailsState.DESCRIPTION));
			EditCollectionButton.onClick.AddListener(OnEditCollectionClicked);
			OpenOwnedByPopupButton.onClick.AddListener(OnOpenOwnedByClicked);
			MintNftButton.onClick.AddListener(OnMintNftClicked);
			SelfDestructButton.onClick.AddListener(OnSelfDestructClicked);
			mTransparentButtons = GetComponentsInChildren<TransparentButton>();
			MoreToggle.isOn = false;
		}

		private void OnDisable()
		{
			OpenSymbolButton.onClick.RemoveListener(OnSymbolClicked);
			NFTTabButton.onClick.RemoveAllListeners();
			ActivityTabButton.onClick.RemoveAllListeners();
			DescriptionTabButton.onClick.RemoveAllListeners();
			EditCollectionButton.onClick.RemoveListener(OnEditCollectionClicked);
			MintNftButton.onClick.RemoveListener(OnMintNftClicked);
			OpenOwnedByPopupButton.onClick.RemoveListener(OnOpenOwnedByClicked);
			SelfDestructButton.onClick.RemoveListener(OnSelfDestructClicked);
		}

		#endregion

		#region PublicMethods

		public async void Initialize(CollectionCreatedEvent collection)
		{
			CollectionDetailsState = eCollectionDetailsState.LOADING;

			mCollectionCreated = collection;
			mCreatorUser = collection.Creator;
			mCollectionDetails = await DataManager.Instance.GetCollectionDetailsWithCache(collection.CollectionContract);
			DescriptionTabButton.gameObject.SetActive(mCollectionDetails != null && !string.IsNullOrEmpty(mCollectionDetails.Description));
			MintNftButton.gameObject.SetActive(collection.Creator == UserManager.Instance.CurrentUserAddress);
			EditCollectionButton.gameObject.SetActive(collection.Creator == UserManager.Instance.CurrentUserAddress);
			CollectionNameText.text = collection.Name;
			CollectionSymbolText.text = collection.Symbol;
			OpenUserProfileButton.Initialize(collection.Creator);
			MoreToggle.gameObject.SetActive(mCreatorUser == UserManager.Instance.CurrentUserAddress);

			UniTask task1 = RefreshCollectionDetails();
			UniTask task2 = RefreshNFTTab();
			await (task1, task2);

			SelfDestructButton.gameObject.SetActive(mItems.Count == 0);
			ProfileFilterPanel.Initialize(this);
			RebuildAllVerticalRect();

			CollectionDetailsState = eCollectionDetailsState.NFT;
		}


		public void OnFilterOrderByChanged(eFilterOrderBy orderBy)
		{
			mFilterOrderBy = orderBy;
			RefreshData();
		}

		public void OnCollectionFilterChanged(string collectionName)
		{
			
		}

		#endregion

		#region PrivateMethods

		private void RefreshData()
		{
			List<NftOwnerWithDetails> list = new List<NftOwnerWithDetails>();

			switch (mFilterOrderBy)
			{
				case eFilterOrderBy.PRICE_HIGHEST_FIRST:
					list = mItems.OrderByDescending(item => item.BuyPriceInEther).ToList();
					break;
				case eFilterOrderBy.PRICE_LOWEST_FIRST:
					list = mItems.OrderBy(item => item.BuyPriceInEther).ToList();
					break;
				case eFilterOrderBy.NEWEST:
					list = mItems.OrderByDescending(item => item.MintedDate).ToList();
					break;
				case eFilterOrderBy.OLDEST:
					list = mItems.OrderBy(item => item.MintedDate).ToList();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mFilterOrderBy), mFilterOrderBy, null);
			}

			ProfileNftGridAdaptater.Initialize(list);
		}

		private void OnSymbolClicked()
		{
			string url = ConfigManager.Instance.EtherScanBaseUrl + "address/" + mCollectionCreated.CollectionContract;
			Application.OpenURL(url);
		}

		private void OnSwitchTabClicked(eCollectionDetailsState collectionDetailsState)
		{
			CollectionDetailsState = collectionDetailsState;
			RebuildAllVerticalRect();
		}

		private void OnEditCollectionClicked()
		{
			if (mCollectionDetails == null)
			{
				MessagePopup.ShowEditCollectionPopup(string.Empty, string.Empty, string.Empty, OnCollectionUpdated, null);
			}
			else
			{
				MessagePopup.ShowEditCollectionPopup(mCollectionDetails.LogoImageUrl, mCollectionDetails.CoverImageUrl, mCollectionDetails.Description, OnCollectionUpdated, null);
			}
		}

		private async UniTask RefreshCollectionDetails()
		{
			if (mCollectionDetails != null)
			{
				await ImageUtils.DownloadAndApplyWholeImage(mCollectionDetails.CoverImageUrl, MainImage);
				if (!string.IsNullOrEmpty(mCollectionDetails.LogoImageUrl))
				{
					LogoImage.transform.parent.gameObject.SetActive(true);
					await ImageUtils.DownloadAndApplyImageAndCrop(mCollectionDetails.LogoImageUrl, LogoImage, 184, 184);
				}
				else
				{
					LogoImage.transform.parent.gameObject.SetActive(false);
				}

				foreach (TransparentButton transparentButton in mTransparentButtons)
				{
					transparentButton.ImageBackgroundActive = !string.IsNullOrEmpty(mCollectionDetails.CoverImageUrl);
				}
				DescriptionText.text = mCollectionDetails.Description;
				DescriptionTabButton.gameObject.SetActive(!string.IsNullOrEmpty(mCollectionDetails.Description));
				if (!string.IsNullOrEmpty(mCollectionDetails.CoverImageUrl))
				{
					CollectionNameText.color = Color.white;
				}
			}
			else
			{
				foreach (TransparentButton transparentButton in mTransparentButtons)
				{
					transparentButton.ImageBackgroundActive = false;
				}

				MainImage.texture = null;
				LogoImage.texture = null;
				LogoImage.transform.parent.gameObject.SetActive(false);
				CollectionNameText.color = Color.black;
				DescriptionTabButton.gameObject.SetActive(false);
			}
		}

		private async UniTask RefreshNFTTab()
		{
			mItems.Clear();
			mCollectionCache= await DataManager.Instance.GetNftCollectionWithCache(mCollectionCreated.CollectionContract);
			foreach (NftOwner nft in mCollectionCache.NftOwnerCollection.Result.Where(t => !string.IsNullOrEmpty(t.Metadata)))
			{
				mItems.Add(new NftOwnerWithDetails(nft));
			}

			ProfileNftGridAdaptater.Initialize(mItems);

			int countActive = mItems.Count;
			CollectionOfCountText.text = countActive.ToString();
			NoItemOwnerFoundPanel.SetActive(countActive == 0 && mCreatorUser == UserManager.Instance.CurrentUserAddress);
			OwnedByCountText.text = mCollectionCache.NftOwnerCollection.Result.Select(t => t.OwnerOf).Where(t => !string.Equals(t, ConfigManager.Instance.SmartContractAddress.VoxToVFXMarketAddress, StringComparison.InvariantCultureIgnoreCase)).Distinct().Count().ToString();
			NoItemFoundPanel.SetActive(countActive == 0 && mCreatorUser != UserManager.Instance.CurrentUserAddress);
		}

		private async void OnCollectionUpdated(Models.CollectionDetails collectionDetails)
		{
			collectionDetails.CollectionContract = mCollectionCreated.CollectionContract;
			mCollectionDetails = collectionDetails;
			await CollectionDetailsManager.Instance.SaveCollectionDetails(mCollectionDetails);
			DataManager.Instance.SaveCollectionDetails(mCollectionDetails);
			await RefreshCollectionDetails();
		}

		private void OnMintNftClicked()
		{
			CanvasPlayerPCManager.Instance.OpenCreationPanel(mCollectionCreated);
		}

		private void OnOpenOwnedByClicked()
		{
			MessagePopup.ShowOwnedByPopup(mCollectionCache.NftOwnerCollection.Result.Where(t => !string.Equals(t.OwnerOf, ConfigManager.Instance.SmartContractAddress.VoxToVFXMarketAddress, StringComparison.InvariantCultureIgnoreCase)).ToList());
		}

		private void OnSelfDestructClicked()
		{
			CanvasPlayerPCManager.Instance.OpenUpdateCollectionPanel(eCollectionUpdateTargetType.BURN, mCollectionCreated);
		}

		#endregion


	}
}
