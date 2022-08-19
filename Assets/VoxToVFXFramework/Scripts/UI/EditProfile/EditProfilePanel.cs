using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Objects;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.UI.Popups;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.EditProfile
{
	public class EditProfilePanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TMP_InputField NameInputField;
		[SerializeField] private TMP_InputField UserNameInputField;
		[SerializeField] private TMP_InputField BiographyInputField;
		[SerializeField] private SelectImage ProfileSelectImage;
		[SerializeField] private SelectImage BannerSelectImage;

		[Header("Social Links")]
		[SerializeField] private TMP_InputField WebsiteInputField;

		[SerializeField] private TMP_InputField DiscordInputField;
		[SerializeField] private TMP_InputField YoutubeInputField;
		[SerializeField] private TMP_InputField FacebookInputField;
		[SerializeField] private TMP_InputField TwitchInputField;
		[SerializeField] private TMP_InputField TikTokInputField;


		[SerializeField] private Button SaveButton;


		#endregion

		#region Fields

		

		#endregion

		

		#region UnityMethods

		private async void OnEnable()
		{
			SaveButton.onClick.AddListener(OnSaveClicked);
			UserManager.Instance.OnUserInfoUpdated += OnUserInfoUpdated;
			await Refresh();
		}

		private void OnDisable()
		{
			SaveButton.onClick.RemoveListener(OnSaveClicked);

			if (UserManager.Instance != null)
			{
				UserManager.Instance.OnUserInfoUpdated -= OnUserInfoUpdated;
			}

		}

		
		#endregion

		#region PrivateMethods

		private async UniTask Refresh()
		{
			MoralisUser moralisUser = await Moralis.GetUserAsync();
			CustomUser customUser = await UserManager.Instance.LoadFromUser(moralisUser);
			if (customUser != null)
			{
				NameInputField.text = customUser.Name;
				UserNameInputField.text = customUser.UserName;
				BiographyInputField.text = customUser.Bio;

				ProfileSelectImage.Initialize(customUser.PictureUrl);
				BannerSelectImage.Initialize(customUser.BannerUrl);

				WebsiteInputField.text = customUser.WebsiteUrl;
				DiscordInputField.text = customUser.Discord;
				YoutubeInputField.text = customUser.YoutubeUrl;
				FacebookInputField.text = customUser.FacebookUrl;
				TwitchInputField.text = customUser.TwitchUsername;
				TikTokInputField.text = customUser.TikTokUsername;
			}
			else
			{
				ProfileSelectImage.Initialize(string.Empty);
				BannerSelectImage.Initialize(string.Empty);
			}
		}

		private async void OnUserInfoUpdated(CustomUser customUser)
		{
			await Refresh();
		}

		private async void OnSaveClicked()
		{
			if (string.IsNullOrEmpty(UserNameInputField.text) || UserNameInputField.text.Length < 3)
			{
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_MISSING_USERNAME.Translate());
				return;
			}

			if (UserNameInputField.text.Contains(" ") || UserNameInputField.text.HasSpecialChars())
			{
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_USERNAME_NO_SPACE.Translate());
				return;
			}

			if (!string.IsNullOrEmpty(DiscordInputField.text) && !Regex.IsMatch(DiscordInputField.text, "^.{3,32}#[0-9]{4}$"))
			{
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_INVALID_DISCORD.Translate());
				return;
			}

			if (!string.IsNullOrEmpty(WebsiteInputField.text) && !IsValidUrl(WebsiteInputField.text, "https://"))
			{
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_INVALID_WEBSITE_URL.Translate());
				return;
			}

			if (!string.IsNullOrEmpty(FacebookInputField.text) && !IsValidUrl(FacebookInputField.text, "https://www.facebook.com/"))
			{
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_INVALID_FACEBOOK_URL.Translate());
				return;
			}

			if (!string.IsNullOrEmpty(YoutubeInputField.text) && !IsValidUrl(YoutubeInputField.text, "https://www.youtube.com/channel/"))
			{
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_INVALID_YOUTUBE_URL.Translate());
				return;
			}

			if (!string.IsNullOrEmpty(TwitchInputField.text) && !IsValidUrl(TwitchInputField.text, "https://www.twitch.tv/"))
			{
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_INVALID_TWITCH_URL.Translate());
				return;
			}

			if (!string.IsNullOrEmpty(TikTokInputField.text) && !IsValidUrl(TikTokInputField.text, "https://www.tiktok.com/"))
			{
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_INVALID_TIKTOK_URL.Translate());
				return;
			}

			await UpdateUserInfo();
		}

		private async UniTask UpdateUserInfo()
		{
			MoralisUser moralisUser = await Moralis.GetUserAsync();

			string cleanWebsiteUrl = CleanUrl(WebsiteInputField.text, "https://");
			cleanWebsiteUrl = CleanUrl(cleanWebsiteUrl, "http://");

			string cleanFacebookUrl = CleanUrl(FacebookInputField.text, "https://www.facebook.com/");
			string cleanYoutubeUrl = CleanUrl(YoutubeInputField.text, "https://www.youtube.com/channel/");
			string cleanTwitchUrl = CleanUrl(TwitchInputField.text, "https://www.twitch.tv/");
			string cleanTikTokUrl = CleanUrl(TikTokInputField.text, "https://www.tiktok.com/");

			CustomUser currentUser = new CustomUser
			{
				UserId = moralisUser.objectId,
				PictureUrl = ProfileSelectImage.ImageUrl,
				BannerUrl = BannerSelectImage.ImageUrl,
				Bio = BiographyInputField.text,
				UserName = UserNameInputField.text,
				Name = NameInputField.text,
				WebsiteUrl = cleanWebsiteUrl,
				Discord = DiscordInputField.text,
				YoutubeUrl = cleanYoutubeUrl,
				FacebookUrl = cleanFacebookUrl,
				TwitchUsername = cleanTwitchUrl,
				TikTokUsername = cleanTikTokUrl,
				EthAddress =moralisUser.ethAddress
			};
			MoralisError error = await UserManager.Instance.UpdateUserInfo(currentUser);
			if (error == null)
			{
				Debug.Log("Successfully updated user info");
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_UPDATE_SUCCESS.Translate());
			}
			else
			{
				MessagePopup.Show(error.Error, LogType.Error);
			}
		}

		private bool IsValidUrl(string url, string mustContains = "")
		{
			if (!url.Contains(mustContains))
			{
				url = mustContains + url;
			}

			bool result = Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
						  && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

			return result;
		}

		private string CleanUrl(string url, string oldValue)
		{
			return url.Replace(oldValue, string.Empty);
		}

	

		#endregion
	}
}
