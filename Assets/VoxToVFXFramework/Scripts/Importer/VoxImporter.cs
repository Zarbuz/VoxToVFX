using FileToVoxCore.Vox;
using FileToVoxCore.Vox.Chunks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxToVFXFramework.Scripts.Common;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Jobs;
using Color = FileToVoxCore.Drawing.Color;

namespace VoxToVFXFramework.Scripts.Importer
{
	public static class VoxImporter
	{
		private class ShapeModelCount
		{
			public int Total;
			public int Count;
		}

		#region Fields

		public static VoxelMaterialVFX[] Materials { get; private set; }

		private static VoxModelCustom mVoxModel;
		private static readonly Dictionary<int, Matrix4x4> mModelMatrix = new Dictionary<int, Matrix4x4>();
		private static readonly Dictionary<int, ShapeModelCount> mShapeModelCounts = new Dictionary<int, ShapeModelCount>();

		#endregion

		#region PublicMethods

		public static IEnumerator LoadVoxModelAsync(string path, Action<float> onProgressCallback, Action<VoxelResult> onFrameLoadedCallback, Action<bool> onFinishedCallback)
		{
			CustomVoxReader voxReader = new CustomVoxReader();
			mVoxModel = voxReader.LoadModel(path) as VoxModelCustom;
			if (mVoxModel == null)
			{
				onFinishedCallback?.Invoke(false);
			}
			else
			{
				InitShapeModelCounts();
				for (int i = 0; i < mVoxModel.TransformNodeChunks.Count; i++)
				{
					TransformNodeChunk transformNodeChunk = mVoxModel.TransformNodeChunks[i];
					int childId = transformNodeChunk.ChildId;

					if (mModelMatrix.ContainsKey(transformNodeChunk.Id))
					{
						mModelMatrix[transformNodeChunk.Id] *= ReadMatrix4X4FromRotation(transformNodeChunk.RotationAt(), transformNodeChunk.TranslationAt());
					}
					else
					{
						mModelMatrix[transformNodeChunk.Id] = ReadMatrix4X4FromRotation(transformNodeChunk.RotationAt(), transformNodeChunk.TranslationAt());
					}

					GroupNodeChunk groupNodeChunk = mVoxModel.GroupNodeChunks.FirstOrDefault(grp => grp.Id == childId);
					if (groupNodeChunk != null)
					{
						foreach (int child in groupNodeChunk.ChildIds)
						{
							mModelMatrix[child] = ReadMatrix4X4FromRotation(transformNodeChunk.RotationAt(), transformNodeChunk.TranslationAt());
						}
					}
					else
					{
						ShapeNodeChunk shapeNodeChunk = mVoxModel.ShapeNodeChunks.FirstOrDefault(shp => shp.Id == childId);
						if (shapeNodeChunk == null)
						{
							Debug.LogError("Failed to find chunk with ID: " + childId);
						}
						else
						{
							foreach (ShapeModel shapeModel in shapeNodeChunk.Models)
							{
								int modelId = shapeModel.ModelId;
								VoxelDataCustom voxelData = mVoxModel.VoxelFramesCustom[modelId];
								mShapeModelCounts[shapeModel.ModelId].Count++;
								WriteVoxelFrameData(transformNodeChunk.Id, voxelData, onFrameLoadedCallback);
								if (mShapeModelCounts[shapeModel.ModelId].Count == mShapeModelCounts[shapeModel.ModelId].Total)
								{
									voxelData.VoxelNativeArray.Dispose();
								}
							}
						}
					}

					onProgressCallback?.Invoke(i / (float)mVoxModel.TransformNodeChunks.Count);
					yield return new WaitForEndOfFrame();
				}

				Materials = WriteMaterialData();
				onFinishedCallback?.Invoke(true);
				Dispose();
			}

			yield return null;
		}

		public static int GetGridPos(int x, int y, int z, Vector3 volumeSize)
			=> (int)((volumeSize.x * volumeSize.y * z) + volumeSize.x * y + x);

		#endregion

		#region PrivateMethods

		private static void InitShapeModelCounts()
		{

			foreach (ShapeModel shapeModel in mVoxModel.ShapeNodeChunks.SelectMany(shapeNodeChunk => shapeNodeChunk.Models))
			{
				if (!mShapeModelCounts.ContainsKey(shapeModel.ModelId))
				{
					mShapeModelCounts[shapeModel.ModelId] = new ShapeModelCount();
				}

				mShapeModelCounts[shapeModel.ModelId].Total++;
			}
		}

		private static void Dispose()
		{
			foreach (VoxelDataCustom voxelDataCustom in mVoxModel.VoxelFramesCustom.Where(voxelDataCustom => voxelDataCustom.VoxelNativeArray.IsCreated))
			{
				voxelDataCustom.VoxelNativeArray.Dispose();
			}

			Materials = null;
			mShapeModelCounts.Clear();
			mModelMatrix.Clear();
			mVoxModel = null;
			GC.Collect();
		}

