using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class BackendMediaManager : SimpleSingleton<BackendMediaManager>
	{
		public async UniTask<Texture2D> DownloadTexture(string url)
		{
			using UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
			await www.SendWebRequest();

			if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
			{
				Debug.LogError("[DownloadTexture] url : " + url + " " + www.error);
				return null;
			}

			Texture2D tex2D = DownloadHandlerTexture.GetContent(www);
			return tex2D;
		}

	}
}
