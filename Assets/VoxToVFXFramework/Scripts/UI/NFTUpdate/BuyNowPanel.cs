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
using Vector3 = UnityEngine.Vector3;

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public class BuyNowPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Button BuyNowButton;
		[SerializeField] private Image ArrowIcon;
		[SerializeField] private Toggle AvailableBalanceToggle;
		[SerializeField] private GameObject MarketplacePanel;
		[SerializeField] private TextMeshProUGUI PriceText;
		[SerializeField] private TextMeshProUGUI AvailableBalanceText;
		[SerializeField] private TextMeshProUGUI MarketplaceBalanceText;
		[SerializeField] private TextMeshProUGUI WalletBalanceText;

		#endregion

		#region Fields

		private NFTUpdatePanel mNftUpdatePanel;
		private NFTDetailsContractType mDetails;
		private decimal mTotalBalance;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			BuyNowButton.onClick.AddListener(OnBuyNowClicked);
			AvailableBalanceToggle.onValueChanged.AddListener(OnToggleValueChanged);
		}

		private void OnDisable()
		{
			BuyNowButton.onClick.RemoveListener(OnBuyNowClicked);
			AvailableBalanceToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
		}

		#endregion

		#region PublicMethods

		public async void Initialize(NFTUpdatePanel updatePanel)
		{
			mNftUpdatePanel = updatePanel;
			mDetails = await DataManager.Instance.GetNFTDetailsWithCache(updatePanel.Nft.TokenAddress, updatePanel.Nft.TokenId);
			PriceText.text = mDetails.BuyPriceInEtherFixedPoint + " " + Moralis.CurrentChain.Symbol;
			AccountInfoContractType accountInfo = await UserManager.Instance.GetAccountInfo();

			decimal marketplaceBalance = accountInfo.AvailableBalance;
			decimal walletBalance = accountInfo.Balance;
			mTotalBalance = marketplaceBalance + walletBalance;

			AvailableBalanceText.text = mTotalBalance.ToString("0.####") + " " + Moralis.CurrentChain.Symbol;
			MarketplaceBalanceText.text = marketplaceBalance.ToString("0.####") + " " + Moralis.CurrentChain.Symbol;
			WalletBalanceText.text = walletBalance.ToString("0.####") + " " + Moralis.CurrentChain.Symbol;
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

		private void OnToggleValueChanged(bool active)
		{
			ArrowIcon.transform.eulerAngles = active ? new Vector3(0, 0, 90) : new Vector3(0, 0, 270);
			MarketplacePanel.SetActive(active);
		}

		private void OnBuyAccepted(AbstractContractEvent obj)
		{
			mNftUpdatePanel.SetCongratulations(LocalizationKeys.BUY_NOW_SUCCESS_TITLE.Translate(), LocalizationKeys.BUY_NOW_SUCCESS_DESCRIPTION.Translate());
		}

		#endregion
	}
}
