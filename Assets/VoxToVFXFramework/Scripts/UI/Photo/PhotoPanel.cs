using System;
using UnityEngine;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI.Photo
{
	public class PhotoPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Button SaveButton;
		[SerializeField] private Button CloseButton;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			SaveButton.onClick.AddListener(OnSaveClicked);
			CloseButton.onClick.AddListener(OnCloseClicked);
			CameraManager.Instance.SetCameraState(eCameraState.FREE);
			CanvasPlayerPCManager.Instance.PauseLockedState = true;
		}

		private void OnDisable()
		{
			SaveButton.onClick.RemoveListener(OnSaveClicked);
			CloseButton.onClick.RemoveListener(OnCloseClicked);
		}

		#endregion

		#region PrivateMethods

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
