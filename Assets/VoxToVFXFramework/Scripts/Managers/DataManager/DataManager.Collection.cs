using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using System.Collections.Generic;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, NftCollection> NftCollection = new Dictionary<string, NftCollection>();

		public async UniTask<NftCollection> GetNftCollectionWithCache(string address)
		{
			if (NftCollection.TryGetValue(address, out NftCollection collection))
			{
				return collection;
			}

			NftCollection result = await NFTManager.Instance.GetAllTokenIds(address);
			if (result != null)
			{
				NftCollection[address] = result;
				return result;
			}

			return null;
		}
	}
}
