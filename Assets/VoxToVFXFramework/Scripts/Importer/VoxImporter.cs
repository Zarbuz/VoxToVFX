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

		public static IEnumerator LoadVoxModelAsync(string path, Action<float> onProgressCallback, Action<NativeArray<Vector4>> onFrameLoadedCallback, Action<bool> onFinishedCallback)
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
								WriteVoxelFrameData(voxelData, mModelMatrix[transformNodeChunk.Id], onFrameLoadedCallback);
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
		#endregion

		#region PrivateMethods

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

		private static void WriteVoxelFrameData(VoxelDataCustom data, Matrix4x4 matrix4X4, Action<NativeArray<Vector4>> onFrameLoadedCallback)
		{
			IntVector3 originSize = new IntVector3(data.VoxelsWide, data.VoxelsTall, data.VoxelsDeep);
			originSize.y = data.VoxelsDeep;
			originSize.z = data.VoxelsTall;

			UnityEngine.Vector3 pivot = new UnityEngine.Vector3(originSize.x / 2, originSize.y / 2, originSize.z / 2);
			UnityEngine.Vector3 fpivot = new UnityEngine.Vector3(originSize.x / 2f, originSize.y / 2f, originSize.z / 2f);

			UpdateVoxelPositionJob job = new UpdateVoxelPositionJob();
			job.Matrix4X4 = matrix4X4;
			job.Size = new UnityEngine.Vector3(data.VoxelsWide, data.VoxelsTall, data.VoxelsDeep);
			job.Pivot = pivot;
			job.FPivot = fpivot;
			job.Result = data.VoxelNativeArray;

			// Schedule a parallel-for job. First parameter is how many for-each iterations to perform.
			// The second parameter is the batch size,
			// essentially the no-overhead innerloop that just invokes Execute(i) in a loop.
			// When there is a lot of work in each iteration then a value of 1 can be sensible.
			// When there is very little work values of 32 or 64 can make sense.
			JobHandle jobHandle = job.Schedule(data.VoxelNativeArray.Length, 64);

			// Ensure the job has completed.
			// It is not recommended to Complete a job immediately,
			// since that reduces the chance of having other jobs run in parallel with this one.
			// You optimally want to schedule a job early in a frame and then wait for it later in the frame.
			jobHandle.Complete();

			onFrameLoadedCallback?.Invoke(data.VoxelNativeArray);
		}

		private static Matrix4x4 ReadMatrix4X4FromRotation(Rotation rotation, FileToVoxCore.Schematics.Tools.Vector3 transform)
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
