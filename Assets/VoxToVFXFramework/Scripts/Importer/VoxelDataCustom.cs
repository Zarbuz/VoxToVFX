using FileToVoxCore.Vox;
using Unity.Collections;
using UnityEngine;

namespace VoxToVFXFramework.Scripts.Importer
{
	public class VoxelDataCustom : VoxelData
	{
		public NativeHashMap<int, Vector4> VoxelNativeHashMap;
	}
}
