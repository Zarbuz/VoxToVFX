using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace VoxToVFXFramework.Scripts.ScriptableObjets
{
	[CreateAssetMenu(fileName = "VFXListAsset", menuName = "VoxToVFX/VFXListAsset")]
	public class VFXListAsset : ScriptableObject
	{
		public bool ForceVisualAsset;
		public VisualEffectAsset ForceVisualEffectAsset;
		public List<VisualEffectAsset> VisualEffectAssets;
	}
}
