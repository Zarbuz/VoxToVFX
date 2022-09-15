using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using VoxToVFXFramework.Scripts.Models;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, FollowUserCache> UserFollowers = new Dictionary<string, FollowUserCache>();
		public Dictionary<string, FollowUserCache> UserFollowings = new Dictionary<string, FollowUserCache>();


		/// <summary>
		/// Retrieves the count of users following this user
		/// </summary>
		/// <param name="address">The ETH address of the user</param>
		/// <returns>The count of users</returns>
		public async UniTask<int> GetCountUserFollowers(string address)
		{
			if (UserFollowers.TryGetValue(address, out FollowUserCache followUserCache))
			{
				if ((DateTime.UtcNow - followUserCache.LastUpdate).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
				{
					return followUserCache.UserFollowings.Count;
				}
			}

			List<FollowUser> list = await FollowUserManager.Instance.GetFollowingUsers(address);
			UserFollowers[address] = new FollowUserCache()
			{
				LastUpdate = DateTime.UtcNow,
				UserFollowings = list.Select(t => t.Follow).ToList()
			};

			return UserFollowers[address].UserFollowings.Count;
		}

		/// <summary>
		/// Retrieves the list of users that the user is following
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public async UniTask<int> GetCountUserFollowings(string address)
		{
			if (UserFollowings.TryGetValue(address, out FollowUserCache followUserCache))
			{
				if ((DateTime.UtcNow - followUserCache.LastUpdate).Minutes < MINUTES_BEFORE_UPDATE_CACHE)
				{
					return followUserCache.UserFollowings.Count;
				}
			}

			List<FollowUser> list = await FollowUserManager.Instance.GetFollowersUsers(address);
			UserFollowings[address] = new FollowUserCache()
			{
				LastUpdate = DateTime.UtcNow,
				UserFollowings = list.Select(t => t.Follow).ToList()
			};

			return UserFollowings[address].UserFollowings.Count;
		}

		public async UniTask FollowUser(string address)
		{
			UserFollowings[UserManager.Instance.CurrentUserAddress].UserFollowings.Add(address);
			if (UserFollowers.ContainsKey(address))
			{
				UserFollowers[address].UserFollowings.Add(UserManager.Instance.CurrentUserAddress);
			}
			else
			{
				UserFollowings[address] = new FollowUserCache()
				{
					UserFollowings = new List<string>()
					{
						UserManager.Instance.CurrentUserAddress
					},
					LastUpdate = DateTime.UtcNow
				};
			}
			await FollowUserManager.Instance.FollowUser(address);
		}

		public async UniTask UnFollowUser(string address)
		{
			UserFollowings[UserManager.Instance.CurrentUserAddress].UserFollowings.Remove(address);
			if (UserFollowers.ContainsKey(address))
			{
				UserFollowers[address].UserFollowings.Remove(UserManager.Instance.CurrentUserAddress);
			}

			await FollowUserManager.Instance.UnFollowUser(address);
		}

		public bool IsUserFollowing(string user)
		{
			return UserFollowings[UserManager.Instance.CurrentUserAddress].UserFollowings.Any(t => t == user);
		}


		public class FollowUserCache
		{
			public DateTime LastUpdate { get; set; }
			public List<string> UserFollowings { get; set; }
		}
	}
}
