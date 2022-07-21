using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.Utils;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class QualityManager : SimpleSingleton<QualityManager>
	{
		#region ConstStatic

		private const string RESOLUTION_SCALER_KEY = "ResolutionScaler";
		private const string DLSS_ACTIVE_KEY = "DeepLearningSuperSampling";
		private const string QUALITY_LEVEL_KEY = "QualityLevel";
		private const string SHADOW_QUALITY_LEVEL_KEY = "ShadowQualityLevel";
		private const string VSYNC_ACTIVE_KEY = "VSync";
		private const string FOV_VALUE_KEY = "FieldOfView";
		private const string LOD_0_DISTANCE_KEY = "Lod0";
		private const string LOD_1_DISTANCE_KEY = "Lod1";
		private const string DEPTH_OF_FIELD_KEY = "DepthOfField";
		private const string RENDER_DISTANCE_KEY = "RenderDistance";
		#endregion

		#region Fields

		public float CurrentResolutionScaler { get; protected set; }
		public bool IsDLSSActive { get; protected set; }
		public int QualityLevel { get; protected set; }
		public int ShadowQualityLevel { get; protected set; }
		public bool IsVSyncActive { get; protected set; }
		public int FieldOfView { get; protected set; }
		public int Lod0Distance { get; protected set; }
		public int Lod1Distance { get; protected set; }
		public bool IsDepthOfFieldActive { get; protected set; }
		public int RenderDistance { get; protected set; }

		private HDAdditionalLightData mDirectionalLight;
		#endregion

		#region PublicMethods

		public void Initialize()
		{
			mDirectionalLight = SceneUtils.FindObjectOfType<HDAdditionalLightData>();

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

			Lod0Distance = PlayerPrefs.GetInt(LOD_0_DISTANCE_KEY, 115);
			SetLod0Distance(Lod0Distance);

			Lod1Distance = PlayerPrefs.GetInt(LOD_1_DISTANCE_KEY, 300);
			SetLod1Distance(Lod1Distance);

			ShadowQualityLevel = PlayerPrefs.GetInt(SHADOW_QUALITY_LEVEL_KEY, 1);
			SetShadowQualityLevel(ShadowQualityLevel);

			IsDepthOfFieldActive = PlayerPrefs.GetInt(DEPTH_OF_FIELD_KEY, 0) == 1;
			SetDepthOfField(IsDepthOfFieldActive);

			RenderDistance = PlayerPrefs.GetInt(RENDER_DISTANCE_KEY, 2500);
			SetRenderDistance(RenderDistance);
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

		public void SetShadowQualityLevel(int index)
		{
			ShadowQualityLevel = index;
			PlayerPrefs.SetInt(SHADOW_QUALITY_LEVEL_KEY, index);
			switch (index)
			{
				case 0:
					mDirectionalLight.SetShadowResolutionLevel(2);
					break;
				case 1:
					mDirectionalLight.SetShadowResolutionLevel(1);
					break;
				case 2:
					mDirectionalLight.SetShadowResolutionLevel(0);
					break;
				default:
					mDirectionalLight.SetShadowResolutionLevel(1);
					break;
			}
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
			CameraManager.Instance.SetFieldOfView(value);
			RuntimeVoxManager.Instance.RefreshChunksToRender();
		}

		public void SetLod0Distance(int value)
		{
			Lod0Distance = value;
			PlayerPrefs.SetInt(LOD_0_DISTANCE_KEY, value);
			RuntimeVoxManager.Instance.LodDistanceLod0.Value = value;
		}

		public void SetLod1Distance(int value)
		{
			Lod1Distance = value;
			PlayerPrefs.SetInt(LOD_1_DISTANCE_KEY, value);
			RuntimeVoxManager.Instance.LodDistanceLod1.Value = value;
		}

		public void SetDepthOfField(bool active)
		{
			IsDepthOfFieldActive = active;
			PlayerPrefs.SetInt(DEPTH_OF_FIELD_KEY, active ? 1 : 0);
		}

		public void SetRenderDistance(int distance)
		{
			RenderDistance = distance;
			PlayerPrefs.SetInt(RENDER_DISTANCE_KEY, distance);
			CameraManager.Instance.SetRenderDistance(distance);
			RuntimeVoxManager.Instance.RefreshChunksToRender();
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
