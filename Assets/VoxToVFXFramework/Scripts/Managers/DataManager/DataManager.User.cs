using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using VoxToVFXFramework.Scripts.Models;

namespace VoxToVFXFramework.Scripts.Managers.DataManager
{
	public partial class DataManager
	{
		public Dictionary<string, CustomUser> Users = new Dictionary<string, CustomUser>();

		public async UniTask<CustomUser> GetUserWithCache(string ethAddress)
		{
			if (Users.TryGetValue(ethAddress, out CustomUser user))
			{
				return user;
			}

			CustomUser userFromEth = await UserManager.Instance.LoadUserFromEthAddress(ethAddress);
			if (userFromEth != null)
			{
				Users[ethAddress] = userFromEth;
				return userFromEth;
			}
			return null;
		}
	}
}
