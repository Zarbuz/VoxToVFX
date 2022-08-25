using System.Numerics;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using Nethereum.Hex.HexTypes;
using UnityEngine;
using VoxToVFXFramework.Scripts.ScriptableObjets;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.Utils.ContractFunction;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class NFTMarketManager : SimpleSingleton<NFTMarketManager>
	{
		#region Fields

		public SmartContractAddressConfig SmartContractAddressConfig => ConfigManager.Instance.SmartContractAddress;

		#endregion

		#region PublicMethods

		public async UniTask<string> SetBuyPrice(string nftContract, string tokenId, BigInteger price)
		{
			object[] parameters = {
				nftContract, tokenId, price
			};

			// Set gas estimate
			HexBigInteger value = new HexBigInteger(0);
			HexBigInteger gas = new HexBigInteger(100000);
			HexBigInteger gasPrice = new HexBigInteger(0); //useless
			string resp = await ExecuteContractFunctionUtils.ExecuteContractFunction(SmartContractAddressConfig.VoxToVFXMarketAddress, SmartContractAddressConfig.VoxToVFXMarketABI, "setBuyPrice", parameters, value, gas, gasPrice);

			Debug.Log("[NFTManager] SetBuyPrice: " + resp);
			return resp;
		}
		

		#endregion
	}
}
