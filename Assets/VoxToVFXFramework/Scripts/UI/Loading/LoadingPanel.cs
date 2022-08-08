using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;

namespace VoxToVFXFramework.Scripts.UI.Loading
{
	public class LoadingPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI ProgressText;
		[SerializeField] private Image ProgressBarFilled;

		#endregion

		#region Fields

		private Action mOnLoadingFinished;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			VoxelDataCreatorManager.Instance.LoadProgressCallback += OnLoadProgressUpdate;
			RuntimeVoxManager.Instance.LoadFinishedCallback += OnLoadFinished;
		}

		private void OnDisable()
		{
			if (VoxelDataCreatorManager.Instance != null)
			{
				VoxelDataCreatorManager.Instance.LoadProgressCallback -= OnLoadProgressUpdate;
			}

			if (RuntimeVoxManager.Instance != null)
			{
				RuntimeVoxManager.Instance.LoadFinishedCallback -= OnLoadFinished;
			}
		}

		#endregion

		#region PublicMethods

		public void Initialize(Action onLoadingFinished)
		{
			CanvasPlayerPCManager.Instance.GenericTogglePanel(CanvasPlayerPCState.Loading);
			mOnLoadingFinished = onLoadingFinished;
		}

		#endregion

		#region PrivateMethods

		private void OnLoadProgressUpdate(int step, float progress)
		{
			ProgressText.text = $"{progress.ToString("P", CultureInfo.InvariantCulture)}";
			ProgressBarFilled.fillAmount = progress;
		}

		private void OnLoadFinished()
		{
			mOnLoadingFinished?.Invoke();
		}


		#endregion
	}
}
