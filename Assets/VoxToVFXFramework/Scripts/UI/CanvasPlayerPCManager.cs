using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI.ImportScene;
using VoxToVFXFramework.Scripts.UI.Loading;
using VoxToVFXFramework.Scripts.UI.Photo;
using VoxToVFXFramework.Scripts.UI.Preview;
using VoxToVFXFramework.Scripts.UI.Settings;
using VoxToVFXFramework.Scripts.UI.Weather;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using Cursor = UnityEngine.Cursor;

namespace VoxToVFXFramework.Scripts.UI
{
	public enum CanvasPlayerPCState
	{
		Closed,
		Empty,
		Pause,
		ImportScene,
		Settings,
		Weather,
		Photo,
		Collection,
		Creation,
		Loading,
	
	}

	public class CanvasPlayerPCManager : ModuleSingleton<CanvasPlayerPCManager>
	{
		#region ScriptParameters

		[SerializeField] private PausePanel PausePanel;
		[SerializeField] private ImportScenePanel ImportScenePanel;
		[SerializeField] private SettingsPanel SettingsPanel;
		[SerializeField] private WeatherPanel WeatherPanel;
		[SerializeField] private PhotoPanel PhotoPanel;
		
		[SerializeField] private LoadingPanel LoadingPanel;
		

		#endregion

		#region Fields

		private CanvasPlayerPCState mCanvasPlayerPcState;

		public CanvasPlayerPCState CanvasPlayerPcState
		{
			get => mCanvasPlayerPcState;
			set
			{
				mCanvasPlayerPcState = value;
				PausePanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Pause);
				ImportScenePanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.ImportScene);
				SettingsPanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Settings);
				WeatherPanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Weather);
				PhotoPanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Photo);
				
				LoadingPanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Loading);
				RefreshCursorState();
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
			}
			else if (Keyboard.current.tabKey.wasPressedThisFrame && (CanvasPlayerPcState == CanvasPlayerPCState.Photo || CanvasPlayerPcState == CanvasPlayerPCState.Closed))
			{
				if (RuntimeVoxManager.Instance.IsReady)
				{
					GenericTogglePanel(CanvasPlayerPCState.Photo);
				}
			}
			else if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				RefreshCursorState();
			}
		}


		private void OnApplicationFocus(bool hasFocus)
		{
			RefreshCursorState();
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
			PauseLockedState = false;
		}

		public void OpenImportScenePanel(ImportScenePanel.EDataImportType dataImportType)
		{
			SetCanvasPlayerState(CanvasPlayerPCState.ImportScene);
			ImportScenePanel.Initialize(dataImportType);
		}

		
		#endregion

		#region PrivateMethods

		private void RefreshCursorState()
		{
			Cursor.visible = !RuntimeVoxManager.Instance.IsReady || mCanvasPlayerPcState != CanvasPlayerPCState.Closed;
			Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
		}

		#endregion
	}
}
