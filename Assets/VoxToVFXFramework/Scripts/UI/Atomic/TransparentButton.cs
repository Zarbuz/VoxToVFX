using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	[RequireComponent(typeof(Button))]
	public class TransparentButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		#region Scriptparameters

		[SerializeField] private Image FrameImage;
		[SerializeField] private Image BackgroundImage;
		[SerializeField] private Image OptionalIcon;
		[SerializeField] private TextMeshProUGUI ButtonText;
		[SerializeField] private Color BackgroundActive;
		[SerializeField] private Color BackgroundDisable;
		[SerializeField] private Color FrameColorActive;
		[SerializeField] private Color FrameColorDisable;
		[SerializeField] private Color OptionalIconColorDisable;
		#endregion

		#region Fields

		private bool mImageBackgroundActive;
		private Button mButton;
		public bool ImageBackgroundActive
		{
			get => mImageBackgroundActive;
			set
			{
				mImageBackgroundActive = value;
				FrameImage.enabled = !mImageBackgroundActive;
				BackgroundImage.color = mImageBackgroundActive ? BackgroundDisable : BackgroundActive;
				ButtonText.color = mImageBackgroundActive ? Color.white : Color.black;
			}
		}

		#endregion

		#region UnityMethods

		private void Awake()
		{
			mButton = GetComponent<Button>();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!mButton.interactable)
			{
				return;
			}

			if (mImageBackgroundActive)
			{
				ButtonText.color = Color.black;
				BackgroundImage.color = Color.white;
			}
			else
			{
				FrameImage.color = FrameColorActive;
			}

			if (OptionalIcon != null)
			{
				OptionalIcon.color = Color.black;
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!mButton.interactable)
			{
				return;
			}

			if (mImageBackgroundActive)
			{
				ButtonText.color = Color.white;
				BackgroundImage.color = BackgroundDisable;
			}
			else
			{
				FrameImage.color = FrameColorDisable;
			}

			if (OptionalIcon != null)
			{
				OptionalIcon.color = OptionalIconColorDisable;
			}

		}


		#endregion

	}
}
