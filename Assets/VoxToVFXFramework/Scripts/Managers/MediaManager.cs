using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using Object = UnityEngine.Object;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class MediaManager : SimpleSingleton<MediaManager>
	{
		public class RefTexture
		{
			public int Count = 1;
			public Texture2D Texture;

			//indicate if we can GC collect it
			public bool DoNotGC = false;
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

		public static Texture2D CheckMaxSize(Texture2D text, int maxWidth)
		{
			if (text == null)
			{
				return text;
			}

			if (text.width > maxWidth)
			{
				Texture2D res = Resize(text, maxWidth, Mathf.FloorToInt(maxWidth * (text.height / (float)text.width)));
				GameObject.Destroy(text);
				return res;
			}

			return text;
		}

		public async UniTask<Texture2D> DownloadImage(string imageUrl, int maxWidth, bool cropToCenter)
		{
			return await DownloadImage(imageUrl, maxWidth, false, cropToCenter);
		}

		public async UniTask<Texture2D> DownloadImage(string imageUrl, int maxWidth, bool keepPermanent, bool cropToCenter)
		{
			return await DownloadImageStandalone(imageUrl, maxWidth, keepPermanent, cropToCenter);
		}

		public void GC()
		{
			Dictionary<string, RefTexture> spritesWeKeepDict = new Dictionary<string, RefTexture>();
			if (mCachedTexture.Count(el => !el.Value.DoNotGC) > MAX_CACHED_SIZE)
			{
				spritesWeKeepDict = mCachedTexture.Where((el) => !el.Value.DoNotGC).OrderByDescending((el) => el.Value.Count)
					.Take(CACHED_SIZE_WHEN_CLEAN)
					.ToDictionary((el) => el.Key, (el) => el.Value);

				foreach (KeyValuePair<string, RefTexture> el in mCachedTexture.Where((el) => el.Value.DoNotGC))
				{
					spritesWeKeepDict.Add(el.Key, el.Value);
				}

				foreach (KeyValuePair<string, RefTexture> reference in mCachedTexture.Where(reference => !spritesWeKeepDict.ContainsKey(reference.Key)))
				{
					Object.Destroy(reference.Value.Texture);
				}

				mCachedTexture = spritesWeKeepDict;

				Debug.Log("[Media] GC done");
			}
		}

		#endregion

		#region PrivateMethods

		private static Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
		{
			RenderTexture rt = new RenderTexture(targetX, targetY, 24);
			RenderTexture.active = rt;
			Graphics.Blit(texture2D, rt);
			Texture2D result = new Texture2D(targetX, targetY);
			result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
			result.Apply();
			Object.Destroy(rt);
			return result;
		}

		private async UniTask<Texture2D> DownloadImageStandalone(string imageUrl, int maxWidth, bool keepPermanent, bool cropToCenter)
		{
			if (string.IsNullOrEmpty(imageUrl))
			{
				Debug.LogError("wrong url, abort");
				return null;
			}

			//we first check the runtime memory cache
			if (mCachedTexture.TryGetValue(imageUrl, out RefTexture reference))
			{
				if (reference != null)
				{
					reference.Count++;
					return reference.Texture;
				}

				mCachedTexture.Remove(imageUrl);
			}

			Texture2D textRes = await BackendMediaManager.Instance.DownloadTexture(imageUrl);
			textRes = CheckMaxSize(textRes, maxWidth);
			textRes.name = "hello";

			if (cropToCenter)
			{
				textRes = textRes.ResampleAndCrop(maxWidth, maxWidth);
			}

			if (!mCachedTexture.ContainsKey(imageUrl))
			{
				mCachedTexture.Add(imageUrl, new RefTexture()
				{
					Texture = textRes,
					DoNotGC = keepPermanent,
				});
			}
		

			return textRes;
		}
		
	
		#endregion
	}
}
