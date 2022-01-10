using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;
using VoxToVFXFramework.Scripts.Data;

namespace VoxToVFXFramework.Scripts.ScriptableObjects
{
	[CreateAssetMenu(fileName = "VisualEffectDataConfig", menuName = "VoxToVFX/VisualEffectDataConfig", order = 2)]
	public class VisualEffectDataConfig : ScriptableObject
	{
		[ListDrawerSettings(NumberOfItemsPerPage = 20)]
		public List<ListWrapper> DataOpaque;
		[ListDrawerSettings(NumberOfItemsPerPage = 20)]
		public List<ListWrapper> DataTransparent;
		public VoxelMaterialVFX[] Material;
	}

	public class CustomSchematicValueEntry
	{
		public long Key;
		[ListDrawerSettings(NumberOfItemsPerPage = 20)]
		public List<VoxelVFX> List;
	}

	[System.Serializable]
	public class ListWrapper
	{
		[ListDrawerSettings(NumberOfItemsPerPage = 20)]
		public List<VoxelVFX> List;

		public ListWrapper()
		{
			List = new List<VoxelVFX>();
		}
	}
}
