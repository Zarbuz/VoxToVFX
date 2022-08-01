using UnityEngine;
using UnityEngine.Serialization;

namespace VoxToVFXFramework.Scripts.Utils.Misc
{
	public class Rotator : MonoBehaviour
	{
		#region ConstStatic

		// Used to avoid jumping too hard when the rotation is stopped for a long amount of time
		private const float MAX_JUMP = 1f / 30f;

		#endregion

		#region ScriptParameters

		public Space Space;

		[FormerlySerializedAs("RotationSpeed")]
		public Vector3 RotationSpeedMax;

		public Vector3 RotationSpeedMin;

		#endregion

		#region Fields

		private Vector3 mRotationSpeed;

		#endregion

		#region UnityMethods

		private void Start()
		{
			if (RotationSpeedMin == Vector3.zero)
			{
				RotationSpeedMin = RotationSpeedMax;
			}

			mRotationSpeed = new Vector3(Random.Range(RotationSpeedMin.x, RotationSpeedMax.x), Random.Range(RotationSpeedMin.y, RotationSpeedMax.y), Random.Range(RotationSpeedMin.z, RotationSpeedMax.z));
		}

		private void Update()
		{
			transform.Rotate(mRotationSpeed * Mathf.Min(Time.deltaTime, MAX_JUMP), Space);
		}

		#endregion
	}
}
