using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.UI.Popups.Descriptor;

namespace VoxToVFXFramework.Scripts.UI.Popups.Popup
{
	public class ConfirmationWalletPopup : PopupWithAlpha<ConfirmationWalletDescriptor>
	{
		#region Fields

		private ConfirmationWalletDescriptor mConfirmationWalletDescriptor;

		#endregion

		#region PublicMethods

		public override void Init(ConfirmationWalletDescriptor descriptor)
		{
			base.Init(descriptor);
			mConfirmationWalletDescriptor = descriptor;
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
				Hide();
			}
			else
			{
				mConfirmationWalletDescriptor.OnActionSuccessful?.Invoke(transactionId);
				Hide();
			}
		}

		#endregion
	}
}
