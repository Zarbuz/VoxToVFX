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
using VoxToVFXFramework.Scripts.UI.Popups;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.EditProfile
{
	public class EditProfilePanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Button CloseButton;
		[SerializeField] private TMP_InputField NameInputField;
		[SerializeField] private TMP_InputField UserNameInputField;
		[SerializeField] private TMP_InputField BiographyInputField;
		[SerializeField] private Button SelectFileButton;
		[SerializeField] private Image PictureProfileImage;
		[SerializeField] private Image NoAvatarImage;
		[SerializeField] private Button DeletePictureButton;
		[SerializeField] private Image SpinnerImage;

		[Header("Social Links")]
		[SerializeField] private TMP_InputField WebsiteInputField;

		[SerializeField] private TMP_InputField DiscordInputField;
		[SerializeField] private TMP_InputField YoutubeInputField;
		[SerializeField] private TMP_InputField FacebookInputField;
		[SerializeField] private TMP_InputField TwitchInputField;
		[SerializeField] private TMP_InputField TikTokInputField;
		[SerializeField] private TMP_InputField SnapchatInputField;


		[SerializeField] private Button SaveButton;


		#endregion

		#region Fields

		private string mProfilePictureUrl;

		#endregion

		#region UnityMethods

		private async void OnEnable()
		{
			SpinnerImage.gameObject.SetActive(false);
			SelectFileButton.onClick.AddListener(OnSelectFileClicked);
			DeletePictureButton.onClick.AddListener(OnDeletePictureClicked);
			SaveButton.onClick.AddListener(OnSaveClicked);
			CloseButton.onClick.AddListener(OnCloseClicked);

			UserManager.Instance.OnUserInfoUpdated += OnUserInfoUpdated;
			await Refresh();
		}

		private void OnDisable()
		{
			SelectFileButton.onClick.RemoveListener(OnSelectFileClicked);
			DeletePictureButton.onClick.RemoveListener(OnDeletePictureClicked);
			SaveButton.onClick.RemoveListener(OnSaveClicked);
			CloseButton.onClick.RemoveListener(OnCloseClicked);

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
				CloseButton.gameObject.SetActive(true);
				NameInputField.text = customUser.Name;
				UserNameInputField.text = customUser.UserName;
				BiographyInputField.text = customUser.Bio;

				NoAvatarImage.gameObject.SetActive(string.IsNullOrEmpty(customUser.PictureUrl));
				DeletePictureButton.gameObject.SetActive(!string.IsNullOrEmpty(customUser.PictureUrl));
				SelectFileButton.gameObject.SetActive(string.IsNullOrEmpty(customUser.PictureUrl));
				PictureProfileImage.gameObject.SetActive(!string.IsNullOrEmpty(customUser.PictureUrl));

				WebsiteInputField.text = customUser.WebsiteUrl;
				DiscordInputField.text = customUser.Discord;
				YoutubeInputField.text = customUser.YoutubeUrl;
				FacebookInputField.text = customUser.FacebookUrl;
				TwitchInputField.text = customUser.TwitchUsername;
				TikTokInputField.text = customUser.TikTokUsername;
				SnapchatInputField.text = customUser.SnapchatUsername;
				mProfilePictureUrl = customUser.PictureUrl;
				if (!string.IsNullOrEmpty(customUser.PictureUrl))
				{
					await ImageUtils.DownloadAndApplyImage(customUser.PictureUrl, PictureProfileImage, 512, true, true, true);
				}
			}
			else
			{
				CloseButton.gameObject.SetActive(false);
			}
		}

		private async void OnUserInfoUpdated(CustomUser customUser)
		{
			await Refresh();
		}

		private void OnCloseClicked()
		{
			CanvasPlayerPCManager.Instance.GenericClosePanel();
		}

		private async void OnSelectFileClicked()
		{
			ExtensionFilter extensionFilters = new ExtensionFilter("Images", new[] { "png", "jpg", "jpeg", "gif" });
			string[] paths = StandaloneFileBrowser.OpenFilePanel("Select image", "", new[] { extensionFilters }, false);
			if (paths.Length > 0)
			{
				await UploadSelectedFile(paths[0]);
			}
		}

		private async UniTask UploadSelectedFile(string path)
		{
			ShowSpinnerImage(true);
			mProfilePictureUrl = await FileManager.Instance.UploadFile(path);
			if (!string.IsNullOrEmpty(mProfilePictureUrl))
			{
				SpinnerImage.gameObject.SetActive(false);
				byte[] data = await File.ReadAllBytesAsync(path);
				Texture2D texture = new Texture2D(2, 2);
				texture.LoadImage(data);
				texture.Apply(updateMipmaps: true);
				texture = texture.ResampleAndCrop(256, 256);
				PictureProfileImage.gameObject.SetActive(true);
				PictureProfileImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
				PictureProfileImage.preserveAspect = true;

				NoAvatarImage.gameObject.SetActive(false);
				SelectFileButton.gameObject.SetActive(false);
				DeletePictureButton.gameObject.SetActive(true);
			}
			else
			{
				ShowSpinnerImage(false);
			}
		}

		private void ShowSpinnerImage(bool showSpinner)
		{
			SpinnerImage.gameObject.SetActive(showSpinner);
			NoAvatarImage.gameObject.SetActive(!showSpinner);
		}

		private void OnDeletePictureClicked()
		{
			mProfilePictureUrl = string.Empty;
			SelectFileButton.gameObject.SetActive(true);
			PictureProfileImage.gameObject.SetActive(false);
			DeletePictureButton.gameObject.SetActive(false);
			NoAvatarImage.gameObject.SetActive(true);
		}

		private async void OnSaveClicked()
		{
			if (string.IsNullOrEmpty(UserNameInputField.text) || UserNameInputField.text.Length < 3)
			{
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_MISSING_USERNAME.Translate());
				return;
			}

			if (UserNameInputField.text.Contains(" ") || HasSpecialChars(UserNameInputField.text))
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

			if (!string.IsNullOrEmpty(SnapchatInputField.text) && !IsValidUrl(SnapchatInputField.text))
			{
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_INVALID_SNAPCHAT_URL.Translate());
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
				PictureUrl = mProfilePictureUrl,
				Bio = BiographyInputField.text,
				UserName = UserNameInputField.text,
				Name = NameInputField.text,
				WebsiteUrl = cleanWebsiteUrl,
				Discord = DiscordInputField.text,
				YoutubeUrl = cleanYoutubeUrl,
				FacebookUrl = cleanFacebookUrl,
				TwitchUsername = cleanTwitchUrl,
				TikTokUsername = cleanTikTokUrl,
				SnapchatUsername = SnapchatInputField.text
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

		private bool HasSpecialChars(string yourString)
		{
			return yourString.Any(ch => !char.IsLetterOrDigit(ch));
		}

		#endregion
	}
}
