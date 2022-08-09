using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Platform.Services.ClientServices;
using UnityEngine;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class FileManager : SimpleSingleton<FileManager>
	{
		#region PublicMethods

		public async UniTask<string> UploadFile(string filePath)
		{
			Debug.Log("[FileManager] Start to upload file: " + filePath);
			string filename = Path.GetFileName(filePath);
			string filteredName = Regex.Replace(filename, @"\s", "");

			byte[] data = await File.ReadAllBytesAsync(filePath);

			//Dictionary<string, object> parameters = new Dictionary<string, object>();
			//parameters.Add("content", Convert.ToBase64String(data));
			//string url = await Moralis.Cloud.RunAsync<string>("uploadToIPFS", parameters);

			string url = await SaveImageToIpfs(filteredName, data);
			Debug.Log(url);
			return url;
		}

		public async UniTask<List<string>> UploadMultipleFiles(List<string> filePaths)
		{
			try
			{
				List<string> result = new List<string>();

				foreach (string filePath in filePaths)
				{
					string filename = Path.GetFileName(filePath);
					string filteredName = Regex.Replace(filename, @"\s", "");

					byte[] data = await File.ReadAllBytesAsync(filePath);

					string url = await SaveToIpfs(filteredName, Convert.ToBase64String(data));
					if (url != null)
					{
						result.Add(url);
					}
				}

				foreach (string path in result)
				{
					Debug.Log("[FileManager] Url: " + path);
				}

				return result;
			}
			catch (Exception exp)
			{
				Debug.LogError($"IPFS Save failed: {exp.Message}");
				return null;
			}
		}

		public async UniTask<string> SaveToIpfs(string name, string data)
		{
			string pinPath = null;

			try
			{
				IpfsFileRequest request = new IpfsFileRequest()
				{
					Path = name,
					Content = data
				};

				List<IpfsFileRequest> requests = new List<IpfsFileRequest> { request };
				List<IpfsFile> resp = await Moralis.GetClient().Web3Api.Storage.UploadFolder(requests);

				IpfsFile ipfs = resp.FirstOrDefault();
				if (ipfs != null)
				{
					pinPath = ipfs.Path;
				}
			}
			catch (Exception exp)
			{
				Debug.LogError($"IPFS Save failed: {exp.Message}");
			}

			return pinPath;
		}

		#endregion

		#region PrivateMethods

		private async UniTask<string> SaveImageToIpfs(string name, byte[] imageData)
		{
			return await SaveToIpfs(name, Convert.ToBase64String(imageData));
		}




		#endregion
	}
}
