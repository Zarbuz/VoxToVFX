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

	[Header("Physics")]
	[SerializeField] private TMP_Dropdown PhysicsQualityDropdown;
	[SerializeField] private TextMeshProUGUI MaxDistanceText;
	[SerializeField] private Slider MaxDistanceSlider;
	#endregion

	#region UnityMethods

	private void OnEnable()
	{
		TogglePanelButton.onClick.AddListener(OnTogglePanelClicked);
		ForceLevelLODSlider.onValueChanged.AddListener(OnForceLevelLODValueChanged);
		ShowLODToggle.onValueChanged.AddListener(OnShowLODValueChanged);
		PhysicsQualityDropdown.onValueChanged.AddListener(OnPhysicsQualityValueChanged);
		MaxDistanceSlider.onValueChanged.AddListener(OnMaxDistanceValueChanged);
		RefreshValues();
	}

	private void OnDisable()
	{
		TogglePanelButton.onClick.RemoveListener(OnTogglePanelClicked);
		ForceLevelLODSlider.onValueChanged.RemoveListener(OnForceLevelLODValueChanged);
		ShowLODToggle.onValueChanged.RemoveListener(OnShowLODValueChanged);
		PhysicsQualityDropdown.onValueChanged.RemoveListener(OnPhysicsQualityValueChanged);
		MaxDistanceSlider.onValueChanged.RemoveListener(OnMaxDistanceValueChanged);
	}

	#endregion

	#region PrivateMethods

	private void OnTogglePanelClicked()
	{
		ContentPanel.SetActive(!ContentPanel.activeSelf);
	}

	private void RefreshValues()
	{
		ForceLevelLODText.text = "Force Level LOD: " + RuntimeVoxManager.Instance.ForcedLevelLod;
		ForceLevelLODSlider.SetValueWithoutNotify(RuntimeVoxManager.Instance.ForcedLevelLod);
		ShowLODToggle.SetIsOnWithoutNotify(RuntimeVoxManager.Instance.DebugLod);
	}
		
	private void OnForceLevelLODValueChanged(float value)
	{
		RuntimeVoxManager.Instance.ForcedLevelLod = ((int)value);
		ForceLevelLODText.text = "Force Level LOD: " + value;
	}

	private void OnShowLODValueChanged(bool value)
	{
		RuntimeVoxManager.Instance.DebugLod = value;
	}

	private void OnPhysicsQualityValueChanged(int index)
	{
		RuntimeVoxManager.Instance.LodLevelForColliders = index switch
		{
			0 => 1,
			1 => 2,
			2 => 4,
			_ => RuntimeVoxManager.Instance.LodLevelForColliders
		};
	}

	private void OnMaxDistanceValueChanged(float value)
	{
		RuntimeVoxManager.Instance.MaxDistanceColliders = (int)value;
		MaxDistanceText.text = "Max Distance: " + (int)value;
	}
	#endregion
}
