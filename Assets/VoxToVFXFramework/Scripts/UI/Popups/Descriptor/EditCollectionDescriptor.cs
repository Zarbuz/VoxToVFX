using System;

namespace VoxToVFXFramework.Scripts.UI.Popups.Descriptor
{
	public class EditCollectionDescriptor : IMessagePopupDescriptor
	{
		public MessagePopupUnicityTag UnicityTag { get; set; }
		public float PopupDisplayDuration { get; }
		public string LogoImageUrl { get; set; }
		public string CoverImageUrl { get; set; }
		public string Description { get; set; }
		public Action<Models.CollectionDetails> OnConfirmAction { get; set; }
		public Action OnCancelAction { get; set; }
	}
}
