using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI;

public class VoxelDataCreatorManager : ModuleSingleton<VoxelDataCreatorManager>
{
	#region Fields

	public event Action<int, float> LoadProgressCallback;
	public event Action LoadFinishedCallback;

	private string mOutputPath;
	private string mInputFileName;
	private WorldData mWorldData;

	#endregion

	#region PublicMethods

	public void CreateZipFile(string inputPath, string outputPath)
	{
		mInputFileName = Path.GetFileNameWithoutExtension(inputPath);
		mOutputPath = outputPath;
		CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Loading);
		StartCoroutine(VoxImporter.LoadVoxModelAsync(inputPath, OnLoadFrameProgress, OnVoxLoadFinished));
	}

	#endregion

	#region PrivateMethods

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
		using (FileStream stream = File.Open(Path.Combine(Application.persistentDataPath, mInputFileName, ".materials"), FileMode.Create))
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			for (int i = 0; i < VoxImporter.Materials.Length; i++)
			{
				VoxelMaterialVFX mat = VoxImporter.Materials[i];
				binaryWriter.Write((int)mat.color.x);
				binaryWriter.Write((int)mat.color.y);
				binaryWriter.Write((int)mat.color.z);
				binaryWriter.Write(mat.emission);
				binaryWriter.Write(mat.metallic);
				binaryWriter.Write(mat.smoothness);
			}
		}

		StartCoroutine(worldData.ComputeLodsChunks(OnChunkLoadResult, OnChunkLoadedFinished));
	}

	private void OnChunkLoadResult(float progress, VoxelResult voxelResult)
	{
		LoadProgressCallback?.Invoke(2, progress);

		if (voxelResult.Data.Length == 0)
		{
			return;
		}

		string fileName = $"{mInputFileName}_{voxelResult.LodLevel}_{voxelResult.FrameWorldPosition.x}_{voxelResult.FrameWorldPosition.y}_{voxelResult.FrameWorldPosition.z}.data";

		using (FileStream stream = File.Open(Path.Combine(Application.persistentDataPath, fileName), FileMode.Create))
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(voxelResult.ChunkIndex);
			binaryWriter.Write(voxelResult.LodLevel);
			binaryWriter.Write(voxelResult.FrameWorldPosition.x);
			binaryWriter.Write(voxelResult.FrameWorldPosition.y);
			binaryWriter.Write(voxelResult.FrameWorldPosition.z);
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
		//using (FileStream zipFile = File.Open("compressed_files.zip", FileMode.Create))
		//{
		//	// Files to be added to archive
		//	FileInfo fi1 = new FileInfo("alice29.txt");
		//	FileInfo fi2 = new FileInfo("fields.c");

		//	using (var archive = new Archive())
		//	{
		//		// Add files to the archive
		//		archive.CreateEntry("alice29.txt", fi1);
		//		archive.CreateEntry("fields.c", fi2);
		//		// Create ZIP archive
		//		archive.Save(zipFile, new ArchiveSaveOptions() { Encoding = Encoding.UTF8 });
		//	}
		//}
		LoadFinishedCallback?.Invoke();
	}

	#endregion
}
