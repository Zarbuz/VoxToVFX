using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class FileManager : SimpleSingleton<FileManager>
	{
		#region PublicMethods

		public async UniTask<string> UploadFile(string filePath)
		{
			string filename = Path.GetFileNameWithoutExtension(filePath);
			string filteredName = Regex.Replace(filename, @"\s", "");
			byte[] data = await File.ReadAllBytesAsync(filePath);

			string ipfsImagePath = await SaveImageToIpfs(filteredName, data);

			if (string.IsNullOrEmpty(ipfsImagePath))
			{
				Debug.LogError("Failed to save image to IPFS");
				return null;
			}

			Debug.Log("Image file saved successfully to IPFS:" + ipfsImagePath);
			return ipfsImagePath;
		}

		public async UniTask<string> SaveImageToIpfs(string name, byte[] imageData)
		{
			return await SaveToIpfs(name, Convert.ToBase64String(imageData));
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

				IpfsFile ipfs = resp.FirstOrDefault<IpfsFile>();
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
	}
}
