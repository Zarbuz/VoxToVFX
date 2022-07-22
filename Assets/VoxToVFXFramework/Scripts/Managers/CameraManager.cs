using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using VoxToVFXFramework.Scripts.Camera;
using VoxToVFXFramework.Scripts.Singleton;

public enum eCameraState
{
	FIRST_PERSON,
	FREE
}

public class CameraManager : ModuleSingleton<CameraManager>
{
	#region ScriptParameters

	public CinemachineVirtualCamera FirstPersonCamera;
	public CinemachineVirtualCamera FreeCamera;

	#endregion

	#region Fields

	private eCameraState mCameraState;
	private FreeCamera mFreeCamera;

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


	public int SpeedCamera { get; private set; }
	#endregion

	#region ConstStatic

	private const string SPEED_CAMERA_KEY = "SpeedCamera";

	#endregion

	#region UnityMethods

	protected override void OnStart()
	{
		CameraState = eCameraState.FIRST_PERSON;
		mFreeCamera = FreeCamera.GetComponent<FreeCamera>();
		SpeedCamera = PlayerPrefs.GetInt(SPEED_CAMERA_KEY, 10);
		SetSpeedCamera(SpeedCamera);
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

	public void SetSpeedCamera(int value)
	{
		mFreeCamera.MoveSpeed = value;
		mFreeCamera.Turbo = value * 1.2f;
		PlayerPrefs.SetInt(SPEED_CAMERA_KEY, value);
	}

	#endregion
}
