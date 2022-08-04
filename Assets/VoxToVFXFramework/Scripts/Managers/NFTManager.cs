using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class NFTManager : SimpleSingleton<NFTManager>
	{
		#region PublicMethods

		public async UniTask<NftOwnerCollection> FetchNFTsForContract(string address, string contract)
		{
			NftOwnerCollection nftOwnerCollection = await Moralis.Web3Api.Account.GetNFTsForContract(address.ToLower(), contract, ChainList.eth);
			return nftOwnerCollection;
		}

		#endregion
	}
}
