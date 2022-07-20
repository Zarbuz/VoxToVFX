using System;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;

namespace VoxToVFXFramework.Scripts.UI.Weather
{
	public class WeatherPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Button SunriseButton;
		[SerializeField] private Button SunnyButton;
		[SerializeField] private Button FoggyButton;
		[SerializeField] private Button SunsetButton;
		[SerializeField] private Button DuskButton;
		[SerializeField] private Button DawnButton;

		[SerializeField] private Button CloseButton;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			SunriseButton.onClick.AddListener(() => OnSkyboxClicked(eSkyboxType.Sunrise));
			SunnyButton.onClick.AddListener(() => OnSkyboxClicked(eSkyboxType.Sunny));
			FoggyButton.onClick.AddListener(() => OnSkyboxClicked(eSkyboxType.Foggy));
			SunsetButton.onClick.AddListener(() => OnSkyboxClicked(eSkyboxType.Sunset));
			DuskButton.onClick.AddListener(() => OnSkyboxClicked(eSkyboxType.Dusk));
			DawnButton.onClick.AddListener(() => OnSkyboxClicked(eSkyboxType.Dawn));

			CloseButton.onClick.AddListener(OnCloseClicked);
		}

		private void OnDisable()
		{
			SunriseButton.onClick.RemoveAllListeners();
			SunnyButton.onClick.RemoveAllListeners();
			FoggyButton.onClick.RemoveAllListeners();
			SunsetButton.onClick.RemoveAllListeners();
			DawnButton.onClick.RemoveAllListeners();
			DuskButton.onClick.RemoveAllListeners();
			CloseButton.onClick.RemoveListener(OnCloseClicked);
		}

		#endregion

		#region PrivateMethods

		private void OnSkyboxClicked(eSkyboxType skyboxType)
		{
			SkyboxManager.Instance.SetSkyboxType(skyboxType);
		}

		private void OnCloseClicked()
		{
			CanvasPlayerPCManager.Instance.GenericClosePanel();
		}

		#endregion
	}
}
