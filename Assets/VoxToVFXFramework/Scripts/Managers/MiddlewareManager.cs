using System;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.ScriptableObjets;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class MiddlewareManager : SimpleSingleton<MiddlewareManager>
	{
		#region Fields

		public SmartContractAddressConfig SmartContractAddressConfig => ConfigManager.Instance.SmartContractAddress;

		#endregion

		#region PublicMethods

		public async UniTask<AccountInfoContractType> GetAccountInfo(string account)
		{
			MoralisClient moralisClient = Moralis.GetClient();

			dynamic abi = JArray.Parse(SmartContractAddressConfig.VoxToVFXMiddlewareABI);

			RunContractDto runContractDto = new RunContractDto()
			{
				Abi = abi,
				Params = new { account }
			};

			try
			{
				AccountInfoContractType result = await moralisClient.Web3Api.Native.RunContractFunction<AccountInfoContractType>(SmartContractAddressConfig.VoxToVFXMiddlewareAddress, "getAccountInfo",
					runContractDto, ConfigManager.Instance.ChainList);

				return result;
			}
			catch (Exception e)
			{
				Debug.LogError("[MiddlewareManager] Failed to GetAccountInfo: " + e.Message);
				return null;
			}
		}

		#endregion
	}
}
