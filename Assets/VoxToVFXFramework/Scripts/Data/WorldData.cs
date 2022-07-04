using System;
using System.Collections;
using System.Globalization;
using FileToVoxCore.Vox;
using FileToVoxCore.Vox.Chunks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.Jobs;
using Color = FileToVoxCore.Drawing.Color;

namespace VoxToVFXFramework.Scripts.Data
{
	public class WorldData : IDisposable
	{
		#region Fields
		public UnsafeHashMap<int, UnsafeHashMap<int, VoxelData>> WorldDataPositions;
		public NativeArray<VoxelMaterialVFX> Materials { get; private set; }
		public RendererSettingChunk EdgeSetting { get; private set; }
		#endregion

		#region ConstStatic

		public const int CHUNK_SIZE = 100;
		public static readonly int3 ChunkVolume = new int3(CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE);
		public static readonly int3 RelativeWorldVolume = new int3(2000 / CHUNK_SIZE, 2000 / CHUNK_SIZE, 2000 / CHUNK_SIZE);
		#endregion

		#region PublicMethods

		public WorldData(VoxModelCustom voxModel)
		{
			WorldDataPositions = new UnsafeHashMap<int, UnsafeHashMap<int, VoxelData>>(256, Allocator.Persistent);
			WriteMaterials(voxModel);
			WriteRenderSettings(voxModel);
		}

		public void AddVoxels(NativeList<Vector4> voxels)
		{
			JobHandle addVoxelsJob = new AddVoxelsJob()
			{
				Voxels = voxels,
				WorldDataPositions = WorldDataPositions
			}.Schedule(voxels.Length, new JobHandle());
			addVoxelsJob.Complete();
		}

		public IEnumerator ComputeLodsChunks(Action<float, VoxelResult> onChunkLoadedCallback, Action onChunkLoadedFinished)
		{
			NativeArray<int> keys = WorldDataPositions.GetKeyArray(Allocator.Persistent);
			
			for (int index = 0; index < keys.Length; index++)
			{
				int chunkIndex = keys[index];
				int3 chunkWorldPosition = GetChunkWorldPosition(chunkIndex);
				Vector3 chunkCenterWorldPosition = GetCenterChunkWorldPosition(chunkIndex);
				UnsafeHashMap<int, VoxelData> resultLod0 = WorldDataPositions[chunkIndex];
				
				UnsafeHashMap<int, VoxelData> resultLod1 = ComputeLod(resultLod0, chunkWorldPosition, 1, 2);
				NativeList<VoxelData> finalResultLod0 = ComputeRotation(resultLod0, WorldDataPositions, Materials, 1, chunkWorldPosition);

				VoxelResult voxelResult = new VoxelResult
				{
					Data = finalResultLod0,
					ChunkIndex = chunkIndex,
					ChunkCenterWorldPosition = chunkCenterWorldPosition,
					ChunkWorldPosition = new Vector3(chunkWorldPosition.x, chunkWorldPosition.y, chunkWorldPosition.z),
					LodLevel = 1
				};
				onChunkLoadedCallback?.Invoke(index / (float)keys.Length, voxelResult);
				voxelResult.Data.Dispose();
				yield return new WaitForEndOfFrame();

				UnsafeHashMap<int, VoxelData> resultLod2 = ComputeLod(resultLod1, chunkWorldPosition, 2, 4);
				NativeList<VoxelData> finalResultLod1 = ComputeRotation(resultLod1, WorldDataPositions,  Materials, 2, chunkWorldPosition);
				resultLod1.Dispose();
				voxelResult.Data = finalResultLod1;
				voxelResult.LodLevel = 2;
				onChunkLoadedCallback?.Invoke(index / (float)keys.Length, voxelResult);
				voxelResult.Data.Dispose();
				yield return new WaitForEndOfFrame();

				NativeList<VoxelData> finalResultLod2 = ComputeRotation(resultLod2, WorldDataPositions, Materials, 4, chunkWorldPosition);
				resultLod2.Dispose();
				voxelResult.Data = finalResultLod2;
				voxelResult.LodLevel = 4;
				onChunkLoadedCallback?.Invoke(index / (float)keys.Length, voxelResult);
				voxelResult.Data.Dispose();
				yield return new WaitForEndOfFrame();

				
			}

			keys.Dispose();
			onChunkLoadedFinished?.Invoke();
		}

		public void Dispose()
		{
			if (Materials.IsCreated)
			{
				Materials.Dispose();
			}

			if (WorldDataPositions.IsCreated)
			{
				WorldDataPositions.Dispose();
			}
		}

		#endregion

		#region PrivateMethods

