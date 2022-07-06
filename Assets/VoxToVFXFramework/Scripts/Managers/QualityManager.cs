using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class QualityManager : SimpleSingleton<QualityManager>
	{
		#region ConstStatic

		private const string RESOLUTION_SCALER_KEY = "ResolutionScaler";
		private const string DLSS_ACTIVE_KEY = "DeepLearningSuperSampling";
		private const string QUALITY_LEVEL_KEY = "QualityLevel";
		private const string VSYNC_ACTIVE_KEY = "VSync";
		private const string FOV_VALUE_KEY = "FieldOfView";
		#endregion

		#region Fields

		public float CurrentResolutionScaler { get; protected set; }
		public bool IsDLSSActive { get; protected set; }
		public int QualityLevel { get; protected set; }
		public bool IsVSyncActive { get; protected set; }
		public int FieldOfView { get; protected set; }
		#endregion

		#region PublicMethods

		protected override void Init()
		{
			CurrentResolutionScaler = PlayerPrefs.GetFloat(RESOLUTION_SCALER_KEY, 1);
			SetDynamicResolution(CurrentResolutionScaler);

			IsDLSSActive = PlayerPrefs.GetInt(DLSS_ACTIVE_KEY, 0) == 1;
			SetDeepLearningSuperSampling(IsDLSSActive);

			QualityLevel = PlayerPrefs.GetInt(QUALITY_LEVEL_KEY, 1);
			SetQualityLevel(QualityLevel);

			IsVSyncActive = PlayerPrefs.GetInt(VSYNC_ACTIVE_KEY, 0) == 1;
			SetVerticalSync(IsVSyncActive);

			FieldOfView = PlayerPrefs.GetInt(FOV_VALUE_KEY, 60);
			SetFieldOfView(FieldOfView);
		}

		public void SetDynamicResolution(float resolution)
		{
			CurrentResolutionScaler = resolution;
			PlayerPrefs.SetFloat(RESOLUTION_SCALER_KEY, CurrentResolutionScaler);
			DynamicResolutionHandler.SetDynamicResScaler(Scaler);
		}

		public void SetDeepLearningSuperSampling(bool active)
		{
			IsDLSSActive = active;
			PlayerPrefs.SetInt(DLSS_ACTIVE_KEY, active ? 1 : 0);
			UnityEngine.Camera.main.gameObject.GetComponent<HDAdditionalCameraData>().allowDeepLearningSuperSampling = active;
		}

		public void SetQualityLevel(int index)
		{
			QualityLevel = index;
			PlayerPrefs.SetInt(QUALITY_LEVEL_KEY, index);
			PostProcessingManager.Instance.SetQualityLevel(index);
		}

		public void SetVerticalSync(bool active)
		{
			IsVSyncActive = active;
			PlayerPrefs.SetInt(VSYNC_ACTIVE_KEY, active ? 1 : 0);
			QualitySettings.vSyncCount = active ? 1 : 0;
		}

		public void SetFieldOfView(int value)
		{
			FieldOfView = value;
			PlayerPrefs.SetInt(FOV_VALUE_KEY, value);
			UnityEngine.Camera.main.fieldOfView = value;
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
