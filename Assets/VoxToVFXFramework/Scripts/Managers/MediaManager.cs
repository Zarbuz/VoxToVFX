using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.Utils.Extensions;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class MediaManager : SimpleSingleton<MediaManager>
	{
		public class RefTexture
		{
			public Texture2D Texture;
		}

		#region Fields

		private Dictionary<string, RefTexture> mCachedTexture = new Dictionary<string, RefTexture>();
		private readonly Dictionary<string, List<Action<Texture2D>>> mDownloadingQueue = new Dictionary<string, List<Action<Texture2D>>>();

		#endregion

		#region ConstStatic

		public const int MAX_CACHED_SIZE = 35;
		public const int CACHED_SIZE_WHEN_CLEAN = 30;

		#endregion

		#region PublicMethods


		public async UniTask<Texture2D> DownloadImage(string imageUrl, bool keepPermanent, bool cropToCenter, int cropSizeWidth, int cropSizeHeight)
		{
			return await DownloadImageStandalone(imageUrl, keepPermanent, cropToCenter, cropSizeWidth, cropSizeHeight); 
		}

		#endregion

		#region PrivateMethods

		private async UniTask<Texture2D> DownloadImageStandalone(string imageUrl, bool keepPermanent, bool cropToCenter, int cropSizeWidth, int cropSizeHeight)
		{
			if (string.IsNullOrEmpty(imageUrl))
			{
				Debug.LogError("wrong url, abort");
				return null;
			}


			string imageUrlCache = imageUrl;
			if (cropToCenter)
			{
				imageUrlCache += "&cropped_" + cropSizeWidth + "x" + cropSizeHeight;
			}

			//we first check the runtime memory cache
			if (mCachedTexture.TryGetValue(imageUrlCache, out RefTexture reference))
			{
				if (reference != null)
				{
					return reference.Texture;
				}

				mCachedTexture.Remove(imageUrlCache);
			}


			Texture2D texture;
			if (File.Exists(GetLocalFilePath(imageUrlCache)))
			{
				texture = await BackendMediaManager.Instance.DownloadTexture("file:///" + GetLocalFilePath(imageUrl));
			}
			else
			{
				texture = await BackendMediaManager.Instance.DownloadTexture(imageUrl);
				SaveTexture(texture, imageUrlCache);
			}

			texture.name = "hello";

			if (cropToCenter)
			{
				texture = texture.ResampleAndCrop(cropSizeWidth, cropSizeHeight);
			}

			if (!mCachedTexture.ContainsKey(imageUrlCache) && keepPermanent)
			{
				mCachedTexture.Add(imageUrlCache, new RefTexture()
				{
					Texture = texture,
				});
			}
		

			return texture;
		}

		private void SaveTexture(Texture2D text, string imageUrl)
		{
			string path = GetLocalFilePath(imageUrl);
			if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "ImgCache")))
			{
				Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "ImgCache"));
			}

			if (imageUrl.EndsWith("png"))
			{
				SaveTextureAsPNG(text, path);
			}
			else
			{
				SaveTextureAsJPG(text, path);
			}
		}

		private static void SaveTextureAsPNG(Texture2D texture, string fullPath)
		{
			byte[] bytes = texture.EncodeToPNG();
			File.WriteAllBytes(fullPath, bytes);
		}

		private static void SaveTextureAsJPG(Texture2D texture, string fullPath)
		{
			byte[] bytes = texture.EncodeToJPG();
			File.WriteAllBytes(fullPath, bytes);
		}

		private static string GetCleanImageNameFromUrl(string imageUrl)
		{
			string cleanImageName = imageUrl.Replace("https://", string.Empty);
			cleanImageName = cleanImageName.Replace("/", string.Empty);
			cleanImageName = cleanImageName.Replace("ipfs.moralis.io:2053", string.Empty);
			return cleanImageName;
		}

		private static string GetLocalFilePath(string imageUrl)
		{
			string cleanImageName = GetCleanImageNameFromUrl(imageUrl);
			return Path.Combine(Application.persistentDataPath, "ImgCache", cleanImageName);
		}
		
	
		#endregion
	}
}
