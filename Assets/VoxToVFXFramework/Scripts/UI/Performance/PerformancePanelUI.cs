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

	[SerializeField] private Toggle ShowLODToggle;
	[SerializeField] private Toggle ForceLoadAllChunkToggle;
	[SerializeField] private GameObject ContentPanel;

	#endregion

	#region UnityMethods

	private void OnEnable()
	{
		TogglePanelButton.onClick.AddListener(OnTogglePanelClicked);
		ForceLevelLODSlider.onValueChanged.AddListener(OnForceLevelLODValueChanged);
		ShowLODToggle.onValueChanged.AddListener(OnShowLODValueChanged);
		ForceLoadAllChunkToggle.onValueChanged.AddListener(OnForceLoadAllChunksChunks);
		RefreshValues();
	}

	private void OnDisable()
	{
		TogglePanelButton.onClick.RemoveListener(OnTogglePanelClicked);
		ForceLevelLODSlider.onValueChanged.RemoveListener(OnForceLevelLODValueChanged);
		ShowLODToggle.onValueChanged.RemoveListener(OnShowLODValueChanged);
		ForceLoadAllChunkToggle.onValueChanged.RemoveListener(OnForceLoadAllChunksChunks);
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
		ShowLODToggle.SetIsOnWithoutNotify(RuntimeVoxManager.Instance.ForceLoadAllChunks);
		ForceLoadAllChunkToggle.transform.parent.gameObject.SetActive(RuntimeVoxManager.Instance.ForcedLevelLod >= 0);

	}

	private void OnForceLevelLODValueChanged(float value)
	{
		RuntimeVoxManager.Instance.SetForceLODValue((int)value);
		ForceLevelLODText.text = "Force Level LOD: " + value;
		ForceLoadAllChunkToggle.transform.parent.gameObject.SetActive(value >= 0);
	}

	private void OnShowLODValueChanged(bool value)
	{
		RuntimeVoxManager.Instance.SetDebugLodValue(value);
	}

	private void OnForceLoadAllChunksChunks(bool value)
	{
		RuntimeVoxManager.Instance.SetDisableCullingChunks(value);
	}
	#endregion
}
