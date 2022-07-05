using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;

namespace VoxToVFXFramework.Scripts.UI.Settings
{
	public class GraphicsTabSettings : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TMP_Dropdown RenderScaleDropdown;
		[SerializeField] private TMP_Dropdown QualityDropdown;
		[SerializeField] private Slider FieldOfViewSlider;
		[SerializeField] private Toggle DepthOfFieldToggle;
		[SerializeField] private Toggle DLSSToggle;
		[SerializeField] private Toggle VSyncToggle;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			RenderScaleDropdown.onValueChanged.AddListener(OnRenderScaleValueChanged);
			DLSSToggle.onValueChanged.AddListener(OnDLSSValueChanged);
			Initialize();
		}

		private void OnDisable()
		{
			RenderScaleDropdown.onValueChanged.RemoveListener(OnRenderScaleValueChanged);
			DLSSToggle.onValueChanged.RemoveListener(OnDLSSValueChanged);
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

			DLSSToggle.SetIsOnWithoutNotify(QualityManager.Instance.IsDLSSActive);
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

		#endregion
	}
}
