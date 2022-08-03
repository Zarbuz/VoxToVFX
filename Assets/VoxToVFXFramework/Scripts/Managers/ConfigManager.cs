﻿using UnityEngine;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.ScriptableObjets;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public enum ELanguage
	{
		None,
		French,
		English,
	}

	public enum BlockchainType
	{
		TestNet,
		MainNet
	}

	public class ConfigManager : ModuleSingleton<ConfigManager>
	{
		#region SerializeFields

		[SerializeField] private ELanguage ForceLanguage = ELanguage.None;
		[SerializeField] private BlockchainType BlockchainType = BlockchainType.TestNet;
		[SerializeField] private SmartContractAddressConfig TestNetSmartContractAddressConfig;
		[SerializeField] private SmartContractAddressConfig MainNetSmartContractAddressConfig;

		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			SetConfig();
			QualityManager.Instance.Initialize();
		}

		#endregion

		#region Fields

		public SmartContractAddressConfig SmartContractAddress
		{
			get
			{
				if (BlockchainType == BlockchainType.MainNet)
				{
					return MainNetSmartContractAddressConfig;
				}

				return TestNetSmartContractAddressConfig;
			}
		}

		public string EtherScanBaseUrl
		{
			get
			{
				if (BlockchainType == BlockchainType.MainNet)
				{
					return "https://etherscan.io/";
				}

				return "https://goerli.etherscan.io/";
			}
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
