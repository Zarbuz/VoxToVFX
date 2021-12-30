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

		[SerializeField] private TextMeshProUGUI ChunkLoadDistanceText;
		[SerializeField] private Slider ChunkLoadDistanceSlider;

		[SerializeField] private TextMeshProUGUI LoadDetailDistanceText;
		[SerializeField] private Slider DetailLoadDistanceSlider;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			ChunkLoadDistanceSlider.onValueChanged.AddListener(OnChunkLoadDistanceValueChanged);
			DetailLoadDistanceSlider.onValueChanged.AddListener(OnDetailLoadDistanceValueChanged);
			RefreshSettings();
		}

		private void OnDisable()
		{
			ChunkLoadDistanceSlider.onValueChanged.RemoveListener(OnChunkLoadDistanceValueChanged);
			DetailLoadDistanceSlider.onValueChanged.RemoveListener(OnDetailLoadDistanceValueChanged);
		}

		#endregion

		#region PrivateMethods

		private void RefreshSettings()
		{
			ChunkLoadDistanceText.text = "Chunk Load Distance: " + RuntimeVoxManager.Instance.ChunkLoadDistance;
			ChunkLoadDistanceSlider.SetValueWithoutNotify(RuntimeVoxManager.Instance.ChunkLoadDistance);

			LoadDetailDistanceText.text = "Load Details Distance: " + RuntimeVoxManager.Instance.DetailLoadDistance;
			DetailLoadDistanceSlider.SetValueWithoutNotify(RuntimeVoxManager.Instance.DetailLoadDistance);
		}

		private void OnChunkLoadDistanceValueChanged(float value)
		{
			ChunkLoadDistanceText.text = "Chunk Load Distance: " + value;
			RuntimeVoxManager.Instance.SetChunkLoadDistance((int)value);
		}

		private void OnDetailLoadDistanceValueChanged(float value)
		{
			LoadDetailDistanceText.text = "Load Details Distance: " + value;
			RuntimeVoxManager.Instance.DetailLoadDistance = ((int)value);
		}

		#endregion
	}
}
