using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using VoxToVFXFramework.Scripts.Singleton;

public enum eCameraState
{
	FIRST_PERSON,
	FREE
}

public class CameraManager : ModuleSingleton<CameraManager>
{
	#region ScriptParameters

	[SerializeField] private CinemachineVirtualCamera FirstPersonCamera;
	[SerializeField] private CinemachineVirtualCamera FreeCamera;

	#endregion

	#region Fields

	private eCameraState mCameraState;

	public eCameraState CameraState
	{
		get => mCameraState;
		set
		{
			mCameraState = value;
			FirstPersonCamera.gameObject.SetActive(mCameraState == eCameraState.FIRST_PERSON);
			FreeCamera.gameObject.SetActive(mCameraState == eCameraState.FREE);
		}
	}

	#endregion

	#region UnityMethods

	protected override void OnStart()
	{
		CameraState = eCameraState.FIRST_PERSON;
	}

	#endregion

	#region PublicMethods

	public void SetCameraState(eCameraState cameraState)
	{
		CameraState = cameraState;
		if (cameraState == eCameraState.FREE)
		{
			FreeCamera.transform.position = FirstPersonCamera.transform.position + Vector3.up;
		}
	}

	public void SetFieldOfView(int value)
	{
		FreeCamera.m_Lens.FieldOfView = value;
		FirstPersonCamera.m_Lens.FieldOfView = value;
	}

	public void SetRenderDistance(int distance)
	{
		FreeCamera.m_Lens.FarClipPlane = distance;
		FirstPersonCamera.m_Lens.FarClipPlane = distance;
	}

	#endregion
}
