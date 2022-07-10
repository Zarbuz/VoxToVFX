using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Utils.Extensions;

namespace VoxToVFXFramework.Scripts.UI.Settings
{
	public class InputTabSettingsItem : MonoBehaviour
	{
		#region ScriptParameters
		[SerializeField] private LocalizedText DisplayText;
		[SerializeField] private GameObject WaitingPanel;
		[SerializeField] private GameObject WarningIcon;
		[SerializeField] private Button Button;

		public InputInfo InputInfo { get; private set; }

		#endregion

		#region Fields

		private string mWarningMessage;

		#endregion

		#region PublicMethods

		public void Initialize(InputInfo inputInfo, Action<InputTabSettingsItem> onButtonClicked)
		{
			WarningIcon.SetActiveSafe(false);
			InputInfo = inputInfo;
			DisplayText.SetKey(inputInfo.DisplayName);
			Button.onClick.AddListener(() =>
			{
				WaitingPanel.SetActiveSafe(true);
				onButtonClicked?.Invoke(this);
			});
			Button.GetComponentInChildren<TextMeshProUGUI>().text = InputManager.Instance.GetKey(InputInfo.KeyName).ToString();
		}

		public void RefreshKey()
		{
			WaitingPanel.SetActiveSafe(false);
			Button.GetComponentInChildren<TextMeshProUGUI>().text = InputManager.Instance.GetKey(InputInfo.KeyName).ToString();
		}

		public void DisplayWarningIcon(KeyCode newKey)
		{
			WarningIcon.SetActiveSafe(true);
			mWarningMessage = string.Format(LocalizationKeys.SETTINGS_KEY_ERROR_ALREADY_SET.Translate(), newKey.ToString());
			RefreshKey();
		}

		public void HideWarningIcon()
		{
			WarningIcon.SetActiveSafe(false);
		}

		public void UpdateToolTipText()
		{
			WarningIcon.GetComponentInChildren<TextMeshProUGUI>().text = mWarningMessage;
		}

		#endregion
	}
}