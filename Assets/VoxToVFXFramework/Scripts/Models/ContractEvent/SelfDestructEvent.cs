using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models.ContractEvent
{
	public class SelfDestructEvent : AbstractContractEvent
	{
		[JsonProperty("address")]
		public string Address { get; set; }

		[JsonProperty("owner")]
		public string Owner { get; set; }
	}
}
