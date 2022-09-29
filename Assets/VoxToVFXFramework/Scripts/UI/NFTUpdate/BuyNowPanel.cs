using MoralisUnity;
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
	public class BuyNowPanel : AbstractMarketplacePanel
	{
		#region ScriptParameters

		[Header("BuyNowPanel")]
		[SerializeField] private Button BuyNowButton;
		[SerializeField] private TextMeshProUGUI PriceText;

		#endregion

		#region Fields

		private NFTDetailsContractType mDetails;

		#endregion

		#region UnityMethods

		protected override void OnEnable()
		{
			base.OnEnable();
			BuyNowButton.onClick.AddListener(OnBuyNowClicked);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			BuyNowButton.onClick.RemoveListener(OnBuyNowClicked);
		}

		#endregion

		#region PublicMethods

		public override async void Initialize(NFTUpdatePanel updatePanel)
		{
			base.Initialize(updatePanel);
			mDetails = await DataManager.Instance.GetNFTDetailsWithCache(updatePanel.Nft.TokenAddress, updatePanel.Nft.TokenId);
			PriceText.text = mDetails.BuyPriceInEtherFixedPoint + " " + Moralis.CurrentChain.Symbol;
		}

		#endregion

		#region PrivateMethods

		private void OnBuyNowClicked()
		{
			if (mDetails.BuyPriceInEther > mTotalBalance)
			{
				MessagePopup.Show(LocalizationKeys.BUY_NOW_NOT_ENOUGH_ETH.Translate());
				return;
			}

			MessagePopup.ShowConfirmationWalletPopup(NFTMarketManager.Instance.Buy(mNftUpdatePanel.Nft.TokenAddress, mNftUpdatePanel.Nft.TokenId, mDetails.BuyPriceInEther),
				(transactionId) =>
				{
					MessagePopup.ShowConfirmationBlockchainPopup(
						LocalizationKeys.BUY_NOW_WAITING_TITLE.Translate(),
						LocalizationKeys.BUY_NOW_WAITING_DESCRIPTION.Translate(),
						transactionId,
						OnBuyAccepted);
				});
		}

		private void OnBuyAccepted(AbstractContractEvent obj)
		{
			mNftUpdatePanel.SetCongratulations(LocalizationKeys.BUY_NOW_SUCCESS_TITLE.Translate(), LocalizationKeys.BUY_NOW_SUCCESS_DESCRIPTION.Translate());
		}

		#endregion
	}
}
