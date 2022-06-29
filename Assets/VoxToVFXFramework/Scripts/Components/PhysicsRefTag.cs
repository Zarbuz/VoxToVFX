using Unity.Entities;

namespace VoxToVFXFramework.Scripts.Components
{
	[GenerateAuthoringComponent]
	public struct PhysicsRefTag : IComponentData
	{
		public int PhysicsRefIndex;
	}
}
