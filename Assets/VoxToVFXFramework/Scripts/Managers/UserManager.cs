using System.Linq;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Platform.Queries;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class UserManager : SimpleSingleton<UserManager>
	{
		#region Fields

		public CustomUserDTO CurrentUser { get; private set; }

		#endregion

		#region PublicMethods

		public async UniTask<CustomUserDTO> CreateUser(MoralisUser user, string name, string bio, string profileUrl)
		{
			CustomUserDTO customUser = new CustomUserDTO(user);
			customUser.Name = name;
			customUser.Bio = bio;
			customUser.PictureUrl = profileUrl;

			await customUser.SaveAsync();
			return customUser;
		}

		public async UniTask<CustomUserDTO> LoadFromUser(MoralisUser user)
		{
			return await LoadFromUser(user.objectId);
		}

		public async UniTask<CustomUserDTO> LoadFromUser(string objectId)
		{
			MoralisQuery<_User> q = await Moralis.Query<_User>();
			_User user = await q.WhereEqualTo("objectId", objectId).FirstOrDefaultAsync();

			return user;
		}

		public async UniTask<CustomUserDTO> LoadCurrentUser()
		{
			MoralisUser currentUser = await Moralis.GetUserAsync();
			if (currentUser != null)
			{
				CustomUserDTO user = await LoadFromUser(currentUser);
				CurrentUser = user;
				return CurrentUser;
			}
			return null;
		}

		#endregion
	}
}
