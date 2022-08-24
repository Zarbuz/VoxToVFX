using Cysharp.Threading.Tasks;
using VoxToVFXFramework.Scripts.ScriptableObjets;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class NFTMarketManager : SimpleSingleton<NFTMarketManager>
	{
		#region Fields

		public SmartContractAddressConfig SmartContractAddressConfig => ConfigManager.Instance.SmartContractAddress;

		#endregion

		#region PublicMethods

		public async UniTask SetBuyPrice(string nftContract, int tokenId, int price)
		{

		}
		

		#endregion
	}
}
