using System;
using System.IO;
using System.Linq;
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
			await Refresh();
		}

		private void OnDisable()
		{
			SelectFileButton.onClick.RemoveListener(OnSelectFileClicked);
			DeletePictureButton.onClick.RemoveListener(OnDeletePictureClicked);
			SaveButton.onClick.RemoveListener(OnSaveClicked);
			CloseButton.onClick.RemoveListener(OnCloseClicked);
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
				NameInputField.SetTextWithoutNotify(customUser.Name);
				UserNameInputField.SetTextWithoutNotify(customUser.UserName);
				BiographyInputField.SetTextWithoutNotify(customUser.Bio);

				NoAvatarImage.gameObject.SetActive(string.IsNullOrEmpty(customUser.PictureUrl));
				DeletePictureButton.gameObject.SetActive(!string.IsNullOrEmpty(customUser.PictureUrl));
				SelectFileButton.gameObject.SetActive(string.IsNullOrEmpty(customUser.PictureUrl));
				PictureProfileImage.gameObject.SetActive(!string.IsNullOrEmpty(customUser.PictureUrl));

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
			SpinnerImage.gameObject.SetActive(true);
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

			await UpdateUserInfo();
		}

		private async UniTask UpdateUserInfo()
		{
			CustomUser currentUser = new CustomUser();
			MoralisUser moralisUser = await Moralis.GetUserAsync();
			currentUser.UserId = moralisUser.objectId;
			currentUser.PictureUrl = mProfilePictureUrl;
			currentUser.Bio = BiographyInputField.text;
			currentUser.UserName = UserNameInputField.text;
			currentUser.Name = NameInputField.text;
			bool success = await UserManager.Instance.UpdateUserInfo(currentUser);
			if (success)
			{
				Debug.Log("Successfully updated user info");
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_UPDATE_SUCCESS.Translate());
			}
			else
			{
				Debug.LogError("Failed to update user infos");
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_UPDATE_FAILED.Translate(), LogType.Error);
			}
		}

		private bool HasSpecialChars(string yourString)
		{
			return yourString.Any(ch => !char.IsLetterOrDigit(ch));
		}

		#endregion
	}
}
