using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.NFTDetails
{
	public class NFTDetailsPanel : AbstractComplexDetailsPanel
	{
		#region ScriptParameters

		[Header("Global")]
		[SerializeField] private Image LoadingBackgroundImage;

		[Header("Top")]
		[SerializeField] private Image MainImage;

		[Header("Left")]
		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private TextMeshProUGUI DescriptionLabel;
		[SerializeField] private TextMeshProUGUI Description;
		[SerializeField] private Button OpenTransactionButton;
		[SerializeField] private TextMeshProUGUI MintedDateText;
		[SerializeField] private OpenUserProfileButton OpenUserProfileButton;
		[SerializeField] private TextMeshProUGUI CollectionNameText;
		[SerializeField] private Image CollectionImage;
		[SerializeField] private Button ViewEtherscanButton;
		[SerializeField] private Button ViewMetadataButton;
		[SerializeField] private Button ViewIpfsButton;
		[SerializeField] private Button OpenCollectionButton;

		[Header("Right")]
		[SerializeField] private Button LoadVoxModelButton;
		[SerializeField] private NFTDetailsManagePanel NFTDetailsManagePanel;
		[SerializeField] private NFTLastActionPanel NFTLastActionPanel;
		[SerializeField] private ProvenanceNFTItem ProvenanceNftItemPrefab;
		[SerializeField] private VerticalLayoutGroup RightPart;

		[Header("EmptySpace")]
		[SerializeField] private LayoutElement EmptySpaceLeft;
		[SerializeField] private LayoutElement EmptySpaceRight;

		#endregion

		#region Fields

		private CollectionCreatedEvent mCollectionCreated;
		private CollectionMintedEvent mCollectionMinted;
		private NftWithDetails mNft;

		private readonly List<ProvenanceNFTItem> mProvenanceNFTItemList = new List<ProvenanceNFTItem>();
		#endregion

		#region UnityMethods

		protected override void OnEnable()
		{
			base.OnEnable();
			OpenTransactionButton.onClick.AddListener(OnOpenTransactionClicked);
			ViewEtherscanButton.onClick.AddListener(OnViewEtherscanClicked);
			ViewMetadataButton.onClick.AddListener(OnViewMetadataClicked);
			ViewIpfsButton.onClick.AddListener(OnViewIpfsClicked);
			LoadVoxModelButton.onClick.AddListener(OnLoadVoxModelClicked);
			OpenCollectionButton.onClick.AddListener(OnOpenCollectionClicked);
		}

		private void OnDisable()
		{
			OpenTransactionButton.onClick.RemoveListener(OnOpenTransactionClicked);
			ViewEtherscanButton.onClick.RemoveListener(OnViewEtherscanClicked);
			ViewMetadataButton.onClick.RemoveListener(OnViewMetadataClicked);
			ViewIpfsButton.onClick.RemoveListener(OnViewIpfsClicked);
			LoadVoxModelButton.onClick.RemoveListener(OnLoadVoxModelClicked);
			OpenCollectionButton.onClick.RemoveListener(OnOpenCollectionClicked);
		}

		#endregion

		#region ConstStatic

		private const int HEIGHT_SPACE_LEFT_WITH_BUY_NOW = 174;
		private const int HEIGHT_SPACE_LEFT_WITHOUT_BUY_NOW = 24;
		private const int HEIGHT_SPACE_LEFT_MANAGE = 66;

		private const int HEIGHT_SPACE_RIGHT_WITH_BUY_NOW = 290;
		private const int HEIGHT_SPACE_RIGHT_WITHOUT_BUY_NOW = 140;
		private const int HEIGHT_SPACE_RIGHT_MANAGE = 185;

		#endregion

		#region PublicMethods

		public async void Initialize(NftWithDetails nft)
		{
			mNft = nft;

			LoadingBackgroundImage.gameObject.SetActive(true);

			string creatorAddress = await DataManager.Instance.GetCreatorOfCollection(nft.TokenAddress);
			CustomUser creatorUser = await DataManager.Instance.GetUserWithCache(creatorAddress);
			Models.CollectionDetails collectionDetails = await DataManager.Instance.GetCollectionDetailsWithCache(nft.TokenAddress);
			NFTDetailsContractType details = await DataManager.Instance.GetNFTDetailsWithCache(nft.TokenAddress, nft.TokenId);
			mCollectionCreated = await DataManager.Instance.GetCollectionCreatedEventWithCache(nft.TokenAddress);
			List<AbstractContractEvent> events = await DataManager.Instance.GetAllEventsForNFT(nft.TokenAddress, nft.TokenId);
			mCollectionMinted = (CollectionMintedEvent)events.First(e => e is CollectionMintedEvent);
			BuildProvenanceDetails(events);

			MintedDateText.text = string.Format(LocalizationKeys.MINTED_ON_DATE.Translate(), mCollectionMinted.createdAt.Value.ToShortDateString());
			NFTDetailsManagePanel.gameObject.SetActive(details.OwnerInLowercase == UserManager.Instance.CurrentUserAddress);
			NFTDetailsManagePanel.Initialize(nft, creatorUser, details);

			NFTLastActionPanel.gameObject.SetActive(details.OwnerInLowercase != UserManager.Instance.CurrentUserAddress);
			NFTLastActionPanel.Initialize(nft, details, events);

			EmptySpaceRight.minHeight = NFTDetailsManagePanel.gameObject.activeSelf ? HEIGHT_SPACE_RIGHT_MANAGE :
				details.IsInEscrow ? HEIGHT_SPACE_RIGHT_WITH_BUY_NOW : HEIGHT_SPACE_RIGHT_WITHOUT_BUY_NOW;
			EmptySpaceLeft.minHeight = NFTDetailsManagePanel.gameObject.activeSelf ? HEIGHT_SPACE_LEFT_MANAGE :
				details.IsInEscrow ? HEIGHT_SPACE_LEFT_WITH_BUY_NOW : HEIGHT_SPACE_LEFT_WITHOUT_BUY_NOW;

			OpenUserProfileButton.Initialize(creatorAddress);
			CollectionNameText.text = mCollectionCreated.Name;
			Title.text = nft.MetadataObject.Name;
			DescriptionLabel.gameObject.SetActive(!string.IsNullOrEmpty(nft.MetadataObject.Description));
			Description.gameObject.SetActive(!string.IsNullOrEmpty(nft.MetadataObject.Description));
			Description.text = nft.MetadataObject.Description;

			CollectionImage.transform.parent.gameObject.SetActive(collectionDetails != null && !string.IsNullOrEmpty(collectionDetails.LogoImageUrl));
			if (collectionDetails != null)
			{
				if (!string.IsNullOrEmpty(collectionDetails.LogoImageUrl))
				{
					bool success = await ImageUtils.DownloadAndApplyImageAndCrop(collectionDetails.LogoImageUrl, CollectionImage, 32, 32);
					if (!success)
					{
						CollectionImage.transform.parent.gameObject.SetActive(false);
					}
				}
			}

			await ImageUtils.DownloadAndApplyImage(nft.MetadataObject.Image, MainImage);
			await UniTask.WaitForEndOfFrame();
			RebuildAllVerticalRect();
			LoadingBackgroundImage.gameObject.SetActive(false);
		}

		#endregion

		#region PrivateMethods


		private void BuildProvenanceDetails(List<AbstractContractEvent> events)
		{
			foreach (ProvenanceNFTItem item in mProvenanceNFTItemList)
			{
				Destroy(item.gameObject);
			}

			mProvenanceNFTItemList.Clear();

			foreach (AbstractContractEvent contractEvent in events)
			{
				ProvenanceNFTItem item = Instantiate(ProvenanceNftItemPrefab, RightPart.transform, false);
				item.Initialize(contractEvent);
				mProvenanceNFTItemList.Add(item);
			}
		}


		private void OnOpenCollectionClicked()
		{
			CanvasPlayerPCManager.Instance.OpenCollectionDetailsPanel(mCollectionCreated);
		}

		private void OnViewEtherscanClicked()
		{
			string url = ConfigManager.Instance.EtherScanBaseUrl + "nft/" + mNft.TokenAddress + "/" + mNft.TokenId;
			Application.OpenURL(url);
		}

		private void OnViewMetadataClicked()
		{
			Application.OpenURL(mNft.TokenUri);
		}

		private void OnViewIpfsClicked()
		{
			Application.OpenURL(mNft.MetadataObject.Image);
		}

		private void OnOpenTransactionClicked()
		{
			string url = ConfigManager.Instance.EtherScanBaseUrl + "tx/" + mCollectionMinted.TransactionHash;
			Application.OpenURL(url);
		}

		private async void OnLoadVoxModelClicked()
		{
			string fileName = mCollectionMinted.TransactionHash + ".zip";
			string zipPath = Path.Combine(Application.persistentDataPath, VoxelDataCreatorManager.VOX_FOLDER_CACHE_NAME, fileName);

			if (File.Exists(zipPath))
			{
				ReadZipPath(zipPath);
			}
			else
			{
				CanvasPlayerPCManager.Instance.OpenLoadingPanel(LocalizationKeys.LOADING_DOWNLOAD_MODEL.Translate());
				await VoxelDataCreatorManager.Instance.DownloadVoxModel(mNft.MetadataObject.FilesUrl, zipPath);
				ReadZipPath(zipPath);
			}
		}

		private void ReadZipPath(string zipPath)
		{
			CanvasPlayerPCManager.Instance.OpenLoadingPanel(LocalizationKeys.LOADING_SCENE_DESCRIPTION.Translate(),
				() =>
				{
					CanvasPlayerPCManager.Instance.GenericClosePanel();
				});

			VoxelDataCreatorManager.Instance.ReadZipFile(zipPath);
		}
		#endregion
	}
}
