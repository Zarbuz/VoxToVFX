using Cysharp.Threading.Tasks;
using UnityEngine;
using VoxToVFXFramework.Scripts.Managers;

namespace VoxToVFXFramework.Scripts.Utils.Image
{
	public static class ImageUtils
	{
		public static async UniTask<bool> DownloadAndApplyImage(string imageUrl, UnityEngine.UI.Image image, bool preserveAspect = true, bool keepPermanentMedia = true)
		{
			if (!string.IsNullOrEmpty(imageUrl))
			{
				Texture2D texture = await MediaManager.Instance.DownloadImage(imageUrl, keepPermanentMedia, false, 0, 0);
				if (texture != null)
				{
					image.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
					image.preserveAspect = preserveAspect;
					return true;
				}

				Debug.LogWarning("[ImageUtils] Failed to get logo texture: " + imageUrl);
				return false;
			}

			return false;
		}

		public static async UniTask<bool> DownloadAndApplyImageAndCrop(string imageUrl, UnityEngine.UI.Image image, int cropSizeWidth, int cropSizeHeight, bool preserveAspect = true, bool keepPermanentMedia = true)
		{
			if (!string.IsNullOrEmpty(imageUrl))
			{
				Texture2D texture = await MediaManager.Instance.DownloadImage(imageUrl, keepPermanentMedia, true, cropSizeWidth, cropSizeHeight);
				if (texture != null)
				{
					image.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
					image.preserveAspect = preserveAspect;
					return true;
				}

				Debug.LogWarning("[ImageUtils] Failed to get logo texture: " + imageUrl);
				return false;
			}

			return false;
		}

		public static async UniTask<bool> DownloadAndApplyImageAndCrop(string imageUrl, UnityEngine.UI.RawImage image, int cropSizeWidth, int cropSizeHeight, bool keepPermanentMedia = true)
		{
			if (!string.IsNullOrEmpty(imageUrl))
			{
				Texture2D texture = await MediaManager.Instance.DownloadImage(imageUrl, keepPermanentMedia, true, cropSizeWidth, cropSizeHeight);
				if (texture != null)
				{
					image.texture = texture;
					return true;
				}

				Debug.LogWarning("[ImageUtils] Failed to get logo texture: " + imageUrl);
				return false;
			}

			return false;
		}

		public static async UniTask<bool> DownloadAndApplyWholeImage(string imageUrl, UnityEngine.UI.RawImage image)
		{
			if (!string.IsNullOrEmpty(imageUrl))
			{
				RectTransform rt = image.GetComponent<RectTransform>();
				Texture2D texture = await MediaManager.Instance.DownloadImage(imageUrl, true, true, (int)rt.rect.width, (int)rt.rect.height);
				image.texture = texture;
				return true;
			}

			return false;
		}
	}
}
