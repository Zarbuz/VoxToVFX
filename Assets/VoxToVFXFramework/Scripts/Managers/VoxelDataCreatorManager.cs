using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using VoxToVFXFramework.Scripts.Converter;
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
	private VoxImporter mImporter;
	private string mAppPersistantPath;

	private string mCurrentInputFolder;
	private readonly ConcurrentBag<ChunkDataFile> mChunksWritten = new ConcurrentBag<ChunkDataFile>();
	private readonly List<Task> mTaskList = new List<Task>();
	private const string EXTRACT_TMP_FOLDER_NAME = "extract_tmp";
	private int mReadCompleted;

	private int mMinX = int.MaxValue;
	private int mMaxX = int.MinValue;
	private int mMinY = int.MaxValue;
	private int mMaxY = int.MinValue;
	private int mMinZ = int.MaxValue;
	private int mMaxZ = int.MinValue;
	#endregion

	#region UnityMethods

	protected override void OnStart()
	{
		mAppPersistantPath = Application.persistentDataPath;
	}

	private void OnApplicationQuit()
	{
		mImporter?.Dispose();
	}

	#endregion

	#region PublicMethods

	public void CreateZipFile(string inputPath, string outputPath)
	{
		mInputFileName = Path.GetFileNameWithoutExtension(inputPath);
		mOutputPath = outputPath;
		mChunksWritten.Clear();
		mImporter = new VoxImporter();
		StartCoroutine(mImporter.LoadVoxModelAsync(inputPath, OnLoadFrameProgress, OnVoxLoadFinished));
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
			Task lastTask = ReadChunkDataFile(index, chunkVFX, files[index]);
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

		int minX = reader.ReadInt32();
		int maxX = reader.ReadInt32();

		int minY = reader.ReadInt32();
		int maxY = reader.ReadInt32();

		int minZ = reader.ReadInt32();
		int maxZ = reader.ReadInt32();

		RuntimeVoxManager.Instance.MinMaxX = new Vector2(minX, maxX);
		RuntimeVoxManager.Instance.MinMaxY = new Vector2(minY, maxY);
		RuntimeVoxManager.Instance.MinMaxZ = new Vector2(minZ, maxZ);
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

		//Edge settings
		int edgeR = reader.ReadInt32();
		int edgeG = reader.ReadInt32();
		int edgeB = reader.ReadInt32();
		float width = reader.ReadSingle();

		PostProcessingManager.Instance.SetEdgePostProcess(width, new Color(edgeR / (float)255, edgeG / (float)255, edgeB / (float)255));
		return files;
	}


	private async Task ReadChunkDataFile(int chunkIndex, ChunkVFX chunk, string filename)
	{
		string filePath = Path.Combine(mCurrentInputFolder, filename);
		byte[] data = await File.ReadAllBytesAsync(filePath);
		//using AsyncFileReader reader = new AsyncFileReader();
		//(IntPtr ptr, long size) = await reader.LoadAsync(filePath);

		await UnityMainThreadManager.Instance.EnqueueAsync(() =>
		{
			UnsafeList<VoxelVFX> voxels = VoxelDataConverter.Decode(chunkIndex,chunk,  data);
			RuntimeVoxManager.Instance.SetVoxelChunk(chunkIndex, voxels);
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

		StartCoroutine(ComputeLodCo(worldData));

	}

	private IEnumerator ComputeLodCo(WorldData worldData)
	{
		Task task = Task.Run(() => worldData.ComputeLodsChunks(OnChunkLoadResult, OnProgressChunkLoadResult, OnChunkLoadedFinished));

		while (!task.IsCompleted)
		{
			yield return null;
		}
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

	private void OnChunkLoadResult(VoxelResult voxelResult)
	{
		if (voxelResult.Data.Length == 0)
		{
			return;
		}

		string fileName = $"{mInputFileName}_{voxelResult.LodLevel}_{voxelResult.ChunkCenterWorldPosition.x}_{voxelResult.ChunkCenterWorldPosition.y}_{voxelResult.ChunkCenterWorldPosition.z}.data";
		using (FileStream stream = File.Open(Path.Combine(mAppPersistantPath, EXTRACT_TMP_FOLDER_NAME, fileName), FileMode.Create))
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

				int worldPositionX = (int)(voxelResult.ChunkWorldPosition.x + voxelResult.Data[i].PosX);
				int worldPositionY = (int)(voxelResult.ChunkWorldPosition.y + voxelResult.Data[i].PosY);
				int worldPositionZ = (int)(voxelResult.ChunkWorldPosition.z + voxelResult.Data[i].PosZ);

				mMinX = Mathf.Min(mMinX, worldPositionX);
				mMaxX = Mathf.Max(mMaxX, worldPositionX);
				mMinY = Mathf.Min(mMinY, worldPositionY);
				mMaxY = Mathf.Max(mMaxY, worldPositionY);
				mMinZ = Mathf.Min(mMinZ, worldPositionZ);
				mMaxZ = Mathf.Max(mMaxZ, worldPositionZ);
			}
		}

		ChunkDataFile chunk = new ChunkDataFile
		{
			ChunkIndex = voxelResult.ChunkIndex,
			Filename = fileName,
			WorldCenterPosition = voxelResult.ChunkCenterWorldPosition,
			WorldPosition = voxelResult.ChunkWorldPosition,
			LodLevel = voxelResult.LodLevel,
			Length = voxelResult.Data.Length,
		};
		mChunksWritten.Add(chunk);
	}

	private void OnProgressChunkLoadResult(float progress)
	{
		UnityMainThreadManager.Instance.Enqueue(() =>
		{
			LoadProgressCallback?.Invoke(2, progress);
		});
	}

	private void WriteStructureFile()
	{
		using FileStream stream = File.Open(Path.Combine(mAppPersistantPath, EXTRACT_TMP_FOLDER_NAME, mInputFileName + ".structure"), FileMode.Create);
		using BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(mMinX);
		binaryWriter.Write(mMaxX);
		binaryWriter.Write(mMinY);
		binaryWriter.Write(mMaxY);
		binaryWriter.Write(mMinZ);
		binaryWriter.Write(mMaxZ);
		binaryWriter.Write(mChunksWritten.Count);
		foreach (ChunkDataFile chunk in mChunksWritten)
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

		binaryWriter.Write(mImporter.WorldData.Materials.Length);
		for (int i = 0; i < mImporter.WorldData.Materials.Length; i++)
		{
			VoxelMaterialVFX mat = mImporter.WorldData.Materials[i];
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

		//Edge settings
		string color = mImporter.WorldData.EdgeSetting.Attributes["_color"];
		string width = mImporter.WorldData.EdgeSetting.Attributes["_width"];
		string[] edgeColor = color.Split(" ");
		binaryWriter.Write(Convert.ToInt32(edgeColor[0]));
		binaryWriter.Write(Convert.ToInt32(edgeColor[1]));
		binaryWriter.Write(Convert.ToInt32(edgeColor[2]));
		binaryWriter.Write(Convert.ToSingle(width, CultureInfo.InvariantCulture));
	}

	private void OnChunkLoadedFinished()
	{
		WriteStructureFile();
		mImporter.Dispose();
		mImporter = null;
		string inputFolder = Path.Combine(mAppPersistantPath, EXTRACT_TMP_FOLDER_NAME);
		if (File.Exists(mOutputPath))
		{
			File.Delete(mOutputPath);
		}
		ZipFile.CreateFromDirectory(inputFolder, mOutputPath, CompressionLevel.Optimal, false, Encoding.UTF8);
		string md5ResultFile = GetMd5Checksum(mOutputPath);

		string outputFolder = Path.Combine(mAppPersistantPath, md5ResultFile);
		if (Directory.Exists(outputFolder))
		{
			Directory.Delete(outputFolder);
		}
		Directory.CreateDirectory(outputFolder);
		MoveFilesFromFolder(inputFolder, outputFolder);
		Process.Start(Path.GetDirectoryName(mOutputPath) ?? string.Empty);

		UnityMainThreadManager.Instance.Enqueue(() =>
		{
			LoadFinishedCallback?.Invoke();
		});
	}

	#endregion


}
