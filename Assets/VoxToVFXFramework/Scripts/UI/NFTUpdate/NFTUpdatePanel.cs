using System;
using System.Globalization;
using System.Numerics;
using System.Threading;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using Nethereum.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Popups;
using VoxToVFXFramework.Scripts.UI.Profile;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public enum eUpdateTargetType
	{
		SET_BUY_PRICE,
		CHANGE_BUY_PRICE,
		REMOVE_BUY_PRICE,
		CHANGE_RESERVE,
		LIST_FOR_AUCTION
	}

	public class NFTUpdatePanel : MonoBehaviour
	{
		#region ScriptParameters

		[Header("Panels")]
		[SerializeField] private GameObject MainPanel;
		[SerializeField] private GameObject CongratulationsPanel;
		[SerializeField] private GameObject RemoveBuyPricePanel;
		[SerializeField] private ProfileListNFTItem ProfileListNftItem;

		[Header("Main")]
		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private TextMeshProUGUI Description;
		[SerializeField] private TMP_InputField PriceInputField;
		[SerializeField] private Button SetButton;
		[SerializeField] private TextMeshProUGUI SetButtonText;
		[SerializeField] private Toggle MarketplaceToggle;
		[SerializeField] private GameObject MarketplacePanel;
		[SerializeField] private TextMeshProUGUI MarketplaceFeeCountText;
		[SerializeField] private TextMeshProUGUI ReceiveCountText;
		[SerializeField] private Image ArrowIcon;

		[Header("CongratulationsPanel")]
		[SerializeField] private TextMeshProUGUI CongratulationsTitle;
		[SerializeField] private TextMeshProUGUI CongratulationsDescription;
		[SerializeField] private Button ViewNFTButton;
		[SerializeField] private Button ViewCollectionButton;

		[Header("RemoveBuyPrice")]
		[SerializeField] private Button RemoveBuyPriceButton;

		#endregion

		#region Enum

		private enum eNFTUpdatePanelState
		{
			MAIN,
			CONGRATULATIONS,
			REMOVE_BUY_PRICE
		}

		#endregion

		#region Fields

		private eUpdateTargetType mTargetType;
		private CollectionMintedEvent mCollectionItem;

		private eNFTUpdatePanelState mPanelState;
		private eNFTUpdatePanelState NftUpdatePanelState
		{
			get => mPanelState;
			set
			{
				mPanelState = value;
				MainPanel.SetActive(mPanelState == eNFTUpdatePanelState.MAIN);
				CongratulationsPanel.SetActive(mPanelState == eNFTUpdatePanelState.CONGRATULATIONS);
				RemoveBuyPricePanel.SetActive(mPanelState == eNFTUpdatePanelState.REMOVE_BUY_PRICE);
				ProfileListNftItem.gameObject.SetActive(mPanelState != eNFTUpdatePanelState.CONGRATULATIONS);
			}
		}

		#endregion

		#region ConstStatic

		public const int MARKETPLACE_FEES = 5;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			PriceInputField.onValueChanged.AddListener(OnPriceValueChanged);
			SetButton.onClick.AddListener(OnSetClicked);
			MarketplaceToggle.onValueChanged.AddListener(OnMarketplaceValueChanged);
			ViewNFTButton.onClick.AddListener(OnViewNFTClicked);
			ViewCollectionButton.onClick.AddListener(OnViewCollectionClicked);
			RemoveBuyPriceButton.onClick.AddListener(OnRemoveBuyPriceClicked);
		}

		private void OnDisable()
		{
			PriceInputField.onValueChanged.RemoveListener(OnPriceValueChanged);
			SetButton.onClick.RemoveListener(OnSetClicked);
			MarketplaceToggle.onValueChanged.RemoveListener(OnMarketplaceValueChanged);
			ViewNFTButton.onClick.RemoveListener(OnViewNFTClicked);
			ViewCollectionButton.onClick.RemoveListener(OnViewCollectionClicked);
			RemoveBuyPriceButton.onClick.RemoveListener(OnRemoveBuyPriceClicked);
		}

		#endregion

		#region PublicMethods

		public async void Initialize(eUpdateTargetType updateTargetType, CollectionMintedEvent collectionMintedItem)
		{
			NftUpdatePanelState = eNFTUpdatePanelState.MAIN;
			mTargetType = updateTargetType;
			mCollectionItem = collectionMintedItem;
			switch (updateTargetType)
			{
				case eUpdateTargetType.SET_BUY_PRICE:
					Title.text = LocalizationKeys.SET_BUY_PRICE_TITLE.Translate();
					Description.text = LocalizationKeys.SET_BUY_PRICE_DESCRIPTION.Translate();
					SetButtonText.text = LocalizationKeys.SET_BUY_AMOUNT_REQUIRED.Translate();
					break;
				case eUpdateTargetType.CHANGE_RESERVE:
					break;
				case eUpdateTargetType.LIST_FOR_AUCTION:
					break;
				case eUpdateTargetType.CHANGE_BUY_PRICE:
					Title.text = LocalizationKeys.CHANGE_BUY_NOW_PRICE_TITLE.Translate();
					Description.text = LocalizationKeys.SET_BUY_PRICE_DESCRIPTION.Translate();
					SetButtonText.text = LocalizationKeys.SET_BUY_PRICE.Translate();
					NFTDetailsContractType details = await DataManager.Instance.GetNFTDetailsWithCache(collectionMintedItem.Address, collectionMintedItem.TokenID);
					PriceInputField.text = details.BuyPriceInEtherFixedPoint;
					break;
				case eUpdateTargetType.REMOVE_BUY_PRICE:
					NftUpdatePanelState = eNFTUpdatePanelState.REMOVE_BUY_PRICE;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(updateTargetType), updateTargetType, null);
			}

			ProfileListNftItem.IsReadyOnly = true;
			await ProfileListNftItem.Initialize(collectionMintedItem);

		}

		#endregion

		#region PrivateMethods

		private void OnPriceValueChanged(string text)
		{
			bool success = float.TryParse(text, NumberStyles.Any, Thread.CurrentThread.CurrentCulture, out float value);
			Debug.Log(value);
			if (!success)
			{
				SetButtonText.text = LocalizationKeys.SET_BUY_AMOUNT_REQUIRED.Translate();
				SetButton.interactable = false;
				ReceiveCountText.text = "0.00 " + Moralis.CurrentChain.Symbol;
				MarketplaceFeeCountText.text = "0.00 " + Moralis.CurrentChain.Symbol;
			}
			else
			{
				if (value < 0.01)
				{
					SetButtonText.text = LocalizationKeys.SET_BUY_AT_LEAST_0_01ETH.Translate();
					SetButton.interactable = false;
					MarketplaceFeeCountText.text = "0.00 " + Moralis.CurrentChain.Symbol;
					ReceiveCountText.text = "0.00 " + Moralis.CurrentChain.Symbol;
				}
				else
				{
					SetButtonText.text = LocalizationKeys.SET_BUY_PRICE.Translate();
					SetButton.interactable = true;
					float marketplaceFees = value * (MARKETPLACE_FEES / (float)100);
					float willReceiveCount = value - marketplaceFees;
					MarketplaceFeeCountText.text = marketplaceFees + " " + Moralis.CurrentChain.Symbol;
					ReceiveCountText.text = willReceiveCount + " " + Moralis.CurrentChain.Symbol; ;
				}
			}
		}

		private void OnSetClicked()
		{
			switch (mTargetType)
			{
				case eUpdateTargetType.SET_BUY_PRICE:
				case eUpdateTargetType.CHANGE_BUY_PRICE:
					OnSetBuyPrice();
					break;
				case eUpdateTargetType.CHANGE_RESERVE:
					break;
				case eUpdateTargetType.LIST_FOR_AUCTION:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void OnSetBuyPrice()
		{
			float price = float.Parse(PriceInputField.text);
			BigInteger priceInWei = UnitConversion.Convert.ToWei(price, 18);
			MessagePopup.ShowConfirmationWalletPopup(NFTMarketManager.Instance.SetBuyPrice(mCollectionItem.Address, mCollectionItem.TokenID, priceInWei),
				(transactionId) =>
				{
					MessagePopup.ShowConfirmationBlockchainPopup(
						LocalizationKeys.SET_BUY_PRICE_WAITING_TITLE.Translate(),
						LocalizationKeys.SET_BUY_PRICE_WAITING_DESCRIPTION.Translate(),
						transactionId,
						OnBuyPriceSet);
				});
		}

		private void OnBuyPriceSet(AbstractContractEvent obj)
		{
			NftUpdatePanelState = eNFTUpdatePanelState.CONGRATULATIONS;
			BuyPriceSetEvent buyPriceSetEvent = obj as BuyPriceSetEvent;
			BigInteger priceInWei = BigInteger.Parse(buyPriceSetEvent.Price);
			decimal price = UnitConversion.Convert.FromWei(priceInWei);
			CongratulationsTitle.text = LocalizationKeys.SET_BUY_PRICE_SUCCESS_TITLE.Translate();
			CongratulationsDescription.text = string.Format(LocalizationKeys.SET_BUY_PRICE_SUCCESS_DESCRIPTION.Translate(), price.ToString("F2"));
		}

		private void OnMarketplaceValueChanged(bool active)
		{
			ArrowIcon.transform.eulerAngles = active ? new Vector3(0, 0, 90) : new Vector3(0, 0, 270);
			MarketplacePanel.SetActive(active);
		}

		private async void OnViewNFTClicked()
		{
			Nft metadata = await DataManager.Instance.GetTokenIdMetadataWithCache(mCollectionItem.Address, mCollectionItem.TokenID);
			if (metadata != null)
			{
				CanvasPlayerPCManager.Instance.OpenNftDetailsPanel(mCollectionItem, metadata);
			}
		}

		private async void OnViewCollectionClicked()
		{
			CollectionCreatedEvent collection  = await DataManager.Instance.GetCollectionWithCache(mCollectionItem.Address);
			CanvasPlayerPCManager.Instance.OpenCollectionDetailsPanel(collection);
		}

		private void OnRemoveBuyPriceClicked()
		{
			MessagePopup.ShowConfirmationWalletPopup(NFTMarketManager.Instance.CancelBuyPrice(mCollectionItem.Address, mCollectionItem.TokenID),
				(transactionId) =>
				{
					MessagePopup.ShowConfirmationBlockchainPopup(
						LocalizationKeys.REMOVE_BUY_NOW_WAITING_PRICE_TITLE.Translate(),
						LocalizationKeys.REMOVE_BUY_NOW_WAITING_PRICE_DESCRIPTION.Translate(),
						transactionId,
						OnBuyPriceRemoved);
				});
		}

		private void OnBuyPriceRemoved(AbstractContractEvent obj)
		{
			NftUpdatePanelState = eNFTUpdatePanelState.CONGRATULATIONS;
			CongratulationsTitle.text = LocalizationKeys.REMOVE_BUY_NOW_REMOVED_TITLE.Translate();
			CongratulationsDescription.text = LocalizationKeys.REMOVE_BUY_NOW_REMOVED_DESCRIPTION.Translate();
		}
		#endregion
	}
}
