using MoralisUnity;
using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.UI.Login;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using VoxToVFXFramework.Scripts.Utils.Image;

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
		[SerializeField] private TextMeshProUGUI WalletBalanceText;
		[SerializeField] private TextMeshProUGUI MarketplaceBalanceText;

		#endregion

		#region UnityMethods

		private void Awake()
		{
			Moralis.Start();
		}

		private async void OnEnable()
		{
			OpenProfileButton.onClick.AddListener(OnOpenProfileClicked);
			ConnectWalletButton.onClick.AddListener(OnConnectWalletClicked);
			CreateItemButton.onClick.AddListener(OnCreateItemClicked);
			LoginPanel.OnWalletConnected += OnWalletConnected;
			await RefreshToolbar();
		}

		private void OnDisable()
		{
			OpenProfileButton.onClick.RemoveListener(OnOpenProfileClicked);
			ConnectWalletButton.onClick.RemoveListener(OnConnectWalletClicked);
			CreateItemButton.onClick.RemoveListener(OnCreateItemClicked);
			LoginPanel.OnWalletConnected -= OnWalletConnected;
		}

		#endregion

		#region PrivateMethods

		private async UniTask RefreshToolbar()
		{
			CustomUserDTO user = UserManager.Instance.CurrentUser;
			ConnectWalletButton.gameObject.SetActive(user == null);
			OpenProfileButton.gameObject.SetActive(user != null);
			CreateItemButton.gameObject.SetActive(user != null);

			if (user != null)
			{
				NoAvatarImage.gameObject.SetActive(string.IsNullOrEmpty(user.PictureUrl));
				AvatarImage.gameObject.SetActive(!string.IsNullOrEmpty(user.PictureUrl));
				if (!string.IsNullOrEmpty(user.PictureUrl))
				{
					bool success = await ImageUtils.DownloadAndApplyImage(user.PictureUrl, AvatarImage, 128, true, true, true);
				}

				UserNameText.text = "@" + user.username;
				NameText.text = user.Name;
			}
		}

		private void OnOpenProfileClicked()
		{
			ProfilePopup.SetActiveSafe(!ProfilePopup.activeSelf);
		}

		private void OnConnectWalletClicked()
		{
			CanvasPlayerPCManager.Instance.PauseLockedState = true;
			CanvasPlayerPCManager.Instance.GenericTogglePanel(CanvasPlayerPCState.Login);
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
