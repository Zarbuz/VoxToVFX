using Cysharp.Threading.Tasks;
using MoralisUnity.Platform.Queries;
using MoralisUnity;
using Nethereum.Hex.HexTypes;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.ScriptableObjets;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.Utils.ContractFunction;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class CollectionFactoryManager : SimpleSingleton<CollectionFactoryManager>
	{
		#region Fields
		public SmartContractAddressConfig SmartContractAddressConfig => ConfigManager.Instance.SmartContractAddress;

		#endregion
		#region PublicMethods

		public async UniTask<string> CreateCollection(string collectionName, string collectionSymbol)
		{
			int nounce = UnityEngine.Random.Range(1, 1000000000);
			Debug.Log("CreateCollection nounce: " + nounce);
			object[] parameters = {
				collectionName,
				collectionSymbol,
				nounce
			};

			// Set gas estimate
			HexBigInteger value = new HexBigInteger(0);
			HexBigInteger gas = new HexBigInteger(100000);
			HexBigInteger gasPrice = new HexBigInteger(0); //useless

			string resp = await ExecuteContractFunctionUtils.ExecuteContractFunction(SmartContractAddressConfig.CollectionFactoryAddress, SmartContractAddressConfig.CollectionFactoryABI, "createCollection", parameters, value, gas, gasPrice);

			Debug.Log("[CollectionFactoryManager] CreateCollection: " + resp);
			return resp;
		}

		

		public async UniTask<List<CollectionCreatedEvent>> GetUserListContract(string userAddress)
		{
			MoralisQuery<CollectionCreatedEvent> q = await Moralis.Query<CollectionCreatedEvent>();
			q = q.WhereEqualTo("creator", userAddress);
			IEnumerable<CollectionCreatedEvent> result = await q.FindAsync();
			return result.ToList();
		}

		public async UniTask<CollectionCreatedEvent> GetCollection(string collectionContract)
		{
			MoralisQuery<CollectionCreatedEvent> q = await Moralis.Query<CollectionCreatedEvent>();
			q = q.WhereEqualTo("collectionContract", collectionContract);
			CollectionCreatedEvent collection = await q.FirstOrDefaultAsync();
			return collection;
		}

		#endregion
	}
}
