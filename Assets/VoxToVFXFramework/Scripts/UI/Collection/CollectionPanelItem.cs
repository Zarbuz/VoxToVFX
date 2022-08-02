using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI.Collection
{
	public class CollectionPanelItem : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI NameText;
		[SerializeField] private Button SelectButton;
		[SerializeField] private Image CollectionImage;
		[SerializeField] private Image PlusImage;

		#endregion

		#region PublicMethods

		public void Initialize()
		{

		}

		#endregion
	}
}
