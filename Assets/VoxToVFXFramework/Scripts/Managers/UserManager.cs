using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Platform.Queries;
using System;
using UnityEngine;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Singleton;

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
			q = q.WhereEqualTo("UserId", customUser.UserId);

			CustomUser result = await q.FirstOrDefaultAsync();
			if (result == null)
			{
				MoralisUser moralisUser = await Moralis.GetUserAsync();

				CustomUser newUserInfo = Moralis.Create<CustomUser>();
				newUserInfo.ACL = new MoralisAcl(moralisUser);
				newUserInfo = UpdateFields(newUserInfo, customUser);
				await newUserInfo.SaveAsync();
			}
			else
			{
				result = UpdateFields(result, customUser);
				await result.SaveAsync();
			}
		}

		private CustomUser UpdateFields(CustomUser input, CustomUser fromUser)
		{
			input.PictureUrl = fromUser.PictureUrl;
			input.BannerUrl = fromUser.BannerUrl;
			input.Name = fromUser.Name;
			input.Bio = fromUser.Bio;
			input.UserId = fromUser.UserId;
			input.UserName = fromUser.UserName;
			input.FacebookUrl = fromUser.FacebookUrl;
			input.Discord = fromUser.Discord;
			input.WebsiteUrl = fromUser.WebsiteUrl;
			input.YoutubeUrl = fromUser.YoutubeUrl;
			input.SnapchatUsername = fromUser.SnapchatUsername;
			input.TikTokUsername = fromUser.TikTokUsername;
			input.TwitchUsername = fromUser.TwitchUsername;
			return input;
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
