using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.UI.Popups;
using VoxToVFXFramework.Scripts.Utils.Extensions;

namespace VoxToVFXFramework.Scripts.UI.Collection
{
	public class CollectionPanel : MonoBehaviour
	{
		#region Enum

		private enum eCollectionPanelState
		{
			LIST,
			HELP_INFO,
			CREATE,
			CONGRATULATIONS
		}

		private enum eCollectionLeftPartState
		{
			CREATION,
			CONFIRMATION_WALLET,
			CONFIRMATION_BLOCKCHAIN
		}

		#endregion

		#region ScriptParameters

		[Header("Panels")]
		[SerializeField] private GameObject ListCollectionPanel;
		[SerializeField] private GameObject HelpInfoPanel;
		[SerializeField] private GameObject CreationCollectionPanel;
		[SerializeField] private GameObject CongratulationsPanel;

		[Header("ListCollection")]
		[SerializeField] private Transform ListCollectionParent;
		[SerializeField] private Button CreateCollectionButton;
		[SerializeField] private ScrollRect ListScrollRect;
		[SerializeField] private CollectionPanelItem CollectionPanelItemPrefab;
		[SerializeField] private Image Spinner;
		[SerializeField] private Button HelpInfoButton;

		[Header("HelpInfo")]
		[SerializeField] private TextMeshProUGUI HelpText;
		[SerializeField] private Button BackButton;


		[Header("CreateCollection")]

		[SerializeField] private GameObject LeftCreationPanel;
		[SerializeField] private GameObject LeftConfirmationPanel;
		[SerializeField] private GameObject LeftWaitingBlockchainPanel;

		[Space(10)]
		[SerializeField] private TextMeshProUGUI CollectionNameText;
		[SerializeField] private TextMeshProUGUI CollectionSymbolText;
		[SerializeField] private TMP_InputField CollectionNameInputField;
		[SerializeField] private TMP_InputField CollectionSymbolInputField;
		[SerializeField] private TextMeshProUGUI SubTitleCreateCollection;
		[SerializeField] private Image CreateSpinner;
		[SerializeField] private Button RetryButton;

		[SerializeField] private Button ContinueButton;
		[SerializeField] private Button HelpSmartContractButton;
		[SerializeField] private Button OpenEtherscanButton;

		[Header("Congratulations")]
		[SerializeField] private Button BackCongratulationsButton;

		#endregion

		#region Fields

		private eCollectionPanelState mPreviouState;
		private eCollectionPanelState mCollectionPanelState;

		private eCollectionPanelState CollectionPanelState
		{
			get => mCollectionPanelState;
			set
			{
				mPreviouState = mCollectionPanelState;
				mCollectionPanelState = value;
				ListCollectionPanel.gameObject.SetActive(mCollectionPanelState == eCollectionPanelState.LIST);
				HelpInfoPanel.gameObject.SetActive(mCollectionPanelState == eCollectionPanelState.HELP_INFO);
				CreationCollectionPanel.gameObject.SetActive(mCollectionPanelState == eCollectionPanelState.CREATE);
				CongratulationsPanel.SetActive(mCollectionPanelState == eCollectionPanelState.CONGRATULATIONS);
			}
		}

		private eCollectionLeftPartState mCollectionLeftPartState;

		private eCollectionLeftPartState CollectionLeftPartState
		{
			get => mCollectionLeftPartState;
			set
			{
				mCollectionLeftPartState = value;
				LeftCreationPanel.SetActive(mCollectionLeftPartState == eCollectionLeftPartState.CREATION);
				LeftConfirmationPanel.SetActive(mCollectionLeftPartState == eCollectionLeftPartState.CONFIRMATION_WALLET);
				SubTitleCreateCollection.gameObject.SetActive(mCollectionLeftPartState == eCollectionLeftPartState.CREATION);
				LeftWaitingBlockchainPanel.SetActive(mCollectionLeftPartState == eCollectionLeftPartState.CONFIRMATION_BLOCKCHAIN);
			}
		}

		private string mTransactionId;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			CollectionPanelState = eCollectionPanelState.LIST;
			HelpInfoButton.onClick.AddListener(OnHelpInfoClicked);
			BackButton.onClick.AddListener(OnBackClicked);
			CreateCollectionButton.onClick.AddListener(OnCreateCollectionClicked);
			CollectionNameInputField.onValueChanged.AddListener(OnCollectionNameValueChanged);
			CollectionSymbolInputField.onValueChanged.AddListener(OnCollectionSymbolValueChanged);
			HelpSmartContractButton.onClick.AddListener(OnHelpSmartContractClicked);
			ContinueButton.onClick.AddListener(OnContinueClicked);
			OpenEtherscanButton.onClick.AddListener(OnOpenEtherscanClicked);
			BackCongratulationsButton.onClick.AddListener(OnBackCongratulationsClicked);
			RetryButton.onClick.AddListener(OnRetryButtonClicked);
			ContinueButton.interactable = false;

			CollectionFactoryManager.Instance.CollectionCreatedEvent += OnCollectionCreated;
			RefreshCollectionList();
		}

