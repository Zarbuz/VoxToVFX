using MoralisUnity.Web3Api.Models;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Popups;

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public class BurnPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Button BurnNFTButton;

		#endregion

		#region Fields

		private NFTUpdatePanel mNftUpdatePanel;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			BurnNFTButton.onClick.AddListener(OnBurnClicked);
		}

		private void OnDisable()
		{
			BurnNFTButton.onClick.RemoveListener(OnBurnClicked);
		}

		#endregion

		#region PublicMethods

		public void Initialize(NFTUpdatePanel updatePanel)
		{
			mNftUpdatePanel = updatePanel;
		}

		#endregion

		#region PrivateMethods

		private void OnBurnClicked()
		{
			MessagePopup.ShowConfirmationWalletPopup(NFTManager.Instance.BurnNFT(mNftUpdatePanel.Nft.TokenAddress, mNftUpdatePanel.Nft.TokenId),
				(transactionId) =>
				{
					MessagePopup.ShowConfirmationBlockchainPopup(
						LocalizationKeys.BURN_NFT_WAITING_TITLE.Translate(),
						LocalizationKeys.BURN_NFT_WAITING_DESCRIPTION.Translate(),
						transactionId,
						OnNFTBurned);
				});
		}

		private void OnNFTBurned(AbstractContractEvent obj)
		{
			mNftUpdatePanel.SetCongratulations(LocalizationKeys.BURN_NFT_SUCCESS_TITLE.Translate(), LocalizationKeys.BURN_NFT_SUCCESS_DESCRIPTION.Translate(), false);
		}

		#endregion
	}
}
