using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI;
using System.IO.Compression;
using Unity.Collections;
using VoxToVFXFramework.Scripts.Managers;
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

	private const string IMPORT_TMP_FOLDER_NAME = "import_tmp";
	private const string EXTRACT_TMP_FOLDER_NAME = "extract_tmp";
	#endregion

	#region PublicMethods

	public void CreateZipFile(string inputPath, string outputPath)
	{
		mInputFileName = Path.GetFileNameWithoutExtension(inputPath);
		mOutputPath = outputPath;
		CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Loading);
		StartCoroutine(VoxImporter.LoadVoxModelAsync(inputPath, OnLoadFrameProgress, OnVoxLoadFinished));
	}

	public void ReadZipFile(string inputPath)
	{
		RuntimeVoxManager.Instance.Release();
		string tmpPath = Path.Combine(Application.persistentDataPath, IMPORT_TMP_FOLDER_NAME);

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

	private IEnumerator StartReadImportFilesCo(string tmpPath)
	{
		string[] materialFiles = Directory.GetFiles(tmpPath, "*.materials", SearchOption.AllDirectories);
		if (materialFiles.Length != 1)
		{
			Debug.LogError("No materials file found, abort reading.");
			yield break;
		}

		CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Loading);
		ReadMaterialFile(materialFiles[0]);

		string[] dataFiles = Directory.GetFiles(tmpPath, "*.data", SearchOption.AllDirectories);
		for (int i = 0; i < dataFiles.Length; i++)
		{
			string file = dataFiles[i];
			ReadDataFile(file);
			LoadProgressCallback?.Invoke(1, i / (float)dataFiles.Length);
			yield return new WaitForEndOfFrame();
		}
		RuntimeVoxManager.Instance.OnChunkLoadedFinished();
	}


	private void ReadDataFile(string filePath)
	{
		using (FileStream stream = File.Open(filePath, FileMode.Open))
		{
			BinaryReader reader = new BinaryReader(stream);
			VoxelResult voxelResult = new VoxelResult();
			voxelResult.ChunkIndex = reader.ReadInt32();
			voxelResult.LodLevel = reader.ReadInt32();
			voxelResult.FrameWorldPosition = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
			int length = reader.ReadInt32();
			voxelResult.Data = new NativeArray<Vector4>(length, Allocator.Temp);
			for (int i = 0; i < length; i++)
			{
				voxelResult.Data[i] = new Vector4(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
			}

			RuntimeVoxManager.Instance.SetVoxelChunk(voxelResult);
			voxelResult.Data.Dispose();
		}
	}

	private void ReadMaterialFile(string filePath)
	{
		using (FileStream stream = File.Open(filePath, FileMode.Open))
		{
			BinaryReader reader = new BinaryReader(stream);
			int length = reader.ReadInt32();
			VoxelMaterialVFX[] materials = new VoxelMaterialVFX[length];
			for (int i = 0; i < length; i++)
			{
				VoxelMaterialVFX mat = new VoxelMaterialVFX();
				mat.color = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
				mat.emission = reader.ReadSingle();
				mat.metallic = reader.ReadSingle();
				mat.smoothness = reader.ReadSingle();
				materials[i] = mat;
			}
			RuntimeVoxManager.Instance.SetMaterials(materials);
		}
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

		using (FileStream stream = File.Open(Path.Combine(Application.persistentDataPath, EXTRACT_TMP_FOLDER_NAME, mInputFileName + ".materials"), FileMode.Create))
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
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
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(voxelResult.ChunkIndex);
			binaryWriter.Write(voxelResult.LodLevel);
			binaryWriter.Write((int)voxelResult.FrameWorldPosition.x);
			binaryWriter.Write((int)voxelResult.FrameWorldPosition.y);
			binaryWriter.Write((int)voxelResult.FrameWorldPosition.z);
			binaryWriter.Write(voxelResult.Data.Length);
			for (int i = 0; i < voxelResult.Data.Length; i++)
			{
				binaryWriter.Write((int)voxelResult.Data[i].x);
				binaryWriter.Write((int)voxelResult.Data[i].y);
				binaryWriter.Write((int)voxelResult.Data[i].z);
				binaryWriter.Write((int)voxelResult.Data[i].w);
			}
		}

	}

	private void OnChunkLoadedFinished()
	{
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
}
