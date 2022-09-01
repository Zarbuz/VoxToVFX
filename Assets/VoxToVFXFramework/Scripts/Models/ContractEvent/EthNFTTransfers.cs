using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models.ContractEvent
{
	public class EthNFTTransfers : AbstractContractEvent
	{
		[JsonProperty("token_id")]
		public string TokenId { get; set; }

		[JsonProperty("contract_type")]
		public string ContractType { get; set; }

		[JsonProperty("token_address")]
		public string TokenAddress { get; set; }

		[JsonProperty("to_address")]
		public string ToAddress { get; set; }

		[JsonProperty("amount")]
		public int Amount { get; set; }

		[JsonProperty("from_address")]
		public string FromAddress { get; set; }
	}
}
