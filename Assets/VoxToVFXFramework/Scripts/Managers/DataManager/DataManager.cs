using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager : SimpleSingleton<DataManager>
	{
		#region ConstStatic

		public const int MINUTES_BEFORE_UPDATE_CACHE = 2;

		#endregion

		public void ClearAll()
		{
			NFTEventsCache.Clear();
			NFTDetailsCache.Clear();
			CollectionDetails.Clear();
			AccountDetails.Clear();
			ContractCreatedPerUsers.Clear();
			NftCollection.Clear();
			UserOwner.Clear();
			Users.Clear();
		}
	
	}

	
}
