using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.UI.Popups.Descriptor;

namespace VoxToVFXFramework.Scripts.UI.Popups.Popup
{
	public class ConfirmationWalletPopup : PopupWithAlpha<ConfirmationWalletDescriptor>
	{
		#region ScriptParameters

		[SerializeField] private Button RetryButton;
		[SerializeField] private Image Spinner;

		#endregion

		#region Fields

		private ConfirmationWalletDescriptor mConfirmationWalletDescriptor;

		#endregion

		#region PublicMethods

		public override void Init(ConfirmationWalletDescriptor descriptor)
		{
			base.Init(descriptor);
			mConfirmationWalletDescriptor = descriptor;
			RetryButton.gameObject.SetActive(false);
			RetryButton.onClick.RemoveAllListeners();
			RetryButton.onClick.AddListener(OnRetryClicked);
			ExecuteAction();
		}

		#endregion

		#region PrivateMethods

		private async void ExecuteAction()
		{
			string transactionId = await mConfirmationWalletDescriptor.ActionToExecute;
			if (string.IsNullOrEmpty(transactionId))
			{
				//error
				MessagePopup.Show(LocalizationKeys.COLLECTION_EXECUTE_CONTRACT_ERROR.Translate());
				Spinner.gameObject.SetActive(false);
				RetryButton.gameObject.SetActive(true);
			}
			else
			{
				mConfirmationWalletDescriptor.OnActionSuccessful?.Invoke(transactionId);
				Hide();
			}
		}

		private void OnRetryClicked()
		{
			ExecuteAction();
		}

		#endregion
	}
}
