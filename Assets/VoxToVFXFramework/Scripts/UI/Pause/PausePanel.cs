using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.UI;
using VoxToVFXFramework.Scripts.UI.ImportScene;

public class PausePanel : MonoBehaviour
{
	#region ScriptParameters

	[SerializeField] private Button ContinueButton;
	[SerializeField] private Button ImportSceneButton;
	[SerializeField] private Button OpenSceneButton;
	[SerializeField] private Button OpenSettingsButton;
	[SerializeField] private Button QuitSceneButton;
	[SerializeField] private Button ChangeWeatherButton;

	#endregion

	#region UnityMethods

	private void OnEnable()
	{
		ContinueButton.onClick.AddListener(OnContinueClicked);
		ImportSceneButton.onClick.AddListener(OnImportSceneClicked);
		OpenSceneButton.onClick.AddListener(OnOpenSceneClicked);
		OpenSettingsButton.onClick.AddListener(OnOpenSettingsClicked);
		QuitSceneButton.onClick.AddListener(OnQuitSceneClicked);
		ChangeWeatherButton.onClick.AddListener(OnChangeWeatherClicked);
	}

	private void OnDisable()
	{
		ContinueButton.onClick.RemoveListener(OnContinueClicked);
		ImportSceneButton.onClick.RemoveListener(OnImportSceneClicked);
		OpenSceneButton.onClick.RemoveListener(OnOpenSceneClicked);
		OpenSettingsButton.onClick.RemoveListener(OnOpenSettingsClicked);
		QuitSceneButton.onClick.RemoveListener(OnQuitSceneClicked);
		ChangeWeatherButton.onClick.RemoveListener(OnChangeWeatherClicked);
	}

	#endregion

	#region PrivateMethods

	private void OnContinueClicked()
	{
		CanvasPlayerPCManager.Instance.GenericClosePanel();
	}

	private void OnImportSceneClicked()
	{
		CanvasPlayerPCManager.Instance.OpenImportScenePanel(ImportScenePanel.EDataImportType.VOX);
	}

	private void OnOpenSceneClicked()
	{
		CanvasPlayerPCManager.Instance.OpenImportScenePanel(ImportScenePanel.EDataImportType.CUSTOM);
	}

	private void OnOpenSettingsClicked()
	{
		CanvasPlayerPCManager.Instance.GenericTogglePanel(CanvasPlayerPCState.Settings);
	}

	private void OnChangeWeatherClicked()
	{
		throw new NotImplementedException();
	}

	private void OnQuitSceneClicked()
	{
		throw new NotImplementedException();
	}

	

	#endregion
}
