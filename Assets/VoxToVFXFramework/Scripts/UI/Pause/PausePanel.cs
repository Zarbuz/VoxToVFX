using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
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
	[SerializeField] private Button PhotoModeButton;

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
		PhotoModeButton.onClick.AddListener(OnPhotoModeClicked);
		Refresh();
	}


	private void OnDisable()
	{
		ContinueButton.onClick.RemoveListener(OnContinueClicked);
		ImportSceneButton.onClick.RemoveListener(OnImportSceneClicked);
		OpenSceneButton.onClick.RemoveListener(OnOpenSceneClicked);
		OpenSettingsButton.onClick.RemoveListener(OnOpenSettingsClicked);
		QuitSceneButton.onClick.RemoveListener(OnQuitSceneClicked);
		ChangeWeatherButton.onClick.RemoveListener(OnChangeWeatherClicked);
		PhotoModeButton.onClick.RemoveListener(OnPhotoModeClicked);
	}

	#endregion

	#region PrivateMethods

	private void Refresh()
	{
		ImportSceneButton.gameObject.SetActive(!RuntimeVoxManager.Instance.IsReady && Application.isEditor);
		OpenSceneButton.gameObject.SetActive(!RuntimeVoxManager.Instance.IsReady && Application.isEditor);
		QuitSceneButton.gameObject.SetActive(RuntimeVoxManager.Instance.IsReady);
		PhotoModeButton.gameObject.SetActive(RuntimeVoxManager.Instance.IsReady);
		ChangeWeatherButton.gameObject.SetActive(RuntimeVoxManager.Instance.IsReady);
	}

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
		CanvasPlayerPCManager.Instance.GenericTogglePanel(CanvasPlayerPCState.Weather);
	}

	private void OnQuitSceneClicked()
	{
		RuntimeVoxManager.Instance.Release();
		Refresh();
	}

	private void OnPhotoModeClicked()
	{
		CanvasPlayerPCManager.Instance.GenericTogglePanel(CanvasPlayerPCState.Photo);
	}


	#endregion
}
