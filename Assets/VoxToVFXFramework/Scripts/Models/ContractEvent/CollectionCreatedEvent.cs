using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models.ContractEvent
{
	public class CollectionCreatedEvent : AbstractContractEvent
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("creator")]
		public string Creator { get; set; }

		[JsonProperty("collectionContract")]
		public string CollectionContract { get; set; }

		[JsonProperty("address")]
		public string Address { get; set; }

		[JsonProperty("symbol")]
		public string Symbol { get; set; }

	}
}