		private static VoxelMaterialVFX[] WriteMaterialData()
		{
			VoxelMaterialVFX[] materials = new VoxelMaterialVFX[256];
			for (int i = 0; i < mVoxModel.Palette.Length; i++)
			{
				Color c = mVoxModel.Palette[i];
				materials[i].color = new UnityEngine.Vector3(c.R / (float)255, c.G / (float)255, c.B / (float)255);
			}

			for (int i = 0; i < mVoxModel.MaterialChunks.Count; i++)
			{
				MaterialChunk materialChunk = mVoxModel.MaterialChunks[i];
				materials[i].emission = materialChunk.Emission == 0 ? 1 : materialChunk.Emission * 10;
				materials[i].smoothness = materialChunk.Smoothness;
				materials[i].metallic = materialChunk.Metallic;
			}

			return materials;
		}

		private static void WriteVoxelFrameData(int transformChunkId, VoxelDataCustom data, Action<VoxelResult> onFrameLoadedCallback)
		{
			Vector3 initialVolumeSize = new Vector3(data.VoxelsWide, data.VoxelsTall, data.VoxelsDeep);

			IntVector3 originSize = new IntVector3((int)initialVolumeSize.x, (int)initialVolumeSize.y, (int)initialVolumeSize.z);
			originSize.y = data.VoxelsDeep;
			originSize.z = data.VoxelsTall;

			Vector3 pivot = new Vector3(originSize.x / 2, originSize.y / 2, originSize.z / 2);
			Vector3 fpivot = new Vector3(originSize.x / 2f, originSize.y / 2f, originSize.z / 2f);

			int maxCapacity = (int)(initialVolumeSize.x * initialVolumeSize.y * initialVolumeSize.z);

			NativeList<Vector4> resultLod0 = new NativeList<Vector4>(Allocator.TempJob);
			resultLod0.SetCapacity(maxCapacity);
			JobHandle job = new ComputeVoxelPositionJob
			{
				Matrix4X4 = mModelMatrix[transformChunkId],
				VolumeSize = initialVolumeSize,
				InitialVolumeSize = initialVolumeSize,
				Pivot = pivot,
				FPivot = fpivot,
				Data = data.VoxelNativeArray,
				Result = resultLod0.AsParallelWriter(),
			}.Schedule((int)initialVolumeSize.z, 64);
			job.Complete();

			//NativeArray<byte> work = new NativeArray<byte>(8, Allocator.Persistent);
			NativeArray<byte> arrayLod1 = new NativeArray<byte>((int)(initialVolumeSize.x * initialVolumeSize.y * initialVolumeSize.z), Allocator.TempJob);
			ComputeLodJob computeLodJob = new ComputeLodJob()
			{
				VolumeSize = initialVolumeSize,
				Result = arrayLod1,
				Data = data.VoxelNativeArray,
				Step = 1,
				ModuloCheck = 2
			};
			JobHandle jobHandle = computeLodJob.Schedule((int)initialVolumeSize.z, 64);
			jobHandle.Complete();

			NativeList<Vector4> resultLod1 = new NativeList<Vector4>(Allocator.TempJob);
			resultLod1.SetCapacity(maxCapacity);
			JobHandle job1 = new ComputeVoxelPositionJob
			{
				Matrix4X4 = mModelMatrix[transformChunkId],
				VolumeSize = initialVolumeSize,
				InitialVolumeSize = initialVolumeSize,
				Pivot = pivot,
				FPivot = fpivot,
				Data = arrayLod1,
				Result = resultLod1.AsParallelWriter(),
			}.Schedule((int)initialVolumeSize.z, 64);
			job1.Complete();

			NativeArray<byte> arrayLod2 = new NativeArray<byte>((int)(initialVolumeSize.x * initialVolumeSize.y * initialVolumeSize.z), Allocator.TempJob);
			ComputeLodJob computeLodJob2 = new ComputeLodJob()
			{
				VolumeSize = initialVolumeSize,
				Result = arrayLod2,
				Data = arrayLod1,
				Step = 2,
				ModuloCheck = 4
			};
			JobHandle jobHandle2 = computeLodJob2.Schedule((int)initialVolumeSize.z, 64);
			jobHandle2.Complete();

			NativeList<Vector4> resultLod2 = new NativeList<Vector4>(Allocator.TempJob);
			resultLod2.SetCapacity(maxCapacity);
			JobHandle job2 = new ComputeVoxelPositionJob
			{
				Matrix4X4 = mModelMatrix[transformChunkId],
				VolumeSize = initialVolumeSize,
				InitialVolumeSize = initialVolumeSize,
				Pivot = pivot,
				FPivot = fpivot,
				Data = arrayLod2,
				Result = resultLod2.AsParallelWriter(),
			}.Schedule((int)initialVolumeSize.z, 64);
			job2.Complete();

			arrayLod1.Dispose();
			arrayLod2.Dispose();

			VoxelResult voxelResult = new VoxelResult
			{
				DataLod0 = resultLod0,
				DataLod1 = resultLod1,
				DataLod2 = resultLod2
			};
			IntVector3 frameWorldPosition = ComputeVoxelPositionJob.GetVoxPosition(initialVolumeSize, (int)pivot.x, (int)pivot.y, (int)pivot.z, pivot, fpivot, mModelMatrix[transformChunkId]);
			voxelResult.FrameWorldPosition = new Vector3(frameWorldPosition.x + 1000, frameWorldPosition.y + 1000, frameWorldPosition.z + 1000);

			onFrameLoadedCallback?.Invoke(voxelResult);
			resultLod0.Dispose();
			resultLod1.Dispose();
			resultLod2.Dispose();
		}

