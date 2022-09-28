using MoralisUnity;
using TMPro;
using UnityEngine;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.Managers;

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public abstract class AbstractMarketplacePanel : AbstractUpdatePanel
	{
		#region ScriptParameters

		[Header("MarketplacePanel")]
		[SerializeField] private TextMeshProUGUI AvailableBalanceText;
		[SerializeField] private TextMeshProUGUI MarketplaceBalanceText;
		[SerializeField] private TextMeshProUGUI WalletBalanceText;

		#endregion

		#region Fields

		protected NFTUpdatePanel mNftUpdatePanel;
		protected decimal mTotalBalance;

		#endregion

		#region PublicMethods

		public virtual async void Initialize(NFTUpdatePanel updatePanel)
		{
			mNftUpdatePanel = updatePanel;

			AccountInfoContractType accountInfo = await UserManager.Instance.GetAccountInfo();

			decimal marketplaceBalance = accountInfo.AvailableBalance;
			decimal walletBalance = accountInfo.Balance;
			mTotalBalance = marketplaceBalance + walletBalance;

			AvailableBalanceText.text = mTotalBalance.ToString("0.####") + " " + Moralis.CurrentChain.Symbol;
			MarketplaceBalanceText.text = marketplaceBalance.ToString("0.####") + " " + Moralis.CurrentChain.Symbol;
			WalletBalanceText.text = walletBalance.ToString("0.####") + " " + Moralis.CurrentChain.Symbol;
		}

		#endregion
	}
}
