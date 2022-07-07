using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.UI.Popups.Descriptor;
using VoxToVFXFramework.Scripts.Utils.Extensions;

namespace VoxToVFXFramework.Scripts.UI.Popups.Popup
{
	public class MessagePopupMessage : PopupWithAlpha<MessagePopupMessageDescriptor>
	{
		#region Fields

		public Color[] TextColors;
		public TextMeshProUGUI Message;
		public Button CloseButton;
		public Button OkButton;
		public Button CancelButton;
		public TextMeshProUGUI OkButtonText;
		public TextMeshProUGUI CancelButtonText;

		private Action mOnConfirmCallback;
		private Action mOnCancelCallback;

		#endregion


		#region PublicMethods

		public override void Init(MessagePopupMessageDescriptor descriptor)
		{
			base.Init(descriptor);

			if (CloseButton != null)
			{
				CloseButton.onClick.RemoveAllListeners();
				CloseButton.onClick.AddListener(() => Hide());
			}

			OkButton.onClick.RemoveAllListeners();
			OkButton.onClick.AddListener(Confirm);

			if (CancelButton)
			{
				CancelButton.onClick.RemoveAllListeners();
				CancelButton.onClick.AddListener(Cancel);
				CancelButton.gameObject.SetActive(descriptor.OnCancel != null);
			}


			SetMessage(descriptor.Message, descriptor.LogType);

			if (descriptor.OnConfirm != null || descriptor.OnCancel != null)
			{
				SetConfirm(descriptor.Ok, descriptor.Cancel, descriptor.SetOffsetMessage, descriptor.OnConfirm, descriptor.OnCancel, descriptor.OnDurationOver);
			}
		}

		public virtual void Confirm()
		{
			mOnConfirmCallback?.Invoke();
			Hide();
		}

		public void Cancel()
		{
			mOnCancelCallback?.Invoke();
			Hide();
		}

		public override void UpdateText(string str)
		{
			base.UpdateText(str);
			SetMessage(str, mDescriptor.LogType);
		}
		#endregion

		#region PrivateMethods

		private void SetConfirm(string ok, string cancel, bool setOffsetMessage, Action onConfirm, Action onCancel, Action onDurationOver)
		{
			if (CloseButton != null)
			{
				CloseButton.gameObject.SetActiveSafe(false);
			}

			OkButton.gameObject.SetActiveSafe(true);
			CancelButton.gameObject.SetActiveSafe(true);

			if (OkButtonText != null)
			{
				OkButtonText.text = ok;
			}

			if (CancelButtonText != null)
			{
				if (string.IsNullOrEmpty(cancel))
				{
					CancelButton.gameObject.SetActive(false);
				}
				else
				{
					CancelButton.gameObject.SetActive(true);
					CancelButtonText.text = cancel;
				}

			}

			mOnConfirmCallback = onConfirm;
			mOnCancelCallback = onCancel;
			mOnDurationOver = onDurationOver;

			if (setOffsetMessage)
			{
				Message.GetComponent<RectTransform>().offsetMax = new Vector2(-330, Message.GetComponent<RectTransform>().offsetMax.y);
			}
		}

		private void SetMessage(string message, LogType logType = LogType.Log)
		{
			int index = GetIndexFromLogType(logType);

			if (Message != null)
			{
				Message.text = message;
				Message.color = TextColors[index];
			}
		}

		private int GetIndexFromLogType(LogType logType)
		{
			switch (logType)
			{
				case LogType.Log:
					return 0;
				case LogType.Warning:
					return 1;
				default:
					return 2;
			}
		}

		#endregion

	}
}
