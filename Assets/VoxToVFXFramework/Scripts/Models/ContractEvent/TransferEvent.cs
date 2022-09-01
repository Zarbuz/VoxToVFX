using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models.ContractEvent
{
	public class TransferEvent : AbstractContractEvent
	{
		[JsonProperty("from")]
		public string From { get; set; }

		[JsonProperty("tokenId")]
		public string TokenId { get; set; }

		[JsonProperty("address")]
		public string Address { get; set; }
	}
}
