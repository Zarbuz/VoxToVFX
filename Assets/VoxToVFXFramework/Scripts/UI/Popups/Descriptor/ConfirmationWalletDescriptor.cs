using System;
using Cysharp.Threading.Tasks;

namespace VoxToVFXFramework.Scripts.UI.Popups.Descriptor
{
	public class ConfirmationWalletDescriptor : IMessagePopupDescriptor
	{
		public MessagePopupUnicityTag UnicityTag { get; set; }
		public float PopupDisplayDuration { get; set; }
		public UniTask<string> ActionToExecute { get; set; }
		public Action<string> OnActionSuccessful { get; set; }
	}
}
