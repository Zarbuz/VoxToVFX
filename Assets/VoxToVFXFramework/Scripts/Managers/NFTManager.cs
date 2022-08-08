using System;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using Nethereum.Hex.HexTypes;
using UnityEngine;
using VoxToVFXFramework.Scripts.ScriptableObjets;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class NFTManager : SimpleSingleton<NFTManager>
	{
		#region Fields

		public SmartContractAddressConfig SmartContractAddressConfig => ConfigManager.Instance.SmartContractAddress;

		#endregion
		#region PublicMethods

		public async UniTask<NftOwnerCollection> FetchNFTsForContract(string address, string contract)
		{
			NftOwnerCollection nftOwnerCollection = await Moralis.Web3Api.Account.GetNFTsForContract(address, contract, ConfigManager.Instance.ChainList);
			return nftOwnerCollection;
		}

		public async UniTask<string> MintNft(string tokenCID, string contractAddress)
		{
			string token = tokenCID.Replace("https://", string.Empty);
			Debug.Log(token);

			object[] parameters = {
				token
			};

			// Set gas estimate
			HexBigInteger value = new HexBigInteger(0);
			HexBigInteger gas = new HexBigInteger(0);
			HexBigInteger gasPrice = new HexBigInteger(0); //useless
			string resp = await Moralis.ExecuteContractFunction(contractAddress, SmartContractAddressConfig.CollectionContractABI, "mint", parameters, value, gas, gasPrice);

			Debug.Log("[NFTManager] MintNft: " + resp);
			return resp;
		}

		#endregion
	}
}
