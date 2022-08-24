using System.Numerics;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Web3Api.Models;
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

		[SerializeField] private Button HomeButton;

		[SerializeField] private Button OpenProfilePopupButton;
		[SerializeField] private Button ConnectWalletButton;
		[SerializeField] private Button CreateItemButton;
		[SerializeField] private Image NoAvatarImageTopbar;
		[SerializeField] private Image AvatarImageTopbar;
		[SerializeField] private GameObject Spinner;

		[Header("ProfilePopup")]
		[SerializeField] private GameObject ProfilePopup;

		[SerializeField] private Image CircleImage;
		[SerializeField] private Image NoAvatarImage;
		[SerializeField] private Image AvatarImage;
		[SerializeField] private TextMeshProUGUI NameText;
		[SerializeField] private TextMeshProUGUI UserNameText;
		[SerializeField] private Button OpenProfileButton;
		[SerializeField] private Button SettingsButton;
		[SerializeField] private Button LogoutButton;
		[SerializeField] private TextMeshProUGUI WalletBalanceText;
		[SerializeField] private TextMeshProUGUI MarketplaceBalanceText;
		[SerializeField] private TextMeshProUGUI WalletAddressText;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			HomeButton.onClick.AddListener(OnHomeClicked);
			OpenProfilePopupButton.onClick.AddListener(OnOpenPopupProfileClicked);
			ConnectWalletButton.onClick.AddListener(OnConnectWalletClicked);
			CreateItemButton.onClick.AddListener(OnCreateItemClicked);
			OpenProfileButton.onClick.AddListener(OnOpenProfileClicked);
			SettingsButton.onClick.AddListener(OnSettingsClicked);
			LogoutButton.onClick.AddListener(OnLogoutClicked);
			LoginPanel.OnWalletConnected += OnWalletConnected;
			UserManager.Instance.OnUserInfoUpdated += OnUserInfoRefresh;
			ProfilePopup.gameObject.SetActive(false);

			RefreshToolbar();
		}

		private void OnDisable()
		{
			HomeButton.onClick.RemoveListener(OnHomeClicked);

			OpenProfilePopupButton.onClick.RemoveListener(OnOpenPopupProfileClicked);
			ConnectWalletButton.onClick.RemoveListener(OnConnectWalletClicked);
			CreateItemButton.onClick.RemoveListener(OnCreateItemClicked);
			OpenProfileButton.onClick.RemoveListener(OnOpenProfileClicked);
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

		private async void RefreshToolbar()
		{
			CustomUser user = UserManager.Instance.CurrentUser;
			ConnectWalletButton.gameObject.SetActive(user == null);
			OpenProfilePopupButton.gameObject.SetActive(user != null);
			CreateItemButton.gameObject.SetActive(user != null);

			if (user != null)
			{
				LockOpenProfileButton(true);

				NoAvatarImage.gameObject.SetActive(true);
				NoAvatarImageTopbar.gameObject.SetActive(true);

				UserNameText.text = "@" + user.UserName;
				NameText.text = user.Name;

				if (!string.IsNullOrEmpty(user.PictureUrl))
				{
					bool success = await ImageUtils.DownloadAndApplyImageAndCropAfter(user.PictureUrl, AvatarImage, 256, 256);
					if (success)
					{
						AvatarImageTopbar.sprite = AvatarImage.sprite;

						UpdateAvatarDisplay(true);
					}
					else
					{
						UpdateAvatarDisplay(false);
					}
				}
				else
				{
					UpdateAvatarDisplay(false);
				}

				WalletAddressText.text = user.EthAddress.FormatEthAddress(4);

				// Retrienve the user's native balance;
				NativeBalance balanceResponse = await Moralis.Web3Api.Account.GetNativeBalance(user.EthAddress, Moralis.CurrentChain.EnumValue);

				double balance = 0.0;
				float decimals = Moralis.CurrentChain.Decimals * 1.0f;
				string sym = Moralis.CurrentChain.Symbol;

				// Make sure a response to the balanace request weas received. The 
				// IsNullOrWhitespace check may not be necessary ...
				if (balanceResponse != null && !string.IsNullOrWhiteSpace(balanceResponse.Balance))
				{
					double.TryParse(balanceResponse.Balance, out balance);
				}

				// Display native token amount token in fractions of token.
				// NOTE: May be better to link this to chain since some tokens may have
				// more than 18 sigjnificant figures.
				WalletBalanceText.text = $"{(balance / Mathf.Pow(10.0f, decimals)):0.####} {sym}";
				MarketplaceBalanceText.text = $"{(UserManager.Instance.AccountInfoContractType.AvailableFethBalance / (BigInteger)Mathf.Pow(10.0f, decimals)):0.####} {sym}";
				LockOpenProfileButton(false);
			}
			else
			{
				Spinner.gameObject.SetActive(false);
			}
		}

		private void OnHomeClicked()
		{
			CanvasPlayerPCManager.Instance.GenericClosePanel();
		}

		private void UpdateAvatarDisplay(bool imageAvatarFound)
		{
			AvatarImage.transform.parent.gameObject.SetActive(imageAvatarFound);
			AvatarImageTopbar.transform.parent.gameObject.SetActive(imageAvatarFound);

			NoAvatarImage.gameObject.SetActive(!imageAvatarFound);
			NoAvatarImageTopbar.gameObject.SetActive(!imageAvatarFound);
		}

		private void LockOpenProfileButton(bool isLocked)
		{
			Spinner.gameObject.SetActive(isLocked);
			OpenProfilePopupButton.gameObject.SetActive(!isLocked);
		}

		private void OnUserInfoRefresh(CustomUser customUser)
		{
			RefreshToolbar();
		}

		private void OnOpenPopupProfileClicked()
		{
			ProfilePopup.SetActiveSafe(!ProfilePopup.activeSelf);
			RefreshCircle();
		}

		private void OnOpenProfileClicked()
		{
			ProfilePopup.gameObject.SetActive(false);
			RefreshCircle();
			CanvasPlayerPCManager.Instance.PauseLockedState = true;
			CanvasPlayerPCManager.Instance.OpenProfilePanel(UserManager.Instance.CurrentUser);
		}

		private void OnConnectWalletClicked()
		{
			CanvasPlayerPCManager.Instance.PauseLockedState = true;
			CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Login);
		}

		private void OnSettingsClicked()
		{
			CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Settings);
			ProfilePopup.gameObject.SetActive(false);
			RefreshCircle();
		}

		private async void OnLogoutClicked()
		{
			await UserManager.Instance.Logout();
			CanvasPlayerPCManager.Instance.GenericClosePanel();
			ProfilePopup.gameObject.SetActive(false);
		}

		private void RefreshCircle()
		{
			CircleImage.color = ProfilePopup.activeSelf ? Color.black : new Color(242 / 255f, 242 / 255f, 242 / 255f);
		}

		private void OnWalletConnected()
		{
			RefreshToolbar();
		}

		private void OnCreateItemClicked()
		{
			CanvasPlayerPCManager.Instance.OpenCollectionPanel();
		}

		#endregion
	}
}
