using Newtonsoft.Json;
using SFB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MoralisUnity.Web3Api.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.UI.NFTUpdate;
using VoxToVFXFramework.Scripts.UI.Popups;
using VoxToVFXFramework.Scripts.Utils.MetadataBuilder;

namespace VoxToVFXFramework.Scripts.UI.Creation
{
	public class CreationPanel : MonoBehaviour
	{
		#region Enum

		private enum eCreationState
		{
			SELECT,
			CONVERSION,
			UPLOAD,
			DETAILS,
			CONGRATULATIONS,
		}

		#endregion

		#region ScriptParameters

		[Header("Panels")]
		[SerializeField] private GameObject SelectFilePanel;
		[SerializeField] private GameObject ConversionPanel;
		[SerializeField] private GameObject UploadPanel;
		[SerializeField] private GameObject AddDetailsPanel;
		[SerializeField] private GameObject CongratulationsPanel;

		[Header("SelectFile")]
		[SerializeField] private Button SelectFileButton;

		[Header("Conversion")]
		[SerializeField] private TextMeshProUGUI ProgressStepText;
		[SerializeField] private ProgressBar ProgressBar;

		[Header("Upload")]
		[SerializeField] private ProgressBar UploadProgressBar;

		[Header("Details")]
		[SerializeField] private TMP_InputField NameInputField;
		[SerializeField] private TMP_InputField DescriptionInputField;
		[SerializeField] private TextMeshProUGUI DescriptionCounter;
		[SerializeField] private TextMeshProUGUI CollectionNameText;
		[SerializeField] private TextMeshProUGUI CollectionSymbolText;
		[SerializeField] private SelectImage ImageSelectImage;
		[SerializeField] private SwitchController CreateSplitToggle;
		[SerializeField] private Button PreviewButton;
		[SerializeField] private Button MintButton;


		[Header("Congratulations")]
		[SerializeField] private Button ViewCollectionButton;
		[SerializeField] private Button OpenSetBuyPricePanelButton;

		#endregion

		#region Fields

		private CollectionCreatedEvent mCollectionCreated;
		private CollectionMintedEvent mCollectionMintedItem;
		private List<string> mIpfsFiles;
		private string mZipLocalPath;
		private eCreationState mCreationState;
		private string mIpfsMetadataPath;

		private eCreationState CreationState
		{
			get => mCreationState;
			set
			{
				mCreationState = value;
				SelectFilePanel.SetActive(mCreationState == eCreationState.SELECT);
				ConversionPanel.SetActive(mCreationState == eCreationState.CONVERSION);
				UploadPanel.SetActive(mCreationState == eCreationState.UPLOAD);
				AddDetailsPanel.SetActive(mCreationState == eCreationState.DETAILS);
				CongratulationsPanel.SetActive(mCreationState == eCreationState.CONGRATULATIONS);
			}
		}

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			SelectFileButton.onClick.AddListener(OnSelectFileClicked);
			DescriptionInputField.onValueChanged.AddListener(OnDescriptionValueChanged);
			MintButton.onClick.AddListener(OnMintClicked);
			ViewCollectionButton.onClick.AddListener(OnViewCollectionClicked);
			OpenSetBuyPricePanelButton.onClick.AddListener(OnOpenSetBuyPriceClicked);
			PreviewButton.onClick.AddListener(OnPreviewClicked);

			VoxelDataCreatorManager.Instance.LoadProgressCallback += OnLoadProgressUpdate;
			VoxelDataCreatorManager.Instance.LoadFinishedCallback += OnLoadVoxFinished;
		}

		private void OnDisable()
		{
			SelectFileButton.onClick.RemoveListener(OnSelectFileClicked);
			DescriptionInputField.onValueChanged.RemoveListener(OnDescriptionValueChanged);
			MintButton.onClick.RemoveListener(OnMintClicked);
			ViewCollectionButton.onClick.RemoveListener(OnViewCollectionClicked);
			OpenSetBuyPricePanelButton.onClick.RemoveListener(OnOpenSetBuyPriceClicked);
			PreviewButton.onClick.RemoveListener(OnPreviewClicked);

			if (VoxelDataCreatorManager.Instance != null)
			{
				VoxelDataCreatorManager.Instance.LoadProgressCallback -= OnLoadProgressUpdate;
				VoxelDataCreatorManager.Instance.LoadFinishedCallback -= OnLoadVoxFinished;
			}
		}

		#endregion

		#region PublicMethods

		public void Initialize(CollectionCreatedEvent collectionCreated)
		{
			CreationState = eCreationState.SELECT;
			mCollectionCreated = collectionCreated;
			CollectionNameText.text = collectionCreated.Name;
			CollectionSymbolText.text = collectionCreated.Symbol;
			MintButton.interactable = true;
			CanvasPlayerPCManager.Instance.GenericTogglePanel(CanvasPlayerPCState.Creation);
		}

		#endregion

		#region PrivateMethods

