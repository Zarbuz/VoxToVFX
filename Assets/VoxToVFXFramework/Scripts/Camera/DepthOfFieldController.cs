using System;
using UnityEngine;
using VoxToVFXFramework.Scripts.Managers;

namespace VoxToVFXFramework.Scripts.Camera
{
	public class DepthOfFieldController : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private float FocusSpeed;

		#endregion

		#region Fields

		private Ray mRay;
		private float mHitDistance;
		private bool mIsHit;
		private RaycastHit mHit;

		#endregion

		#region UnityMethods

		private void Update()
		{
			if (!QualityManager.Instance.IsDepthOfFieldActive)
			{
				return;
			}

			mRay = new Ray(transform.position, transform.forward * 100);
			if (Physics.Raycast(mRay, out mHit))
			{
				mHitDistance = Vector3.Distance(transform.position, mHit.point);
				mIsHit = true;
			}
			else
			{
				mHitDistance = 500;
				mIsHit = false;
			}
			SetFocus();
		}

		private void OnDrawGizmos()
		{
			if (mIsHit)
			{
				Gizmos.DrawSphere(mHit.point, 0.1f);
				Debug.DrawRay(transform.position, transform.forward * Vector3.Distance(transform.position, mHit.point));
			}
			else
			{
				Debug.DrawRay(transform.position, transform.forward * 100);
			}
		}

		#endregion

		#region PrivateMethods

		private void SetFocus()
		{
			float value = Mathf.Lerp(PostProcessingManager.Instance.DepthOfField.farFocusEnd.value, mHitDistance, Time.deltaTime * FocusSpeed);
			PostProcessingManager.Instance.SetDepthOfFieldFocus(value, mIsHit);
		}

		#endregion
	}
}
