using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using UnityEngine;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Localization
{
	[Serializable]
	public class LocalizationData
	{
		public List<LocalizationItem> Items;
	}

	[Serializable]
	public class LocalizationItem
	{
		public string Key;
		public string Value;

		public LocalizationItem()
		{
		}

		public LocalizationItem(KeyValuePair<string, string> item)
		{
			Key = item.Key;
			Value = item.Value;
		}
	}

	public class LocalizationManager : SimpleSingleton<LocalizationManager>
	{
		#region ConstStatic

		public const string DEFAULT_LANGUAGE = "en";
		public const string SAVE_LANGUAGE_PLAYER_PREFS_KEY = "SavedLanguage";
		private const string LOG_HEADER = "[LocalizationManager]";

		public static bool DoForceOtherLanguage = false;
		public static string ForcedLanguage = "en";

		#endregion

		#region Fields

		public bool IsReady { get; private set; }
		public string CurrentLanguage { get; private set; }

		public CultureInfo CurrentCultureInfo => new CultureInfo(GetCultureName());
		public Dictionary<string, string> DictionaryText { get; private set; } = new Dictionary<string, string>();

		private string mCurrentFilename;
		private bool mIsInit;

		private readonly List<LocalizedField> mListLocalizedText = new List<LocalizedField>();
		private const string FILENAME = "localizedText";

		#endregion

		#region PublicMethods

		protected override void Init()
		{
			if (mIsInit)
			{
				Debug.LogWarning("[Localization] Init has already been called at least once");
			}
			mIsInit = true;

			if (DoForceOtherLanguage)
			{
				CurrentLanguage = ForcedLanguage;
				Debug.Log("[Localization] ForceLang: " + CurrentLanguage);
			}
			else
			{
				string languagePlayerPrefs = PlayerPrefs.GetString(SAVE_LANGUAGE_PLAYER_PREFS_KEY, string.Empty);
				if (!string.IsNullOrEmpty(languagePlayerPrefs))
				{
					CurrentLanguage = languagePlayerPrefs;
					Debug.Log("[Localization] PlayerPrefs: " + CurrentLanguage);
				}
				else
				{
					CurrentLanguage = ConvertFromSystemLanguage(Application.systemLanguage);
					Debug.Log("[Localization] prefix " + CurrentLanguage + " lang " + Application.systemLanguage
					          + " two letters previous code " + CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
				}
			}

			mCurrentFilename = GetFolder() + FILENAME + "_" + CurrentLanguage;
			Debug.LogFormat("[Localization] {0}: load file {1}", LOG_HEADER, mCurrentFilename);
			DownloadFile();
		}

		public string GetCultureName()
		{
			//http://www.codedigest.com/CodeDigest/207-Get-All-Language-Country-Code-List-for-all-Culture-in-C---ASP-Net.aspx
			switch (CurrentLanguage)
			{
				case "en":
					return "en-US";
				case "fr":
					return "fr-FR";
				//case "zh":
				//	return "zh-CN";
				//case "de":
				//	return "de-DE";
				//case "es":
				//	return "es-VE";
				//case "ko":
				//	return "ko-KR";
				//case "it":
				//	return "it-IT";
				//case "ja":
				//	return "ja-JP";
				//case "ru":
				//	return "ru-RU";
				default:
					return "en-US";
			}
		}

		public void AssignToManager(LocalizedField text)
		{
			if (!mListLocalizedText.Contains(text))
			{
				mListLocalizedText.Add(text);
			}
		}

		public string GetValue(string key, params (string key, object value)[] variables)
		{
			if (DictionaryText.TryGetValue(key, out string text))
			{
				foreach (var variable in variables)
				{
					text = text.Replace("{" + variable.key + "}", variable.value.ToString());
				}

				return text.Replace("\\n", "\n");
			}

			Debug.LogWarning($"[Localization] {key} not found. Current language : {CurrentLanguage}. Dico size : {DictionaryText.Count}.");
			return key;
		}

		public string GetValue(string key, string defaultValue)
		{
			return DictionaryText.TryGetValue(key, out string value) ? value : defaultValue;
		}

		public void SwitchLocalization(string newLanguage)
		{
			PlayerPrefs.SetString(SAVE_LANGUAGE_PLAYER_PREFS_KEY, newLanguage);
			Debug.Log("[Localization] SwitchLocalization, previous: " + CurrentLanguage + ", new: " + newLanguage);
			newLanguage = newLanguage.ToLower();
			if (CurrentLanguage == newLanguage)
			{
				return;
			}

			CurrentLanguage = newLanguage;
			Init();
		}

		public void OverrideLocalization(List<LocalizationItem> items)
		{
			if (items != null)
			{
				Debug.Log("[Localization] - OverrideLocalization for:" + items.Count + " items");

				foreach (LocalizationItem item in items)
				{
					if (DictionaryText.ContainsKey(item.Key) && !string.IsNullOrEmpty(item.Value))
					{
						DictionaryText[item.Key] = item.Value;
					}
				}
			}

			UpdateCurrentText();
		}

		public static bool IsLocalizationKey(string elt)
		{
			if (string.IsNullOrEmpty(elt))
			{
				return false;
			}

			return elt[0] == '[' && elt[elt.Length - 1] == ']';
		}

		#endregion

		#region PrivateMethods

		private string ConvertFromSystemLanguage(SystemLanguage language)
		{
			switch (language)
			{
				case SystemLanguage.English:
					return "en";
				case SystemLanguage.French:
					return "fr";
				//case SystemLanguage.Italian:
				//	return "it";
				//case SystemLanguage.Chinese:
				//case SystemLanguage.ChineseSimplified:
				//case SystemLanguage.ChineseTraditional:
				//	return "zh";
				//case SystemLanguage.German:
				//	return "de";
				//case SystemLanguage.Spanish:
				//	return "es";
				//case SystemLanguage.Japanese:
				//	return "ja";
				//case SystemLanguage.Korean:
				//	return "ko";

			}
			return DEFAULT_LANGUAGE;
		}

		private void UpdateCurrentText()
		{
			foreach (LocalizedField textField in mListLocalizedText)
			{
				textField.UpdateLocalisation();
			}
		}

		private void DownloadFile()
		{
			DownloadFileToResources();
		}

		private void DownloadFileToResources()
		{
			TextAsset data = Resources.Load(mCurrentFilename) as TextAsset;

			if (data == null)
			{
				Debug.LogError("[Localisation] Not found the file " + mCurrentFilename + " in Resources folder");
				mCurrentFilename = GetFolder() + FILENAME + "_" + DEFAULT_LANGUAGE;

				data = Resources.Load(mCurrentFilename) as TextAsset;
				if (data == null)
				{
					Debug.LogError("[Localisation] default language doesn't exist, no localization");
					return;
				}
			}

			LocalizationData deserializedData = FromJsonTextAsset<LocalizationData>(data);

			if (deserializedData != null)
			{
				InitDictionary(deserializedData);
			}
		}

		private void InitDictionary(LocalizationData data)
		{
			DictionaryText.Clear();
			foreach (LocalizationItem item in data.Items)
			{
				DictionaryText.Add(item.Key, item.Value);
			}

			IsReady = true;
			UpdateCurrentText();

			Debug.LogFormat("[Localisation] {0}: Success file loaded {1}", LOG_HEADER, mCurrentFilename);
		}

		private string GetFolder()
		{
			return "Json/";
		}

		private T FromJsonTextAsset<T>(TextAsset textAsset)
		{
			try
			{
				T newObject = JsonUtility.FromJson<T>(textAsset.text);
				return newObject;
			}
			catch (Exception e)
			{
				Debug.LogError("Serialization.FromJsonTextAsset - exception " + e.Message);
				return default;
			}
		}

		#endregion
	}

	public static class ExtensionLocalization
	{
		public static string Translate(this string str, params (string key, object value)[] variables)
		{
			return Localization.LocalizationManager.Get.GetValue(str, variables);
		}
	}
}