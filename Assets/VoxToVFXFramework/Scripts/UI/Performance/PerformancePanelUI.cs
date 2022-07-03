using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;

public class PerformancePanelUI : MonoBehaviour
{
	#region ScriptParamaters

	[SerializeField] private Button TogglePanelButton;
	[SerializeField] private TextMeshProUGUI ForceLevelLODText;
	[SerializeField] private Slider ForceLevelLODSlider;

	[Header("Rendering")]
	[SerializeField] private Toggle ShowLODToggle;
	[SerializeField] private GameObject ContentPanel;

	
	#endregion

	#region UnityMethods

	private void OnEnable()
	{
		TogglePanelButton.onClick.AddListener(OnTogglePanelClicked);
		ForceLevelLODSlider.onValueChanged.AddListener(OnForceLevelLODValueChanged);
		ShowLODToggle.onValueChanged.AddListener(OnShowLODValueChanged);
		RefreshValues();
	}

	private void OnDisable()
	{
		TogglePanelButton.onClick.RemoveListener(OnTogglePanelClicked);
		ForceLevelLODSlider.onValueChanged.RemoveListener(OnForceLevelLODValueChanged);
		ShowLODToggle.onValueChanged.RemoveListener(OnShowLODValueChanged);
	}

	#endregion

	#region PrivateMethods

	private void OnTogglePanelClicked()
	{
		ContentPanel.SetActive(!ContentPanel.activeSelf);
	}

	private void RefreshValues()
	{
		ForceLevelLODText.text = "Force Level LOD: " + RuntimeVoxManager.Instance.ForcedLevelLod.Value;
		ForceLevelLODSlider.SetValueWithoutNotify(RuntimeVoxManager.Instance.ForcedLevelLod.Value);
		ShowLODToggle.SetIsOnWithoutNotify(RuntimeVoxManager.Instance.DebugLod.Value);
	}

	private void OnForceLevelLODValueChanged(float value)
	{
		RuntimeVoxManager.Instance.ForcedLevelLod.Value = ((int)value);
		ForceLevelLODText.text = "Force Level LOD: " + value;
	}

	private void OnShowLODValueChanged(bool value)
	{
		RuntimeVoxManager.Instance.DebugLod.Value = value;
	}

	#endregion
}
