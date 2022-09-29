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

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public class SetBuyPricePanel : AbstractUpdatePanel
	{
		#region ScriptParameters

		[Header("SetBuyPricePanel")]
		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private TextMeshProUGUI Description;
		[SerializeField] private TMP_InputField PriceInputField;
		[SerializeField] private Button SetButton;
		[SerializeField] private TextMeshProUGUI SetButtonText;
		[SerializeField] private TextMeshProUGUI MarketplaceFeeCountText;
		[SerializeField] private TextMeshProUGUI ReceiveCountText;

		#endregion

		#region ConstStatic

		public const int MARKETPLACE_FEES = 5;
		private const float MIN_PRICE = 0.01f;
		#endregion

		#region UnityMethods

		protected override void OnEnable()
		{
			base.OnEnable();
			PriceInputField.onValueChanged.AddListener(OnPriceValueChanged);
			SetButton.onClick.AddListener(OnSetClicked);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			PriceInputField.onValueChanged.RemoveListener(OnPriceValueChanged);
			SetButton.onClick.RemoveListener(OnSetClicked);
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
			if (!success)
			{
				SetButtonText.text = LocalizationKeys.SET_BUY_AMOUNT_REQUIRED.Translate();
				SetButton.interactable = false;
				ReceiveCountText.text = "0.00 " + Moralis.CurrentChain.Symbol;
				MarketplaceFeeCountText.text = "0.00 " + Moralis.CurrentChain.Symbol;
			}
			else
			{	
				if (value < MIN_PRICE)
				{
					SetButtonText.text = string.Format(LocalizationKeys.MUST_BE_AT_LEAST_X_LABEL.Translate(), MIN_PRICE);
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
