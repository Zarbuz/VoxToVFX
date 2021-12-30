using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI
{
	public class CanvasPlayerPCManager : ModuleSingleton<CanvasPlayerPCManager>
	{
		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI LoadingProgressText;
		[SerializeField] private Slider ChunkLoadDistanceSlider;
		[SerializeField] private Slider DetailLoadDistanceSlider;

		#endregion
	}
}
