using System;
using UnityEngine;
using UnityEngine.Rendering;
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

		#endregion
	}
}
