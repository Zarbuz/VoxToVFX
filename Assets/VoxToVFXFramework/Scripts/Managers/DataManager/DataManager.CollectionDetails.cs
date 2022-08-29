using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using VoxToVFXFramework.Scripts.Models;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, CollectionDetails> CollectionDetails = new Dictionary<string, CollectionDetails>();


		public async UniTask<CollectionDetails> GetCollectionDetailsWithCache(string collectionContract)
		{
			if (CollectionDetails.TryGetValue(collectionContract, out CollectionDetails collectionDetails))
			{
				return collectionDetails;
			}

			CollectionDetails details = await CollectionDetailsManager.Instance.GetCollectionDetails(collectionContract);
			CollectionDetails[collectionContract] = details;
			return details;
		}



		public void SaveCollectionDetails(CollectionDetails collectionDetails)
		{
			CollectionDetails[collectionDetails.CollectionContract] = collectionDetails;
		}
	}
}
