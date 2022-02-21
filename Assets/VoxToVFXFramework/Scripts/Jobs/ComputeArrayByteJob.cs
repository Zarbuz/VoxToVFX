using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxToVFXFramework.Scripts.Importer;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct ComputeArrayByteJob : IJobParallelFor
	{
		[ReadOnly] public Vector3 VolumeSize;
		[ReadOnly] public NativeArray<Vector4> Data;
		[NativeDisableParallelForRestriction]
		[WriteOnly] public NativeArray<byte> Result;

		public void Execute(int index)
		{
			Vector4 vec4 = Data[index];
			Result[VoxImporter.GetGridPos((int)vec4.x, (int)vec4.y, (int)vec4.z, VolumeSize)] = (byte)vec4.w;
		}
	}
}
