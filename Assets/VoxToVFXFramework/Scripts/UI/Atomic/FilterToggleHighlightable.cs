using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	[RequireComponent(typeof(Toggle))]
	public class FilterToggleHighlightable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		#region ScriptParameters

		[SerializeField] private Image Background;
		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private TextMeshProUGUI Count;
		[SerializeField] private Image Circle;
		[SerializeField] private Color DefaultColor;

		#endregion

		#region Fields

		private Toggle mToggle;

		#endregion

		#region UnityMethods

		private void Awake()
		{
			mToggle = GetComponent<Toggle>();
			mToggle.onValueChanged.AddListener(OnToggle);
		}

		private void OnDestroy()
		{
			mToggle.onValueChanged.RemoveListener(OnToggle);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			Circle.color = Color.black;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			Circle.color = DefaultColor;
		}
		#endregion


		#region PrivateMethods

		private void OnToggle(bool active)
		{
			Circle.color = active ? Color.black : DefaultColor;
			Background.color = active ? Color.black : Color.white;
			Title.color = active ? Color.white : Color.black;
			Count.color = active ? Color.white : Color.black;
		}

		#endregion

	}
}
