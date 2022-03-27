using System;
using SFB;
using UnityEngine;
using UnityEngine.UI;

public class DataImportPanelUI : MonoBehaviour
{
	#region ScriptParamaters

	[SerializeField] private Button TogglePanelButton;
	[SerializeField] private Button MagicaVoxelImportButton;
	[SerializeField] private Button VoxelDataImportButton;
	[SerializeField] private GameObject ContentPanel;

	#endregion

	#region UnityMethods

	private void OnEnable()
	{
		TogglePanelButton.onClick.AddListener(OnTogglePanelClicked);
		MagicaVoxelImportButton.onClick.AddListener(OnMagicaVoxelImportClicked);
		VoxelDataImportButton.onClick.AddListener(OnVoxelDataImportClicked);
	}

	private void OnDisable()
	{
		TogglePanelButton.onClick.RemoveListener(OnTogglePanelClicked);
		MagicaVoxelImportButton.onClick.RemoveListener(OnMagicaVoxelImportClicked);
		VoxelDataImportButton.onClick.RemoveListener(OnVoxelDataImportClicked);
	}

	#endregion

	#region PrivateMethods

	private void OnTogglePanelClicked()
	{
		ContentPanel.SetActive(!ContentPanel.activeSelf);
	}

	private void OnMagicaVoxelImportClicked()
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

	private void OnVoxelDataImportClicked()
	{
		string[] paths = StandaloneFileBrowser.OpenFilePanel("Select ZIP file", "", "zip", false);
		if (paths.Length > 0)
		{
			VoxelDataCreatorManager.Instance.ReadZipFile(paths[0]);
		}
	}

	#endregion
}
