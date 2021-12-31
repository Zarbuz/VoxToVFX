using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace VoxToVFXFramework.Scripts.ScriptableObjects
{
	[CreateAssetMenu(fileName = "VisualEffectConfig", menuName = "VoxToVFX/VisualEffectConfig", order = 1)]
	public class VisualEffectConfig : ScriptableObject
	{
		public int StepCapacity = 100000;
		public List<VisualEffectAsset> OpaqueVisualEffects;
		public List<VisualEffectAsset> TransparenceVisualEffects;
	}
}
