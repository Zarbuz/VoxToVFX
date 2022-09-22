using MoralisUnity;
using Nethereum.Util;
using System.Globalization;
using System.Numerics;
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
using Vector3 = UnityEngine.Vector3;

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public class SetBuyPricePanel : MonoBehaviour
	{
		#region ScriptParameters

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

		#endregion

		#region ConstStatic

		public const int MARKETPLACE_FEES = 5;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			PriceInputField.onValueChanged.AddListener(OnPriceValueChanged);
			SetButton.onClick.AddListener(OnSetClicked);
			MarketplaceToggle.onValueChanged.AddListener(OnToggleValueChanged);
		}

		private void OnDisable()
		{
			PriceInputField.onValueChanged.RemoveListener(OnPriceValueChanged);
			SetButton.onClick.RemoveListener(OnSetClicked);
			MarketplaceToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
		}

		#endregion

		#region Fields

		private NFTUpdatePanel mNftUpdatePanel;
		#endregion

		#region PublicMethods

		public async void Initialize(NFTUpdatePanel updatePanel)
		{
			mNftUpdatePanel = updatePanel;
			switch (mNftUpdatePanel.NftUpdatePanelState)
			{
				case eNFTUpdateTargetType.SET_BUY_PRICE:
					Title.text = LocalizationKeys.SET_BUY_PRICE_TITLE.Translate();
					Description.text = LocalizationKeys.SET_BUY_PRICE_DESCRIPTION.Translate();
					SetButtonText.text = LocalizationKeys.SET_BUY_AMOUNT_REQUIRED.Translate();
					break;
				case eNFTUpdateTargetType.CHANGE_BUY_PRICE:
					Title.text = LocalizationKeys.CHANGE_BUY_NOW_PRICE_TITLE.Translate();
					Description.text = LocalizationKeys.SET_BUY_PRICE_DESCRIPTION.Translate();
					SetButtonText.text = LocalizationKeys.SET_BUY_PRICE.Translate();
					NFTDetailsContractType details = await DataManager.Instance.GetNFTDetailsWithCache(updatePanel.Nft.TokenAddress, updatePanel.Nft.TokenId);
					PriceInputField.text = details.BuyPriceInEtherFixedPoint;
					break;
			}
	
		}

		#endregion

		#region PrivateMethods

		private void OnSetClicked()
		{
			OnSetBuyPrice();
		}

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

		private void OnSetBuyPrice()
		{
			float price = float.Parse(PriceInputField.text);
			BigInteger priceInWei = UnitConversion.Convert.ToWei(price, 18);
			MessagePopup.ShowConfirmationWalletPopup(NFTMarketManager.Instance.SetBuyPrice(mNftUpdatePanel.Nft.TokenAddress, mNftUpdatePanel.Nft.TokenId, priceInWei),
				(transactionId) =>
				{
					MessagePopup.ShowConfirmationBlockchainPopup(
						LocalizationKeys.SET_BUY_PRICE_WAITING_TITLE.Translate(),
						LocalizationKeys.SET_BUY_PRICE_WAITING_DESCRIPTION.Translate(),
						transactionId,
						OnBuyPriceSet);
				});
		}

		private void OnToggleValueChanged(bool active)
		{
			ArrowIcon.transform.eulerAngles = active ? new Vector3(0, 0, 90) : new Vector3(0, 0, 270);
			MarketplacePanel.SetActive(active);
		}

		private void OnBuyPriceSet(AbstractContractEvent obj)
		{
			BuyPriceSetEvent buyPriceSetEvent = obj as BuyPriceSetEvent;
			BigInteger priceInWei = BigInteger.Parse(buyPriceSetEvent.Price);
			decimal price = UnitConversion.Convert.FromWei(priceInWei);
			mNftUpdatePanel.SetCongratulations(LocalizationKeys.SET_BUY_PRICE_SUCCESS_TITLE.Translate(), string.Format(LocalizationKeys.SET_BUY_PRICE_SUCCESS_DESCRIPTION.Translate(), price.ToString("F2")));
		}

		#endregion
	}
}