		private void WriteMaterials(VoxModelCustom voxModel)
		{
			NativeArray<VoxelMaterialVFX> materials = new NativeArray<VoxelMaterialVFX>(256, Allocator.Persistent);

			for (int i = 0; i < voxModel.Palette.Length; i++)
			{
				Color c = voxModel.Palette[i];
				VoxelMaterialVFX material = new VoxelMaterialVFX();
				material.color = new UnityEngine.Color(c.R / (float)255, c.G / (float)255, c.B / (float)255);
				materials[i] = material;
			}

			for (int i = 0; i < voxModel.MaterialChunks.Count; i++)
			{
				MaterialChunk materialChunk = voxModel.MaterialChunks[i];
				VoxelMaterialVFX material = materials[i];
				material.alpha = 1; //By default the material is opaque
				switch (materialChunk.Type)
				{
					case MaterialType._diffuse:
						break;
					case MaterialType._metal:
						material.metallic = materialChunk.Metal;
						material.smoothness = materialChunk.Smoothness;
						break;
					case MaterialType._glass:
						material.alpha = 1f - materialChunk.Alpha;
						material.smoothness = materialChunk.Smoothness;
						break;
					case MaterialType._emit:
						material.emission = UnityEngine.Color.Lerp(UnityEngine.Color.black, UnityEngine.Color.white, materialChunk.Emit);
						material.emissionPower = Mathf.Lerp(2f, 12f, materialChunk.Flux / 4f);
						break;
					case MaterialType._media:
						{
							material.alpha = 1f - materialChunk.Alpha;
							materialChunk.Properties.TryGetValue("_d", out string _d);
							float.TryParse(_d, NumberStyles.Any, CultureInfo.InvariantCulture, out float density);
							material.alpha *= density * 10f;
						}

						break;

					case MaterialType._blend:
						{
							//material.alpha = 1f - materialChunk.Alpha; //No alpha for Blend for now
							material.metallic = materialChunk.Metal;
							material.smoothness = materialChunk.Smoothness;
							if (materialChunk.Properties.TryGetValue("_media_type", out string mediaType) && mediaType == "_emit")
							{
								materialChunk.Properties.TryGetValue("_d", out string _d);
								float.TryParse(_d, NumberStyles.Any, CultureInfo.InvariantCulture, out float density);

								material.emission = UnityEngine.Color.Lerp(UnityEngine.Color.black, UnityEngine.Color.white, materialChunk.Emit);
								material.emissionPower = density * 10f;
							}
						}

						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				materials[i] = material;
			}

			Materials = materials;
		}

		
		private void WriteRenderSettings(VoxModelCustom voxModel)
		{
			foreach (RendererSettingChunk rendererSettingChunk in voxModel.RendererSettingChunks)
			{
				if (rendererSettingChunk.Type == RenderSettingType._edge)
				{
					EdgeSetting = rendererSettingChunk;
				}

				if (rendererSettingChunk.Type == RenderSettingType._setting)
				{
					int activeEdge = Convert.ToInt32(rendererSettingChunk.Attributes["_edge"]);
					if (activeEdge == 0)
					{
						EdgeSetting.Attributes["_width"] = "0";
					}
				}
			}
		}
		
		private static UnsafeHashMap<int, VoxelData> ComputeLod(UnsafeHashMap<int, VoxelData> data, int3 worldChunkPosition, int step, int moduloCheck)
		{
			UnsafeHashMap<int, VoxelData> resultLod1 = new UnsafeHashMap<int, VoxelData>(data.Count(), Allocator.TempJob);
			ComputeLodJob computeLodJob = new ComputeLodJob()
			{
				VolumeSize = ChunkVolume,
				WorldChunkPosition = worldChunkPosition,
				Result = resultLod1.AsParallelWriter(),
				Data = data,
				Step = step,
				ModuloCheck = moduloCheck
			};
			JobHandle jobHandle = computeLodJob.Schedule(CHUNK_SIZE, 64);
			jobHandle.Complete();

			return resultLod1;
		}

		private static NativeList<VoxelData> ComputeRotation(UnsafeHashMap<int, VoxelData> data, UnsafeHashMap<int, UnsafeHashMap<int, VoxelData>> worldDataPositions, NativeArray<VoxelMaterialVFX> materials, int step, int3 worldChunkPosition)
		{
			NativeArray<int> keys = data.GetKeyArray(Allocator.TempJob);
			NativeList<VoxelData> result = new NativeList<VoxelData>(data.Count(), Allocator.TempJob);
			JobHandle computeRotationJob = new ComputeVoxelRotationJob()
			{
				Data = data,
				Result = result.AsParallelWriter(),
				Step = step,
				VolumeSize = ChunkVolume,
				Keys = keys,
				WorldDataPositions = worldDataPositions,
				WorldChunkPosition = worldChunkPosition,
				Materials = materials,
			}.Schedule(data.Count(), 64);
			computeRotationJob.Complete();
			keys.Dispose();
			return result;
		}

		private static int3 GetChunkWorldPosition(int chunkIndex)
		{
			int3 pos3d = VoxImporter.Get3DPos(chunkIndex, RelativeWorldVolume);
			return pos3d * CHUNK_SIZE;
		}

		private static Vector3 GetCenterChunkWorldPosition(int chunkIndex)
		{
			int3 chunkPosition = GetChunkWorldPosition(chunkIndex);
			int halfChunkSize = CHUNK_SIZE / 2;

			return new Vector3(chunkPosition.x + halfChunkSize, chunkPosition.y + halfChunkSize, chunkPosition.z + halfChunkSize);
		}

		#endregion
	}
}
