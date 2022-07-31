using System;
using System.IO;
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
		[SerializeField] private Button DeletePictureButton;

		[SerializeField] private Button SaveButton;


		#endregion

		#region Fields

		private string mProfilePictureUrl;

		#endregion

		#region UnityMethods

		private async void OnEnable()
		{
			SelectFileButton.onClick.AddListener(OnSelectFileClicked);
			DeletePictureButton.onClick.AddListener(OnDeletePictureClicked);
			SaveButton.onClick.AddListener(OnSaveClicked);
			await Refresh();
		}

		private void OnDisable()
		{
			SelectFileButton.onClick.RemoveListener(OnSelectFileClicked);
			DeletePictureButton.onClick.RemoveListener(OnDeletePictureClicked);
			SaveButton.onClick.RemoveListener(OnSaveClicked);
		}

		#endregion

		#region PrivateMethods

		private async UniTask Refresh()
		{
			MoralisUser moralisUser = await Moralis.GetUserAsync();
			CustomUserDTO customUser = await UserManager.Instance.LoadFromUser(moralisUser);
			if (customUser != null)
			{
				NameInputField.SetTextWithoutNotify(customUser.Name);
				UserNameInputField.SetTextWithoutNotify(customUser.username);
				BiographyInputField.SetTextWithoutNotify(customUser.Bio);

				DeletePictureButton.gameObject.SetActive(!string.IsNullOrEmpty(customUser.PictureUrl));
				SelectFileButton.gameObject.SetActive(string.IsNullOrEmpty(customUser.PictureUrl));
				PictureProfileImage.gameObject.SetActive(!string.IsNullOrEmpty(customUser.PictureUrl));

				if (!string.IsNullOrEmpty(customUser.PictureUrl))
				{
					await ImageUtils.DownloadAndApplyImage(customUser.PictureUrl, PictureProfileImage, 512, true, true, true);
				}
			}
		}

		private async void OnSelectFileClicked()
		{
			ExtensionFilter extensionFilters = new ExtensionFilter("Images", new[] { "png", "jpg", "jpeg", "gif" });
			string[] paths = StandaloneFileBrowser.OpenFilePanel("Select image", "", new []{extensionFilters}, false);
			if (paths.Length > 0)
			{
				await UploadSelectedFile(paths[0]);
			}
		}

		private async UniTask UploadSelectedFile(string path)
		{
			mProfilePictureUrl = await FileManager.Instance.UploadFile(path);
			if (!string.IsNullOrEmpty(mProfilePictureUrl))
			{
				byte[] data = await File.ReadAllBytesAsync(path);
				Texture2D texture = new Texture2D(2, 2);
				texture.SetPixelData(data, 0);
				texture = texture.ResampleAndCrop(256, 256);
				PictureProfileImage.gameObject.SetActive(true);
				PictureProfileImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
				PictureProfileImage.preserveAspect = true;

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
		}

		private void OnSaveClicked()
		{
			if (string.IsNullOrEmpty(UserNameInputField.text) || UserNameInputField.text.Length < 3)
			{
				MessagePopup.Show(LocalizationKeys.EDIT_PROFILE_MISSING_USERNAME.Translate());
				return;
			}


		}

		#endregion
	}
}
