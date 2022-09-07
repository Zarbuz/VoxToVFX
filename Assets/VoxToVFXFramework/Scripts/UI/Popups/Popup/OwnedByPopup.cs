using System.Linq;
using MoralisUnity.Web3Api.Models;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.UI.Popups.Descriptor;
using VoxToVFXFramework.Scripts.UI.Popups.Popup.OwnedBy;

namespace VoxToVFXFramework.Scripts.UI.Popups.Popup
{
	public class OwnedByPopup : PopupWithAlpha<OwnedByDescriptor>
	{
		#region ScriptParameters

		[SerializeField] private VerticalLayoutGroup VerticalParent;
		[SerializeField] private Button CloseButton;
		[SerializeField] private CollectionOwnerItem CollectionOwnerItemPrefab;

		#endregion

		#region PublicMethods

		public override void Init(OwnedByDescriptor descriptor)
		{
			base.Init(descriptor);
			CloseButton.onClick.AddListener(() => Hide());
			foreach (string owner in descriptor.Owners.Select(t => t.OwnerOf).Distinct())
			{
				CollectionOwnerItem item = Instantiate(CollectionOwnerItemPrefab, VerticalParent.transform, false);
				item.Initialize(owner);
			}
		}

		#endregion
	}
}
