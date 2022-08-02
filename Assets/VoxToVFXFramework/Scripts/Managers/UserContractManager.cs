using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Platform.Queries;
using UnityEngine;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class UserContractManager : SimpleSingleton<UserContractManager>
	{
		#region PublicMethods

		public async UniTask<List<UserContract>> GetUserListContract(string userId)
		{
			MoralisQuery<UserContract> q = await Moralis.Query<UserContract>();
			q = q.WhereEqualTo("UserId", userId);
			IEnumerable<UserContract> result = await q.FindAsync();
			return result.ToList();
		}

		public async UniTask<List<UserContract>> GetUserLoggedListContract()
		{
			return await GetUserListContract(UserManager.Instance.CurrentUser.UserId);
		}

		public async UniTask AddUserContract(string contract, string userId)
		{
			UserContract userContract = Moralis.Create<UserContract>();
			userContract.UserId = userId;
			userContract.ContractAddress = userId;

			MoralisUser moralisUser = await Moralis.GetUserAsync();
			userContract.ACL = new MoralisAcl(moralisUser);

			bool success = await userContract.SaveAsync();
			Debug.Log("AddUserContract: " + success);
		}

		#endregion
	}
}
