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

		[SerializeField] private TMP_InputField NameInputField;
		[SerializeField] private TMP_InputField UserNameInputField;
		[SerializeField] private TMP_InputField BiographyInputField;
		[SerializeField] private Button SelectFileButton;
		[SerializeField] private Image PictureProfileImage;
		[SerializeField] private Image NoAvatarImage;
		[SerializeField] private Button DeletePictureButton;
		[SerializeField] private Image SpinnerImage;

		[SerializeField] private Image BannerImage;
		[SerializeField] private Button DeleteBannerButton;
		[SerializeField] private Image SpinnerBannerImage;
		[SerializeField] private Button SelectImageBannerButton;

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

		private string mProfilePictureUrl;
		private string mBannerPictureUrl;

		#endregion

		#region ConstStatic

		private const int MAX_SIZE_IN_MB = 10;

		#endregion

		#region UnityMethods

		private async void OnEnable()
		{
			SpinnerImage.gameObject.SetActive(false);
			SpinnerBannerImage.gameObject.SetActive(false);
			SelectFileButton.onClick.AddListener(OnSelectFileClicked);
			DeletePictureButton.onClick.AddListener(OnDeletePictureClicked);
			SaveButton.onClick.AddListener(OnSaveClicked);

			SelectImageBannerButton.onClick.AddListener(OnSelectBannerClicked);
			DeleteBannerButton.onClick.AddListener(OnDeleteBannerClicked);

			UserManager.Instance.OnUserInfoUpdated += OnUserInfoUpdated;
			await Refresh();
		}

		private void OnDisable()
		{
			SelectFileButton.onClick.RemoveListener(OnSelectFileClicked);
			DeletePictureButton.onClick.RemoveListener(OnDeletePictureClicked);
			SaveButton.onClick.RemoveListener(OnSaveClicked);
			SelectImageBannerButton.onClick.RemoveListener(OnSelectBannerClicked);
			DeleteBannerButton.onClick.RemoveListener(OnDeleteBannerClicked);

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

				NoAvatarImage.gameObject.SetActive(string.IsNullOrEmpty(customUser.PictureUrl));
				DeletePictureButton.gameObject.SetActive(!string.IsNullOrEmpty(customUser.PictureUrl));
				SelectFileButton.gameObject.SetActive(string.IsNullOrEmpty(customUser.PictureUrl));
				PictureProfileImage.gameObject.SetActive(!string.IsNullOrEmpty(customUser.PictureUrl));

				DeleteBannerButton.gameObject.SetActive(!string.IsNullOrEmpty(customUser.BannerUrl));
				SelectImageBannerButton.gameObject.SetActive(string.IsNullOrEmpty(customUser.BannerUrl));
				BannerImage.gameObject.SetActive(!string.IsNullOrEmpty(customUser.BannerUrl));

				WebsiteInputField.text = customUser.WebsiteUrl;
				DiscordInputField.text = customUser.Discord;
				YoutubeInputField.text = customUser.YoutubeUrl;
				FacebookInputField.text = customUser.FacebookUrl;
				TwitchInputField.text = customUser.TwitchUsername;
				TikTokInputField.text = customUser.TikTokUsername;
				mProfilePictureUrl = customUser.PictureUrl;
				mBannerPictureUrl = customUser.BannerUrl;
				if (!string.IsNullOrEmpty(customUser.PictureUrl))
				{
					await ImageUtils.DownloadAndApplyImage(customUser.PictureUrl, PictureProfileImage, 512, true, true, true);
				}

				if (!string.IsNullOrEmpty(customUser.BannerUrl))
				{
					await ImageUtils.DownloadAndApplyImage(customUser.BannerUrl, BannerImage, 512, true, true, true);
				}
			}
		}

		private async void OnUserInfoUpdated(CustomUser customUser)
		{
			await Refresh();
		}

		private async void OnSelectFileClicked()
		{
			ExtensionFilter extensionFilters = new ExtensionFilter("Images", new[] { "png", "jpg", "jpeg", "gif" });
			string[] paths = StandaloneFileBrowser.OpenFilePanel("Select image", "", new[] { extensionFilters }, false);
			if (paths.Length > 0)
			{
				FileInfo fi = new FileInfo(paths[0]);
				double megaBytes = (fi.Length / 1024f) / 1024f;

				if (megaBytes > MAX_SIZE_IN_MB)
				{
					MessagePopup.Show(string.Format(LocalizationKeys.EDIT_PROFILE_FILE_TOO_BIG.Translate(), MAX_SIZE_IN_MB));
				}
				else
				{
					await UploadSelectedFile(paths[0]);
				}
			}
		}

		private async void OnSelectBannerClicked()
		{
			ExtensionFilter extensionFilters = new ExtensionFilter("Images", new[] { "png", "jpg", "jpeg", "gif" });
			string[] paths = StandaloneFileBrowser.OpenFilePanel("Select image", "", new[] { extensionFilters }, false);
			if (paths.Length > 0)
			{
				FileInfo fi = new FileInfo(paths[0]);
				double megaBytes = (fi.Length / 1024f) / 1024f;

				if (megaBytes > MAX_SIZE_IN_MB)
				{
					MessagePopup.Show(string.Format(LocalizationKeys.EDIT_PROFILE_FILE_TOO_BIG.Translate(), MAX_SIZE_IN_MB));
				}
				else
				{
					await UploadSelectedBanner(paths[0]);
				}
			}
		}

		private void OnDeleteBannerClicked()
		{
			mBannerPictureUrl = string.Empty;
			SelectImageBannerButton.gameObject.SetActive(true);
			BannerImage.gameObject.SetActive(false);
			DeleteBannerButton.gameObject.SetActive(false);
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

		private async UniTask UploadSelectedBanner(string path)
		{
			SpinnerBannerImage.gameObject.SetActive(true);
			mBannerPictureUrl = await FileManager.Instance.UploadFile(path);
			if (!string.IsNullOrEmpty(mBannerPictureUrl))
			{
				SpinnerBannerImage.gameObject.SetActive(false);
				byte[] data = await File.ReadAllBytesAsync(path);
				Texture2D texture = new Texture2D(2, 2);
				texture.LoadImage(data);
				texture.Apply(updateMipmaps: true);

				texture = texture.ResampleAndCrop(256, 256);
				BannerImage.gameObject.SetActive(true);
				BannerImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
				BannerImage.preserveAspect = true;

				SelectImageBannerButton.gameObject.SetActive(false);
				DeleteBannerButton.gameObject.SetActive(true);
			}
			else
			{
				SpinnerBannerImage.gameObject.SetActive(false);
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
				PictureUrl = mProfilePictureUrl,
				BannerUrl = mBannerPictureUrl,
				Bio = BiographyInputField.text,
				UserName = UserNameInputField.text,
				Name = NameInputField.text,
				WebsiteUrl = cleanWebsiteUrl,
				Discord = DiscordInputField.text,
				YoutubeUrl = cleanYoutubeUrl,
				FacebookUrl = cleanFacebookUrl,
				TwitchUsername = cleanTwitchUrl,
				TikTokUsername = cleanTikTokUrl,
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
