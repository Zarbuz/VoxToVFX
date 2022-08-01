using UnityEngine;

namespace VoxToVFXFramework.Scripts.ScriptableObjets
{
	[CreateAssetMenu(fileName = "SmartContractAddressConfig", menuName = "VoxToVFX/SmartContractAddressConfig")]
	public class SmartContractAddressConfig : ScriptableObject
	{
		public string VoxToVFXTreasuryAddress;
		public string ExternalProxyCallAddress;
		public string CollectionFactoryAddress;
		public string CollectionContractAddress;

		[TextArea]
		public string VoxToVFXTreasuryABI;

		[TextArea]
		public string ExternalProxyCallABI;

		[TextArea]
		public string CollectionContractABI;

		[TextArea]
		public string CollectionFactoryABI;
	}
}
