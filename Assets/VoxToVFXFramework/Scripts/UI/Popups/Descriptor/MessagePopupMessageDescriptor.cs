using System;
using UnityEngine;

namespace VoxToVFXFramework.Scripts.UI.Popups.Descriptor
{
	public class MessagePopupMessageDescriptor : IMessagePopupDescriptor
	{
		public MessagePopupUnicityTag UnicityTag { get; set; } = MessagePopupUnicityTag.DUPLICATE_ALLOWED;
		public float PopupDisplayDuration { get; set; } = -1;
		public bool PlaySoundOnShow { get; set; } = true;
		public string Message { get; set; }
		public LogType LogType { get; set; } = LogType.Log;
		public bool SetOffsetMessage { get; set; }
		public string Ok { get; set; }
		public string Cancel { get; set; }
		public Action OnConfirm { get; set; }
		public Action OnCancel { get; set; }
		public Action OnDurationOver { get; set; }
	}
}
