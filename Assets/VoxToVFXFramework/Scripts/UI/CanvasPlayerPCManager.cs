using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI.ImportScene;
using VoxToVFXFramework.Scripts.UI.Settings;
using VoxToVFXFramework.Scripts.UI.Weather;

namespace VoxToVFXFramework.Scripts.UI
{
	public enum CanvasPlayerPCState
	{
		Closed,
		Pause,
		ImportScene,
		Settings,
		Weather
	}

	public class CanvasPlayerPCManager : ModuleSingleton<CanvasPlayerPCManager>
	{
		#region ScriptParameters

		[SerializeField] private PausePanel PausePanel;
		[SerializeField] private ImportScenePanel ImportScenePanel;
		[SerializeField] private SettingsPanel SettingsPanel;
		[SerializeField] private WeatherPanel WeatherPanel;
		#endregion

		#region Fields

		private CanvasPlayerPCState mCanvasPlayerPcState;

		public CanvasPlayerPCState CanvasPlayerPcState
		{
			get => mCanvasPlayerPcState;
			set
			{
				mCanvasPlayerPcState = value;
				PausePanel.gameObject.SetActive(mCanvasPlayerPcState == CanvasPlayerPCState.Pause);
				ImportScenePanel.gameObject.SetActive(mCanvasPlayerPcState == CanvasPlayerPCState.ImportScene);
				SettingsPanel.gameObject.SetActive(mCanvasPlayerPcState == CanvasPlayerPCState.Settings);
				WeatherPanel.gameObject.SetActive(mCanvasPlayerPcState == CanvasPlayerPCState.Weather);

				PostProcessingManager.Instance.SetDepthOfField(mCanvasPlayerPcState != CanvasPlayerPCState.Closed);
			}
		}

		public bool PauseLockedState { get; set; }

		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			CanvasPlayerPcState = CanvasPlayerPCState.Closed;
		}

		private void Update()
		{
			if (Keyboard.current.escapeKey.wasPressedThisFrame && !PauseLockedState)
			{
				GenericTogglePanel(CanvasPlayerPCState.Pause);
				RefreshCursorState();
			}
			else if (CanvasPlayerPcState != CanvasPlayerPCState.Closed && !Cursor.visible)
			{
				RefreshCursorState();
			}
		}

		#endregion

		#region PublicMethods

		public void SetCanvasPlayerState(CanvasPlayerPCState state)
		{
			CanvasPlayerPcState = state;
		}

		public void GenericTogglePanel(CanvasPlayerPCState state)
		{
			CanvasPlayerPcState = CanvasPlayerPcState == state ? CanvasPlayerPCState.Closed : state;
		}

		public void GenericClosePanel()
		{
			CanvasPlayerPcState = CanvasPlayerPCState.Closed;
			RefreshCursorState();
		}

		public void OpenImportScenePanel(ImportScenePanel.EDataImportType dataImportType)
		{
			ImportScenePanel.Initialize(dataImportType);
			GenericTogglePanel(CanvasPlayerPCState.ImportScene);
		}

		#endregion

		#region PrivateMethods

		private void RefreshCursorState()
		{
			if (CanvasPlayerPcState != CanvasPlayerPCState.Closed)
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
				Time.timeScale = 0;
			}
			else
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
				Time.timeScale = 1;
			}
		}

		#endregion
	}
}
