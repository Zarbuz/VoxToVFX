using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class QualityManager : SimpleSingleton<QualityManager>
	{
		#region ConstStatic

		private const string RESOLUTION_SCALER = "ResolutionScaler";
		private const string DLSS_ACTIVE = "DeepLearningSuperSampling";
		private const string QUALITY_LEVEL = "QualityLevel";
		#endregion

		#region Fields

		public float CurrentResolutionScaler { get; protected set; }
		public bool IsDLSSActive { get; protected set; }
		public int QualityLevel { get; protected set; }
		#endregion

		#region PublicMethods

		protected override void Init()
		{
			CurrentResolutionScaler = PlayerPrefs.GetFloat(RESOLUTION_SCALER, 1);
			SetDynamicResolution(CurrentResolutionScaler);

			IsDLSSActive = PlayerPrefs.GetInt(DLSS_ACTIVE, 0) == 1;
			SetDeepLearningSuperSampling(IsDLSSActive);

			QualityLevel = PlayerPrefs.GetInt(QUALITY_LEVEL, 1);
			SetQualityLevel(QualityLevel);
		}

		public void SetDynamicResolution(float resolution)
		{
			CurrentResolutionScaler = resolution;
			PlayerPrefs.SetFloat(RESOLUTION_SCALER, CurrentResolutionScaler);
			DynamicResolutionHandler.SetDynamicResScaler(Scaler);
		}

		public void SetDeepLearningSuperSampling(bool active)
		{
			IsDLSSActive = active;
			PlayerPrefs.SetInt(DLSS_ACTIVE, active ? 1 : 0);
			UnityEngine.Camera.main.gameObject.GetComponent<HDAdditionalCameraData>().allowDeepLearningSuperSampling = active;
		}

		public void SetQualityLevel(int index)
		{
			QualityLevel = index;
			PlayerPrefs.SetInt(QUALITY_LEVEL, index);
			PostProcessingManager.Instance.SetQualityLevel(index);
		}

		#endregion

		#region PrivateMethods

		private float Scaler()
		{
			return CurrentResolutionScaler;
		}


		#endregion

	}
}
