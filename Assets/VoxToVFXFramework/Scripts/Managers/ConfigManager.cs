using UnityEngine;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public enum ELanguage
	{
		None,
		French,
		English,
	}

	public class ConfigManager : ModuleSingleton<ConfigManager>
	{
		#region SerializeFields

		[SerializeField] private ELanguage ForceLanguage = ELanguage.None;

		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			SetConfig();
		}

		#endregion

		#region PrivateMethods

		private void SetConfig()
		{
			CheckForceLanguage();
		}


		private void CheckForceLanguage()
		{
			if (!LocalizationManager.DoForceOtherLanguage)
			{
				if (ForceLanguage != ELanguage.None)
				{
					LocalizationManager.DoForceOtherLanguage = true;
				}

				switch (ForceLanguage)
				{
					case ELanguage.French:
						LocalizationManager.Instance.SwitchLocalization("fr");
						break;
					case ELanguage.English:
						LocalizationManager.Instance.SwitchLocalization("en");
						break;
				}
			}
		}
		#endregion
	}
}
