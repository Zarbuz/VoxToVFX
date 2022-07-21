using System;
using UnityEngine;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI.Photo
{
	public class PhotoPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Slider SpeedCameraSlider;
		[SerializeField] private Button SaveButton;
		[SerializeField] private Button CloseButton;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			SpeedCameraSlider.onValueChanged.AddListener(OnSpeedCameraValueChanged);
			SaveButton.onClick.AddListener(OnSaveClicked);
			CloseButton.onClick.AddListener(OnCloseClicked);
			CameraManager.Instance.SetCameraState(eCameraState.FREE);
			CanvasPlayerPCManager.Instance.PauseLockedState = true;
			SpeedCameraSlider.SetValueWithoutNotify(CameraManager.Instance.SpeedCamera);
		}

		private void OnDisable()
		{
			SpeedCameraSlider.onValueChanged.RemoveListener(OnSpeedCameraValueChanged);

			SaveButton.onClick.RemoveListener(OnSaveClicked);
			CloseButton.onClick.RemoveListener(OnCloseClicked);
		}

		#endregion

		#region PrivateMethods

		private void OnSpeedCameraValueChanged(float value)
		{
			CameraManager.Instance.SetSpeedCamera((int)value);
		}

		private void OnSaveClicked()
		{

		}

		private void OnCloseClicked()
		{
			CameraManager.Instance.SetCameraState(eCameraState.FIRST_PERSON);
			CanvasPlayerPCManager.Instance.GenericClosePanel();
			CanvasPlayerPCManager.Instance.PauseLockedState = false;
		}

		#endregion
	}
}
