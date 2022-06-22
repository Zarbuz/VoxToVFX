using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;

public class LightingPanelUI : MonoBehaviour
{
	#region SerializeFields

	[SerializeField] private Button TogglePanelButton;
	[SerializeField] private Slider RotationXSlider;
	[SerializeField] private Slider RotationYSlider;
	[SerializeField] private Slider ExposureWeightSlider;
	[SerializeField] private GameObject ContentPanel;

	#endregion

	#region UnityMethods

	private void OnEnable()
	{
		TogglePanelButton.onClick.AddListener(OnTogglePanelClicked);
		RotationXSlider.onValueChanged.AddListener(OnRotationXValueChanged);
		RotationYSlider.onValueChanged.AddListener(OnRotationYValueChanged);
		ExposureWeightSlider.onValueChanged.AddListener(OnExposureWeightValueChanged);

		Vector3 eulerAngles = LightManager.Instance.GetCurrentRotation();
		RotationXSlider.SetValueWithoutNotify(eulerAngles.x);
		RotationXSlider.SetValueWithoutNotify(eulerAngles.y);
	}

	private void OnDisable()
	{
		TogglePanelButton.onClick.RemoveListener(OnTogglePanelClicked);
		RotationXSlider.onValueChanged.RemoveListener(OnRotationXValueChanged);
		RotationYSlider.onValueChanged.RemoveListener(OnRotationYValueChanged);
		ExposureWeightSlider.onValueChanged.RemoveListener(OnExposureWeightValueChanged);
	}

	#endregion

	#region PrivateMethods

	private void OnTogglePanelClicked()
	{
		ContentPanel.SetActive(!ContentPanel.activeSelf);
	}

	private void OnRotationYValueChanged(float value)
	{
		LightManager.Instance.SetLightXRotation(value);
	}

	private void OnRotationXValueChanged(float value)
	{
		LightManager.Instance.SetLightYRotation(value);
	}

	private void OnExposureWeightValueChanged(float value)
	{
		RuntimeVoxManager.Instance.ExposureWeight = value;
	}
	#endregion
}
