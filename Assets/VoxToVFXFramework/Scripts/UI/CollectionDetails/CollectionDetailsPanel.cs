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
using VoxToVFXFramework.Scripts.UI.Popups;
using VoxToVFXFramework.Scripts.UI.Profile;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.CollectionDetails
{
	public class CollectionDetailsPanel : MonoBehaviour, IFilterPanelListener
	{
		#region Enum

		private enum eCollectionDetailsState
		{
			NFT,
			DESCRIPTION,
			ACTIVITY
		}

		#endregion

		#region ScriptParameters

		[SerializeField] private Image MainImage;
		[SerializeField] private Image LogoImage;
		[SerializeField] private Image LoadingBackgroundImage;
		[SerializeField] private TextMeshProUGUI CollectionNameText;
		[SerializeField] private OpenUserProfileButton OpenUserProfileButton;
		[SerializeField] private Button EditCollectionButton;
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

		[Header("NFT")]
		[SerializeField] private ProfileListNFTItem ProfileListNftItem;
		[SerializeField] private Transform NFTGridTransform;
		[SerializeField] private ProfileFilterPanel ProfileFilterPanel;


		[Header("Description")]
		[SerializeField] private TextMeshProUGUI DescriptionText;

		#endregion

		#region Fields

		public string UserAddress => mCreatorUser.EthAddress;

		private eFilterOrderBy mFilterOrderBy;
		private CollectionCreatedEvent mCollectionCreated;
		private CustomUser mCreatorUser;
		private Models.CollectionDetails mCollectionDetails;
		private eCollectionDetailsState mCollectionDetailsState;
		private TransparentButton[] mTransparentButtons;
		private VerticalLayoutGroup[] mVerticalLayoutGroups;
		private DataManager.NftCollectionAndOwner mCollectionAndOwner;
		private readonly List<ProfileListNFTItem> mItems = new List<ProfileListNFTItem>();
		private eCollectionDetailsState CollectionDetailsState
		{
			get => mCollectionDetailsState;
			set
			{
				mCollectionDetailsState = value;
				NFTPanel.gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.NFT);
				ActivityPanel.gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.ACTIVITY);
				DescriptionPanel.gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.DESCRIPTION);
				NFTTabButton.transform.GetChild(0).gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.NFT);
				ActivityTabButton.transform.GetChild(0).gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.ACTIVITY);
				DescriptionTabButton.transform.GetChild(0).gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.DESCRIPTION);
			}
		}

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			OpenSymbolButton.onClick.AddListener(OnSymbolClicked);
			NFTTabButton.onClick.AddListener(() => OnSwitchTabClicked(eCollectionDetailsState.NFT));
			ActivityTabButton.onClick.AddListener(() => OnSwitchTabClicked(eCollectionDetailsState.ACTIVITY));
			DescriptionTabButton.onClick.AddListener(() => OnSwitchTabClicked(eCollectionDetailsState.DESCRIPTION));
			EditCollectionButton.onClick.AddListener(OnEditCollectionClicked);
			OpenOwnedByPopupButton.onClick.AddListener(OnOpenOwnedByClicked);
			MintNftButton.onClick.AddListener(OnMintNftClicked);
			mTransparentButtons = GetComponentsInChildren<TransparentButton>();
			mVerticalLayoutGroups = GetComponentsInChildren<VerticalLayoutGroup>();
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
		}

		#endregion

		#region PublicMethods

		public async void Initialize(CollectionCreatedEvent collection)
		{
			mCollectionCreated = collection;
			LoadingBackgroundImage.gameObject.SetActive(true);
			mCreatorUser = await DataManager.Instance.GetUserWithCache(collection.Creator);
			mCollectionDetails = await DataManager.Instance.GetCollectionDetailsWithCache(collection.CollectionContract);
			DescriptionTabButton.gameObject.SetActive(mCollectionDetails != null && !string.IsNullOrEmpty(mCollectionDetails.Description));
			MintNftButton.gameObject.SetActive(collection.Creator == UserManager.Instance.CurrentUserAddress);
			EditCollectionButton.gameObject.SetActive(collection.Creator == UserManager.Instance.CurrentUserAddress);
			CollectionNameText.text = collection.Name;
			CollectionSymbolText.text = collection.Symbol;
			CollectionDetailsState = eCollectionDetailsState.NFT;
			OpenUserProfileButton.Initialize(mCreatorUser);
			UniTask task1 = RefreshCollectionDetails();
			UniTask task2 = RefreshNFTTab();
			await (task1, task2);

			ProfileFilterPanel.Initialize(this);
			RebuildAllVerticalRect();
			LoadingBackgroundImage.gameObject.SetActive(false);
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
			List<ProfileListNFTItem> list = new List<ProfileListNFTItem>();

			switch (mFilterOrderBy)
			{
				case eFilterOrderBy.PRICE_HIGHEST_FIRST:
					list = mItems.OrderByDescending(item => item.BuyPriceInEther).ToList();
					break;
				case eFilterOrderBy.PRICE_LOWEST_FIRST:
					list = mItems.OrderBy(item => item.BuyPriceInEther).ToList();
					break;
				case eFilterOrderBy.NEWEST:
					list = mItems.OrderBy(item => item.MintedDate).ToList();
					break;
				case eFilterOrderBy.OLDEST:
					list = mItems.OrderByDescending(item => item.MintedDate).ToList();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mFilterOrderBy), mFilterOrderBy, null);
			}

			for (int index = 0; index < list.Count; index++)
			{
				ProfileListNFTItem item = list[index];
				item.transform.SetSiblingIndex(index);
			}
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
					await ImageUtils.DownloadAndApplyImageAndCropAfter(mCollectionDetails.LogoImageUrl, LogoImage, 184, 184);
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

				MainImage.sprite = null;
				LogoImage.sprite = null;
				LogoImage.transform.parent.gameObject.SetActive(false);
				CollectionNameText.color = Color.black;
				DescriptionTabButton.gameObject.SetActive(false);
			}
		}

		private async UniTask RefreshNFTTab()
		{
			NFTGridTransform.DestroyAllChildren();
			mItems.Clear();
			mCollectionAndOwner= await DataManager.Instance.GetNftCollectionWithCache(mCollectionCreated.CollectionContract);
			List<UniTask> tasks = new List<UniTask>();
			foreach (Nft nft in mCollectionAndOwner.NftCollection.Result.Where(t => !string.IsNullOrEmpty(t.Metadata)))
			{
				ProfileListNFTItem item = Instantiate(ProfileListNftItem, NFTGridTransform, false);
				mItems.Add(item);
				NftOwner nftOwner = mCollectionAndOwner.NftOwnerCollection.Result.FirstOrDefault(t => t.TokenId == nft.TokenId);
				tasks.Add(item.Initialize(nft, nftOwner));
			}

			await UniTask.WhenAll(tasks);

			int countActive = mItems.Count(t => t.InitSuccess);
			CollectionOfCountText.text = countActive.ToString();
			NoItemOwnerFoundPanel.SetActive(countActive == 0 && mCreatorUser.EthAddress == UserManager.Instance.CurrentUserAddress);
			OwnedByCountText.text = mCollectionAndOwner.NftOwnerCollection.Result.Select(t => t.OwnerOf).Distinct().Count().ToString();
			NoItemFoundPanel.SetActive(countActive == 0 && mCreatorUser.EthAddress != UserManager.Instance.CurrentUserAddress);
		}

		private void RebuildAllVerticalRect()
		{
			foreach (VerticalLayoutGroup verticalLayoutGroup in mVerticalLayoutGroups.Reverse())
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(verticalLayoutGroup.GetComponent<RectTransform>());
			}
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
			MessagePopup.ShowOwnedByPopup(mCollectionAndOwner.NftOwnerCollection.Result);
		}

		#endregion

		
	}
}
