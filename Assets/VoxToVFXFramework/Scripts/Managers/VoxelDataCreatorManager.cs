using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using VoxToVFXFramework.Scripts.Converter;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Extensions;
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

	private string mCurrentInputFolder;
	private WorldData mWorldData;
	private readonly List<ChunkDataFile> mChunksWrited = new List<ChunkDataFile>();
	private readonly List<Task> mTaskList = new List<Task>();
	private const string EXTRACT_TMP_FOLDER_NAME = "extract_tmp";
	private int mReadCompleted;
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
		string checksum = GetMd5Checksum(inputPath);
		string inputFolder = Path.Combine(Application.persistentDataPath, checksum);
		mCurrentInputFolder = inputFolder;
		if (!Directory.Exists(inputFolder))
		{
			Directory.CreateDirectory(inputFolder);
			ZipFile.ExtractToDirectory(inputPath, Path.Combine(Application.persistentDataPath, inputFolder));
		}

		StartCoroutine(StartReadImportFilesCo(inputFolder));
	}

	public void OpenCacheFolder()
	{
		Process.Start(Application.persistentDataPath);
	}

	public void ClearCacheFolder()
	{
		//TODO: Add confirm popup
		//TODO: Add success message popup
		DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath);
		foreach (DirectoryInfo dir in di.EnumerateDirectories())
		{
			dir.Delete(true);
		}
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

		mTaskList.Clear();
		mReadCompleted = 0;
		for (int index = 0; index < RuntimeVoxManager.Instance.Chunks.Length; index++)
		{
			ChunkVFX chunkVFX = RuntimeVoxManager.Instance.Chunks[index];
			if (mTaskList.Count >= 10)
			{
				yield return new WaitUntil(CanContinueReadFiles);
			}
			Task lastTask = ReadChunkDataFile(index, files[index]);
			mTaskList.Add(lastTask);
		}

		yield return new WaitWhile(() => mReadCompleted != RuntimeVoxManager.Instance.Chunks.Length);
		RuntimeVoxManager.Instance.OnChunkLoadedFinished();
	}

	private IEnumerator RefreshLoadProgressCo()
	{
		LoadProgressCallback?.Invoke(1, mReadCompleted / (float)RuntimeVoxManager.Instance.Chunks.Length);
		yield return new WaitForEndOfFrame();
	}

	private bool CanContinueReadFiles()
	{
		mTaskList.RemoveAll(t => t.IsCompleted);
		return mTaskList.Count == 0;
	}

	private List<string> ReadStructureFile(string filePath)
	{
		List<string> files = new List<string>();
		using FileStream stream = File.Open(filePath, FileMode.Open);
		using BinaryReader reader = new BinaryReader(stream);
		int chunkLength = reader.ReadInt32();

		NativeArray<ChunkVFX> chunks = new NativeArray<ChunkVFX>(chunkLength, Allocator.Persistent);
		for (int i = 0; i < chunkLength; i++)
		{
			ChunkVFX chunkVFX = new ChunkVFX();
			chunkVFX.ChunkIndex = reader.ReadInt32();
			chunkVFX.LodLevel = reader.ReadInt32();
			chunkVFX.Length = reader.ReadInt32();
			chunkVFX.CenterWorldPosition = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
			chunkVFX.WorldPosition = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
			files.Add(reader.ReadString());
			chunks[i] = chunkVFX;
		}
		RuntimeVoxManager.Instance.SetChunks(chunks);
		int materialLength = reader.ReadInt32();

		VoxelMaterialVFX[] materials = new VoxelMaterialVFX[materialLength];
		for (int i = 0; i < materialLength; i++)
		{
			VoxelMaterialVFX mat = new VoxelMaterialVFX();
			mat.color = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			mat.emission = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			mat.emissionPower = reader.ReadSingle();
			mat.metallic = reader.ReadSingle();
			mat.smoothness = reader.ReadSingle();
			mat.alpha = reader.ReadSingle();
			materials[i] = mat;
		}

		RuntimeVoxManager.Instance.SetMaterials(materials);
		return files;
	}


	private async Task ReadChunkDataFile(int chunkIndex, string filename)
	{
		string filePath = Path.Combine(mCurrentInputFolder, filename);
		byte[] data = await File.ReadAllBytesAsync(filePath);
		//using AsyncFileReader reader = new AsyncFileReader();
		//(IntPtr ptr, long size) = await reader.LoadAsync(filePath);

		await UnityMainThreadManager.Instance.EnqueueAsync(() =>
		{
			UnsafeList<VoxelVFX> chunk = VoxelDataConverter.Decode(chunkIndex, data);

			RuntimeVoxManager.Instance.SetVoxelChunk(chunkIndex, chunk);
			mReadCompleted++;
			StartCoroutine(RefreshLoadProgressCo());
		});
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

	private void MoveFilesFromFolder(string inputFolder, string outputFolder)
	{
		DirectoryInfo di = new DirectoryInfo(inputFolder);
		foreach (FileInfo file in di.GetFiles())
		{
			file.MoveTo(Path.Combine(outputFolder, file.Name));
		}
	}

	private void OnChunkLoadResult(float progress, VoxelResult voxelResult)
	{
		LoadProgressCallback?.Invoke(2, progress);

		if (voxelResult.Data.Length == 0)
		{
			return;
		}

		string fileName = $"{mInputFileName}_{voxelResult.LodLevel}_{voxelResult.ChunkCenterWorldPosition.x}_{voxelResult.ChunkCenterWorldPosition.y}_{voxelResult.ChunkCenterWorldPosition.z}.data";

		using (FileStream stream = File.Open(Path.Combine(Application.persistentDataPath, EXTRACT_TMP_FOLDER_NAME, fileName), FileMode.Create))
		{
			using BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(voxelResult.Data.Length);
			for (int i = 0; i < voxelResult.Data.Length; i++)
			{
				binaryWriter.Write(voxelResult.Data[i].PosX);
				binaryWriter.Write(voxelResult.Data[i].PosY);
				binaryWriter.Write(voxelResult.Data[i].PosZ);
				binaryWriter.Write(voxelResult.Data[i].ColorIndex);
				binaryWriter.Write((short)voxelResult.Data[i].Face);
			}

		}

		ChunkDataFile chunk = new ChunkDataFile
		{
			ChunkIndex = voxelResult.ChunkIndex,
			Filename = fileName,
			WorldCenterPosition = voxelResult.ChunkCenterWorldPosition,
			WorldPosition = voxelResult.ChunkWorldPosition,
			LodLevel = voxelResult.LodLevel,
			//Length = sum
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
			binaryWriter.Write((int)chunk.WorldCenterPosition.x);
			binaryWriter.Write((int)chunk.WorldCenterPosition.y);
			binaryWriter.Write((int)chunk.WorldCenterPosition.z);
			binaryWriter.Write((int)chunk.WorldPosition.x);
			binaryWriter.Write((int)chunk.WorldPosition.y);
			binaryWriter.Write((int)chunk.WorldPosition.z);
			binaryWriter.Write(chunk.Filename);
		}

		binaryWriter.Write(VoxImporter.Materials.Length);
		for (int i = 0; i < VoxImporter.Materials.Length; i++)
		{
			VoxelMaterialVFX mat = VoxImporter.Materials[i];
			binaryWriter.Write(mat.color.r);
			binaryWriter.Write(mat.color.g);
			binaryWriter.Write(mat.color.b);
			binaryWriter.Write(mat.emission.r);
			binaryWriter.Write(mat.emission.g);
			binaryWriter.Write(mat.emission.b);
			binaryWriter.Write(mat.emissionPower);
			binaryWriter.Write(mat.metallic);
			binaryWriter.Write(mat.smoothness);
			binaryWriter.Write(mat.alpha);
		}
	}

	private void OnChunkLoadedFinished()
	{
		WriteStructureFile();
		VoxImporter.DisposeMaterials();
		mWorldData.Dispose();
		string inputFolder = Path.Combine(Application.persistentDataPath, EXTRACT_TMP_FOLDER_NAME);
		if (File.Exists(mOutputPath))
		{
			File.Delete(mOutputPath);
		}
		ZipFile.CreateFromDirectory(inputFolder, mOutputPath, CompressionLevel.Optimal, false, Encoding.UTF8);
		string md5ResultFile = GetMd5Checksum(mOutputPath);

		string outputFolder = Path.Combine(Application.persistentDataPath, md5ResultFile);
		if (Directory.Exists(outputFolder))
		{
			Directory.Delete(outputFolder);
		}
		Directory.CreateDirectory(outputFolder);
		MoveFilesFromFolder(inputFolder, outputFolder);
		LoadFinishedCallback?.Invoke();
	}

	#endregion


}
