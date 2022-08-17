using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.UI.Atomic;

namespace VoxToVFXFramework.Scripts.UI.Loading
{
	public class LoadingPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI Description;
		[SerializeField] private ProgressBar ProgressBar;
		[SerializeField] private Image Spinner;
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

		public void Initialize(string description, Action onLoadingFinished)
		{
			CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Loading);
			Spinner.gameObject.SetActive(onLoadingFinished == null);
			ProgressBar.gameObject.SetActive(onLoadingFinished != null);
			Description.text = description;
			mOnLoadingFinished = onLoadingFinished;
		}

		public void Initialize(string description)
		{
			Initialize(description, null);
		}

		#endregion

		#region PrivateMethods

		private void OnLoadProgressUpdate(int step, float progress)
		{
			ProgressBar.SetProgress(progress);
		}

		private void OnLoadFinished()
		{
			mOnLoadingFinished?.Invoke();
		}


		#endregion
	}
}
