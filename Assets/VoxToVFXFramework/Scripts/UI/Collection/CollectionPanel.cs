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
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
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

		[Space(10)]
		[SerializeField] private TextMeshProUGUI CollectionNameText;
		[SerializeField] private TextMeshProUGUI CollectionSymbolText;
		[SerializeField] private TMP_InputField CollectionNameInputField;
		[SerializeField] private TMP_InputField CollectionSymbolInputField;
		[SerializeField] private TextMeshProUGUI SubTitleCreateCollection;

		[SerializeField] private Button ContinueButton;
		[SerializeField] private Button HelpSmartContractButton;

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
				SubTitleCreateCollection.gameObject.SetActive(mCollectionLeftPartState == eCollectionLeftPartState.CREATION);
			}
		}

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			HelpInfoButton.onClick.AddListener(OnHelpInfoClicked);
			BackButton.onClick.AddListener(OnBackClicked);
			CreateCollectionButton.onClick.AddListener(OnCreateCollectionClicked);
			CollectionNameInputField.onValueChanged.AddListener(OnCollectionNameValueChanged);
			CollectionSymbolInputField.onValueChanged.AddListener(OnCollectionSymbolValueChanged);
			HelpSmartContractButton.onClick.AddListener(OnHelpSmartContractClicked);
			ContinueButton.onClick.AddListener(OnContinueClicked);
			BackCongratulationsButton.onClick.AddListener(OnBackCongratulationsClicked);
		}

		private void OnDisable()
		{
			HelpInfoButton.onClick.RemoveListener(OnHelpInfoClicked);
			BackButton.onClick.RemoveListener(OnBackClicked);
			CreateCollectionButton.onClick.RemoveListener(OnCreateCollectionClicked);
			HelpSmartContractButton.onClick.RemoveListener(OnHelpSmartContractClicked);
			ContinueButton.onClick.RemoveListener(OnContinueClicked);
			BackCongratulationsButton.onClick.RemoveListener(OnBackCongratulationsClicked);
			CollectionNameInputField.onValueChanged.RemoveListener(OnCollectionNameValueChanged);
			CollectionSymbolInputField.onValueChanged.RemoveListener(OnCollectionSymbolValueChanged);
		}

		#endregion

		#region PublicMethods

		public void Initialize()
		{
			CollectionPanelState = eCollectionPanelState.LIST;
			ContinueButton.interactable = false;
			RefreshCollectionList();
		}

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
			ListCollectionParent.DestroyAllChildren(true);
			List<CollectionCreatedEvent> userContracts = await DataManager.Instance.GetUserListContractWithCache(UserManager.Instance.CurrentUserAddress);
			List<UniTask> tasks = new List<UniTask>(); 
			foreach (CollectionCreatedEvent collection in userContracts.OrderByDescending(c => c.createdAt))
			{
				CollectionPanelItem item = Instantiate(CollectionPanelItemPrefab, ListCollectionParent, false);
				List<NftWithDetails> nftCollection = await DataManager.Instance.GetNftCollectionWithCache(collection.CollectionContract);
				tasks.Add(item.Initialize(collection, nftCollection.Count, OnCollectionSelected));
			}

			await UniTask.WhenAll(tasks);
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

		private void CreateCollection()
		{
			MessagePopup.ShowConfirmationWalletPopup((CollectionFactoryManager.Instance.CreateCollection(CollectionNameInputField.text, CollectionSymbolInputField.text.ToUpperInvariant())).Preserve(),
				(transactionId) =>
				{
					//CollectionLeftPartState = eCollectionLeftPartState.CONFIRMATION_BLOCKCHAIN;
					MessagePopup.ShowConfirmationBlockchainPopup(
						LocalizationKeys.COLLECTION_WAITING_CONFIRMATION_TITLE.Translate(),
						LocalizationKeys.COLLECTION_SMART_CONTRACT_DEPLOYMENT_TITLE.Translate(),
						transactionId,
						OnCollectionCreated);
				});
		}

		private void OnCollectionCreated(AbstractContractEvent collectionCreated)
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
