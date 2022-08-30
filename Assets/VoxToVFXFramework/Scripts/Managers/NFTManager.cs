using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Queries;
using MoralisUnity.Web3Api.Models;
using Nethereum.Hex.HexTypes;
using UnityEngine;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.ScriptableObjets;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.Utils.ContractFunction;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class NFTManager : SimpleSingleton<NFTManager>
	{
		#region Fields

		public SmartContractAddressConfig SmartContractAddressConfig => ConfigManager.Instance.SmartContractAddress;

		#endregion

		#region PublicMethods

		public async UniTask<List<CollectionMintedEvent>> FetchNFTsForContract(string creator, string contract)
		{
			MoralisQuery<CollectionMintedEvent> q = await Moralis.Query<CollectionMintedEvent>();
			q = q.WhereEqualTo("creator", creator);
			q = q.WhereEqualTo("address", contract);

			IEnumerable<CollectionMintedEvent> result = await q.FindAsync();
			return result.ToList();
		}

		public async UniTask<CollectionMintedEvent> GetCollectionMintedItem(string creator, string contract, string tokenId)
		{
			MoralisQuery<CollectionMintedEvent> q = await Moralis.Query<CollectionMintedEvent>();
			q = q.WhereEqualTo("creator", creator);
			q = q.WhereEqualTo("address", contract);
			q = q.WhereEqualTo("tokenId", tokenId);
			return await q.FirstOrDefaultAsync();
		}

		public async UniTask<NftCollection> GetAllTokenIds(string address)
		{
			NftCollection collection = await Moralis.Web3Api.Token.GetAllTokenIds(address, ConfigManager.Instance.ChainList, null);
			return collection;
		}

		public async UniTask<bool> SyncNFTContract(string address)
		{
			bool success = await Moralis.Web3Api.Token.SyncNFTContract(address, ConfigManager.Instance.ChainList);
			Debug.Log("[NFTManager] SyncNFTContract: " +success);
			return success;
		}

		public async UniTask<string> MintNftAndApprove(string tokenCID, string contractAddress)
		{
			string token = tokenCID.Replace("https://", string.Empty);
			Debug.Log(token);

			object[] parameters = {
				token,
				SmartContractAddressConfig.VoxToVFXMarketAddress
			};

			// Set gas estimate
			HexBigInteger value = new HexBigInteger(0);
			HexBigInteger gas = new HexBigInteger(0);
			HexBigInteger gasPrice = new HexBigInteger(0); //useless
			string resp = await ExecuteContractFunctionUtils.ExecuteContractFunction(contractAddress, SmartContractAddressConfig.CollectionContractABI, "mintAndApprove", parameters, value, gas, gasPrice);

			Debug.Log("[NFTManager] MintNft: " + resp);
			return resp;
		}

		#endregion
	}
}
