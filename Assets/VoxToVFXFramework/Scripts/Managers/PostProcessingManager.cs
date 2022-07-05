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

		private Volume mVolume;

		#endregion

		#region UnityMethods

		protected override void OnAwake()
		{
			mVolume = GetComponent<Volume>();
		}

		#endregion

		#region PublicMethods

		public void SetEdgePostProcess(float intensity, Color color)
		{
			if (mVolume.sharedProfile.TryGet(typeof(Sobel), out Sobel sobel))
			{
				sobel.intensity.value = Mathf.Clamp01(intensity);
				sobel.outlineColour.value = color;
			}

		}

		public void SetDepthOfField(bool active)
		{
			if (mVolume.sharedProfile.TryGet(typeof(DepthOfField), out DepthOfField depthOfField))
			{
				depthOfField.active = active;
			}
		}

		public void SetQualityLevel(int index)
		{
			ScalableSettingLevelParameter scalableSettingLevelParameter;
			switch (index)
			{
				case 0:
					scalableSettingLevelParameter = new ScalableSettingLevelParameter((int)ScalableSettingLevelParameter.Level.High, false);
					break;
				case 1:
					scalableSettingLevelParameter = new ScalableSettingLevelParameter((int)ScalableSettingLevelParameter.Level.Medium, false);
					break;
				case 2:
					scalableSettingLevelParameter = new ScalableSettingLevelParameter((int)ScalableSettingLevelParameter.Level.Low, false);
					break;
				default:
					scalableSettingLevelParameter = new ScalableSettingLevelParameter((int)ScalableSettingLevelParameter.Level.High, false);
					break;

			}

			if (mVolume.sharedProfile.TryGet(typeof(AmbientOcclusion), out AmbientOcclusion ambientOcclusion))
			{
				ambientOcclusion.quality = scalableSettingLevelParameter;
			}

			if (mVolume.sharedProfile.TryGet(typeof(Fog), out Fog fog))
			{
				fog.quality = scalableSettingLevelParameter;
			}

			if (mVolume.sharedProfile.TryGet(typeof(GlobalIllumination), out GlobalIllumination globalIllumination))
			{
				globalIllumination.quality = scalableSettingLevelParameter;
			}

			if (mVolume.sharedProfile.TryGet(typeof(Bloom), out Bloom bloom))
			{
				bloom.quality = scalableSettingLevelParameter;
			}

			if (mVolume.sharedProfile.TryGet(typeof(ScreenSpaceReflection), out ScreenSpaceReflection screenSpaceReflection))
			{
				screenSpaceReflection.quality = scalableSettingLevelParameter;
			}

			if (mVolume.sharedProfile.TryGet(typeof(DepthOfField), out DepthOfField depthOfField))
			{
				depthOfField.quality = scalableSettingLevelParameter;
			}
		}

		#endregion


	}
}
