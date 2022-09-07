using System.Collections.Generic;
using MoralisUnity.Web3Api.Models;

namespace VoxToVFXFramework.Scripts.UI.Popups.Descriptor
{
	public class OwnedByDescriptor : IMessagePopupDescriptor
	{
		public MessagePopupUnicityTag UnicityTag { get; set; }
		public float PopupDisplayDuration { get; set; }
		public List<NftOwner> Owners { get; set; }
	}
}
