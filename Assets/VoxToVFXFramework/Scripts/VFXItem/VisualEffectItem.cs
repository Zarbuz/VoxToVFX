using UnityEngine;
using UnityEngine.VFX;

namespace VoxToVFXFramework.Scripts.VFXItem
{
	public class VisualEffectItem : MonoBehaviour
	{
		public Vector3 FramePosition;
		public int InitialBurstLod0;
		public int InitialBurstLod1;
		public int InitialBurstLod2;
		public int InitialBurstLod3;
		public VisualEffect OpaqueVisualEffect;
		public int CurrentLod;
	}
}