		private void OnSelectFileClicked()
		{
			string[] paths = StandaloneFileBrowser.OpenFilePanel("Select VOX", "", "vox", false);
			if (paths.Length > 0)
			{
				CreationState = eCreationState.CONVERSION;
				VoxelDataCreatorManager.Instance.CreateZipFile(paths[0]);
			}
		}

		private void OnLoadProgressUpdate(int step, float progress)
		{
			ProgressStepText.text = $"Step: {step}/{VoxelDataCreatorManager.MAX_STEPS_ON_IMPORT}";
			ProgressBar.SetProgress(progress);
		}

		private async void OnLoadVoxFinished(string outputZipPath, List<string> outputChunkPaths)
		{
			CreationState = eCreationState.UPLOAD;
			mZipLocalPath = outputZipPath;
			List<string> fileUrls = await FileManager.Instance.UploadMultipleFiles(outputChunkPaths, OnUploadProgress);
			if (fileUrls == null)
			{
				MessagePopup.Show(LocalizationKeys.CREATION_UPLOAD_ERROR.Translate());
				CreationState = eCreationState.SELECT;
			}
			else
			{
				VoxelDataCreatorManager.Instance.DestroyFiles(outputChunkPaths);
				OnUploadVoxFinished(fileUrls);
			}
		}

		private void OnUploadProgress(float progress)
		{
			UploadProgressBar.SetProgress(progress);
		}

		private void OnUploadVoxFinished(List<string> fileUrls)
		{
			mIpfsFiles = fileUrls;
			CreationState = eCreationState.DETAILS;
			ImageSelectImage.Initialize(string.Empty);
		}

		private void OnDescriptionValueChanged(string value)
		{
			DescriptionCounter.text = value.Length + " / 1000";
		}

		private void OnPreviewClicked()
		{
			CanvasPlayerPCManager.Instance.OpenLoadingPanel(LocalizationKeys.LOADING_SCENE_DESCRIPTION.Translate(), () =>
			{
				CanvasPlayerPCManager.Instance.GenericClosePanel();
				CanvasPlayerPCManager.Instance.OpenPreviewPanel(NameInputField.text, DescriptionInputField.text, () =>
				{
					RuntimeVoxManager.Instance.Release();
					CanvasPlayerPCManager.Instance.GenericTogglePanel(CanvasPlayerPCState.Creation);
					CreationState = eCreationState.DETAILS;
				});
			});
			VoxelDataCreatorManager.Instance.ReadZipFile(mZipLocalPath);
		}

		private async void OnMintClicked()
		{
			if (NameInputField.text.Length <= 3)
			{
				MessagePopup.Show(LocalizationKeys.CREATION_MINT_NAME_TOO_SHORT.Translate());
				return;
			}

			if (string.IsNullOrEmpty(ImageSelectImage.ImageUrl))
			{
				MessagePopup.Show(LocalizationKeys.CREATION_MINT_MISSING_IMAGE.Translate());
				return;
			}

			//TODO: Handle Split

			MintButton.interactable = false;
			MetadataObject metadata = MetadataBuilder.BuildMetadata(NameInputField.text, DescriptionInputField.text, ImageSelectImage.ImageUrl, mIpfsFiles);
			string dateTime = DateTime.Now.Ticks.ToString();

			string filteredName = Regex.Replace(NameInputField.text, @"\s", "");
			string metadataName = $"{filteredName}" + $"_{dateTime}" + ".json";

			// Store metadata to IPFS
			string json = JsonConvert.SerializeObject(metadata);
			string base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

			mIpfsMetadataPath = await FileManager.Instance.SaveToIpfs(metadataName, base64Data);
			mIpfsMetadataPath = mIpfsMetadataPath.Replace("https://ipfs.moralis.io:2053/ipfs/", string.Empty);
			Mint();
		}

		private void Mint()
		{
			MessagePopup.ShowConfirmationWalletPopup(NFTManager.Instance.MintNftAndApprove(mIpfsMetadataPath, mCollectionCreated.CollectionContract).Preserve(),
				(transactionId) =>
				{
					MessagePopup.ShowConfirmationBlockchainPopup(
						LocalizationKeys.MINT_IN_PROGRESS_TITLE.Translate(),
						LocalizationKeys.MINT_IN_PROGRESS_DESCRIPTION.Translate(),
						transactionId,
						OnCollectionMinted);
				});
		}

		private void OnCollectionMinted(AbstractContractEvent collectionMinted)
		{
			Debug.Log("[CollectionPanel OnCollectionMinted received!");
			mCollectionMintedItem = collectionMinted as CollectionMintedEvent;
			CreationState = eCreationState.CONGRATULATIONS;
		}

		private void OnViewCollectionClicked()
		{
			CanvasPlayerPCManager.Instance.OpenCollectionDetailsPanel(mCollectionCreated);
		}

		private void OnOpenSetBuyPriceClicked()
		{
			CanvasPlayerPCManager.Instance.OpenUpdateNftPanel(eNFTUpdateTargetType.SET_BUY_PRICE, new Nft()
			{
				TokenAddress = mCollectionMintedItem.Address,
				TokenId = mCollectionMintedItem.TokenID,
			});
		}

		#endregion
	}
}
