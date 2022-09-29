using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Com.TheFallenGames.OSA.Core.Data.Gallery
{
	/// <summary>
	/// See <see cref="GalleryAnimation"/>
	/// </summary>
	[Serializable]
	public class GalleryScale : GalleryAnimation
	{
		[Range(0f, 1f)]
		[SerializeField]
		[FormerlySerializedAs("_MinScale")]
		[Tooltip("No item will have its scale smaller than this")]
		float _MinValue = 0f;
		/// <summary>No item will have its scale smaller than this</summary>
		public float MinValue { get { return _MinValue; } set { _MinValue = value; } }

		[SerializeField]
		[Tooltip("From which to which value to interpolate the scale, per component")]
		[FormerlySerializedAs("_ScaleSpace")]
		Vector3Space _TransformSpace = new Vector3Space(Vector3.zero, Vector3.one);
		/// <summary>From which to which value to interpolate the scale, per component</summary>
		public override Vector3Space TransformSpace { get { return _TransformSpace; } set { _TransformSpace = value; } }


		public GalleryScale()
		{
			// Keeping older versions of OSA somewhat compatible. Since the older GalleryEffectAmount will be assigned to the GalleryEffectParams.OverallAmount, 
			// setting this to 1 would result in the same final value for the scale effect (they're multiplied)
			Amount = 1f;
		}
	}
}
