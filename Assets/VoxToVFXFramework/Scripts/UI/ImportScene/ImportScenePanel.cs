using System;
using System.Diagnostics;
using System.Globalization;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI.ImportScene
{
	public class ImportScenePanel : MonoBehaviour
	{
		private enum EImportState
		{
			NORMAL,
			IMPORT_IN_PROGRESS
		}

		#region ScriptParameters

		[SerializeField] private Button ImportSceneButton;
		[SerializeField] private Image ProgressBar;
		[SerializeField] private Image ProgressBarFilled;
		[SerializeField] private TextMeshProUGUI ProgressText;
		[SerializeField] private TextMeshProUGUI ProgressStepText;

		#endregion

		#region Fields

		private ButtonHighlightable mImportButtonHighlightable;
		private EImportState mImportState;

		private EImportState ImportState
		{
			get => mImportState;
			set
			{
				mImportState = value;
				ProgressBar.gameObject.SetActive(mImportState == EImportState.IMPORT_IN_PROGRESS);
				ProgressStepText.gameObject.SetActive(mImportState == EImportState.IMPORT_IN_PROGRESS);
				mImportButtonHighlightable.SetInteractable(mImportState == EImportState.NORMAL);
			}
		}

		#endregion

		#region ConstStatic

		private const int MAX_STEPS_ON_IMPORT = 2;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			mImportButtonHighlightable = ImportSceneButton.GetComponent<ButtonHighlightable>();
			ImportSceneButton.onClick.AddListener(OnImportSceneClicked);
			
			ImportState = EImportState.NORMAL;
			VoxelDataCreatorManager.Instance.LoadProgressCallback += OnLoadProgressUpdate;
			VoxelDataCreatorManager.Instance.LoadFinishedCallback += OnLoadFinished;
		}

		private void OnDisable()
		{
			ImportSceneButton.onClick.RemoveListener(OnImportSceneClicked);
			if (VoxelDataCreatorManager.Instance != null)
			{
				VoxelDataCreatorManager.Instance.LoadProgressCallback -= OnLoadProgressUpdate;
				VoxelDataCreatorManager.Instance.LoadFinishedCallback -= OnLoadFinished;
			}
		}

		#endregion

		#region PrivateMethods

		private void OnImportSceneClicked()
		{
			string[] paths = StandaloneFileBrowser.OpenFilePanel("Select MagicaVoxel file", "", "vox", false);
			if (paths.Length > 0)
			{
				string outputPath = StandaloneFileBrowser.SaveFilePanel("Select destination", "", "", "zip");
				if (!string.IsNullOrEmpty(outputPath))
				{
					VoxelDataCreatorManager.Instance.CreateZipFile(paths[0], outputPath);
				}
			}
		}

		private void OnLoadProgressUpdate(int step, float progress)
		{
			CanvasPlayerPCManager.Instance.PauseLockedState = true;
			ImportState = EImportState.IMPORT_IN_PROGRESS;
			ProgressStepText.text = $"Step: {step}/{MAX_STEPS_ON_IMPORT}";
			ProgressText.text = $"{progress.ToString("P", CultureInfo.InvariantCulture)}";
			ProgressBarFilled.fillAmount = progress;
		}

		private void OnLoadFinished()
		{
			ImportState = EImportState.NORMAL;
			CanvasPlayerPCManager.Instance.PauseLockedState = false;
		}
		#endregion
	}
}
