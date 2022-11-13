using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using VoxToVFXFramework.Scripts.Singleton;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace VoxToVFXFramework.Scripts.Managers
{
	[RequireComponent(typeof(UnityEngine.Camera))]
	public class ManualRTASManager : ModuleSingleton<ManualRTASManager>
	{
		#region ScriptParameters

		[SerializeField] private Mesh Mesh;
		[SerializeField] private Material DebugMaterial;

		#endregion

		#region Fields

		private RayTracingAccelerationStructure mRtas;
		private HDCamera mHdCamera;

		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			mHdCamera = HDCamera.GetOrCreate(GetComponent<UnityEngine.Camera>());
			mRtas ??= new RayTracingAccelerationStructure();
		}

		private void OnDestroy()
		{
			mRtas?.Dispose();
		}

		#endregion

		#region PublicMethods

		public void Build(Dictionary<int, List<Matrix4x4>> chunks)
		{
			mRtas.ClearInstances();

			RayTracingInstanceCullingConfig cullingConfig = new RayTracingInstanceCullingConfig();

			cullingConfig.subMeshFlagsConfig.opaqueMaterials = RayTracingSubMeshFlags.Enabled | RayTracingSubMeshFlags.ClosestHitOnly;
			cullingConfig.subMeshFlagsConfig.alphaTestedMaterials = RayTracingSubMeshFlags.Enabled;
			cullingConfig.subMeshFlagsConfig.transparentMaterials = RayTracingSubMeshFlags.Disabled;

			RayTracingInstanceCullingTest cullingTest = new RayTracingInstanceCullingTest();
			cullingTest.allowAlphaTestedMaterials = true;
			cullingTest.allowOpaqueMaterials = true;
			cullingTest.allowTransparentMaterials = false;
			cullingTest.instanceMask = 255;
			cullingTest.layerMask = -1;
			cullingTest.shadowCastingModeMask = -1;

			cullingConfig.instanceTests = new RayTracingInstanceCullingTest[1];
			cullingConfig.instanceTests[0] = cullingTest;

			mRtas.CullInstances(ref cullingConfig);

			foreach (KeyValuePair<int, List<Matrix4x4>> pair in chunks)
			{
				RayTracingMeshInstanceConfig config = new RayTracingMeshInstanceConfig(Mesh, 0, DebugMaterial);
				mRtas.AddInstances(config, pair.Value);
			}

			// Build the RTAS
			mRtas.Build(transform.position);

			// Assign it to the camera
			mHdCamera.rayTracingAccelerationStructure = mRtas;
		}


		#endregion
	}
}
