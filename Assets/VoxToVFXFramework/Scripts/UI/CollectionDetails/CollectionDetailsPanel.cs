using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.UI.Popups;
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
		[SerializeField] private Button MintNftButton;

		[SerializeField] private Button OpenSymbolButton;
		[SerializeField] private TextMeshProUGUI CollectionSymbolText;

		[SerializeField] private GameObject NFTPanel;
		[SerializeField] private GameObject ActivityPanel;
		[SerializeField] private GameObject DescriptionPanel;
		[SerializeField] private TextMeshProUGUI DescriptionText;

		#endregion

		#region Fields

		private CollectionCreatedEvent mCollectionCreated;
		private Models.CollectionDetails mCollectionDetails;
		private eCollectionDetailsState mCollectionDetailsState;
		private TransparentButton[] mTransparentButtons;

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

		public async UniTask Initialize(CollectionCreatedEvent collection)
		{
			mCollectionCreated = collection;
			LoadingBackgroundImage.gameObject.SetActive(true);
			CustomUser creatorUser = await UserManager.Instance.LoadUserFromEthAddress(collection.Creator);
			mCollectionDetails = await CollectionDetailsManager.Instance.GetCollectionDetails(collection.CollectionContract);
			DescriptionTabButton.gameObject.SetActive(mCollectionDetails != null && !string.IsNullOrEmpty(mCollectionDetails.Description));
			MintNftButton.gameObject.SetActive(collection.Creator == UserManager.Instance.CurrentUser.EthAddress);
			EditCollectionButton.gameObject.SetActive(collection.Creator == UserManager.Instance.CurrentUser.EthAddress);
			CollectionNameText.text = collection.Name;
			CollectionSymbolText.text = collection.Symbol;
			CollectionDetailsState = eCollectionDetailsState.NFT;
			OpenUserProfileButton.Initialize(creatorUser);
			RefreshCollectionDetails();
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

		private async void RefreshCollectionDetails()
		{
			if (mCollectionDetails != null)
			{
				await ImageUtils.DownloadAndApplyWholeImage(mCollectionDetails.CoverImageUrl, MainImage);

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

				CollectionNameText.color = Color.black;
				NFTTabButton.gameObject.SetActive(false);
			}
			LoadingBackgroundImage.gameObject.SetActive(false);
		}

		private async void OnCollectionUpdated(Models.CollectionDetails collectionDetails)
		{
			collectionDetails.CollectionContract = mCollectionCreated.CollectionContract;
			mCollectionDetails = collectionDetails;
			await CollectionDetailsManager.Instance.SaveCollectionDetails(mCollectionDetails);
			RefreshCollectionDetails();
		}

		private void OnMintNftClicked()
		{
			CanvasPlayerPCManager.Instance.OpenCreationPanel(mCollectionCreated);
		}
		#endregion
	}
}
