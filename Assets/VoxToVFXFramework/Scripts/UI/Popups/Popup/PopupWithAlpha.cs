using System;
using UnityEngine;
using VoxToVFXFramework.Scripts.UI.Popups.Descriptor;

namespace VoxToVFXFramework.Scripts.UI.Popups.Popup
{
	public abstract class PopupWithAlpha<T> : MonoBehaviour, InitalizablePopup<T> where T : IMessagePopupDescriptor
	{
		#region Fields

		public MessagePopupUnicityTag UnicityTag { get; set; }

		protected Action mOnDurationOver;
		protected T mDescriptor;

		private float mTime;
		private float mDuration;
		private CanvasGroup mCanvasGroup;

		#endregion

		#region UnityMethods

		protected virtual void Awake()
		{
			mCanvasGroup = GetComponent<CanvasGroup>();
			if (mCanvasGroup != null)
				mCanvasGroup.alpha = 0;
		}

		protected virtual void Update()
		{
			if (mCanvasGroup == null)
				return;

			mTime += Time.unscaledDeltaTime;
			mCanvasGroup.alpha = Mathf.Clamp01(mTime * 2f);

			if (mDuration > 0)
			{
				mCanvasGroup.alpha *= Mathf.Clamp01((mDuration - mTime) * 2f);

				if (mTime >= mDuration)
				{
					mOnDurationOver?.Invoke();
					Hide();
				}
			}
		}

		#endregion

		#region PublicMethods

		public virtual void Init(T descriptor)
		{
			mDescriptor = descriptor;
			mDuration = descriptor.PopupDisplayDuration;
			UnicityTag = descriptor.UnicityTag;
		}

		public void Show()
		{
			mTime = 0f;
			gameObject.SetActive(true);
		}

		public void Hide(bool removeFromList = true)
		{
			transform.SetParent(null);
			Destroy(gameObject);
			if (removeFromList)
			{
				MessagePopup.Instance.OnPopupDestroyed(this);
			}
		}

		public virtual void UpdateText(string str)
		{

		}

		#endregion


	}
}
