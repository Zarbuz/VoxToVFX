using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Platform.Queries;
using System.Collections.Generic;
using System.Linq;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class FollowUserManager : SimpleSingleton<FollowUserManager>
	{
		#region PublicMethods

		public async UniTask<List<FollowUser>> GetFollowersUsers(string address)
		{
			MoralisQuery<FollowUser> q = await Moralis.Query<FollowUser>();
			q = q.WhereEqualTo("User",address);
			IEnumerable<FollowUser> result = await q.FindAsync();
			List<FollowUser> list = result.ToList();
			return list;
		}

		public async UniTask<List<FollowUser>> GetFollowingUsers(string address)
		{
			MoralisQuery<FollowUser> q = await Moralis.Query<FollowUser>();
			q = q.WhereEqualTo("Follow", address);
			IEnumerable<FollowUser> result = await q.FindAsync();
			List<FollowUser> list = result.ToList();
			return list;
		}

		public async UniTask FollowUser(string user)
		{
			FollowUser followUser = Moralis.Create<FollowUser>();
			followUser.User = UserManager.Instance.CurrentUserAddress;
			followUser.Follow = user;
			followUser.ACL = new MoralisAcl(await Moralis.GetUserAsync())
			{
				PublicReadAccess = true
			};
			await followUser.SaveAsync();
		}

		public async UniTask UnFollowUser(string user)
		{
			MoralisQuery<FollowUser> q = await Moralis.Query<FollowUser>();
			q = q.WhereEqualTo("User", UserManager.Instance.CurrentUserAddress);
			q = q.WhereEqualTo("Follow", user);
			FollowUser first = await q.FirstOrDefaultAsync();
			if (first != null)
			{
				Moralis.DeleteAsync(first);
			}
		}

		#endregion
	}
}
