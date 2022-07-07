using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxToVFXFramework.Scripts.UI.Settings
{
	[CreateAssetMenu(fileName = "InputSettings", menuName = "VoxToVFX/InputSettings.asset", order = 21)]
	public class InputSettings : ScriptableObject
	{
		public List<InputInfo> Settings;
	}

	[Serializable]
	public class InputInfo
	{
		public string KeyName;
		public KeyCode DefaultKey;
		public string DisplayName;
	}
}