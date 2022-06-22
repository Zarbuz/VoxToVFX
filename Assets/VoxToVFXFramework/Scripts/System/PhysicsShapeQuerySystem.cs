using Unity.Entities;
using Unity.Physics;
using VoxToVFXFramework.Scripts.Components;

namespace VoxToVFXFramework.Scripts.System
{
	public partial class PhysicsShapeQuerySystem : SystemBase
	{
		public BlobAssetReference<Collider> ColliderLod1;
		public BlobAssetReference<Collider> ColliderLod2;
		public BlobAssetReference<Collider> ColliderLod4;
		public EntityQuery EntityQuery;

		protected override void OnStartRunning()
		{
			Entities
				.ForEach((Entity e, in PhysicsCollider physicsCollider, in PhysicsRefTag physicsRefTag) =>
				{
					switch (physicsRefTag.PhysicsRefIndex)
					{
						case 1:
							ColliderLod1 = physicsCollider.Value;
							break;
						case 2:
							ColliderLod2 = physicsCollider.Value;
							break;
						case 4:
							ColliderLod4 = physicsCollider.Value;
							break;
					}
				}).WithoutBurst()
				.Run();

			EntityQuery = GetEntityQuery(typeof(VoxelPrefabTag));
		}

		protected override void OnUpdate()
		{

		}
	}
}
