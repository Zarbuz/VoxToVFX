using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	[RequireComponent(typeof(Button))]
	public class ButtonEmptyHighlightable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		#region ScriptParameters

		[SerializeField] private Sprite EmptySprite;
		[SerializeField] private Sprite FullSprite;
		[SerializeField] private TextMeshProUGUI Text;
		[SerializeField] private Image Frame;
		[SerializeField] private Color EmptyColorFrame;

		#endregion

		public void OnPointerEnter(PointerEventData eventData)
		{
			Frame.sprite = FullSprite;
			Frame.color = Color.black;
			Text.color =  Color.white;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			Frame.sprite = EmptySprite;
			Frame.color = EmptyColorFrame;
			Text.color = Color.black;
		}
	}
}
