using MoralisUnity;
using Nethereum.Util;
using System.Globalization;
using System.Numerics;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Popups;

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public class MakeOfferPanel : AbstractMarketplacePanel
	{
		#region ScriptParameters

		[Header("MakeOfferPanel")]
		[SerializeField] private TMP_InputField InputPrice;
		[SerializeField] private Button MakeOfferButton;
		[SerializeField] private TextMeshProUGUI MakeOfferButtonText;

		#endregion

		#region ConstStatic

		private const float MIN_PRICE = 0.05f;

		#endregion

		#region UnityMethods

		protected override void OnEnable()
		{
			base.OnEnable();
			OnPriceValueChanged("0");
			InputPrice.onValueChanged.AddListener(OnPriceValueChanged);
			MakeOfferButton.onClick.AddListener(OnMakeOfferClicked);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			InputPrice.onValueChanged.RemoveListener(OnPriceValueChanged);
			MakeOfferButton.onClick.RemoveListener(OnMakeOfferClicked);
		}

		
		#endregion

		#region PublicMethods




		#endregion

		#region PrivateMethods

		private void OnPriceValueChanged(string text)
		{
			bool success = float.TryParse(text, NumberStyles.Any, Thread.CurrentThread.CurrentCulture, out float value);
			if (!success)
			{
				MakeOfferButtonText.text = LocalizationKeys.SET_BUY_AMOUNT_REQUIRED.Translate();
				MakeOfferButton.interactable = false;
			}
			else
			{
				if (value < MIN_PRICE)
				{
					MakeOfferButtonText.text = string.Format(LocalizationKeys.MUST_BE_AT_LEAST_X_LABEL.Translate(), MIN_PRICE);
					MakeOfferButton.interactable = false;
				}
				else
				{
					MakeOfferButtonText.text = LocalizationKeys.SET_BUY_PRICE.Translate();
					MakeOfferButton.interactable = true;
				}
			}
		}

		private void OnMakeOfferClicked()
		{
			float price = float.Parse(InputPrice.text);
			BigInteger priceInWei = UnitConversion.Convert.ToWei(price, 18);
			MessagePopup.ShowConfirmationWalletPopup(NFTMarketManager.Instance.MakeOffer(mNftUpdatePanel.Nft.TokenAddress, mNftUpdatePanel.Nft.TokenId, priceInWei),
				(transactionId) =>
				{
					MessagePopup.ShowConfirmationBlockchainPopup(
						LocalizationKeys.MAKE_OFFER_WAITING_TITLE.Translate(),
						LocalizationKeys.MAKE_OFFER_WAITING_DESCRIPTION.Translate(),
						transactionId,
						OnMakeOfferSet);
				});
		}

		private void OnMakeOfferSet(AbstractContractEvent obj)
		{
			mNftUpdatePanel.SetCongratulations(LocalizationKeys.MAKE_OFFER_SUCCESS_TITLE.Translate(), LocalizationKeys.MAKE_OFFER_SUCCESS_DESCRIPTION.Translate(), true, false);
		}

		#endregion
	}
}
