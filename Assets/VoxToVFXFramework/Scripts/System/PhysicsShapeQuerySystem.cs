//using Unity.Entities;
//using Unity.Physics;
//using VoxToVFXFramework.Scripts.Components;
//using VoxToVFXFramework.Scripts.Managers;

//namespace VoxToVFXFramework.Scripts.System
//{
//	public partial class PhysicsShapeQuerySystem : SystemBase
//	{
//		public EntityQuery EntityQuery;
//		private BlobAssetReference<Collider> mColliderLod1;
//		private BlobAssetReference<Collider> mColliderLod2;
//		private BlobAssetReference<Collider> mColliderLod4;

//		public BlobAssetReference<Collider> GetBlobAssetReference(int lodLevel)
//		{
//			return lodLevel switch
//			{
//				1 => mColliderLod1,
//				2 => mColliderLod2,
//				4 => mColliderLod4,
//				_ => mColliderLod1
//			};
//		}

//		protected override void OnStartRunning()
//		{
//			Entities
//				.ForEach((Entity e, in PhysicsCollider physicsCollider, in PhysicsRefTag physicsRefTag) =>
//				{
//					switch (physicsRefTag.PhysicsRefIndex)
//					{
//						case 1:
//							mColliderLod1 = physicsCollider.Value;
//							break;
//						case 2:
//							mColliderLod2 = physicsCollider.Value;
//							break;
//						case 4:
//							mColliderLod4 = physicsCollider.Value;
//							break;
//					}
//				}).WithoutBurst()
//				.Run();

//			EntityQuery = GetEntityQuery(typeof(VoxelPrefabTag));
//		}

//		protected override void OnUpdate()
//		{

//		}
//	}
//}
