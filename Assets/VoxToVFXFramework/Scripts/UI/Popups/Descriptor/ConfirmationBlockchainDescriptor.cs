using System;
using VoxToVFXFramework.Scripts.Models.ContractEvent;

namespace VoxToVFXFramework.Scripts.UI.Popups.Descriptor
{
	public class ConfirmationBlockchainDescriptor : IMessagePopupDescriptor
	{
		public MessagePopupUnicityTag UnicityTag { get; set; }
		public float PopupDisplayDuration { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string TransactionId { get; set; }
		public Action<AbstractContractEvent> OnActionSuccessful { get; set; }
	}
}
