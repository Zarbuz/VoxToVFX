using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI.Settings
{
	public class EditKeyboardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] private TextMeshProUGUI Text;
		[SerializeField] private Image Icon;
		[SerializeField] private Color HighlightColor;


		public void OnPointerEnter(PointerEventData eventData)
		{
			Text.color = HighlightColor;
			Icon.color = HighlightColor;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			Text.color = Color.white;
			Icon.color = Color.white;
		}
	}

}
