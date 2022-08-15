using Cysharp.Threading.Tasks;
using SFB;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.UI.Popups;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using VoxToVFXFramework.Scripts.Utils.Image;

public class SelectImage : MonoBehaviour
{
	#region ScriptParameters

	[SerializeField] private Button SelectImageButton;
	[SerializeField] private Image Image;
	[SerializeField] private Image NoImageFound;
	[SerializeField] private Button DeleteImageButton;
	[SerializeField] private Image SpinnerImage;
	[SerializeField] private bool NoImageFoundActive;
	 
	#endregion

	#region ConstStatic

	public const int MAX_SIZE_IN_MB = 10;

	#endregion

	#region Fields

	public string ImageUrl { get; private set; }

	#endregion

	#region UnityMethods

	private void OnEnable()
	{
		SpinnerImage.gameObject.SetActive(false);
		SelectImageButton.onClick.AddListener(OnSelectImageClicked);
		DeleteImageButton.onClick.AddListener(OnDeleteImageClicked);
	}

	private void OnDisable()
	{
		SelectImageButton.onClick.RemoveListener(OnSelectImageClicked);
		DeleteImageButton.onClick.RemoveListener(OnDeleteImageClicked);
	}


	#endregion

	#region PublicMethods

	public async void Initialize(string imageUrl)
	{
		if (NoImageFoundActive)
		{
			NoImageFound.gameObject.SetActive(string.IsNullOrEmpty(imageUrl));
		}
		else
		{
			if (NoImageFound)
			{
				NoImageFound.gameObject.SetActive(false);
			}
		}

		DeleteImageButton.gameObject.SetActive(!string.IsNullOrEmpty(imageUrl));
		SelectImageButton.gameObject.SetActive(string.IsNullOrEmpty(imageUrl));
		Image.gameObject.SetActive(!string.IsNullOrEmpty(imageUrl));

		if (!string.IsNullOrEmpty(imageUrl))
		{
			await ImageUtils.DownloadAndApplyImage(imageUrl, Image, 512, true, true, true);
		}
		ImageUrl = imageUrl;
	}

	#endregion


	#region PrivateMethods

	private async void OnSelectImageClicked()
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

	private async UniTask UploadSelectedFile(string path)
	{
		ShowSpinnerImage(true);
		ImageUrl = await FileManager.Instance.UploadFile(path);
		if (!string.IsNullOrEmpty(ImageUrl))
		{
			SpinnerImage.gameObject.SetActive(false);
			byte[] data = await File.ReadAllBytesAsync(path);
			Texture2D texture = new Texture2D(2, 2);
			texture.LoadImage(data);
			texture.Apply(updateMipmaps: true);

			texture = texture.ResampleAndCrop(256, 256);
			Image.gameObject.SetActive(true);
			Image.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
			Image.preserveAspect = true;

			if (NoImageFoundActive)
			{
				NoImageFound.gameObject.SetActive(false);
			}

			SelectImageButton.gameObject.SetActive(false);
			DeleteImageButton.gameObject.SetActive(true);
		}
		else
		{
			ShowSpinnerImage(false);
		}
	}

	private void ShowSpinnerImage(bool showSpinner)
	{
		SpinnerImage.gameObject.SetActive(showSpinner);

		if (NoImageFoundActive)
		{
			NoImageFound.gameObject.SetActive(!showSpinner);
		}
	}

	private void OnDeleteImageClicked()
	{
		ImageUrl = string.Empty;
		SelectImageButton.gameObject.SetActive(true);
		Image.gameObject.SetActive(false);
		DeleteImageButton.gameObject.SetActive(false);

		if (NoImageFoundActive)
		{
			NoImageFound.gameObject.SetActive(true);
		}
	}

	#endregion
}
