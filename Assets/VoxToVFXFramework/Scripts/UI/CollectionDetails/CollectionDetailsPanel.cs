using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.UI.Popups;
using VoxToVFXFramework.Scripts.UI.Profile;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.CollectionDetails
{
	public class CollectionDetailsPanel : MonoBehaviour
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

		[Header("Description")]
		[SerializeField] private TextMeshProUGUI DescriptionText;

		#endregion

		#region Fields

		private CollectionCreatedEvent mCollectionCreated;
		private CustomUser mCreatorUser;
		private Models.CollectionDetails mCollectionDetails;
		private eCollectionDetailsState mCollectionDetailsState;
		private TransparentButton[] mTransparentButtons;
		private VerticalLayoutGroup[] mVerticalLayoutGroups;
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
			MintNftButton.gameObject.SetActive(collection.Creator == UserManager.Instance.CurrentUser.EthAddress);
			EditCollectionButton.gameObject.SetActive(collection.Creator == UserManager.Instance.CurrentUser.EthAddress);
			CollectionNameText.text = collection.Name;
			CollectionSymbolText.text = collection.Symbol;
			CollectionDetailsState = eCollectionDetailsState.NFT;
			OpenUserProfileButton.Initialize(mCreatorUser);
			await RefreshCollectionDetails();
			await RefreshNFTTab();
			RebuildAllVerticalRect();
			LoadingBackgroundImage.gameObject.SetActive(false);
		}

		#endregion

		#region PrivateMethods

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
					await ImageUtils.DownloadAndApplyImageAndCropAfter(mCollectionDetails.LogoImageUrl, LogoImage, 184,184);
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
				NFTTabButton.gameObject.SetActive(!string.IsNullOrEmpty(mCollectionDetails.Description));
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

				LogoImage.transform.parent.gameObject.SetActive(false);
				CollectionNameText.color = Color.black;
				NFTTabButton.gameObject.SetActive(false);
			}
		}

		private async UniTask RefreshNFTTab()
		{
			NFTGridTransform.DestroyAllChildren();
			List<CollectionMintedEvent> listNfTsForContract = await DataManager.Instance.GetNFTForContractWithCache(mCreatorUser.EthAddress, mCollectionCreated.CollectionContract);
			int countActive = 0;
			foreach (CollectionMintedEvent nft in listNfTsForContract.OrderBy(t => t.createdAt))
			{
				ProfileListNFTItem item = Instantiate(ProfileListNftItem, NFTGridTransform, false);
				bool initSuccess = await item.Initialize(nft, mCreatorUser);
				item.gameObject.SetActive(initSuccess);
				if (initSuccess)
				{
					countActive++;
				}
			}

			CollectionOfCountText.text = countActive.ToString();
			NoItemOwnerFoundPanel.SetActive(countActive == 0 && mCreatorUser.EthAddress == UserManager.Instance.CurrentUser.EthAddress);
			NoItemFoundPanel.SetActive(countActive == 0 && mCreatorUser.EthAddress != UserManager.Instance.CurrentUser.EthAddress);
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
		#endregion
	}
}
