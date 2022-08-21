using Cysharp.Threading.Tasks;
using UnityEngine;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Utils.Extensions;

namespace VoxToVFXFramework.Scripts.Utils.Image
{
	public static class ImageUtils
	{
		public static async UniTask<bool> DownloadAndApplyImage(string imageUrl, UnityEngine.UI.Image image, int maxWidth, bool preserveAspect = true, bool keepPermanentMedia = true, bool cropToCenter = false)
		{
			if (!string.IsNullOrEmpty(imageUrl))
			{
				Texture2D texture = await MediaManager.Instance.DownloadImage(imageUrl, maxWidth, keepPermanentMedia, cropToCenter);
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

		public static async UniTask<bool> DownloadAndApplyImageAndCropAfter(string imageUrl, UnityEngine.UI.Image image, int maxWidth, int sizeCrop, bool preserveAspect = true, bool keepPermanentMedia = true)
		{
			if (!string.IsNullOrEmpty(imageUrl))
			{
				Texture2D texture = await MediaManager.Instance.DownloadImage(imageUrl, maxWidth, keepPermanentMedia, false);
				if (texture != null)
				{
					texture = texture.ResampleAndCrop(sizeCrop, sizeCrop);
					image.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
					image.preserveAspect = preserveAspect;
					return true;
				}

				Debug.LogWarning("[ImageUtils] Failed to get logo texture: " + imageUrl);
				return false;
			}

			return false;
		}

		public static async UniTask<bool> DownloadAndApplyWholeImage(string imageUrl, UnityEngine.UI.Image image)
		{
			if (!string.IsNullOrEmpty(imageUrl))
			{
				Texture2D texture = await MediaManager.Instance.DownloadImage(imageUrl, int.MaxValue, true, false);
				RectTransform rt = image.GetComponent<RectTransform>();
				texture = texture.ResampleAndCrop((int)rt.rect.width, (int)rt.rect.height);

				image.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
				image.preserveAspect = false;
				return true;
			}

			return false;
		}

	}
}
