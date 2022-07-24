using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public enum eSkyboxType
	{
		Sunrise,
		Sunny,
		Foggy,
		Sunset,
		Dusk,
		Dawn
	}

	[RequireComponent(typeof(Volume))]
	public class SkyboxManager : ModuleSingleton<SkyboxManager>
	{
		#region ScriptParameters

		[SerializeField] private Texture SunriseMaterial;
		[SerializeField] private Texture SunnyMaterial;
		[SerializeField] private Texture FoggyMaterial;
		[SerializeField] private Texture SunsetMaterial;
		[SerializeField] private Texture DuskMaterial;
		[SerializeField] private Texture DawnMaterial;

		#endregion

		#region Fields

		private Volume mVolume;
		private HDRISky mHdriSky;

		#endregion

		#region UnityMethods

		protected override void OnAwake()
		{
			mVolume = GetComponent<Volume>();
			mVolume.profile.TryGet(typeof(HDRISky), out mHdriSky);
		}

		#endregion

		#region PublicMethods

		public void SetSkyboxType(eSkyboxType skyboxType)
		{
			switch (skyboxType)
			{
				case eSkyboxType.Sunrise:
					mHdriSky.hdriSky.value = SunriseMaterial;
					break;
				case eSkyboxType.Sunny:
					mHdriSky.hdriSky.value = SunnyMaterial;
					break;
				case eSkyboxType.Foggy:
					mHdriSky.hdriSky.value = FoggyMaterial;
					break;
				case eSkyboxType.Sunset:
					mHdriSky.hdriSky.value = SunsetMaterial;
					break;
				case eSkyboxType.Dawn:
					mHdriSky.hdriSky.value = DawnMaterial;
					break;
				case eSkyboxType.Dusk:
					mHdriSky.hdriSky.value = DuskMaterial;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(skyboxType), skyboxType, null);
			}
		}

		#endregion
	}
}
