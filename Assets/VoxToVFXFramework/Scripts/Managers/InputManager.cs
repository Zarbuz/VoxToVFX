using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI.Settings;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class InputManager : ModuleSingleton<InputManager>
	{
		public InputSettings InputSettings;
		public Dictionary<string, InfoKey> ConfigKeys = new Dictionary<string, InfoKey>();

		protected override void OnAwake()
		{
			foreach (InputInfo inputInfo in InputSettings.Settings)
			{
				ConfigKeys[inputInfo.KeyName] = new InfoKey((KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(inputInfo.KeyName, inputInfo.DefaultKey.ToString())));
			}
		}

		public bool SetKey(string keyName, KeyCode newKeyCode)
		{
			if (ConfigKeys.Values.Any(key => key.Key == newKeyCode))
			{
				return false;
			}

			PlayerPrefs.SetString(keyName, newKeyCode.ToString());
			ConfigKeys[keyName].Key = newKeyCode;

			return true;
		}

		public KeyCode GetKey(string keyName)
		{
			return ConfigKeys[keyName].Key;
		}

		public void ResetSettings()
		{
			foreach (InputInfo inputInfo in InputSettings.Settings)
			{
				ConfigKeys[inputInfo.KeyName].Key = inputInfo.DefaultKey;
				PlayerPrefs.SetString(inputInfo.KeyName, inputInfo.DefaultKey.ToString());
			}
		}
	}

	public class InfoKey
	{
		public KeyCode Key { get; set; }

		public InfoKey(KeyCode key)
		{
			Key = key;
		}
	}
}