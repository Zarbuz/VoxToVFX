using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using Nethereum.Util;
using System;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
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
using Vector3 = UnityEngine.Vector3;

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public enum eNFTUpdateTargetType
	{
		SET_BUY_PRICE,
		CHANGE_BUY_PRICE,
		REMOVE_BUY_PRICE,
		CHANGE_RESERVE,
		LIST_FOR_AUCTION,
		TRANSFER_NFT
	}

	public class NFTUpdatePanel : MonoBehaviour
	{
		#region ScriptParameters

		[Header("Panels")]
		[SerializeField] private GameObject SetBuyNowPanel;
		[SerializeField] private GameObject CongratulationsPanel;
		[SerializeField] private GameObject RemoveBuyPricePanel;
		[SerializeField] private GameObject TransferPanel;
		[SerializeField] private ProfileListNFTItem ProfileListNftItem;

		[Header("SetBuyPrice")]
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

		[Header("Congratulations")]
		[SerializeField] private TextMeshProUGUI CongratulationsTitle;
		[SerializeField] private TextMeshProUGUI CongratulationsDescription;
		[SerializeField] private Button ViewNFTButton;
		[SerializeField] private Button ViewCollectionButton;

		[Header("RemoveBuyPrice")]
		[SerializeField] private Button RemoveBuyPriceButton;

		[Header("Transfer")]
		[SerializeField] private TMP_InputField TransferAddressInputField;
		[SerializeField] private Button TransferNFTButton;
		[SerializeField] private TextMeshProUGUI TransferButtonText;
		#endregion

		#region Enum

		private enum eNFTUpdatePanelState
		{
			SET_BUY_PRICE,
			CONGRATULATIONS,
			REMOVE_BUY_PRICE,
			TRANSFER_NFT
		}

		#endregion

		#region Fields

		private Nft mNft;

		private eNFTUpdatePanelState mPanelState;
		private eNFTUpdatePanelState NftUpdatePanelState
		{
			get => mPanelState;
			set
			{
				mPanelState = value;
				SetBuyNowPanel.SetActive(mPanelState == eNFTUpdatePanelState.SET_BUY_PRICE);
				CongratulationsPanel.SetActive(mPanelState == eNFTUpdatePanelState.CONGRATULATIONS);
				RemoveBuyPricePanel.SetActive(mPanelState == eNFTUpdatePanelState.REMOVE_BUY_PRICE);
				TransferPanel.SetActive(mPanelState == eNFTUpdatePanelState.TRANSFER_NFT);
				ProfileListNftItem.transform.parent.gameObject.SetActive(mPanelState != eNFTUpdatePanelState.CONGRATULATIONS);
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
			TransferAddressInputField.onValueChanged.AddListener(OnTransferAddressValueChanged);
			TransferNFTButton.onClick.AddListener(OnTransferClicked);
		}

		private void OnDisable()
		{
			PriceInputField.onValueChanged.RemoveListener(OnPriceValueChanged);
			SetButton.onClick.RemoveListener(OnSetClicked);
			MarketplaceToggle.onValueChanged.RemoveListener(OnMarketplaceValueChanged);
			ViewNFTButton.onClick.RemoveListener(OnViewNFTClicked);
			ViewCollectionButton.onClick.RemoveListener(OnViewCollectionClicked);
			RemoveBuyPriceButton.onClick.RemoveListener(OnRemoveBuyPriceClicked);
			TransferAddressInputField.onValueChanged.RemoveListener(OnTransferAddressValueChanged);
			TransferNFTButton.onClick.RemoveListener(OnTransferClicked);
		}

		#endregion

		#region PublicMethods

		public async void Initialize(eNFTUpdateTargetType nftUpdateTargetType, Nft nft)
		{
			mNft = nft;
			switch (nftUpdateTargetType)
			{
				case eNFTUpdateTargetType.SET_BUY_PRICE:
					NftUpdatePanelState = eNFTUpdatePanelState.SET_BUY_PRICE;
					Title.text = LocalizationKeys.SET_BUY_PRICE_TITLE.Translate();
					Description.text = LocalizationKeys.SET_BUY_PRICE_DESCRIPTION.Translate();
					SetButtonText.text = LocalizationKeys.SET_BUY_AMOUNT_REQUIRED.Translate();
					break;
				case eNFTUpdateTargetType.CHANGE_RESERVE:
					break;
				case eNFTUpdateTargetType.LIST_FOR_AUCTION:
					break;
				case eNFTUpdateTargetType.CHANGE_BUY_PRICE:
					NftUpdatePanelState = eNFTUpdatePanelState.SET_BUY_PRICE;
					Title.text = LocalizationKeys.CHANGE_BUY_NOW_PRICE_TITLE.Translate();
					Description.text = LocalizationKeys.SET_BUY_PRICE_DESCRIPTION.Translate();
					SetButtonText.text = LocalizationKeys.SET_BUY_PRICE.Translate();
					NFTDetailsContractType details = await DataManager.Instance.GetNFTDetailsWithCache(nft.TokenAddress, nft.TokenId);
					PriceInputField.text = details.BuyPriceInEtherFixedPoint;
					break;
				case eNFTUpdateTargetType.REMOVE_BUY_PRICE:
					NftUpdatePanelState = eNFTUpdatePanelState.REMOVE_BUY_PRICE;
					break;
				case eNFTUpdateTargetType.TRANSFER_NFT:
					NftUpdatePanelState = eNFTUpdatePanelState.TRANSFER_NFT;
					TransferAddressInputField.text = string.Empty;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(nftUpdateTargetType), nftUpdateTargetType, null);
			}

			ProfileListNftItem.IsReadyOnly = true;
			await ProfileListNftItem.Initialize(nft, null);

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
			OnSetBuyPrice();

		}

		private void OnSetBuyPrice()
		{
			float price = float.Parse(PriceInputField.text);
			BigInteger priceInWei = UnitConversion.Convert.ToWei(price, 18);
			MessagePopup.ShowConfirmationWalletPopup(NFTMarketManager.Instance.SetBuyPrice(mNft.TokenAddress, mNft.TokenId, priceInWei),
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

		private void OnViewNFTClicked()
		{
			CanvasPlayerPCManager.Instance.OpenNftDetailsPanel(mNft);
		}

		private async void OnViewCollectionClicked()
		{
			CollectionCreatedEvent collection = await DataManager.Instance.GetCollectionCreatedEventWithCache(mNft.TokenAddress);
			CanvasPlayerPCManager.Instance.OpenCollectionDetailsPanel(collection);
		}

		private void OnRemoveBuyPriceClicked()
		{
			MessagePopup.ShowConfirmationWalletPopup(NFTMarketManager.Instance.CancelBuyPrice(mNft.TokenAddress, mNft.TokenId),
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

		private void OnTransferAddressValueChanged(string value)
		{
			value = value.ToLowerInvariant().Trim();
			
			if (string.IsNullOrEmpty(value))
			{
				TransferButtonText.text = LocalizationKeys.TRANSFER_NFT_ADDRESS_MANDATORY.Translate();
				TransferNFTButton.interactable = false;
			}
			else if (Regex.IsMatch(value, "^(0x)?[0-9a-f]{40}$", RegexOptions.IgnoreCase))
			{
				TransferNFTButton.interactable = true;
				TransferButtonText.text = LocalizationKeys.TRANSFER_NFT.Translate();
			}
			else
			{
				TransferButtonText.text = LocalizationKeys.TRANSFER_NFT_ADDRESS_INVALID.Translate();
				TransferNFTButton.interactable = false;
			}
		}

		private void OnTransferClicked()
		{
			string to = TransferAddressInputField.text;
			MessagePopup.ShowConfirmationWalletPopup(NFTManager.Instance.TransferItem(mNft.TokenAddress, mNft.TokenId, to),
				(transactionId) =>
				{
					MessagePopup.ShowConfirmationBlockchainPopup(
						LocalizationKeys.TRANSFER_NFT_WAITING_TITLE.Translate(),
						LocalizationKeys.TRANSFER_NFT_WAITING_DESCRIPTION.Translate(),
						transactionId,
						OnNFTTransfered);
				});
		}

		private void OnNFTTransfered(AbstractContractEvent obj)
		{
			NftUpdatePanelState = eNFTUpdatePanelState.CONGRATULATIONS;
			CongratulationsTitle.text = LocalizationKeys.TRANSFER_NFT_SUCCESS_TITLE.Translate();
			CongratulationsDescription.text = LocalizationKeys.TRANSFER_NFT_SUCCESS_DESCRIPTION.Translate();
		}
		#endregion
	}
}
