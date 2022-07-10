using UnityEngine;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI.ImportScene;
using VoxToVFXFramework.Scripts.UI.Settings;

namespace VoxToVFXFramework.Scripts.UI
{
	public enum CanvasPlayerPCState
	{
		Closed,
		Pause,
		ImportScene,
		Settings
	}

	public class CanvasPlayerPCManager : ModuleSingleton<CanvasPlayerPCManager>
	{
		#region ScriptParameters

		[SerializeField] private PausePanel PausePanel;
		[SerializeField] private ImportScenePanel ImportScenePanel;
		[SerializeField] private SettingsPanel SettingsPanel;
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
			if (Input.GetKeyDown(KeyCode.Escape) && !PauseLockedState)
			{
				GenericTogglePanel(CanvasPlayerPCState.Pause);
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
		}

		public void OpenImportScenePanel(ImportScenePanel.EDataImportType dataImportType)
		{
			ImportScenePanel.Initialize(dataImportType);
			GenericTogglePanel(CanvasPlayerPCState.ImportScene);
		}

		#endregion

		
	}
}
