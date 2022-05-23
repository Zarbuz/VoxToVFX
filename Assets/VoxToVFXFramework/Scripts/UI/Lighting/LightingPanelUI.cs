using UnityEngine;
using UnityEngine.UI;

public class LightingPanelUI : MonoBehaviour
{
	#region SerializeFields

	[SerializeField] private Button TogglePanelButton;
	[SerializeField] private Slider RotationXSlider;
	[SerializeField] private Slider RotationYSlider;
	[SerializeField] private GameObject ContentPanel;

	#endregion

	#region UnityMethods

	private void OnEnable()
	{
		TogglePanelButton.onClick.AddListener(OnTogglePanelClicked);
		RotationXSlider.onValueChanged.AddListener(OnRotationXValueChanged);
		RotationYSlider.onValueChanged.AddListener(OnRotationYValueChanged);

		Vector3 eulerAngles = LightManager.Instance.GetCurrentRotation();
		RotationXSlider.SetValueWithoutNotify(eulerAngles.x);
		RotationXSlider.SetValueWithoutNotify(eulerAngles.y);
	}

	private void OnDisable()
	{
		TogglePanelButton.onClick.RemoveListener(OnTogglePanelClicked);
		RotationXSlider.onValueChanged.RemoveListener(OnRotationXValueChanged);
		RotationYSlider.onValueChanged.RemoveListener(OnRotationYValueChanged);
	}

	#endregion

	#region PrivateMethods

	private void OnTogglePanelClicked()
	{
		ContentPanel.SetActive(!ContentPanel.activeSelf);
	}

	private void OnRotationYValueChanged(float value)
	{
		LightManager.Instance.SetLightXRotation((int)value);
	}

	private void OnRotationXValueChanged(float value)
	{
		LightManager.Instance.SetLightYRotation((int)value);
	}

	#endregion
}
