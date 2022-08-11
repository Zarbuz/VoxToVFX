using MoralisUnity.Platform.Objects;
using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models.ContractEvent
{
	public abstract class AbstractContractEvent : MoralisObject
	{
		[JsonProperty("transaction_hash")]
		public string TransactionHash { get; set; }

		[JsonProperty("nounce")]
		public int Nounce { get; set; }

		[JsonProperty("confirmed")]
		public bool Confirmed { get; set; }

		[JsonProperty("block_number")]
		public int BlockNumber { get; set; }

		[JsonProperty("log_index")]
		public int LogIndex { get; set; }
	}
}
