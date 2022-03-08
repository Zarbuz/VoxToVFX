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
	}

	private void OnDisable()
	{
		TogglePanelButton.onClick.RemoveListener(OnTogglePanelClicked);
		MagicaVoxelImportButton.onClick.RemoveListener(OnMagicaVoxelImportClicked);
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


	#endregion
}
