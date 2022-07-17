using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	[RequireComponent(typeof(Volume))]
	public class PostProcessingManager : ModuleSingleton<PostProcessingManager>
	{
		#region Fields

		public DepthOfField DepthOfField;

		private Volume mVolume;

		#endregion

		#region UnityMethods

		protected override void OnAwake()
		{
			mVolume = GetComponent<Volume>();
			mVolume.profile.TryGet(typeof(DepthOfField), out DepthOfField);
		}

		#endregion

		#region PublicMethods

		public void SetEdgePostProcess(float intensity, Color color)
		{
			if (mVolume.profile.TryGet(typeof(Sobel), out Sobel sobel))
			{
				sobel.intensity.value = Mathf.Clamp01(intensity);
				sobel.outlineColour.value = color;
			}
		}

		public void SetDepthOfField(bool active)
		{
			DepthOfField.active = active;
		}

		public void SetDepthOfFieldFocus(float distance, bool isHit)
		{
			DepthOfField.active = true;
			DepthOfField.farFocusEnd.value = distance;
			DepthOfField.nearFocusEnd.value = isHit ? 0.2f : 6f;
			DepthOfField.focusMode = new DepthOfFieldModeParameter(DepthOfFieldMode.Manual);
		}

		public void SetQualityLevel(int index)
		{
			ScalableSettingLevelParameter scalableSettingLevelParameter;
			switch (index)
			{
				case 0:
					scalableSettingLevelParameter = new ScalableSettingLevelParameter((int)ScalableSettingLevelParameter.Level.High, false, true);
					break;
				case 1:
					scalableSettingLevelParameter = new ScalableSettingLevelParameter((int)ScalableSettingLevelParameter.Level.Medium, false, true);
					break;
				case 2:
					scalableSettingLevelParameter = new ScalableSettingLevelParameter((int)ScalableSettingLevelParameter.Level.Low, false, true);
					break;
				default:
					scalableSettingLevelParameter = new ScalableSettingLevelParameter((int)ScalableSettingLevelParameter.Level.High, false, true);
					break;

			}

			if (mVolume.profile.TryGet(typeof(AmbientOcclusion), out AmbientOcclusion ambientOcclusion))
			{
				ambientOcclusion.quality = scalableSettingLevelParameter;
			}

			if (mVolume.profile.TryGet(typeof(Fog), out Fog fog))
			{
				fog.quality = scalableSettingLevelParameter;
			}

			if (mVolume.profile.TryGet(typeof(GlobalIllumination), out GlobalIllumination globalIllumination))
			{
				globalIllumination.quality = scalableSettingLevelParameter;
			}

			if (mVolume.profile.TryGet(typeof(Bloom), out Bloom bloom))
			{
				bloom.quality = scalableSettingLevelParameter;
			}

			if (mVolume.profile.TryGet(typeof(ScreenSpaceReflection), out ScreenSpaceReflection screenSpaceReflection))
			{
				screenSpaceReflection.quality = scalableSettingLevelParameter;
			}

			if (mVolume.profile.TryGet(typeof(DepthOfField), out DepthOfField depthOfField))
			{
				depthOfField.quality = scalableSettingLevelParameter;
			}
		}

		#endregion


	}
}
