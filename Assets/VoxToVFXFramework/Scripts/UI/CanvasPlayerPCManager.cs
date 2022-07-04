using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.UI
{
	public enum CanvasPlayerPCState
	{
		None,
		Loading,
	}

	public class CanvasPlayerPCManager : ModuleSingleton<CanvasPlayerPCManager>
	{
		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI LoadingProgressText;
		[SerializeField] private GameObject PausePanel;

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
			}
		}

		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			VoxelDataCreatorManager.Instance.LoadProgressCallback += OnLoadProgressUpdate;
			RuntimeVoxManager.Instance.LoadFinishedCallback += OnLoadFinished;
			VoxelDataCreatorManager.Instance.LoadFinishedCallback += OnLoadFinished;
			CanvasPlayerPcState = CanvasPlayerPCState.None;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				PausePanel.SetActive(!PausePanel.activeSelf);
			}
		}	

		private void OnDestroy()
		{
			if (RuntimeVoxManager.Instance != null)
			{
				VoxelDataCreatorManager.Instance.LoadProgressCallback -= OnLoadProgressUpdate;
				RuntimeVoxManager.Instance.LoadFinishedCallback -= OnLoadFinished;
				VoxelDataCreatorManager.Instance.LoadFinishedCallback -= OnLoadFinished;
			}
		}

		#endregion

		#region PublicMethods

		public void SetCanvasPlayerState(CanvasPlayerPCState state)
		{
			CanvasPlayerPcState = state;
		}

		#endregion

		#region PrivateMethods

		private void OnLoadProgressUpdate(int step, float progress)
		{
			LoadingProgressText.text = "Step: " + step + " - Progress: " + progress.ToString("P", CultureInfo.InvariantCulture);
		}

		private void OnLoadFinished()
		{
			SetCanvasPlayerState(CanvasPlayerPCState.None);
		}

		#endregion
	}
}
