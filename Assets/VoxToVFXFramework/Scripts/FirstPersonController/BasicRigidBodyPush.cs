using UnityEngine;

namespace VoxToVFXFramework.Scripts.FirstPersonController
{
	public class BasicRigidBodyPush : MonoBehaviour
	{
		#region ScriptParameters

		public LayerMask PushLayers;
		public bool CanPush;
		[Range(0.5f, 5f)] public float Strength = 1.1f;

		#endregion

		#region UnityMethods

		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			if (CanPush) PushRigidBodies(hit);
		}

		#endregion

		#region PrivateMethods

		private void PushRigidBodies(ControllerColliderHit hit)
		{
			// https://docs.unity3d.com/ScriptReference/CharacterController.OnControllerColliderHit.html

			// make sure we hit a non kinematic rigidbody
			Rigidbody body = hit.collider.attachedRigidbody;
			if (body == null || body.isKinematic) return;

			// make sure we only push desired layer(s)
			var bodyLayerMask = 1 << body.gameObject.layer;
			if ((bodyLayerMask & PushLayers.value) == 0) return;

			// We dont want to push objects below us
			if (hit.moveDirection.y < -0.3f) return;

			// Calculate push direction from move direction, horizontal motion only
			Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

			// Apply the push and take strength into account
			body.AddForce(pushDir * Strength, ForceMode.Impulse);
		}

		#endregion
	}
}