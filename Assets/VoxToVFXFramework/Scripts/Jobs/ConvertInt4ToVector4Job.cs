using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct ConvertInt4ToVector4Job : IJobParallelFor
	{
		[ReadOnly] public NativeArray<int4> Data;
		[WriteOnly] public NativeArray<Vector4> Result;

		public void Execute(int index)
		{
			int4 val = Data[index];
			Result[index] = new Vector4(val.x, val.y, val.z, val.w); 
		}
	}
}
