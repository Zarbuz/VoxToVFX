using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI.Settings;

namespace VoxToVFXFramework.Scripts.UI
{
	public enum CanvasPlayerPCState
	{
		None,
		Loading,
		Settings
	}

	public class CanvasPlayerPCManager : ModuleSingleton<CanvasPlayerPCManager>
	{
		#region ScriptParameters

		[SerializeField] private SettingsPanel SettingsPanel;
		[SerializeField] private TextMeshProUGUI LoadingProgressText;
		

		#endregion

		#region Fields

		private CanvasPlayerPCState mCanvasPlayerPcState;

		private CanvasPlayerPCState CanvasPlayerPcState
		{
			get => mCanvasPlayerPcState;
			set
			{
				mCanvasPlayerPcState = value;
				SettingsPanel.gameObject.SetActive(mCanvasPlayerPcState == CanvasPlayerPCState.Settings);
				LoadingProgressText.gameObject.SetActive(mCanvasPlayerPcState == CanvasPlayerPCState.Loading);
			}
		}

		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			RuntimeVoxManager.Instance.LoadProgressCallback += OnLoadProgressUpdate;
			RuntimeVoxManager.Instance.LoadFinishedCallback += OnLoadFinished;
			
			CanvasPlayerPcState = CanvasPlayerPCState.None;
		}

		private void OnDestroy()
		{
			if (RuntimeVoxManager.Instance != null)
			{
				RuntimeVoxManager.Instance.LoadProgressCallback -= OnLoadProgressUpdate;
				RuntimeVoxManager.Instance.LoadFinishedCallback -= OnLoadFinished;
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

		private void OnLoadProgressUpdate(float progress)
		{
			LoadingProgressText.text = "Progress: " + progress.ToString("P");
		}

		private void OnLoadFinished()
		{
			SetCanvasPlayerState(CanvasPlayerPCState.Settings); //Temporary
		}

		#endregion
	}
}
