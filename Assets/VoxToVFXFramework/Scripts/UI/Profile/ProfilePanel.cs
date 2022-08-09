using System;
using MoralisUnity.Platform.Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public class ProfilePanel : MonoBehaviour
	{

		#region Enum

		private enum eProfileState
		{
			CREATED,
			COLLECTION,
			OWNED
		}

		#endregion

		#region ScriptParameters

		[Header("UserInfo")]
		[SerializeField] private Image BannerImage;
		[SerializeField] private Image ProfileImage;
		[SerializeField] private Image NoAvatarImage;
		[SerializeField] private TextMeshProUGUI AddressText;
		[SerializeField] private TextMeshProUGUI NameText;
		[SerializeField] private TextMeshProUGUI UserNameText;
		[SerializeField] private TextMeshProUGUI BioText;
		[SerializeField] private TextMeshProUGUI BioLabel;
		[SerializeField] private TextMeshProUGUI LinkLabel;

		[SerializeField] private TextMeshProUGUI WebsiteText;
		[SerializeField] private TextMeshProUGUI DiscordText;
		[SerializeField] private TextMeshProUGUI YoutubeText;
		[SerializeField] private TextMeshProUGUI FacebookText;
		[SerializeField] private TextMeshProUGUI TwitchText;
		[SerializeField] private TextMeshProUGUI TikTokText;
		[SerializeField] private TextMeshProUGUI SnapChatText;
		[SerializeField] private TextMeshProUGUI JoinedText;
		[SerializeField] private TextMeshProUGUI FollowingCountText;
		[SerializeField] private TextMeshProUGUI FollowersCountText;
		[SerializeField] private Button EditProfileButton;

		[Header("MainContent")]
		[SerializeField] private Button CreatedTabButton;
		[SerializeField] private Button CollectionTabButton;
		[SerializeField] private Button OwnedTabButton;

		[SerializeField] private TextMeshProUGUI CreatedCountText;
		[SerializeField] private TextMeshProUGUI CollectionCountText;
		[SerializeField] private TextMeshProUGUI OwnedCountText;

		#endregion


		#region Fields

		private eProfileState mProfileState;

		private eProfileState ProfileState
		{
			get => mProfileState;
			set
			{
				mProfileState = value;
				CreatedTabButton.transform.GetChild(0).gameObject.SetActive(mProfileState == eProfileState.CREATED);
				CollectionTabButton.transform.GetChild(0).gameObject.SetActive(mProfileState == eProfileState.COLLECTION);
				OwnedTabButton.transform.GetChild(0).gameObject.SetActive(mProfileState == eProfileState.OWNED);
			}
		}

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			EditProfileButton.onClick.AddListener(OnEditProfileClicked);
			CreatedTabButton.onClick.AddListener(() => OnSwitchMainTab(eProfileState.CREATED));
			CollectionTabButton.onClick.AddListener(() => OnSwitchMainTab(eProfileState.COLLECTION));
			OwnedTabButton.onClick.AddListener(() => OnSwitchMainTab(eProfileState.OWNED));
		}

		private void OnDisable()
		{
			EditProfileButton.onClick.RemoveListener(OnEditProfileClicked);
			CreatedTabButton.onClick.RemoveAllListeners();
			CollectionTabButton.onClick.RemoveAllListeners();
			OwnedTabButton.onClick.RemoveAllListeners();
		}

		#endregion

		#region PublicMethods

		public async void Initialize(CustomUser user)
		{
			NoAvatarImage.gameObject.SetActive(true);
			ProfileImage.gameObject.SetActive(false);

			UserNameText.text = "@" + user.UserName;
			NameText.text = user.Name;

			AddressText.text = user.EthAddress.FormatEthAddress(7);

			BioLabel.gameObject.SetActive(!string.IsNullOrEmpty(user.Bio));
			BioText.gameObject.SetActive(!string.IsNullOrEmpty(user.Bio));
			BioText.text = user.Bio;

			LinkLabel.gameObject.SetActive(!string.IsNullOrEmpty(user.WebsiteUrl) ||
			                               !string.IsNullOrEmpty(user.Discord) ||
			                               !string.IsNullOrEmpty(user.YoutubeUrl) ||
			                               !string.IsNullOrEmpty(user.FacebookUrl) ||
			                               !string.IsNullOrEmpty(user.TwitchUsername) ||
			                               !string.IsNullOrEmpty(user.TikTokUsername) ||
			                               !string.IsNullOrEmpty(user.SnapchatUsername));

			WebsiteText.gameObject.SetActive(!string.IsNullOrEmpty(user.WebsiteUrl));
			WebsiteText.text = user.WebsiteUrl;

			DiscordText.gameObject.SetActive(!string.IsNullOrEmpty(user.Discord));
			DiscordText.text = user.Discord;

			YoutubeText.gameObject.SetActive(!string.IsNullOrEmpty(user.YoutubeUrl));
			YoutubeText.text = user.YoutubeUrl;

			FacebookText.gameObject.SetActive(!string.IsNullOrEmpty(user.FacebookUrl));
			FacebookText.text = FacebookText.text;

			TwitchText.gameObject.SetActive(!string.IsNullOrEmpty(user.TwitchUsername));
			TwitchText.text = user.TwitchUsername;

			TikTokText.gameObject.SetActive(!string.IsNullOrEmpty(user.TikTokUsername));
			TikTokText.text = user.TikTokUsername;

			SnapChatText.gameObject.SetActive(!string.IsNullOrEmpty(user.SnapchatUsername));
			SnapChatText.text = user.SnapchatUsername;

			EditProfileButton.gameObject.SetActive(user.UserId == UserManager.Instance.CurrentUser.UserId);

			JoinedText.gameObject.SetActive(user.createdAt.HasValue);
			if (user.createdAt != null)
			{
				JoinedText.text = user.createdAt.Value.ToShortDateString();
			}

			if (!string.IsNullOrEmpty(user.PictureUrl))
			{
				bool success = await ImageUtils.DownloadAndApplyImage(user.PictureUrl, ProfileImage, 256, true, true, true);
				if (success)
				{
					NoAvatarImage.gameObject.SetActive(false);
					ProfileImage.gameObject.SetActive(true);
				}
				else
				{
					NoAvatarImage.gameObject.SetActive(true);
					ProfileImage.gameObject.SetActive(false);
				}
			}

			if (!string.IsNullOrEmpty(user.BannerUrl))
			{
				Texture2D texture = await MediaManager.Instance.DownloadImage(user.BannerUrl, int.MaxValue, true, false);
				texture = texture.ResampleAndCrop(1920, 280);

				BannerImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
				BannerImage.preserveAspect = false;
			}
		}

		#endregion

		#region PrivateMethods

		private void OnEditProfileClicked()
		{
			CanvasPlayerPCManager.Instance.GenericTogglePanel(CanvasPlayerPCState.EditProfile);
		}

		private void OnSwitchMainTab(eProfileState profileState)
		{
			ProfileState = profileState;
		}

		#endregion
	}
}
