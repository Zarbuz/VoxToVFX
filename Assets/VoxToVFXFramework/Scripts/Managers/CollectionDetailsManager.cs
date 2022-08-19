using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Platform.Queries;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class CollectionDetailsManager: SimpleSingleton<CollectionDetailsManager>
	{
		#region PublicMethods

		public async UniTask<CollectionDetails> GetCollectionDetails(string contractAddress)
		{
			MoralisQuery<CollectionDetails> q = await Moralis.Query<CollectionDetails>();
			q = q.WhereEqualTo("collectionContract", contractAddress);
			CollectionDetails collection = await q.FirstOrDefaultAsync();
			return collection;
		}

		public async UniTask SaveCollectionDetails(CollectionDetails collectionDetails)
		{
			MoralisQuery<CollectionDetails> q = await Moralis.Query<CollectionDetails>();
			q = q.WhereEqualTo("collectionContract", collectionDetails.CollectionContract);

			CollectionDetails result = await q.FirstOrDefaultAsync();
			if (result == null)
			{
				CollectionDetails newCollectionDetails = Moralis.Create<CollectionDetails>();
				newCollectionDetails.ACL = new MoralisAcl();
				MoralisUser moralisUser = await Moralis.GetUserAsync();
				newCollectionDetails.ACL.SetWriteAccess(moralisUser.objectId, true);
				newCollectionDetails.ACL.PublicReadAccess = true;
				newCollectionDetails = UpdateFields(newCollectionDetails, collectionDetails);
				await newCollectionDetails.SaveAsync();
			}
			else
			{
				result = UpdateFields(result, collectionDetails);
				await result.SaveAsync();
			}
		}

		#endregion

		#region PrivateMethods

		private CollectionDetails UpdateFields(CollectionDetails input, CollectionDetails from)
		{
			input.CollectionContract = from.CollectionContract;
			input.CoverImageUrl = from.CoverImageUrl;
			input.Description = from.Description;
			input.LogoImageUrl = from.LogoImageUrl;
			return input;
		}

		#endregion

	}
}
