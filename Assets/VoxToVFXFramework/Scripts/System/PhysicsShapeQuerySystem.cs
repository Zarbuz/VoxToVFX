using Unity.Entities;
using Unity.Physics;

namespace VoxToVFXFramework.Scripts.System
{
	public partial class PhysicsShapeQuerySystem : SystemBase
	{
		public BlobAssetReference<Collider> BlobAssetReference;

		protected override void OnStartRunning()
		{
			Entities.ForEach((Entity e, in PhysicsCollider physicsCollider) =>
				{
					BlobAssetReference = physicsCollider.Value;
				}).WithoutBurst()
				.Run();
		}

		protected override void OnUpdate()
		{

		}
	}
}
