using System;
using System.Collections.Generic;
using System.IO;

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
	}
}
