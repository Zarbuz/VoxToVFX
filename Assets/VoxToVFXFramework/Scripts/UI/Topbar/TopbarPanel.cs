using MoralisUnity;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using MoralisUnity.Platform.Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.UI.Login;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using VoxToVFXFramework.Scripts.Utils.Image;
using WalletConnectSharp.Unity;

namespace VoxToVFXFramework.Scripts.UI.Topbar
{
	public class TopbarPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Button OpenProfileButton;
		[SerializeField] private Button ConnectWalletButton;
		[SerializeField] private Button CreateItemButton;

		[Header("ProfilePopup")]
		[SerializeField] private GameObject ProfilePopup;

		[SerializeField] private Image NoAvatarImage;
		[SerializeField] private Image AvatarImage;
		[SerializeField] private TextMeshProUGUI NameText;
		[SerializeField] private TextMeshProUGUI UserNameText;
		[SerializeField] private Button EditProfileButton;
		[SerializeField] private Button SettingsButton;
		[SerializeField] private Button LogoutButton;
		[SerializeField] private TextMeshProUGUI WalletBalanceText;
		[SerializeField] private TextMeshProUGUI MarketplaceBalanceText;
		[SerializeField] private TextMeshProUGUI WalletAddressText;

		#endregion

		#region UnityMethods

		private void Awake()
		{
			Moralis.Start();
		}

		private async void Start()
		{
			var user = await UserManager.Instance.LoadCurrentUser();
		}

		private async void OnEnable()
		{
			OpenProfileButton.onClick.AddListener(OnOpenProfileClicked);
			ConnectWalletButton.onClick.AddListener(OnConnectWalletClicked);
			CreateItemButton.onClick.AddListener(OnCreateItemClicked);
			EditProfileButton.onClick.AddListener(OnEditProfileClicked);
			SettingsButton.onClick.AddListener(OnSettingsClicked);
			LogoutButton.onClick.AddListener(OnLogoutClicked);
			LoginPanel.OnWalletConnected += OnWalletConnected;
			UserManager.Instance.OnUserInfoUpdated += OnUserInfoRefresh;
			ProfilePopup.gameObject.SetActive(false);

			await RefreshToolbar();
		}

		private void OnDisable()
		{
			OpenProfileButton.onClick.RemoveListener(OnOpenProfileClicked);
			ConnectWalletButton.onClick.RemoveListener(OnConnectWalletClicked);
			CreateItemButton.onClick.RemoveListener(OnCreateItemClicked);
			EditProfileButton.onClick.RemoveListener(OnEditProfileClicked);
			SettingsButton.onClick.RemoveListener(OnSettingsClicked);
			LogoutButton.onClick.RemoveListener(OnLogoutClicked);
			LoginPanel.OnWalletConnected -= OnWalletConnected;

			if (UserManager.Instance != null)
			{
				UserManager.Instance.OnUserInfoUpdated -= OnUserInfoRefresh;
			}
		}

		#endregion

		#region PrivateMethods

		private async UniTask RefreshToolbar()
		{
			CustomUser user = UserManager.Instance.CurrentUser;
			ConnectWalletButton.gameObject.SetActive(user == null);
			OpenProfileButton.gameObject.SetActive(user != null);
			CreateItemButton.gameObject.SetActive(user != null);

			if (user != null)
			{
				NoAvatarImage.gameObject.SetActive(string.IsNullOrEmpty(user.PictureUrl));
				AvatarImage.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(user.PictureUrl));
				if (!string.IsNullOrEmpty(user.PictureUrl))
				{
					bool success = await ImageUtils.DownloadAndApplyImage(user.PictureUrl, AvatarImage, 128, true, true, true);
				}

				MoralisUser moralisUser = await Moralis.GetUserAsync();

				UserNameText.text = "@" + user.UserName;
				NameText.text = user.Name;
				WalletAddressText.text = moralisUser.ethAddress.Substring(0, 4) + "..." + moralisUser.ethAddress.Substring(moralisUser.ethAddress.Length - 4);
			}
		}

		private void OnUserInfoRefresh(CustomUser customUser)
		{
			RefreshToolbar();
		}

		private void OnOpenProfileClicked()
		{
			ProfilePopup.SetActiveSafe(!ProfilePopup.activeSelf);
		}

		private void OnEditProfileClicked()
		{
			ProfilePopup.gameObject.SetActive(false);
			CanvasPlayerPCManager.Instance.PauseLockedState = true;
			CanvasPlayerPCManager.Instance.GenericTogglePanel(CanvasPlayerPCState.EditProfile);
		}

		private void OnConnectWalletClicked()
		{
			CanvasPlayerPCManager.Instance.PauseLockedState = true;
			CanvasPlayerPCManager.Instance.GenericTogglePanel(CanvasPlayerPCState.Login);
		}

		private void OnSettingsClicked()
		{
			CanvasPlayerPCManager.Instance.GenericTogglePanel(CanvasPlayerPCState.Settings);
			ProfilePopup.gameObject.SetActive(false);
		}

		private async void OnLogoutClicked()
		{
			await UserManager.Instance.Logout();
			CanvasPlayerPCManager.Instance.GenericClosePanel();
			ProfilePopup.gameObject.SetActive(false);
		}

		private void OnWalletConnected()
		{
			RefreshToolbar();
		}

		private void OnCreateItemClicked()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
