using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Unity.Collections;
using UnityEngine;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using Debug = UnityEngine.Debug;

public class VoxelDataCreatorManager : ModuleSingleton<VoxelDataCreatorManager>
{
	#region Fields

	public event Action<int, float> LoadProgressCallback;
	public event Action LoadFinishedCallback;

	private string mOutputPath;
	private string mInputFileName;
	private WorldData mWorldData;
	private readonly List<ChunkDataFile> mChunksWrited = new List<ChunkDataFile>();

	private const string IMPORT_TMP_FOLDER_NAME = "import_tmp";
	private const string EXTRACT_TMP_FOLDER_NAME = "extract_tmp";
	private const string INFO_FILE_NAME = "info.json";
	#endregion

	#region PublicMethods

	public void CreateZipFile(string inputPath, string outputPath)
	{
		mInputFileName = Path.GetFileNameWithoutExtension(inputPath);
		mOutputPath = outputPath;
		CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Loading);
		mChunksWrited.Clear();
		StartCoroutine(VoxImporter.LoadVoxModelAsync(inputPath, OnLoadFrameProgress, OnVoxLoadFinished));
	}

	public void ReadZipFile(string inputPath)
	{
		RuntimeVoxManager.Instance.Release();
		string tmpPath = Path.Combine(Application.persistentDataPath, IMPORT_TMP_FOLDER_NAME);
		string infoPath = Path.Combine(Application.persistentDataPath, INFO_FILE_NAME);
		string checksum = GetMd5Checksum(inputPath);

		if (File.Exists(infoPath))
		{
			string json = File.ReadAllText(infoPath);
			InfoFileDTO infoFile = JsonUtility.FromJson<InfoFileDTO>(json);
			if (infoFile.LastMd5FileOpen == checksum)
			{
				StartCoroutine(StartReadImportFilesCo(tmpPath));
				return;
			}
		}

		InfoFileDTO infoFileUpdate = new InfoFileDTO();
		infoFileUpdate.LastMd5FileOpen = checksum;
		string jsn = JsonUtility.ToJson(infoFileUpdate);
		File.WriteAllText(infoPath, jsn);

		if (!Directory.Exists(tmpPath))
		{
			Directory.CreateDirectory(tmpPath);
		}
		else
		{
			CleanFolder(tmpPath);
		}
		ZipFile.ExtractToDirectory(inputPath, Path.Combine(Application.persistentDataPath, IMPORT_TMP_FOLDER_NAME));
		StartCoroutine(StartReadImportFilesCo(tmpPath));

	}

	#endregion

	#region PrivateMethods

	private string GetMd5Checksum(string filepath)
	{
		using MD5 md5 = MD5.Create();
		using FileStream stream = File.OpenRead(filepath);
		byte[] hash = md5.ComputeHash(stream);
		return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
	}

	private IEnumerator StartReadImportFilesCo(string tmpPath)
	{
		string[] structureFiles = Directory.GetFiles(tmpPath, "*.structure", SearchOption.AllDirectories);
		if (structureFiles.Length != 1)
		{
			Debug.LogError("No structure file found, abort reading.");
			yield break;
		}

		CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Loading);
		List<string> files = ReadStructureFile(structureFiles[0]);

		for (int index = 0; index < RuntimeVoxManager.Instance.Chunks.Length; index++)
		{
			Chunk chunk = RuntimeVoxManager.Instance.Chunks[index];
			ReadChunkDataFile(chunk.ChunkIndex, chunk.LodLevel, files[index]);
			LoadProgressCallback?.Invoke(1, index / (float)RuntimeVoxManager.Instance.Chunks.Length);
			yield return new WaitForEndOfFrame();
		}

		
		RuntimeVoxManager.Instance.OnChunkLoadedFinished();
	}
	
	private List<string> ReadStructureFile(string filePath)
	{
		List<string> files = new List<string>();
		using FileStream stream = File.Open(filePath, FileMode.Open);
		using BinaryReader reader = new BinaryReader(stream);
		int chunkLength = reader.ReadInt32();

		Chunk[] chunks = new Chunk[chunkLength];
		for (int i = 0; i < chunkLength; i++)
		{
			Chunk chunk = new Chunk();
			chunk.ChunkIndex = reader.ReadInt32();
			chunk.LodLevel = reader.ReadInt32();
			chunk.Length = reader.ReadInt32();
			chunk.Position = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
			files.Add(reader.ReadString());
			chunks[i] = chunk;
		}
		RuntimeVoxManager.Instance.SetChunks(chunks);
		int materialLength = reader.ReadInt32();

		VoxelMaterialVFX[] materials = new VoxelMaterialVFX[materialLength];
		for (int i = 0; i < materialLength; i++)
		{
			VoxelMaterialVFX mat = new VoxelMaterialVFX();
			mat.color = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			mat.emission = reader.ReadSingle();
			mat.metallic = reader.ReadSingle();
			mat.smoothness = reader.ReadSingle();
			materials[i] = mat;
		}

		RuntimeVoxManager.Instance.SetMaterials(materials);
		return files;
	}


	private void ReadChunkDataFile(int chunkIndex, int lodLevel, string filename)
	{
		string filePath = Path.Combine(Application.persistentDataPath, IMPORT_TMP_FOLDER_NAME, filename);
		using FileStream stream = File.Open(filePath, FileMode.Open);
		using BinaryReader reader = new BinaryReader(stream);

		int length = reader.ReadInt32();
		NativeArray<VoxelVFX> data = new NativeArray<VoxelVFX>(length, Allocator.Temp);
		for (int i = 0; i < length; i++)
		{
			data[i] = new VoxelVFX()
			{
				position = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()),
				paletteIndex = reader.ReadInt32(),
				lodLevel = lodLevel
			};
		}

		RuntimeVoxManager.Instance.SetVoxelChunk(chunkIndex, lodLevel, data);
		data.Dispose();
	}

	private void OnLoadFrameProgress(float progress)
	{
		LoadProgressCallback?.Invoke(1, progress);
	}

	private void OnVoxLoadFinished(WorldData worldData)
	{
		if (worldData == null)
		{
			Debug.LogError("[RuntimeVoxManager] Failed to load vox model");
			return;
		}

		mWorldData = worldData;
		Debug.Log("[RuntimeVoxController] OnVoxLoadFinished");
		string tmpPath = Path.Combine(Application.persistentDataPath, EXTRACT_TMP_FOLDER_NAME);
		if (!Directory.Exists(tmpPath))
		{
			Directory.CreateDirectory(tmpPath);
		}
		else
		{
			CleanFolder(tmpPath);
		}
		StartCoroutine(worldData.ComputeLodsChunks(OnChunkLoadResult, OnChunkLoadedFinished));
	}

	private void CleanFolder(string folderPath)
	{
		string tmpPath = Path.Combine(folderPath);
		DirectoryInfo di = new DirectoryInfo(tmpPath);
		foreach (FileInfo file in di.GetFiles())
		{
			file.Delete();
		}
	}

	private void OnChunkLoadResult(float progress, VoxelResult voxelResult)
	{
		LoadProgressCallback?.Invoke(2, progress);

		if (voxelResult.Data.Length == 0)
		{
			return;
		}

		string fileName = $"{mInputFileName}_{voxelResult.LodLevel}_{voxelResult.FrameWorldPosition.x}_{voxelResult.FrameWorldPosition.y}_{voxelResult.FrameWorldPosition.z}.data";

		using (FileStream stream = File.Open(Path.Combine(Application.persistentDataPath, EXTRACT_TMP_FOLDER_NAME, fileName), FileMode.Create))
		{
			using BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(voxelResult.Data.Length);
			for (int i = 0; i < voxelResult.Data.Length; i++)
			{
				binaryWriter.Write((int)voxelResult.Data[i].x);
				binaryWriter.Write((int)voxelResult.Data[i].y);
				binaryWriter.Write((int)voxelResult.Data[i].z);
				binaryWriter.Write((int)voxelResult.Data[i].w);
			}
		}

		ChunkDataFile chunk = new ChunkDataFile
		{
			ChunkIndex = voxelResult.ChunkIndex,
			Filename = fileName,
			Position = voxelResult.FrameWorldPosition,
			LodLevel = voxelResult.LodLevel,
			Length = voxelResult.Data.Length
		};
		mChunksWrited.Add(chunk);
	}

	private void WriteStructureFile()
	{
		using FileStream stream = File.Open(Path.Combine(Application.persistentDataPath, EXTRACT_TMP_FOLDER_NAME, mInputFileName + ".structure"), FileMode.Create);
		using BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(mChunksWrited.Count);
		foreach (ChunkDataFile chunk in mChunksWrited)
		{
			binaryWriter.Write(chunk.ChunkIndex);
			binaryWriter.Write(chunk.LodLevel);
			binaryWriter.Write(chunk.Length);
			binaryWriter.Write((int)chunk.Position.x);
			binaryWriter.Write((int)chunk.Position.y);
			binaryWriter.Write((int)chunk.Position.z);
			binaryWriter.Write(chunk.Filename);
		}

		binaryWriter.Write(VoxImporter.Materials.Length);
		for (int i = 0; i < VoxImporter.Materials.Length; i++)
		{
			VoxelMaterialVFX mat = VoxImporter.Materials[i];
			binaryWriter.Write(mat.color.x);
			binaryWriter.Write(mat.color.y);
			binaryWriter.Write(mat.color.z);
			binaryWriter.Write(mat.emission);
			binaryWriter.Write(mat.metallic);
			binaryWriter.Write(mat.smoothness);
		}
	}

	private void OnChunkLoadedFinished()
	{
		WriteStructureFile();
		VoxImporter.Materials = null;
		mWorldData.Dispose();
		string path = Path.Combine(Application.persistentDataPath, EXTRACT_TMP_FOLDER_NAME);
		if (File.Exists(mOutputPath))
		{
			File.Delete(mOutputPath);
		}
		ZipFile.CreateFromDirectory(path, mOutputPath, CompressionLevel.Optimal, false, Encoding.UTF8);
		Process.Start(mOutputPath);
		CleanFolder(path);
		LoadFinishedCallback?.Invoke();
	}

	#endregion

	[Serializable]
	public class InfoFileDTO
	{
		public string LastMd5FileOpen;
	}
}
