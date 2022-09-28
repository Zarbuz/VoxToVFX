using System.Collections.Generic;

namespace VoxToVFXFramework.Scripts.UI.Popups.Descriptor
{
	public class OwnedByDescriptor : IMessagePopupDescriptor
	{
		public MessagePopupUnicityTag UnicityTag { get; set; }
		public float PopupDisplayDuration { get; set; }
		public List<string> Owners { get; set; }
	}
}
