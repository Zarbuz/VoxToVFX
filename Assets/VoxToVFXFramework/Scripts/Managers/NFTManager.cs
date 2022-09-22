using System;
using System.Collections;
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

		public async UniTask<NftCollection> GetNFTForContract(string address)
		{
			Debug.Log("[NFTManager] GetNFTForContract: " + address);
			NftCollection owners = await Moralis.Web3Api.Token.GetAllTokenIds(address, ConfigManager.Instance.ChainList);
			return owners;
		}

		public async UniTask<NftOwnerCollection> GetNFTOwners(string address)
		{
			Debug.Log("[NFTManager] GetNFTOwners: " + address);
			NftOwnerCollection owners = await Moralis.Web3Api.Token.GetNFTOwners(address, ConfigManager.Instance.ChainList);
			return owners;
		}

		public async UniTask<NftOwnerCollection> GetNFTForUser(string user)
		{
			Debug.Log("[NFTManager] GetNFTForCurrentUser: " + user);
			try
			{
				NftOwnerCollection collection = await Moralis.Web3Api.Account.GetNFTs(user, ConfigManager.Instance.ChainList);
				return collection;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}


		public async UniTask<List<AbstractContractEvent>> GetAllEventsForNFT(string contract, string tokenId)
		{
			List<AbstractContractEvent> events = new List<AbstractContractEvent>();
			MoralisQuery<CollectionMintedEvent> q = await Moralis.Query<CollectionMintedEvent>();
			q = q.WhereEqualTo("address", contract);
			q = q.WhereEqualTo("tokenId", tokenId);
			CollectionMintedEvent result = await q.FirstAsync();
			events.Add(result);

			MoralisQuery<BuyPriceAcceptedEvent> q2 = await Moralis.Query<BuyPriceAcceptedEvent>();
			q2 = q2.WhereEqualTo("address", contract);
			q2 = q2.WhereEqualTo("tokenId", tokenId);
			IEnumerable<BuyPriceAcceptedEvent> result2 = await q2.FindAsync();
			events.AddRange(result2);

			MoralisQuery<BuyPriceCanceledEvent> q3 = await Moralis.Query<BuyPriceCanceledEvent>();
			q3 = q3.WhereEqualTo("address", contract);
			q3 = q3.WhereEqualTo("tokenId", tokenId);
			IEnumerable<BuyPriceCanceledEvent> result3 = await q3.FindAsync();
			events.AddRange(result3);

			MoralisQuery<BuyPriceSetEvent> q4 = await Moralis.Query<BuyPriceSetEvent>();
			q4 = q4.WhereEqualTo("address", contract);
			q4 = q4.WhereEqualTo("tokenId", tokenId);
			IEnumerable<BuyPriceSetEvent> result4 = await q4.FindAsync();
			events.AddRange(result4);

			MoralisQuery<EthNFTTransfers> q5 = await Moralis.Query<EthNFTTransfers>();
			q5 = q5.WhereEqualTo("token_address", contract);
			q5 = q5.WhereEqualTo("token_id", tokenId);
			IEnumerable<EthNFTTransfers> result5 = await q5.FindAsync();
			events.AddRange(result5);

			return events.OrderByDescending(t => t.createdAt).ToList();
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


		public async UniTask<string> TransferItem(string address, string tokenId, string to)
		{
			object[] parameters = {
				UserManager.Instance.CurrentUserAddress,
				to,
				tokenId
			};

			// Set gas estimate
			HexBigInteger value = new HexBigInteger(0);
			HexBigInteger gas = new HexBigInteger(100000);
			HexBigInteger gasPrice = new HexBigInteger(0); //useless

			string resp = await ExecuteContractFunctionUtils.ExecuteContractFunction(address, SmartContractAddressConfig.CollectionContractABI, "transferFrom", parameters, value, gas, gasPrice);

			Debug.Log("[CollectionFactoryManager] CreateCollection: " + resp);
			return resp;
		}

		public async UniTask<string> BurnNFT(string address, string tokenId)
		{
			object[] parameters = {
				tokenId
			};

			// Set gas estimate
			HexBigInteger value = new HexBigInteger(0);
			HexBigInteger gas = new HexBigInteger(100000);
			HexBigInteger gasPrice = new HexBigInteger(0); //useless

			string resp = await ExecuteContractFunctionUtils.ExecuteContractFunction(address, SmartContractAddressConfig.CollectionContractABI, "burn", parameters, value, gas, gasPrice);

			Debug.Log("[CollectionFactoryManager] SelfDestruct : " + resp);
			return resp;
		}

		public async UniTask<string> CollectionSelfDestruct(string address)
		{
			object[] parameters = {
			};

			// Set gas estimate
			HexBigInteger value = new HexBigInteger(0);
			HexBigInteger gas = new HexBigInteger(100000);
			HexBigInteger gasPrice = new HexBigInteger(0); //useless

			string resp = await ExecuteContractFunctionUtils.ExecuteContractFunction(address, SmartContractAddressConfig.CollectionContractABI, "selfDestruct", parameters, value, gas, gasPrice);

			Debug.Log("[CollectionFactoryManager] SelfDestruct : " + resp);
			return resp;
		}

		#endregion

		#region EditorMethods

#if UNITY_EDITOR
		public async UniTask WatchMintedEventContract()
		{
			try
			{
				Dictionary<string, object> parameters = new Dictionary<string, object>();
				await Moralis.Cloud.RunAsync<object>("watchMintedEventContract", parameters);
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message + " " + e.StackTrace);
			}
		}

		public async UniTask WatchSelfDestructEventContract()
		{
			try
			{
				Dictionary<string, object> parameters = new Dictionary<string, object>();
				await Moralis.Cloud.RunAsync<object>("watchSelfDestructEvent", parameters);
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message + " " + e.StackTrace);
			}
		}


#endif
		#endregion
	}
}
