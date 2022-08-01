using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Platform.Queries;
using System;
using UnityEngine;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI.Popups;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class MoralisError
	{
		public string Error { get; set; }
	}

	public class UserManager : SimpleSingleton<UserManager>
	{
		#region Fields

		public CustomUser CurrentUser { get; private set; }
		public event Action<CustomUser> OnUserInfoUpdated;

		#endregion

		#region PublicMethods

		public async UniTask SaveUserInfo(CustomUser customUser)
		{
			MoralisQuery<CustomUser> q = await Moralis.Query<CustomUser>();
			q.WhereEqualTo("UserId", customUser.UserId);

			CustomUser result = await q.FirstOrDefaultAsync();
			if (result == null)
			{
				MoralisUser moralisUser = await Moralis.GetUserAsync();

				CustomUser newUserInfo = Moralis.Create<CustomUser>();
				newUserInfo.PictureUrl = customUser.PictureUrl;
				newUserInfo.Name = customUser.Name;
				newUserInfo.Bio = customUser.Bio;
				newUserInfo.UserId = customUser.UserId;
				newUserInfo.UserName = customUser.UserName;
				newUserInfo.ACL = new MoralisAcl(moralisUser);
				await newUserInfo.SaveAsync();
			}
			else
			{
				result.PictureUrl = customUser.PictureUrl;
				result.Name = customUser.Name;
				result.Bio = customUser.Bio;
				result.UserId = customUser.UserId;
				result.UserName = customUser.UserName;
				await result.SaveAsync();
			}
		}

		public async UniTask Logout()
		{
			await Moralis.LogOutAsync();
			CurrentUser = null;
			OnUserInfoUpdated?.Invoke(null);
		}

		public async UniTask<CustomUser> LoadFromUser(MoralisUser user)
		{
			return await LoadFromUser(user.objectId);
		}

		public async UniTask<CustomUser> LoadFromUser(string objectId)
		{
			MoralisQuery<CustomUser> q = await Moralis.Query<CustomUser>();
			CustomUser user = await q.WhereEqualTo("UserId", objectId).FirstOrDefaultAsync();

			return user;
		}

		public async UniTask<CustomUser> LoadCurrentUser()
		{
			try
			{
				MoralisUser currentUser = await Moralis.GetUserAsync();
				if (currentUser != null)
				{
					Debug.Log("Found moralis current user: " + currentUser.ethAddress);
					CustomUser user = await LoadFromUser(currentUser);
					CurrentUser = user;
					OnUserInfoUpdated?.Invoke(CurrentUser);
					return CurrentUser;
				}

			}
			catch (Exception e)
			{
				Debug.LogError("Failed to load get user: " + e.Message);
			}

			return null;
		}

		public async UniTask<MoralisError> UpdateUserInfo(CustomUser customUser)
		{
			MoralisQuery<CustomUser> q = await Moralis.Query<CustomUser>();
			q = q.WhereNotEqualTo("UserId", customUser.UserId);
			q = q.WhereEqualTo("UserName", customUser.UserName);
			CustomUser found = await q.FirstOrDefaultAsync();

			if (found != null)
			{
				return new MoralisError()
				{
					Error = LocalizationKeys.EDIT_PROFILE_USERNAME_NOT_AVAILABLE.Translate()
				};
			}

			try
			{
				await SaveUserInfo(customUser);
				CurrentUser = customUser;
				OnUserInfoUpdated?.Invoke(customUser);
				return null;
			}
			catch (Exception e)
			{
				return new MoralisError()
				{
					Error = e.Message
				};
			}

		}

		#endregion
	}
}
