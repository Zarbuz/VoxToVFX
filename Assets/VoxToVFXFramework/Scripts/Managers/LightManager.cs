using UnityEngine;
using VoxToVFXFramework.Scripts.Singleton;

public class LightManager : ModuleSingleton<LightManager>
{
	#region Fields

	private Light mLight;

	#endregion
	#region UnityMethods

	protected override void OnAwake()
	{
		mLight = FindObjectOfType<Light>();
	}

	#endregion

	#region PublicMethods

	public Vector3 GetCurrentRotation()
	{
		return mLight.transform.localEulerAngles;
	}

	public void SetLightXRotation(float angle)
	{
		Vector3 eulerAngles = mLight.transform.localEulerAngles;
		eulerAngles.x = angle;
		mLight.transform.localEulerAngles = eulerAngles;
	}

	public void SetLightYRotation(float angle)
	{
		Vector3 eulerAngles = mLight.transform.localEulerAngles;
		eulerAngles.y = angle;
		mLight.transform.localEulerAngles = eulerAngles;
	}
	#endregion
}
