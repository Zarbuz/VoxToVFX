using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.UI.Popups;

namespace VoxToVFXFramework.Scripts.UI.Settings
{
	public class InputTabSettings : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private List<RebindActionKey> RebindActionKeys;
		[SerializeField] private Button ResetAllButton;

		#endregion

		

		#region UnityMethods

		private void OnEnable()
		{
			CanvasPlayerPCManager.Instance.PauseLockedState = true;
			ResetAllButton.onClick.AddListener(OnResetAllClicked);
		}


		private void OnDisable()
		{
			CanvasPlayerPCManager.Instance.PauseLockedState = false;
			ResetAllButton.onClick.RemoveListener(OnResetAllClicked);
		}

		#endregion

		#region PrivateMethods

		private void OnResetAllClicked()
		{
			MessagePopup.ShowConfirmWithBlocking(LocalizationKeys.SETTINGS_CONFIRM_RESET_ALL.Translate(), () =>
			{
				foreach (RebindActionKey rebindActionKey in RebindActionKeys)
				{
					rebindActionKey.ResetToDefault();
				}
			});
		}
		#endregion
	}
}
