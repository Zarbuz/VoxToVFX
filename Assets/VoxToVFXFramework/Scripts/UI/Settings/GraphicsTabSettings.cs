using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.UI.Atomic;

namespace VoxToVFXFramework.Scripts.UI.Settings
{
	public class GraphicsTabSettings : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TMP_Dropdown RenderScaleDropdown;
		[SerializeField] private TMP_Dropdown QualityDropdown;
		[SerializeField] private Slider FieldOfViewSlider;
		[SerializeField] private Slider MaxDistanceLod0Slider;
		[SerializeField] private Slider MaxDistanceLod1Slider;
		//[SerializeField] private Toggle DepthOfFieldToggle;
		[SerializeField] private ToggleHighlightable DLSSToggle;
		[SerializeField] private ToggleHighlightable VSyncToggle;
		[SerializeField] private ToggleHighlightable DebugLodToggle;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			RenderScaleDropdown.onValueChanged.AddListener(OnRenderScaleValueChanged);
			DLSSToggle.AddListenerToggle(OnDLSSValueChanged);
			QualityDropdown.onValueChanged.AddListener(OnQualityValueChanged);
			VSyncToggle.AddListenerToggle(OnVSyncValueChanged);
			FieldOfViewSlider.onValueChanged.AddListener(OnFieldOfViewValueChanged);
			MaxDistanceLod0Slider.onValueChanged.AddListener(OnMaxDistanceLod0ValueChanged);
			MaxDistanceLod1Slider.onValueChanged.AddListener(OnMaxDistanceLod1ValueChanged);
			DebugLodToggle.AddListenerToggle(OnDebugLodValueChanged);

			Initialize();
		}

		private void OnDisable()
		{
			RenderScaleDropdown.onValueChanged.RemoveListener(OnRenderScaleValueChanged);
			DLSSToggle.RemoteListenerToggle(OnDLSSValueChanged);
			QualityDropdown.onValueChanged.RemoveListener(OnQualityValueChanged);
			VSyncToggle.RemoteListenerToggle(OnVSyncValueChanged);
			FieldOfViewSlider.onValueChanged.RemoveListener(OnFieldOfViewValueChanged);
			MaxDistanceLod0Slider.onValueChanged.RemoveListener(OnMaxDistanceLod0ValueChanged);
			MaxDistanceLod1Slider.onValueChanged.RemoveListener(OnMaxDistanceLod1ValueChanged);
			DebugLodToggle.RemoteListenerToggle(OnDebugLodValueChanged);
		}

	

		#endregion

		#region PrivateMethods

		private void Initialize()
		{
			float scaler = QualityManager.Instance.CurrentResolutionScaler;
			switch (scaler)
			{
				case 1f:
					RenderScaleDropdown.SetValueWithoutNotify(0);
					break;
				case 0.75f:
					RenderScaleDropdown.SetValueWithoutNotify(1);
					break;
				case 0.5f:
					RenderScaleDropdown.SetValueWithoutNotify(2);
					break;
			}

			DLSSToggle.SetIsOn(QualityManager.Instance.IsDLSSActive, false);
			QualityDropdown.SetValueWithoutNotify(QualityManager.Instance.QualityLevel);
			VSyncToggle.SetIsOn(QualityManager.Instance.IsDLSSActive, false);
			FieldOfViewSlider.SetValueWithoutNotify(QualityManager.Instance.FieldOfView);
			MaxDistanceLod0Slider.SetValueWithoutNotify(QualityManager.Instance.Lod0Distance);
			MaxDistanceLod1Slider.SetValueWithoutNotify(QualityManager.Instance.Lod1Distance);
			MaxDistanceLod1Slider.minValue = QualityManager.Instance.Lod0Distance;
		}

		private void OnRenderScaleValueChanged(int index)
		{
			switch (index)
			{
				case 0: //100%
					QualityManager.Instance.SetDynamicResolution(1);
					break;
				case 1: //75%
					QualityManager.Instance.SetDynamicResolution(0.75f);
					break;
				case 2:
					QualityManager.Instance.SetDynamicResolution(0.5f);
					break;
			}
		}

		private void OnDLSSValueChanged(bool active)
		{
			QualityManager.Instance.SetDeepLearningSuperSampling(active);
		}

		private void OnQualityValueChanged(int index)
		{
			QualityManager.Instance.SetQualityLevel(index);
		}

		private void OnVSyncValueChanged(bool active)
		{
			QualityManager.Instance.SetVerticalSync(active);
		}

		private void OnFieldOfViewValueChanged(float value)
		{
			QualityManager.Instance.SetFieldOfView((int)value);
		}

		private void OnMaxDistanceLod0ValueChanged(float value)
		{
			QualityManager.Instance.SetLod0Distance((int)value);
			MaxDistanceLod1Slider.minValue = (int)value + 10;

			if (QualityManager.Instance.Lod1Distance < value + 10)
			{
				MaxDistanceLod1Slider.value = value + 10;
			}
		}

		private void OnMaxDistanceLod1ValueChanged(float value)
		{
			QualityManager.Instance.SetLod1Distance((int)value);
		}

		private void OnDebugLodValueChanged(bool value)
		{
			RuntimeVoxManager.Instance.DebugLod.Value = value;
		}
		#endregion
	}
}
