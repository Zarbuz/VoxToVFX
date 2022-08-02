using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Utils.Extensions;

namespace VoxToVFXFramework.Scripts.UI.Collection
{
	public class CollectionPanel : MonoBehaviour
	{
		#region Enum

		private enum eCollectionPanelState
		{
			LIST_COLLECTION,
			HELP_INFO,
			CREATE_COLLECTION
		}

		#endregion

		#region ScriptParameters

		[Header("Panels")]
		[SerializeField] private GameObject ListCollectionPanel;
		[SerializeField] private GameObject HelpInfoPanel;
		[SerializeField] private GameObject CreationCollectionPanel;

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
		[SerializeField] private TextMeshProUGUI CollectionNameText;
		[SerializeField] private TextMeshProUGUI CollectionSymbolText;
		[SerializeField] private TMP_InputField CollectionNameInputField;
		[SerializeField] private TMP_InputField CollectionSymbolInputField;
		[SerializeField] private Button ContinueButton;
		[SerializeField] private Button HelpSmartContractButton;

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
				ListCollectionPanel.gameObject.SetActive(mCollectionPanelState == eCollectionPanelState.LIST_COLLECTION);
				HelpInfoPanel.gameObject.SetActive(mCollectionPanelState == eCollectionPanelState.HELP_INFO);
				CreationCollectionPanel.gameObject.SetActive(mCollectionPanelState == eCollectionPanelState.CREATE_COLLECTION);
			}
		}

		private readonly List<CollectionPanelItem> mCollectionPanelItems = new List<CollectionPanelItem>();

		#endregion

		#region UnityMethods

		private async void OnEnable()
		{
			CollectionPanelState = eCollectionPanelState.LIST_COLLECTION;
			HelpInfoButton.onClick.AddListener(OnHelpInfoClicked);
			BackButton.onClick.AddListener(OnBackClicked);
			CreateCollectionButton.onClick.AddListener(OnCreateCollectionClicked);
			CollectionNameInputField.onValueChanged.AddListener(OnCollectionNameValueChanged);
			CollectionSymbolInputField.onValueChanged.AddListener(OnCollectionSymbolValueChanged);
			HelpSmartContractButton.onClick.AddListener(OnHelpSmartContractClicked);
			ContinueButton.interactable = false;

			ShowSpinnerImage(true);
			List<UserContract> userContracts = await UserContractManager.Instance.GetUserLoggedListContract();
			RefreshCollectionList();
			
			ShowSpinnerImage(false);
		}

		private void OnDisable()
		{
			HelpInfoButton.onClick.RemoveListener(OnHelpInfoClicked);
			BackButton.onClick.RemoveListener(OnBackClicked);
			CreateCollectionButton.onClick.RemoveListener(OnCreateCollectionClicked);
			HelpSmartContractButton.onClick.RemoveListener(OnHelpSmartContractClicked);

			CollectionNameInputField.onValueChanged.RemoveListener(OnCollectionNameValueChanged);
			CollectionSymbolInputField.onValueChanged.RemoveListener(OnCollectionSymbolValueChanged);
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

		private void RefreshCollectionList()
		{
			mCollectionPanelItems.Clear();
			for (int i = 1; i < ListCollectionParent.childCount; i++)
			{
				Destroy(ListCollectionParent.GetChild(i).gameObject);
			}
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
			CollectionPanelState = eCollectionPanelState.CREATE_COLLECTION;
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
		#endregion
	}
}
