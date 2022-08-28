using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Popups.Descriptor;

namespace VoxToVFXFramework.Scripts.UI.Popups.Popup
{
	public class ConfirmationBlockchainPopup : PopupWithAlpha<ConfirmationBlockchainDescriptor>
	{
		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private TextMeshProUGUI Description;
		[SerializeField] private Button OpenEtherscanButton;
		#endregion

		#region Fields

		private ConfirmationBlockchainDescriptor mConfirmationBlockchainDescriptor;

		#endregion

		#region PublicMethods

		public override void Init(ConfirmationBlockchainDescriptor descriptor)
		{
			mConfirmationBlockchainDescriptor = descriptor;
			EventDatabaseManager.Instance.OnDatabaseEventReceived += OnDatabaseEventReceived;
			base.Init(descriptor);
			Title.text = descriptor.Title;
			Description.text = descriptor.Description;
			OpenEtherscanButton.onClick.RemoveAllListeners();
			OpenEtherscanButton.onClick.AddListener(OnOpenEtherscanClicked);
		}

		#endregion

		#region PrivateMethods

		private void OnOpenEtherscanClicked()
		{
			string url = ConfigManager.Instance.EtherScanBaseUrl + "tx/" + mConfirmationBlockchainDescriptor.TransactionId;
			Application.OpenURL(url);
		}

		private void OnDatabaseEventReceived(AbstractContractEvent item)
		{
			Debug.Log("[ConfirmationBlockchainPopup] OnDatabaseEventReceived item id: " + item.TransactionHash + " waiting: " + mConfirmationBlockchainDescriptor.TransactionId);
			if (item.TransactionHash == mConfirmationBlockchainDescriptor.TransactionId)
			{
				Debug.Log("[ConfirmationBlockchainPopup] TransactionId match !");
				mConfirmationBlockchainDescriptor.OnActionSuccessful?.Invoke(item);
				Hide();
			}
		}

		#endregion
	}
}
