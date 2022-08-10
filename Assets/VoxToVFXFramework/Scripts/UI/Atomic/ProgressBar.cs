using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	public class ProgressBar : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI ProgressText;
		[SerializeField] private Image ProgressBarImage;

		#endregion

		#region PublicMethods

		public void SetProgress(float progress)
		{
			ProgressText.text = $"{progress.ToString("P", CultureInfo.InvariantCulture)}";
			ProgressBarImage.fillAmount = progress;
		}

		#endregion
	}
}
