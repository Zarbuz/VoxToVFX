using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHighlightable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	#region ScriptParameters

	[SerializeField] private TextMeshProUGUI ButtonText;
	[SerializeField] private Button Button;

	#endregion
	#region UnityMethods

	public void OnPointerEnter(PointerEventData eventData)
	{
		ButtonText.color = Button.colors.highlightedColor;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ButtonText.color = Button.colors.normalColor;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		ButtonText.color = Button.colors.pressedColor;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		ButtonText.color = Button.colors.normalColor;
	}

	#endregion


}