		public static bool Contains(int x, int y, int z, Vector3 volumeSize)
			=> x >= 0 && y >= 0 && z >= 0 && x < volumeSize.x && y < volumeSize.y && z < volumeSize.z;
		public static byte GetSafe(int x, int y, int z, NativeArray<byte> data, Vector3 volumeSize)
			=> Contains(x, y, z, volumeSize) ? data[GetGridPos(x, y, z, volumeSize)] : (byte)0;

		private static NativeArray<byte> ComputeLod(LodParameters parameters)
		{
			Vector3 volumeSize = parameters.VolumeSize;
			NativeArray<byte> data = parameters.Data;
			NativeArray<byte> work = parameters.Work;
			//Vector3 volumeResultSize = new Vector3((int)(volumeSize.x + 1) >> 1, (int)(volumeSize.y + 1) >> 1, (int)(volumeSize.z + 1) >> 1);

			NativeArray<byte> result = new NativeArray<byte>((int)(volumeSize.x * volumeSize.y * volumeSize.z), Allocator.TempJob);
			for (int z = 0; z < volumeSize.z; z += parameters.Step * 2)
			{
				int z1 = z + parameters.Step;
				for (int y = 0; y < volumeSize.y; y += parameters.Step * 2)
				{
					int y1 = y + parameters.Step;
					for (int x = 0; x < volumeSize.x; x += parameters.Step * 2)
					{
						int x1 = x + parameters.Step;
						work[0] = GetSafe(x, y, z, data, volumeSize);
						work[1] = GetSafe(x1, y, z, data, volumeSize);
						work[2] = GetSafe(x, y1, z, data, volumeSize);
						work[3] = GetSafe(x1, y1, z, data, volumeSize);
						work[4] = GetSafe(x, y, z1, data, volumeSize);
						work[5] = GetSafe(x1, y, z1, data, volumeSize);
						work[6] = GetSafe(x, y1, z1, data, volumeSize);
						work[7] = GetSafe(x1, y1, z1, data, volumeSize);

						if (work.Any(color => color != 0))
						{
							IOrderedEnumerable<IGrouping<byte, byte>> groups = work.Where(color => color != 0)
								.GroupBy(v => v).OrderByDescending(v => v.Count());
							int count = groups.ElementAt(0).Count();
							IGrouping<byte, byte> group = groups.TakeWhile(v => v.Count() == count)
								.OrderByDescending(v => v.Key).First();

							result[GetGridPos(x, y, z, volumeSize)] = group.Key;
						}
						else
						{
							result[GetGridPos(x, y, z, volumeSize)] = 0;
						}
					}
				}
			}

			return result;
		}

		public static Matrix4x4 ReadMatrix4X4FromRotation(Rotation rotation, FileToVoxCore.Schematics.Tools.Vector3 transform)
		{
			Matrix4x4 result = Matrix4x4.identity;
			{
				byte r = Convert.ToByte(rotation);
				int indexRow0 = (r & 3);
				int indexRow1 = (r & 12) >> 2;
				bool signRow0 = (r & 16) == 0;
				bool signRow1 = (r & 32) == 0;
				bool signRow2 = (r & 64) == 0;

				result.SetRow(0, Vector4.zero);
				switch (indexRow0)
				{
					case 0: result[0, 0] = signRow0 ? 1f : -1f; break;
					case 1: result[0, 1] = signRow0 ? 1f : -1f; break;
					case 2: result[0, 2] = signRow0 ? 1f : -1f; break;
				}
				result.SetRow(1, Vector4.zero);
				switch (indexRow1)
				{
					case 0: result[1, 0] = signRow1 ? 1f : -1f; break;
					case 1: result[1, 1] = signRow1 ? 1f : -1f; break;
					case 2: result[1, 2] = signRow1 ? 1f : -1f; break;
				}
				result.SetRow(2, Vector4.zero);
				switch (indexRow0 + indexRow1)
				{
					case 1: result[2, 2] = signRow2 ? 1f : -1f; break;
					case 2: result[2, 1] = signRow2 ? 1f : -1f; break;
					case 3: result[2, 0] = signRow2 ? 1f : -1f; break;
				}

				result.SetColumn(3, new Vector4(transform.X, transform.Y, transform.Z, 1f));
			}
			return result;
		}

		#endregion
	}
}
