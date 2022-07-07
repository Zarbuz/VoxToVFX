using System;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.UI.Popups;

namespace VoxToVFXFramework.Scripts.UI.Settings
{
	public class DataTabSettings : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Button OpenCacheButton;
		[SerializeField] private Button DeleteCacheButton;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			OpenCacheButton.onClick.AddListener(OnOpenCacheClicked);
			DeleteCacheButton.onClick.AddListener(OnDeleteCacheClicked);
			CanvasPlayerPCManager.Instance.PauseLockedState = true;
		}

		private void OnDisable()
		{
			OpenCacheButton.onClick.RemoveListener(OnOpenCacheClicked);
			DeleteCacheButton.onClick.RemoveListener(OnDeleteCacheClicked);
			CanvasPlayerPCManager.Instance.PauseLockedState = false;

		}

		#endregion

		#region PrivateMethods

		private void OnOpenCacheClicked()
		{
			VoxelDataCreatorManager.Instance.OpenCacheFolder();
		}

		private void OnDeleteCacheClicked()
		{
			MessagePopup.ShowConfirmWithBlocking(LocalizationKeys.SETTINGS_DATA_CONFIRM_DELETE.Translate(),
				() =>
				{
					VoxelDataCreatorManager.Instance.ClearCacheFolder();
				});
		}

		#endregion
	}
}
