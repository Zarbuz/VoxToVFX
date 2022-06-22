using Unity.Entities;
using Unity.Physics;
using VoxToVFXFramework.Scripts.Data;

namespace VoxToVFXFramework.Scripts.System
{
	public partial class PhysicsShapeQuerySystem : SystemBase
	{
		public BlobAssetReference<Collider> BlobAssetReference;
		public EntityQuery EntityQuery;

		protected override void OnStartRunning()
		{
			Entities.ForEach((Entity e, in PhysicsCollider physicsCollider) =>
				{
					BlobAssetReference = physicsCollider.Value;
				}).WithoutBurst()
				.Run();

			EntityQuery = GetEntityQuery(typeof(VoxelPrefabTag));
		}

		protected override void OnUpdate()
		{

		}
	}
}
