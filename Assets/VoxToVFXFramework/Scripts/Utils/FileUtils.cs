using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VoxToVFXFramework.Scripts.Utils
{
	public static class FileUtils
	{
		public static List<string> SplitFile(string inputFile, int chunkSize, string filename, string path)
		{
			const int BUFFER_SIZE = 20 * 1024;
			byte[] buffer = new byte[BUFFER_SIZE];

			List<string> outputList = new List<string>();
			using Stream input = File.OpenRead(inputFile);
			int index = 0;
			while (input.Position < input.Length)
			{
				string filePath = path + "\\" + filename + "_" + index;
				using (Stream output = File.Create(filePath))
				{
					outputList.Add(filePath);
					int remaining = chunkSize, bytesRead;
					while (remaining > 0 && (bytesRead = input.Read(buffer, 0, Math.Min(remaining, BUFFER_SIZE))) > 0)
					{
						output.Write(buffer, 0, bytesRead);
						remaining -= bytesRead;
					}
				}
				index++;
			}

			return outputList;
		}

		public static string Combine(byte[][] arrays, string filename)
		{
			byte[] bytes = new byte[arrays.Sum(a => a.Length)];
			int offset = 0;

			foreach (byte[] array in arrays)
			{
				Buffer.BlockCopy(array, 0, bytes, offset, array.Length);
				offset += array.Length;
			}

			string path = Path.Combine(Application.persistentDataPath, filename);
			File.WriteAllBytes(path, bytes); 

			return path;
		}
	}
}
