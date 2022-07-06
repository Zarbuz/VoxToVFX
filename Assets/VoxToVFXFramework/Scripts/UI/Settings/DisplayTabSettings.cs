using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using VoxToVFXFramework.Scripts.Localization;

namespace VoxToVFXFramework.Scripts.UI.Settings
{
	public class DisplayTabSettings : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TMP_Dropdown ModeDropdown;
		[SerializeField] private TMP_Dropdown ResolutionDropdown;
		[SerializeField] private TMP_Dropdown LanguageDropdown;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			ModeDropdown.onValueChanged.AddListener(OnModeValueChanged);
			ResolutionDropdown.onValueChanged.AddListener(OnResolutionValueChanged);
			LanguageDropdown.onValueChanged.AddListener(OnLanguageValueChanged);
			Initialize();
		}

		private void OnDisable()
		{
			ModeDropdown.onValueChanged.RemoveListener(OnModeValueChanged);
			ResolutionDropdown.onValueChanged.RemoveListener(OnResolutionValueChanged);
			LanguageDropdown.onValueChanged.RemoveListener(OnLanguageValueChanged);
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

			List<TMP_Dropdown.OptionData> resolutions = Screen.resolutions
				.Select(resolution => new TMP_Dropdown.OptionData(resolution.ToString())).ToList();
			ResolutionDropdown.options = resolutions;

			if (Screen.fullScreenMode == FullScreenMode.Windowed)
			{
				Resolution currentResolution = Screen.currentResolution;
				ResolutionDropdown.SetValueWithoutNotify(resolutions.FindIndex(r =>
					r.text == currentResolution.ToString()));
			}

			LanguageDropdown.SetValueWithoutNotify(GetLanguageIndex());
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

		private void OnLanguageValueChanged(int index)
		{
			string newLanguage = GetLanguage(index);
			LocalizationManager.Instance.SwitchLocalization(newLanguage);
		}

		private int GetLanguageIndex()
		{
			string currentLanguage = LocalizationManager.Instance.CurrentLanguage;
			switch (currentLanguage)
			{
				case "fr":
					return 0;
				case "en":
					return 1;
				default:
					return 0;
			}
		}

		private string GetLanguage(int index)
		{
			switch (index)
			{
				case 0:
					return "fr";
				case 1:
					return "en";
				default:
					return "fr";
			}
		}

		#endregion
	}
}
