using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using Newtonsoft.Json;
using SFB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.UI.Popups;

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
			CONFIRMATION_WALLET,
			CONFIRMATION_BLOCKCHAIN,
			CONGRATULATIONS
		}

		#endregion

		#region ScriptParameters

		[Header("Panels")]
		[SerializeField] private GameObject SelectFilePanel;
		[SerializeField] private GameObject ConversionPanel;
		[SerializeField] private GameObject UploadPanel;
		[SerializeField] private GameObject AddDetailsPanel;
		[SerializeField] private GameObject WaitingConfirmationWalletPanel;
		[SerializeField] private GameObject MintInProgressPanel;
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

		[Header("MintInProgress")]
		[SerializeField] private Image MintSpinner;
		[SerializeField] private Button RetryButton;
		[SerializeField] private Button OpenEtherscanButton;

		[Header("Congratulations")]
		[SerializeField] private Button ViewCollectionButton;
		[SerializeField] private Button SetBuyPriceButton;

		#endregion

		#region Fields

		private CollectionCreatedEvent mCollectionCreated;
		private List<string> mIpfsFiles;
		private string mZipLocalPath;
		private string mTransactionId;
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
				WaitingConfirmationWalletPanel.SetActive(mCreationState == eCreationState.CONFIRMATION_WALLET);
				MintInProgressPanel.SetActive(mCreationState == eCreationState.CONFIRMATION_BLOCKCHAIN);
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
			OpenEtherscanButton.onClick.AddListener(OnOpenEtherscanClicked);
			RetryButton.onClick.AddListener(OnRetryClicked);
			ViewCollectionButton.onClick.AddListener(OnViewCollectionClicked);
			SetBuyPriceButton.onClick.AddListener(OnSetBuyPriceClicked);
			PreviewButton.onClick.AddListener(OnPreviewClicked);
			CreationState = eCreationState.SELECT;
			VoxelDataCreatorManager.Instance.LoadProgressCallback += OnLoadProgressUpdate;
			VoxelDataCreatorManager.Instance.LoadFinishedCallback += OnLoadVoxFinished;
			CollectionFactoryManager.Instance.CollectionMintedEvent += OnCollectionMinted;
		}

		private void OnDisable()
		{
			SelectFileButton.onClick.RemoveListener(OnSelectFileClicked);
			DescriptionInputField.onValueChanged.RemoveListener(OnDescriptionValueChanged);
			MintButton.onClick.RemoveListener(OnMintClicked);
			OpenEtherscanButton.onClick.RemoveListener(OnOpenEtherscanClicked);
			RetryButton.onClick.RemoveListener(OnRetryClicked);
			ViewCollectionButton.onClick.RemoveListener(OnViewCollectionClicked);
			SetBuyPriceButton.onClick.RemoveListener(OnSetBuyPriceClicked);
			PreviewButton.onClick.RemoveListener(OnPreviewClicked);

			if (VoxelDataCreatorManager.Instance != null)
			{
				VoxelDataCreatorManager.Instance.LoadProgressCallback -= OnLoadProgressUpdate;
				VoxelDataCreatorManager.Instance.LoadFinishedCallback -= OnLoadVoxFinished;
			}

			if (CollectionFactoryManager.Instance != null)
			{
				CollectionFactoryManager.Instance.CollectionMintedEvent -= OnCollectionMinted;
			}
		}

		#endregion

		#region PublicMethods

		public void Initialize(CollectionCreatedEvent collectionCreated)
		{
			mCollectionCreated = collectionCreated;
			CollectionNameText.text = collectionCreated.Name;
			CollectionSymbolText.text = collectionCreated.Symbol;
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
			CanvasPlayerPCManager.Instance.OpenLoadingPanel(() =>
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

			CreationState = eCreationState.CONFIRMATION_WALLET;
			OpenEtherscanButton.gameObject.SetActive(false);

			MetadataObject metadata = BuildMetadata(NameInputField.text, DescriptionInputField.text, ImageSelectImage.ImageUrl, mIpfsFiles);
			string dateTime = DateTime.Now.Ticks.ToString();

			string filteredName = Regex.Replace(NameInputField.text, @"\s", "");
			string metadataName = $"{filteredName}" + $"_{dateTime}" + ".json";

			// Store metadata to IPFS
			string json = JsonConvert.SerializeObject(metadata);
			string base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

			mIpfsMetadataPath = await FileManager.Instance.SaveToIpfs(metadataName, base64Data);
			Mint();
		}

		private async void Mint()
		{
			mTransactionId = await NFTManager.Instance.MintNft(mIpfsMetadataPath, mCollectionCreated.CollectionContract);

			if (string.IsNullOrEmpty(mTransactionId))
			{
				//error
				MessagePopup.Show(LocalizationKeys.COLLECTION_EXECUTE_CONTRACT_ERROR.Translate());
				MintSpinner.gameObject.SetActive(false);
				RetryButton.gameObject.SetActive(true);
			}
			else
			{
				CreationState = eCreationState.CONFIRMATION_BLOCKCHAIN;
				OpenEtherscanButton.gameObject.SetActive(true);
			}
		}
	
		private void OnCollectionMinted(CollectionMintedEvent collectionMinted)
		{
			Debug.Log("[CollectionPanel OnCollectionMinted received!");
			CreationState = eCreationState.CONGRATULATIONS;
		}

		private void OnRetryClicked()
		{
			Mint();
		}

		private void OnViewCollectionClicked()
		{
			//TODO Page details NFT

		}

		private void OnSetBuyPriceClicked()
		{
			//TODO
		}

		private void OnOpenEtherscanClicked()
		{
			string url = ConfigManager.Instance.EtherScanBaseUrl + "tx/" + mTransactionId;
			Application.OpenURL(url);
		}

		private MetadataObject BuildMetadata(string name, string description, string imageUrl, List<string> files)
		{
			MetadataObject metadata = new MetadataObject
			{
				Description = description,
				Name = name,
				FilesUrl = files,
				Image = imageUrl
			};
			return metadata;
		}

		#endregion
	}
}
