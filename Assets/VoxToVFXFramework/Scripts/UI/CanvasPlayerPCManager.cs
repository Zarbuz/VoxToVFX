using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI.ImportScene;

namespace VoxToVFXFramework.Scripts.UI
{
	public enum CanvasPlayerPCState
	{
		Closed,
		Pause,
		Loading,
		ImportScene
	}

	public class CanvasPlayerPCManager : ModuleSingleton<CanvasPlayerPCManager>
	{
		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI LoadingProgressText;
		[SerializeField] private PausePanel PausePanel;
		[SerializeField] private ImportScenePanel ImportScenePanel;

		#endregion

		#region Fields

		private CanvasPlayerPCState mCanvasPlayerPcState;

		private CanvasPlayerPCState CanvasPlayerPcState
		{
			get => mCanvasPlayerPcState;
			set
			{
				mCanvasPlayerPcState = value;
				LoadingProgressText.gameObject.SetActive(mCanvasPlayerPcState == CanvasPlayerPCState.Loading);
				PausePanel.gameObject.SetActive(mCanvasPlayerPcState == CanvasPlayerPCState.Pause);
				ImportScenePanel.gameObject.SetActive(mCanvasPlayerPcState == CanvasPlayerPCState.ImportScene);

				PostProcessingManager.Instance.SetDepthOfField(mCanvasPlayerPcState != CanvasPlayerPCState.Closed);
			}
		}

		public bool PauseLockedState { get; set; }

		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			VoxelDataCreatorManager.Instance.LoadProgressCallback += OnLoadProgressUpdate;
			RuntimeVoxManager.Instance.LoadFinishedCallback += OnLoadFinished;
			CanvasPlayerPcState = CanvasPlayerPCState.Closed;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape) && !PauseLockedState)
			{
				GenericTogglePanel(CanvasPlayerPCState.Pause);
			}
		}	

		private void OnDestroy()
		{
			if (RuntimeVoxManager.Instance != null)
			{
				VoxelDataCreatorManager.Instance.LoadProgressCallback -= OnLoadProgressUpdate;
				RuntimeVoxManager.Instance.LoadFinishedCallback -= OnLoadFinished;
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

		#endregion



		#region PrivateMethods

		private void OnLoadProgressUpdate(int step, float progress)
		{
			LoadingProgressText.text = "Step: " + step + " - Progress: " + progress.ToString("P", CultureInfo.InvariantCulture);
		}

		private void OnLoadFinished()
		{
			SetCanvasPlayerState(CanvasPlayerPCState.Closed);
		}

		#endregion
	}
}
