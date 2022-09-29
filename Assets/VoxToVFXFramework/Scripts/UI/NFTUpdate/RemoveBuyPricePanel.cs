using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Popups;

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public class RemoveBuyPricePanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Button RemoveBuyPriceButton;

		#endregion

		#region Fields

		private NFTUpdatePanel mNftUpdatePanel;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			RemoveBuyPriceButton.onClick.AddListener(OnRemoveBuyPriceClicked);
		}
		private void OnDisable()
		{
			RemoveBuyPriceButton.onClick.RemoveListener(OnRemoveBuyPriceClicked);
		}

		#endregion

		#region PublicMethods

		public void Initialize(NFTUpdatePanel updatePanel)
		{
			mNftUpdatePanel = updatePanel;
		}

		#endregion

		#region PrivateMethods

		private void OnRemoveBuyPriceClicked()
		{
			MessagePopup.ShowConfirmationWalletPopup(NFTMarketManager.Instance.CancelBuyPrice(mNftUpdatePanel.Nft.TokenAddress, mNftUpdatePanel.Nft.TokenId),
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
			mNftUpdatePanel.SetCongratulations(LocalizationKeys.REMOVE_BUY_NOW_REMOVED_TITLE.Translate(), LocalizationKeys.REMOVE_BUY_NOW_REMOVED_DESCRIPTION.Translate());
		}

		#endregion
	}
}
