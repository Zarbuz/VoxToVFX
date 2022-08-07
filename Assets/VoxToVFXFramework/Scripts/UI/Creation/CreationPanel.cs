using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.UI.ImportScene;
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
			CONFIRMATION_BLOCKCHAIN
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

		[Header("SelectFile")]
		[SerializeField] private Button SelectFileButton;

		[Header("Conversion")]
		[SerializeField] private TextMeshProUGUI ProgressStepText;
		[SerializeField] private TextMeshProUGUI ProgressText;
		[SerializeField] private Image ProgressBarImage;

		[Header("Details")]
		[SerializeField] private TMP_InputField NameInputField;
		[SerializeField] private TMP_InputField DescriptionInputField;
		[SerializeField] private TextMeshProUGUI DescriptionCounter;
		[SerializeField] private TextMeshProUGUI CollectionNameText;
		[SerializeField] private TextMeshProUGUI CollectionSymbolText;
		[SerializeField] private SelectImage ImageSelectImage;
		[SerializeField] private SwitchController CreateSplitToggle;
		[SerializeField] private Button MintButton;

		[Header("MintInProgress")]
		[SerializeField] private Image MintSpinner;
		[SerializeField] private Button RetryButton;
		[SerializeField] private Button OpenEtherscanButton;

		#endregion

		#region Fields

		private CollectionCreatedEvent mCollectionCreated;
		private string mVoxUrl;
		private string mTransactionId;
		private eCreationState mCreationState;

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
			CreationState = eCreationState.SELECT;
			VoxelDataCreatorManager.Instance.LoadProgressCallback += OnLoadProgressUpdate;
			VoxelDataCreatorManager.Instance.LoadFinishedCallback += OnLoadVoxFinished;
		}

		private void OnDisable()
		{
			SelectFileButton.onClick.RemoveListener(OnSelectFileClicked);
			DescriptionInputField.onValueChanged.RemoveListener(OnDescriptionValueChanged);
			MintButton.onClick.RemoveListener(OnMintClicked);
			OpenEtherscanButton.onClick.RemoveListener(OnOpenEtherscanClicked);

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
			CanvasPlayerPCManager.Instance.PauseLockedState = true;
			ProgressStepText.text = $"Step: {step}/{VoxelDataCreatorManager.MAX_STEPS_ON_IMPORT}";
			ProgressText.text = $"{progress.ToString("P", CultureInfo.InvariantCulture)}";
			ProgressBarImage.fillAmount = progress;
		}

		private async void OnLoadVoxFinished(string outputPath)
		{
			CreationState = eCreationState.UPLOAD;
			string fileUrl = await FileManager.Instance.UploadFile(outputPath);
			if (fileUrl == null)
			{
				MessagePopup.Show(LocalizationKeys.CREATION_UPLOAD_ERROR.Translate());
				CreationState = eCreationState.SELECT;
			}
			else
			{
				OnUploadVoxFinished(fileUrl);
			}
		}

		private void OnUploadVoxFinished(string fileUrl)
		{
			mVoxUrl = fileUrl;
			CreationState = eCreationState.DETAILS;
			ImageSelectImage.Initialize(string.Empty);
		}

		private void OnDescriptionValueChanged(string value)
		{
			DescriptionCounter.text = value.Length + " / 1000";
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

			MetadataObject metadata = BuildMetadata(NameInputField.text, DescriptionInputField.text, ImageSelectImage.ImageUrl, mVoxUrl);
			string dateTime = DateTime.Now.Ticks.ToString();

			string filteredName = Regex.Replace(NameInputField.text, @"\s", "");
			string metadataName = $"{filteredName}" + $"_{dateTime}" + ".json";

			// Store metadata to IPFS
			string json = JsonConvert.SerializeObject(metadata);
			string base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

			string ipfsMetadataPath = await FileManager.Instance.SaveToIpfs(metadataName, base64Data);

			mTransactionId = await NFTManager.Instance.MintNft(ipfsMetadataPath, mCollectionCreated.CollectionContract);
			
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

		private void OnOpenEtherscanClicked()
		{
			string url = ConfigManager.Instance.EtherScanBaseUrl + "tx/" + mTransactionId;
			Application.OpenURL(url);
		}

		private MetadataObject BuildMetadata(string name, string description, string imageUrl, string voxUrl)
		{
			MetadataObject metadata = new MetadataObject
			{
				Description = description,
				Name = name,
				ExternalUrl = voxUrl,
				Image = imageUrl
			};
			return metadata;
		}

		#endregion
	}
}
