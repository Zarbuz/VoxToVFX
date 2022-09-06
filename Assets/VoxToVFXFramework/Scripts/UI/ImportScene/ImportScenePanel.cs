using System.Collections.Generic;
using SFB;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;

namespace VoxToVFXFramework.Scripts.UI.ImportScene
{
	//For internal usage only, should not be used anymore
	public class ImportScenePanel : MonoBehaviour
	{
		public enum EDataImportType
		{
			VOX,
			CUSTOM
		}

		private enum EImportState
		{
			NORMAL,
			IMPORT_IN_PROGRESS
		}

		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private TextMeshProUGUI SubTitle;

		[SerializeField] private Button OpenFileClicked;
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
				ProgressStepText.gameObject.SetActive(mImportState == EImportState.IMPORT_IN_PROGRESS && DataImportTypeState == EDataImportType.VOX);
				mImportButtonHighlightable.SetInteractable(mImportState == EImportState.NORMAL);
			}
		}

		private EDataImportType DataImportTypeState { get; set; }

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			mImportButtonHighlightable = OpenFileClicked.GetComponent<ButtonHighlightable>();
			OpenFileClicked.onClick.AddListener(OnOpenFileClicked);
			
			VoxelDataCreatorManager.Instance.LoadProgressCallback += OnLoadProgressUpdate;
			VoxelDataCreatorManager.Instance.LoadFinishedCallback += OnLoadVoxFinished;

			RuntimeVoxManager.Instance.LoadFinishedCallback += OnLoadCustomFinished;

		}

		private void OnDisable()
		{
			OpenFileClicked.onClick.RemoveListener(OnOpenFileClicked);

			if (VoxelDataCreatorManager.Instance != null)
			{
				VoxelDataCreatorManager.Instance.LoadProgressCallback -= OnLoadProgressUpdate;
				VoxelDataCreatorManager.Instance.LoadFinishedCallback -= OnLoadVoxFinished;
			}

			if (RuntimeVoxManager.Instance != null)
			{
				RuntimeVoxManager.Instance.LoadFinishedCallback -= OnLoadCustomFinished;
			}
		}

		#endregion

		#region PublicMethods
		
		public void Initialize(EDataImportType importType)
		{
			DataImportTypeState = importType;

			Title.text = DataImportTypeState == EDataImportType.VOX ? LocalizationKeys.IMPORT_SCENE_TITLE.Translate() : LocalizationKeys.OPEN_SCENE_TITLE.Translate();
			SubTitle.text = DataImportTypeState == EDataImportType.VOX ? LocalizationKeys.IMPORT_SCENE_SUBTITLE.Translate() : LocalizationKeys.OPEN_SCENE_SUBTITLE.Translate();

			ImportState = EImportState.NORMAL;
		}

		#endregion

		#region PrivateMethods

		private void OnOpenFileClicked()
		{
			if (DataImportTypeState == EDataImportType.VOX)
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
			else
			{
				string[] paths = StandaloneFileBrowser.OpenFilePanel("Select ZIP file", "", "zip", false);
				if (paths.Length > 0)
				{
					VoxelDataCreatorManager.Instance.ReadZipFile(paths[0]);
				}
			}
		}

		private void OnLoadProgressUpdate(int step, float progress)
		{
			ImportState = EImportState.IMPORT_IN_PROGRESS;
			ProgressStepText.text = $"Step: {step}/{VoxelDataCreatorManager.MAX_STEPS_ON_IMPORT}";
			ProgressText.text = $"{progress.ToString("P", CultureInfo.InvariantCulture)}";
			ProgressBarFilled.fillAmount = progress;
		}

		private void OnLoadVoxFinished(string outputZip, List<string> outputChunkPaths)
		{
			ImportState = EImportState.NORMAL;
		}

		private void OnLoadCustomFinished()
		{
			CanvasPlayerPCManager.Instance.GenericClosePanel();
		}
		#endregion
	}
}