		private void OnDisable()
		{
			HelpInfoButton.onClick.RemoveListener(OnHelpInfoClicked);
			BackButton.onClick.RemoveListener(OnBackClicked);
			CreateCollectionButton.onClick.RemoveListener(OnCreateCollectionClicked);
			HelpSmartContractButton.onClick.RemoveListener(OnHelpSmartContractClicked);
			ContinueButton.onClick.RemoveListener(OnContinueClicked);
			OpenEtherscanButton.onClick.RemoveListener(OnOpenEtherscanClicked);
			BackCongratulationsButton.onClick.RemoveListener(OnBackCongratulationsClicked);
			RetryButton.onClick.RemoveListener(OnRetryButtonClicked);

			CollectionNameInputField.onValueChanged.RemoveListener(OnCollectionNameValueChanged);
			CollectionSymbolInputField.onValueChanged.RemoveListener(OnCollectionSymbolValueChanged);

			if (CollectionFactoryManager.Instance != null)
			{
				CollectionFactoryManager.Instance.CollectionCreatedEvent -= OnCollectionCreated;
			}
		}

		#endregion

		#region PublicMethods

		public void ClosePanel()
		{
			CanvasPlayerPCManager.Instance.GenericClosePanel();
		}

		#endregion

		#region PrivateMethods

	
		private void ShowSpinnerImage(bool showSpinner)
		{
			Spinner.gameObject.SetActive(showSpinner);
			ListScrollRect.gameObject.SetActive(!showSpinner);
		}

		private async void RefreshCollectionList()
		{
			ShowSpinnerImage(true);
			List<CollectionCreatedEvent> userContracts = await CollectionFactoryManager.Instance.GetUserListContract();
			for (int i = 1; i < ListCollectionParent.childCount; i++)
			{
				Destroy(ListCollectionParent.GetChild(i).gameObject);
			}

			foreach (CollectionCreatedEvent collection in userContracts.OrderByDescending(c => c.createdAt))
			{
				CollectionPanelItem item = Instantiate(CollectionPanelItemPrefab, ListCollectionParent, false);
				NftOwnerCollection nftOwnerCollection = await NFTManager.Instance.FetchNFTsForContract(collection.Creator, collection.CollectionContract);
				item.Initialize(collection, nftOwnerCollection, OnCollectionSelected);
			}

			ShowSpinnerImage(false);
		}

		private void OnCollectionSelected(CollectionCreatedEvent collectionCreated)
		{
			CanvasPlayerPCManager.Instance.OpenCreationPanel(collectionCreated);
		}

		private void OnHelpInfoClicked()
		{
			SetupHelpPanel(LocalizationKeys.COLLECTION_HELP_INFO_TEXT);
		}

		private void SetupHelpPanel(string helpKey)
		{
			CollectionPanelState = eCollectionPanelState.HELP_INFO;
			HelpText.text = helpKey.Translate();
		}

		private void OnBackClicked()
		{
			CollectionPanelState = mPreviouState;
		}

		private void OnCreateCollectionClicked()
		{
			CollectionPanelState = eCollectionPanelState.CREATE;
			CollectionLeftPartState = eCollectionLeftPartState.CREATION;
			CollectionNameInputField.text = string.Empty;
			CollectionSymbolInputField.text = string.Empty;
		}

		private void OnHelpSmartContractClicked()
		{
			SetupHelpPanel(LocalizationKeys.COLLECTION_HELP_SMART_CONTRACT);
		}

		private void OnCollectionNameValueChanged(string value)
		{
			CollectionNameText.text = value;
			CheckContinueButton();
		}

		private void OnCollectionSymbolValueChanged(string value)
		{
			if (value.Contains(" ") || value.HasSpecialChars())
			{
				CollectionSymbolInputField.text = CollectionSymbolText.text;
				return;
			}
			CollectionSymbolText.text = value;
			CheckContinueButton();
		}

		private void CheckContinueButton()
		{
			ContinueButton.interactable = CollectionNameInputField.text.Length > 0 && CollectionSymbolInputField.text.Length > 0;
		}

		private void OnContinueClicked()
		{
			CreateCollection();
		}

		private void OnOpenEtherscanClicked()
		{
			string url = ConfigManager.Instance.EtherScanBaseUrl + "tx/" + mTransactionId;
			Application.OpenURL(url);
		}

		private void OnRetryButtonClicked()
		{
			CreateCollection();
		}

		private async UniTask CreateCollection()
		{
			CreateSpinner.gameObject.SetActive(true);
			RetryButton.gameObject.SetActive(false);
			CollectionLeftPartState = eCollectionLeftPartState.CONFIRMATION_WALLET;
			mTransactionId = await CollectionFactoryManager.Instance.CreateCollection(CollectionNameInputField.text, CollectionSymbolInputField.text.ToUpperInvariant());
			if (!string.IsNullOrEmpty(mTransactionId))
			{
				CollectionLeftPartState = eCollectionLeftPartState.CONFIRMATION_BLOCKCHAIN;
			}
			else
			{
				MessagePopup.Show(LocalizationKeys.COLLECTION_EXECUTE_CONTRACT_ERROR.Translate());
				CreateSpinner.gameObject.SetActive(false);
				RetryButton.gameObject.SetActive(true);
			}
		}

		private void OnCollectionCreated(CollectionCreatedEvent collectionCreated)
		{
			Debug.Log("[CollectionPanel] OnCollectionCreated received!");
			CollectionPanelState = eCollectionPanelState.CONGRATULATIONS;
		}

		private void OnBackCongratulationsClicked()
		{
			CollectionPanelState = eCollectionPanelState.LIST;
			RefreshCollectionList();
		}

		#endregion
	}
}
