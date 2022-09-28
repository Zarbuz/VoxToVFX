using Cysharp.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using System.Numerics;
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

		public async UniTask<string> MakeOffer(string nftContract, string tokenId, BigInteger weiPrice)
		{
			object[] parameters = {
				nftContract, tokenId, weiPrice
			};

			// Set gas estimate
			HexBigInteger value = new HexBigInteger(weiPrice.ToString("x"));
			HexBigInteger gas = new HexBigInteger(100000);
			HexBigInteger gasPrice = new HexBigInteger(0); //useless
			string resp = await ExecuteContractFunctionUtils.ExecuteContractFunction(SmartContractAddressConfig.VoxToVFXMarketAddress, SmartContractAddressConfig.VoxToVFXMarketABI, "makeOffer", parameters, value, gas, gasPrice);

			Debug.Log("[NFTManager] MakeOffer: " + resp);
			return resp;
		}

		public async UniTask<string> CancelBuyPrice(string nftContract, string tokenId)
		{
			object[] parameters = {
				nftContract, tokenId
			};

			// Set gas estimate
			HexBigInteger value = new HexBigInteger(0);
			HexBigInteger gas = new HexBigInteger(100000);
			HexBigInteger gasPrice = new HexBigInteger(0); //useless
			string resp = await ExecuteContractFunctionUtils.ExecuteContractFunction(SmartContractAddressConfig.VoxToVFXMarketAddress, SmartContractAddressConfig.VoxToVFXMarketABI, "cancelBuyPrice", parameters, value, gas, gasPrice);

			Debug.Log("[NFTManager] CancelBuyPrice: " + resp);
			return resp;
		}

		public async UniTask<string> Buy(string nftContract, string tokenId, decimal buyPrice)
		{
			BigInteger weiPrice = UnitConversion.Convert.ToWei(buyPrice);

			object[] parameters = {
				nftContract, tokenId, weiPrice
			};

			// Set gas estimate

			HexBigInteger value = new HexBigInteger(weiPrice.ToString("x"));
			HexBigInteger gas = new HexBigInteger(100000);
			HexBigInteger gasPrice = new HexBigInteger(0); //useless
			string resp = await ExecuteContractFunctionUtils.ExecuteContractFunction(SmartContractAddressConfig.VoxToVFXMarketAddress, SmartContractAddressConfig.VoxToVFXMarketABI, "buy", parameters, value, gas, gasPrice);

			Debug.Log("[NFTManager] SetBuyPrice: " + resp);
			return resp;
		}

		#endregion
	}
}
