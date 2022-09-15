using System;
using System.IO;
using MoralisUnity.Web3Api.Models;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.NFTDetails
{
	public class NFTDetailsPanel : MonoBehaviour
	{
		#region ScriptParameters

		[Header("Global")]
		[SerializeField] private VerticalLayoutGroup VerticalLayoutGroup;
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

		#endregion

		#region Fields

		private CollectionMintedEvent mCollectionMinted;
		private CollectionCreatedEvent mCollectionCreated;
		private NftOwner mNft;
		private MetadataObject mMetadataObject;
		#endregion

		#region UnityMethods

		private void OnEnable()
		{
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

		#region PublicMethods

		public async void Initialize(NftOwner nft)
		{
			mNft = nft;

			LoadingBackgroundImage.gameObject.SetActive(true);

			string creatorAddress = await DataManager.Instance.GetCreatorOfCollection(nft.TokenAddress);
			CustomUser creatorUser = await DataManager.Instance.GetUserWithCache(creatorAddress);
			Models.CollectionDetails details = await DataManager.Instance.GetCollectionDetailsWithCache(nft.TokenAddress);
			mCollectionCreated = await DataManager.Instance.GetCollectionCreatedEventWithCache(nft.TokenAddress);
			mCollectionMinted = await DataManager.Instance.GetCollectionMintedWithCache(nft.TokenAddress, nft.TokenId);

			NFTDetailsManagePanel.gameObject.SetActive(nft.OwnerOf == UserManager.Instance.CurrentUserAddress);
			NFTDetailsManagePanel.Initialize(nft, creatorUser);
			OpenUserProfileButton.Initialize(creatorUser);
			CollectionNameText.text = mCollectionCreated.Name;
			try
			{
				mMetadataObject = JsonConvert.DeserializeObject<MetadataObject>(nft.Metadata);
				Title.text = mMetadataObject.Name;
				DescriptionLabel.gameObject.SetActive(!string.IsNullOrEmpty(mMetadataObject.Description));
				Description.gameObject.SetActive(!string.IsNullOrEmpty(mMetadataObject.Description));
				Description.text = mMetadataObject.Description;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

			CollectionImage.transform.parent.gameObject.SetActive(details != null && !string.IsNullOrEmpty(details.LogoImageUrl));
			if (details != null)
			{
				if (!string.IsNullOrEmpty(details.LogoImageUrl))
				{
					bool success = await ImageUtils.DownloadAndApplyImageAndCropAfter(details.LogoImageUrl, CollectionImage, 32, 32);
					if (!success)
					{
						CollectionImage.transform.parent.gameObject.SetActive(false);	
					}
				}	
			}

			if (mCollectionMinted.createdAt != null)
			{
				MintedDateText.text = string.Format(LocalizationKeys.MINTED_ON_DATE.Translate(), mCollectionMinted.createdAt.Value.ToShortDateString());
			}

			await ImageUtils.DownloadAndApplyImage(mMetadataObject.Image, MainImage);
			LayoutRebuilder.ForceRebuildLayoutImmediate(VerticalLayoutGroup.GetComponent<RectTransform>());
			LoadingBackgroundImage.gameObject.SetActive(false);
		}

		#endregion

		#region PrivateMethods


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
			Application.OpenURL(mMetadataObject.Image);
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
				await VoxelDataCreatorManager.Instance.DownloadVoxModel(mMetadataObject.FilesUrl, zipPath);
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
