using MoralisUnity.Platform.Objects;
using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models
{
	public class CollectionCreatedEvent : MoralisObject
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("creator")]
		public string Creator { get; set; }

		[JsonProperty("transaction_hash")]
		public string TransactionHash { get; set; }

		[JsonProperty("nounce")]
		public int Nounce { get; set; }

		[JsonProperty("collectionContract")]
		public string CollectionContract { get; set; }

		[JsonProperty("address")]
		public string Address { get; set; }

		[JsonProperty("symbol")]
		public string Symbol { get; set; }

		[JsonProperty("confirmed")]
		public bool Confirmed { get; set; }
	}
}
