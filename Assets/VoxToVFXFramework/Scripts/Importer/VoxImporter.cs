using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileToVoxCore.Vox;
using FileToVoxCore.Vox.Chunks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using VoxToVFXFramework.Scripts.Common;
using VoxToVFXFramework.Scripts.Data;
using Color = FileToVoxCore.Drawing.Color;
using Vector3 = FileToVoxCore.Schematics.Tools.Vector3;
using VoxelData = FileToVoxCore.Vox.VoxelData;

namespace VoxToVFXFramework.Scripts.Importer
{
	public static class VoxImporter
	{
		#region Fields

		public static CustomSchematic CustomSchematic { get; private set; }
		public static VoxelMaterialVFX[] Materials { get; private set; }

		private static VoxModelCustom mVoxModel;
		private static readonly Dictionary<int, Matrix4x4> mModelMatrix = new Dictionary<int, Matrix4x4>();

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
								WriteVoxelFrameData(voxelData, mModelMatrix[transformNodeChunk.Id], onFrameLoadedCallback);
							}
						}
					}

					onProgressCallback?.Invoke(i / (float)mVoxModel.TransformNodeChunks.Count);
					yield return new WaitForEndOfFrame();
				}

				Materials = WriteMaterialData();
				onFinishedCallback?.Invoke(true);
				Clean();
			}

			yield return null;
		}

		private static void Clean()
		{
			foreach (VoxelDataCustom voxelDataCustom in mVoxModel.VoxelFramesCustom)
			{
				voxelDataCustom.VoxelNativeArray.Dispose();
			}

			CustomSchematic?.Dispose();
			Materials = null;
			CustomSchematic = new CustomSchematic();
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

			for (int i = 0; i < data.VoxelNativeArray.Length; i++)
			{

				Vector4 voxel = data.VoxelNativeArray[i];
				IntVector3 tmpVoxel = GetVoxPosition(data, (int)voxel.x, (int)voxel.y, (int)voxel.z, pivot, fpivot, matrix4X4);
				data.VoxelNativeArray[i] = new Vector4(tmpVoxel.x + 1000, tmpVoxel.y + 1000, tmpVoxel.z + 1000, voxel.w - 1);
				//data.Get3DPos(key, out int x, out int y, out int z);

				//bool canAdd = false;

				//int left = data.GetSafe(x - 1, y, z);
				//int right = data.GetSafe(x + 1, y, z);

				//int top = data.GetSafe(x, y + 1, z);
				//int bottom = data.GetSafe(x, y - 1, z);

				//int front = data.GetSafe(x, y, z + 1); //y
				//int back = data.GetSafe(x, y, z - 1); //y
				//if (left == 0 || right == 0 || top == 0 || bottom == 0 || front == 0 || back == 0)
				//{
				//	canAdd = true;
				//}

				//if (!canAdd)
				//{
				//	data.VoxelNativeArray.Remove(key);
				//	continue;
				//}
			}

			//TODO: Add this element in the buffer VFX
			onFrameLoadedCallback?.Invoke(data.VoxelNativeArray);

			//Parallel.ForEach(data.Colors, item =>
			//{
			//	data.Get3DPos(item.Key, out int x, out int y, out int z);
			//	IntVector3 tmpVoxel = GetVoxPosition(data, x, y, z, pivot, fpivot, matrix4X4);

			//	bool canAdd = false;

			//	int left = data.GetSafe(x - 1, y, z);
			//	int right = data.GetSafe(x + 1, y, z);

			//	int top = data.GetSafe(x, y + 1, z);
			//	int bottom = data.GetSafe(x, y - 1, z);

			//	int front = data.GetSafe(x, y, z + 1); //y
			//	int back = data.GetSafe(x, y, z - 1); //y
			//	if (left == 0 || right == 0 || top == 0 || bottom == 0 || front == 0 || back == 0)
			//	{
			//		canAdd = true;
			//	}

			//	if (canAdd)
			//	{
			//		CustomSchematic.AddVoxel(tmpVoxel.x + 1000, tmpVoxel.y + 1000, tmpVoxel.z + 1000, item.Value - 1);
			//	}
			//});
		}

		private static IntVector3 GetVoxPosition(VoxelData data, int x, int y, int z, UnityEngine.Vector3 pivot, UnityEngine.Vector3 fpivot, Matrix4x4 matrix4X4)
		{
			IntVector3 tmpVoxel = new IntVector3(x, y, z);
			IntVector3 origPos;
			origPos.x = data.VoxelsWide - 1 - tmpVoxel.x; //invert
			origPos.y = data.VoxelsDeep - 1 - tmpVoxel.z; //swapYZ //invert
			origPos.z = tmpVoxel.y;

			UnityEngine.Vector3 pos = new(origPos.x + 0.5f, origPos.y + 0.5f, origPos.z + 0.5f);
			pos -= pivot;
			pos = matrix4X4.MultiplyPoint(pos);
			pos += pivot;

			pos.x += fpivot.x;
			pos.y += fpivot.y;
			pos.z -= fpivot.z;

			origPos.x = Mathf.FloorToInt(pos.x);
			origPos.y = Mathf.FloorToInt(pos.y);
			origPos.z = Mathf.FloorToInt(pos.z);

			tmpVoxel.x = data.VoxelsWide - 1 - origPos.x; //invert
			tmpVoxel.z = data.VoxelsDeep - 1 - origPos.y; //swapYZ  //invert
			tmpVoxel.y = origPos.z;

			return tmpVoxel;
		}

		private static Matrix4x4 ReadMatrix4X4FromRotation(Rotation rotation, Vector3 transform)
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
