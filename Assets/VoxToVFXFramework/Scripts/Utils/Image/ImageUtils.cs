using Cysharp.Threading.Tasks;
using UnityEngine;
using VoxToVFXFramework.Scripts.Managers;

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

			Debug.LogWarning("[ImageUtils] Media is null ");
			return false;
		}

	}
}
