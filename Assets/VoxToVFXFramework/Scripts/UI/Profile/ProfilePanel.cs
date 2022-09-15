using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public class ProfilePanel : MonoBehaviour
	{

		#region ScriptParameters

		[Header("UserInfo")]
		[SerializeField] private Image BannerImage;
		[SerializeField] private AvatarImage AvatarImage;

		[SerializeField] private VerticalLayoutGroup LeftPartVerticalLayout;

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
		[SerializeField] private ButtonFollowUser FollowButton;

		[Header("MainContent")]
		[SerializeField] private ProfileListingPanel ProfileListingPanel;

		#endregion

		#region Fields

		private CustomUser mUser;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			EditProfileButton.onClick.AddListener(OnEditProfileClicked);
			UserManager.Instance.OnUserInfoUpdated += OnUserInfoUpdated;
		}

		private void OnDisable()
		{
			EditProfileButton.onClick.RemoveListener(OnEditProfileClicked);

			if (UserManager.Instance != null)
			{
				UserManager.Instance.OnUserInfoUpdated -= OnUserInfoUpdated;
			}
		}

		#endregion

		#region PublicMethods

		public async void Initialize(CustomUser user)
		{
			mUser = user;
			ProfileListingPanel.Initialize(user);

			BannerImage.sprite = null;
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

			JoinedText.gameObject.SetActive(user.createdAt.HasValue);
			if (user.createdAt != null)
			{
				JoinedText.text = user.createdAt.Value.ToShortDateString();
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate(LeftPartVerticalLayout.GetComponent<RectTransform>());

			int followers = await DataManager.Instance.GetCountUserFollowers(user.EthAddress);
			int following = await DataManager.Instance.GetCountUserFollowings(user.EthAddress);
			FollowersCountText.text = followers.ToString();
			FollowingCountText.text = following.ToString();

			EditProfileButton.gameObject.SetActive(user.EthAddress == UserManager.Instance.CurrentUserAddress);
			bool isFollowing = DataManager.Instance.IsUserFollowing(user.EthAddress);

			FollowButton.gameObject.SetActive(user.EthAddress != UserManager.Instance.CurrentUserAddress);
			FollowButton.Initialize(isFollowing, user.EthAddress, RefreshFollowers);


			await AvatarImage.Initialize(user);
			await ImageUtils.DownloadAndApplyWholeImage(user.BannerUrl, BannerImage);
		}

		#endregion

		#region PrivateMethods

		private async void RefreshFollowers()
		{
			int followers = await DataManager.Instance.GetCountUserFollowers(mUser.EthAddress);
			int following = await DataManager.Instance.GetCountUserFollowings(mUser.EthAddress);
			FollowersCountText.text = followers.ToString();
			FollowingCountText.text = following.ToString();
		}

		private void OnUserInfoUpdated(CustomUser user)
		{
			Initialize(user);
		}

		private void OnEditProfileClicked()
		{
			CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.EditProfile);
		}

		#endregion
	}
}
