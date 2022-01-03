using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;

namespace VoxToVFXFramework.Scripts.UI.Settings
{
	public class SettingsPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI LoadDetailDistanceText;
		[SerializeField] private Slider DetailLoadDistanceSlider;

		[SerializeField] private TextMeshProUGUI CutOfMarginText;
		[SerializeField] private Slider CutOfMarginSlider;


		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			CutOfMarginSlider.onValueChanged.AddListener(OnCutOfMarginValueChanged);
			DetailLoadDistanceSlider.onValueChanged.AddListener(OnDetailLoadDistanceValueChanged);
			RefreshSettings();
		}

		private void OnDisable()
		{
			CutOfMarginSlider.onValueChanged.RemoveListener(OnCutOfMarginValueChanged);
			DetailLoadDistanceSlider.onValueChanged.RemoveListener(OnDetailLoadDistanceValueChanged);
		}

		#endregion

		#region PrivateMethods

		private void RefreshSettings()
		{
			CutOfMarginText.text = "Cut of margin: " + RuntimeVoxManager.Instance.CutOfMargin;
			CutOfMarginSlider.SetValueWithoutNotify(RuntimeVoxManager.Instance.CutOfMargin);

			LoadDetailDistanceText.text = "Load Details Distance: " + RuntimeVoxManager.Instance.DetailLoadDistance;
			DetailLoadDistanceSlider.SetValueWithoutNotify(RuntimeVoxManager.Instance.DetailLoadDistance);

		}

		private void OnCutOfMarginValueChanged(float value)
		{
			CutOfMarginText.text = "Cut of margin: " + value;
			RuntimeVoxManager.Instance.CutOfMargin = ((int)value);
		}

		private void OnDetailLoadDistanceValueChanged(float value)
		{
			LoadDetailDistanceText.text = "Load Details Distance: " + value;
			RuntimeVoxManager.Instance.DetailLoadDistance = ((int)value);
		}


		#endregion
	}
}
