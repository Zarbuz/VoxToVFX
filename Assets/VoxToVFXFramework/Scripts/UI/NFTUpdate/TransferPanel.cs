using MoralisUnity.Web3Api.Models;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Popups;

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public class TransferPanel : MonoBehaviour
	{
		#region ScriptParameters

		[Header("TransferPanel")]
		[SerializeField] private TMP_InputField TransferAddressInputField;
		[SerializeField] private Button TransferNFTButton;
		[SerializeField] private TextMeshProUGUI TransferButtonText;

		#endregion

		#region Fields

		private NFTUpdatePanel mNftUpdatePanel;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			TransferAddressInputField.onValueChanged.AddListener(OnTransferAddressValueChanged);
			TransferNFTButton.onClick.AddListener(OnTransferClicked);
		}

		private void OnDisable()
		{
			TransferAddressInputField.onValueChanged.RemoveListener(OnTransferAddressValueChanged);
			TransferNFTButton.onClick.RemoveListener(OnTransferClicked);
		}

		#endregion

		#region PublicMethods

		public void Initialize(NFTUpdatePanel updatePanel)
		{
			mNftUpdatePanel = updatePanel;
			TransferAddressInputField.text = string.Empty;
		}

		#endregion

		#region PrivateMethods

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
			MessagePopup.ShowConfirmationWalletPopup(NFTManager.Instance.TransferItem(mNftUpdatePanel.Nft.TokenAddress, mNftUpdatePanel.Nft.TokenId, to),
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
			mNftUpdatePanel.SetCongratulations(LocalizationKeys.TRANSFER_NFT_SUCCESS_TITLE.Translate(), LocalizationKeys.TRANSFER_NFT_SUCCESS_DESCRIPTION.Translate());
		}
		#endregion
	}
}
