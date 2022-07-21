using System;
using System.Collections;
using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Utils.Extensions;

namespace VoxToVFXFramework.Scripts.UI.Photo
{
	public class PhotoPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private CanvasGroup BackgroundImage;
		[SerializeField] private Image PanelBackground;
		[SerializeField] private Slider SpeedCameraSlider;
		[SerializeField] private Button SaveButton;
		[SerializeField] private Button CloseButton;

		#endregion

		#region Fields

		private UnityEngine.Camera mMainCamera;

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

			mMainCamera = UnityEngine.Camera.main;
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
			StartCoroutine(TakeCaptureCo());
		}

		private void OnCloseClicked()
		{
			CameraManager.Instance.SetCameraState(eCameraState.FIRST_PERSON);
			CanvasPlayerPCManager.Instance.GenericClosePanel();
			CanvasPlayerPCManager.Instance.PauseLockedState = false;
		}

		private IEnumerator TakeCaptureCo()
		{
			BackgroundImage.alpha = 0.7f;
			StartCoroutine(BackgroundImage.AlphaFade(0f, .2f));
			PanelBackground.gameObject.SetActive(false);
			yield return new WaitForSeconds(0.8f);

			yield return new WaitForEndOfFrame();

			RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
			mMainCamera.targetTexture = rt;
			Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
			RenderTexture.active = rt;
			mMainCamera.Render();
			screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			mMainCamera.targetTexture = null;
			screenShot.Apply(false, false);
			RenderTexture.active = null;
			Destroy(rt);

			PanelBackground.gameObject.SetActive(true);

			yield return new WaitForEndOfFrame();
			string path = StandaloneFileBrowser.SaveFilePanel("Save capture", "", "", "png");
			if (path != null)
			{
				byte[] bytes = screenShot.EncodeToPNG();
				File.WriteAllBytes(path, bytes);
			}

		}

		#endregion
	}
}
