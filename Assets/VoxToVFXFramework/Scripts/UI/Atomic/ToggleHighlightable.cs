using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Utils.Extensions;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	public class ToggleHighlightable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IPointerDownHandler, IPointerUpHandler
	{
		#region ScriptParameters

		[SerializeField] private Toggle Toggle;
		[SerializeField] private Image Graphic;
		[SerializeField] private GameObject CheckMark;
		[SerializeField] private Sprite BaseSprite;
		[SerializeField] private Sprite SelectedSprite;
		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private Color HighlightColor;
		[SerializeField] private Color PressedColor;

		#endregion

		#region Fields

		private Color mTitleBaseColor;
		private Color mGraphicBaseColor;
		private bool mIsInit;
		private bool mIsDisabled;

		#endregion

		#region UnityMethods

		private void Awake()
		{
			Init();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			OnHoverEnter();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			OnHoverExit();
		}

		public void OnSelect(BaseEventData eventData)
		{
			OnSelected();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			OnPointerDown();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			OnPointerUp();
		}

		#endregion

		#region PublicMethods


		public void OnHoverEnter()
		{
			Init();
			if (mIsDisabled)
			{
				return;
			}

			Title.color = HighlightColor;
			Graphic.color = HighlightColor;
		}

		public void OnHoverExit()
		{
			Init();

			if (mIsDisabled)
			{
				return;
			}

			Title.color = mTitleBaseColor;
			Graphic.color = mGraphicBaseColor;
		}

		public void OnPointerDown()
		{
			Init();

			if (mIsDisabled)
			{
				return;
			}

			Graphic.color = PressedColor;
		}

		public void OnPointerUp()
		{
			Init();

			if (mIsDisabled)
			{
				return;
			}

			Graphic.color = mGraphicBaseColor;
		}

		public void OnSelected()
		{
			Init();

			if (mIsDisabled)
			{
				return;
			}

			OnToggled(true);
		}

		public void OnDeselected()
		{
			Init();

			if (mIsDisabled)
			{
				return;
			}

			OnToggled(false);
		}

		public void SetIsOn(bool isOn, bool notify = true)
		{
			if (mIsDisabled)
			{
				return;
			}

			if (notify)
			{
				Toggle.isOn = isOn;
			}
			else
			{
				Toggle.SetIsOnWithoutNotify(isOn);

			}

			OnToggled(isOn);
		}

		public void AddListenerToggle(UnityAction<bool> callback)
		{
			Toggle.onValueChanged.AddListener(callback);
		}

		public void RemoteListenerToggle(UnityAction<bool> callback)
		{
			Toggle.onValueChanged.RemoveListener(callback);
		}

		public void Enable()
		{
			mIsDisabled = false;

			Graphic.color = Graphic.color.A(1f);
			Title.color = Title.color.A(1f);
			Toggle.enabled = true;
		}

		public void Disable()
		{
			mIsDisabled = true;

			Graphic.color = Graphic.color.A(0.25f);
			Title.color = Title.color.A(0.25f);
			Toggle.enabled = false;
		}

		public bool IsOn()
		{
			return Toggle.isOn;
		}
		#endregion

		#region PrivateMethods

		private void Init()
		{
			if (mIsInit)
			{
				return;
			}

			mTitleBaseColor = Title.color;
			mGraphicBaseColor = Graphic.color;
			Toggle.onValueChanged.AddListener(OnToggled);
			mIsInit = true;
		}

		private void OnToggled(bool isSelected)
		{
			Graphic.sprite = isSelected ? SelectedSprite : BaseSprite;

			if (CheckMark != null)
				CheckMark.SetActiveSafe(Toggle.isOn);
		}

		#endregion


	}
}
