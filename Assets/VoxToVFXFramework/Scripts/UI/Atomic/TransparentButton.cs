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

		[SerializeField] protected Image FrameImage;
		[SerializeField] protected Image BackgroundImage;
		[SerializeField] protected Image OptionalIcon;
		[SerializeField] protected TextMeshProUGUI ButtonText;
		[SerializeField] protected Color BackgroundActive;
		[SerializeField] protected Color BackgroundDisable;
		[SerializeField] protected Color FrameColorActive;
		[SerializeField] protected Color FrameColorDisable;
		[SerializeField] protected Color OptionalIconColorDisable;
		#endregion

		#region Fields

		protected Button mButton;
		protected bool mImageBackgroundActive;
		
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

		protected virtual void Awake()
		{
			mButton = GetComponent<Button>();
		}

		public virtual void OnPointerEnter(PointerEventData eventData)
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

		public virtual void OnPointerExit(PointerEventData eventData)
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
