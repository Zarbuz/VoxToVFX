using Unity.Entities;
using Unity.Physics;

namespace VoxToVFXFramework.Scripts.System
{
	public partial class PhysicsShapeQuerySystem : SystemBase
	{
		public BlobAssetReference<Collider> TopBlobAssetReference;
		public BlobAssetReference<Collider> RightBlobAssetReference;
		public BlobAssetReference<Collider> BottomBlobAssetReference;
		public BlobAssetReference<Collider> LeftBlobAssetReference;
		public BlobAssetReference<Collider> FrontBlobAssetReference;
		public BlobAssetReference<Collider> BackBlobAssetReference;
		protected override void OnStartRunning()
		{
			Entities.ForEach((Entity e, in PhysicsCollider physicsCollider) =>
			{
				string name = EntityManager.GetName(e);
				switch (name)
				{
					case "TopCollider":
						TopBlobAssetReference = physicsCollider.Value;
						break;
					case "RightCollider":
						RightBlobAssetReference = physicsCollider.Value;
						break;
					case "BottomCollider":
						BottomBlobAssetReference = physicsCollider.Value;
						break;
					case "LeftCollider":
						LeftBlobAssetReference = physicsCollider.Value;
						break;
					case "FrontCollider":
						FrontBlobAssetReference = physicsCollider.Value;
						break;
					case "BackCollider":
						BackBlobAssetReference = physicsCollider.Value;
						break;

				}
			}).WithoutBurst()
				.Run();
		}

		protected override void OnUpdate()
		{
			
		}
	}
}
