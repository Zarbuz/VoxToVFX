using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class ButtonTextHighlightable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		#region Fields

		private TextMeshProUGUI mText;

		#endregion

		#region UnityMethods

		private void Awake()
		{
			mText = GetComponent<TextMeshProUGUI>();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			mText.fontStyle = FontStyles.Underline;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			mText.fontStyle = FontStyles.Normal;
		}

		#endregion

	}
}
