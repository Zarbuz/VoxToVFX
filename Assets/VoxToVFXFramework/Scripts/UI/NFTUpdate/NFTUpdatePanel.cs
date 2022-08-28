using System;
using System.Globalization;
using System.Numerics;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using Nethereum.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Popups;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public enum eUpdateTargetType
	{
		SET_BUY_PRICE,
		CHANGE_RESERVE,
		LIST_FOR_AUCTION
	}

	public class NFTUpdatePanel : MonoBehaviour
	{
		#region ScriptParameters

		[Header("Panels")]
		[SerializeField] private GameObject MainPanel;
		[SerializeField] private GameObject SetBuyPriceCongratulationsPanel;

		[Header("Main")]
		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private TextMeshProUGUI Description;
		[SerializeField] private RectTransform BuyPricePanelRectTransform;
		[SerializeField] private TMP_InputField PriceInputField;
		[SerializeField] private Button SetButton;
		[SerializeField] private TextMeshProUGUI SetButtonText;
		[SerializeField] private Toggle MarketplaceToggle;
		[SerializeField] private GameObject MarketplacePanel;
		[SerializeField] private TextMeshProUGUI MarketplaceFeeCountText;
		[SerializeField] private TextMeshProUGUI ReceiveCountText;
		[SerializeField] private Image ArrowIcon;

		[Header("Set Buy Price Congratulations")]
		[SerializeField] private TextMeshProUGUI BuyPriceDescription;
		[SerializeField] private Button ViewNFTButton;
		[SerializeField] private Button ViewCollectionButton;

		#endregion

		#region Enum

		private enum eNFTUpdatePanelState
		{
			MAIN,
			SET_BUY_PRICE_CONGRATULATIONS
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
				SetBuyPriceCongratulationsPanel.SetActive(mPanelState == eNFTUpdatePanelState.SET_BUY_PRICE_CONGRATULATIONS);
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
		}

		private void OnDisable()
		{
			PriceInputField.onValueChanged.RemoveListener(OnPriceValueChanged);
			SetButton.onClick.RemoveListener(OnSetClicked);
			MarketplaceToggle.onValueChanged.RemoveListener(OnMarketplaceValueChanged);
			ViewNFTButton.onClick.RemoveListener(OnViewNFTClicked);
			ViewCollectionButton.onClick.RemoveListener(OnViewCollectionClicked);
		}

		#endregion

		#region PublicMethods

		public void Initialize(eUpdateTargetType updateTargetType, CollectionMintedEvent collectionMintedItem)
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
				default:
					throw new ArgumentOutOfRangeException(nameof(updateTargetType), updateTargetType, null);
			}
		}

		#endregion

		#region PrivateMethods

		private void OnPriceValueChanged(string text)
		{
			bool success = float.TryParse(text, NumberStyles.Any, LocalizationManager.Instance.CurrentCultureInfo, out float value);
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
			NftUpdatePanelState = eNFTUpdatePanelState.SET_BUY_PRICE_CONGRATULATIONS;
			BuyPriceSetEvent buyPriceSetEvent = obj as BuyPriceSetEvent;
			var priceInWei = BigInteger.Parse(buyPriceSetEvent.Price);
			var price = UnitConversion.Convert.FromWei(priceInWei);
			BuyPriceDescription.text = string.Format(LocalizationKeys.SET_BUY_PRICE_SUCCESS_DESCRIPTION.Translate(), price);
		}

		private void OnMarketplaceValueChanged(bool active)
		{
			Vector2 size = BuyPricePanelRectTransform.sizeDelta;
			ArrowIcon.transform.eulerAngles = active ? new Vector3(0, 0, 90) : new Vector3(0, 0, 270);
			BuyPricePanelRectTransform.sizeDelta = new Vector2(size.x, active ? 443 : 390);
			MarketplacePanel.SetActive(active);
		}

		private async void OnViewNFTClicked()
		{
			Nft metadata = await DataManager.Instance.GetTokenIdMetadataWithCache(mCollectionItem.Address, mCollectionItem.TokenID);
			CanvasPlayerPCManager.Instance.OpenNftDetailsPanel(mCollectionItem, metadata);
		}

		private async void OnViewCollectionClicked()
		{
			CollectionCreatedEvent collection  = await DataManager.Instance.GetCollectionWithCache(mCollectionItem.Address);
			CanvasPlayerPCManager.Instance.OpenCollectionDetailsPanel(collection);
		}

		#endregion
	}
}
