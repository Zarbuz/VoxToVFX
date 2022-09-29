using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Com.TheFallenGames.OSA.Core.Data.Gallery
{
	/// <summary>
	/// Parameters for a gallery effect
	/// </summary>
	[Serializable]
	public class GalleryEffectParams
	{
		[Range(0f, 1f)]
		[SerializeField]
		[Tooltip("The amount of the gallery effect itself, independent of the amounts of individual animation types. 0=disabled")]
		float _OverallAmount = 0f;
		/// <summary>
		/// The amount of the gallery effect itself, independent of the amounts of individual animation types. 0=disabled
		/// </summary>
		public float OverallAmount { get { return _OverallAmount; } set { _OverallAmount = value; } }

		[SerializeField]
		GalleryScale _Scale = new GalleryScale();
		public GalleryScale Scale { get { return _Scale; } set { _Scale = value; } }

		[SerializeField]
		GalleryRotation _Rotation = new GalleryRotation();
		public GalleryRotation Rotation { get { return _Rotation; } set { _Rotation = value; } }
	}
}
