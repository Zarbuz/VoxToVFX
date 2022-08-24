using Cysharp.Threading.Tasks;
using MoralisUnity;
using Nethereum.Hex.HexTypes;
using VoxToVFXFramework.Scripts.UI;

namespace VoxToVFXFramework.Scripts.Utils.ContractFunction
{
	public static class ExecuteContractFunctionUtils
	{
		public static async UniTask<string> ExecuteContractFunction(string contractAddress,
			string abi,
			string functionName,
			object[] args,
			HexBigInteger value,
			HexBigInteger gas,
			HexBigInteger gasPrice)
		{
			if (Moralis.Web3Client == null)
			{
				CanvasPlayerPCState previousState = CanvasPlayerPCManager.Instance.CanvasPlayerPcState;
				await CanvasPlayerPCManager.Instance.OpenLoginPanel();
				CanvasPlayerPCManager.Instance.CanvasPlayerPcState = previousState;
				return await ExecuteInternal(contractAddress, abi, functionName, args, value, gas, gasPrice);
			}

			return await ExecuteInternal(contractAddress, abi, functionName, args, value, gas, gasPrice);
		}

		private static async UniTask<string> ExecuteInternal(string contractAddress,
			string abi,
			string functionName,
			object[] args,
			HexBigInteger value,
			HexBigInteger gas,
			HexBigInteger gasPrice)
		{
			string resp = await Moralis.ExecuteContractFunction(contractAddress, abi, functionName, args, value, gas, gasPrice);
			return resp;
		}
	}
}
