using UnityEngine;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI.Settings
{
	public class SettingsPanel : MonoBehaviour
	{
		#region Enum

		private enum ESettingsState
		{
			DISPLAY,
			GRAPHICS,
			INPUT,
			DATA
		}

		#endregion

		#region ScriptParamaters

		[SerializeField] private Button CloseButton;

		[Header("Tabs")]
		[SerializeField] private Button DisplayButton;
		[SerializeField] private Button GraphicsButton;
		[SerializeField] private Button InputButton;
		[SerializeField] private Button DataButton;

		[Header("Panels")]
		[SerializeField] private DisplayTabSettings DisplayTabSettings;
		[SerializeField] private GraphicsTabSettings GraphicsTabSettings;
		[SerializeField] private InputTabSettings InputTabSettings;
		[SerializeField] private DataTabSettings DataTabSettings;
	
		#endregion

		#region Fields

		private ESettingsState mESettingsState;

		private ESettingsState SettingsState
		{
			get => mESettingsState;
			set
			{
				mESettingsState = value;
				DisplayTabSettings.gameObject.SetActive(mESettingsState == ESettingsState.DISPLAY);
				GraphicsTabSettings.gameObject.SetActive(mESettingsState == ESettingsState.GRAPHICS);
				InputTabSettings.gameObject.SetActive(mESettingsState == ESettingsState.INPUT);
				DataTabSettings.gameObject.SetActive(mESettingsState == ESettingsState.DATA);
			}
		}

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			CloseButton.onClick.AddListener(OnCloseClicked);
			DisplayButton.onClick.AddListener(() => OnSettingsTabChanged(ESettingsState.DISPLAY));
			GraphicsButton.onClick.AddListener(() => OnSettingsTabChanged(ESettingsState.GRAPHICS));
			InputButton.onClick.AddListener(() => OnSettingsTabChanged(ESettingsState.INPUT));
			DataButton.onClick.AddListener(() => OnSettingsTabChanged(ESettingsState.DATA));

			OnSettingsTabChanged(ESettingsState.DISPLAY);
		}

		private void OnDisable()
		{
			CloseButton.onClick.RemoveListener(OnCloseClicked);

			DisplayButton.onClick.RemoveAllListeners();
			GraphicsButton.onClick.RemoveAllListeners();
			InputButton.onClick.RemoveAllListeners();
			DataButton.onClick.RemoveAllListeners();
		}

		#endregion

		#region PrivateMethods

		private void OnCloseClicked()
		{
			CanvasPlayerPCManager.Instance.GenericClosePanel();
		}

		private void OnSettingsTabChanged(ESettingsState settingsState)
		{
			SettingsState = settingsState;
			DisplayButton.transform.GetChild(0).gameObject.SetActive(SettingsState == ESettingsState.DISPLAY);
			GraphicsButton.transform.GetChild(0).gameObject.SetActive(SettingsState == ESettingsState.GRAPHICS);
			InputButton.transform.GetChild(0).gameObject.SetActive(SettingsState == ESettingsState.INPUT);
			DataButton.transform.GetChild(0).gameObject.SetActive(SettingsState == ESettingsState.DATA);
		}

		#endregion
	}
}
