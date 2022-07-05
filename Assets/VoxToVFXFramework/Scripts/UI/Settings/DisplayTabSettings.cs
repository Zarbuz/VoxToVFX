using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace VoxToVFXFramework.Scripts.UI.Settings
{
	public class DisplayTabSettings : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TMP_Dropdown ModeDropdown;
		[SerializeField] private TMP_Dropdown ResolutionDropdown;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			ModeDropdown.onValueChanged.AddListener(OnModeValueChanged);
			ResolutionDropdown.onValueChanged.AddListener(OnResolutionValueChanged);
			Initialize();
		}

		private void OnDisable()
		{
			ModeDropdown.onValueChanged.RemoveListener(OnModeValueChanged);
			ResolutionDropdown.onValueChanged.RemoveListener(OnResolutionValueChanged);
		}

		#endregion

		#region PrivateMethods

		private void Initialize()
		{
			List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
			options.Add(new TMP_Dropdown.OptionData(FullScreenMode.FullScreenWindow.ToString()));
			options.Add(new TMP_Dropdown.OptionData(FullScreenMode.Windowed.ToString()));
			ModeDropdown.options = options;
			ModeDropdown.SetValueWithoutNotify(Screen.fullScreenMode == FullScreenMode.FullScreenWindow ? 0 : 1);

			List<TMP_Dropdown.OptionData> resolutions = Screen.resolutions.Select(resolution => new TMP_Dropdown.OptionData(resolution.ToString())).ToList();
			ResolutionDropdown.options = resolutions;

			if (Screen.fullScreenMode == FullScreenMode.Windowed)
			{
				Resolution currentResolution = Screen.currentResolution;
				ResolutionDropdown.SetValueWithoutNotify(resolutions.FindIndex(r => r.text == currentResolution.ToString()));
			}
		}

		private void OnModeValueChanged(int index)
		{
			Screen.fullScreenMode = index == 0 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
		}

		private void OnResolutionValueChanged(int index)
		{
			Resolution selectedResolution = Screen.resolutions[index];
			Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreenMode);
		}

		#endregion
	}
}
