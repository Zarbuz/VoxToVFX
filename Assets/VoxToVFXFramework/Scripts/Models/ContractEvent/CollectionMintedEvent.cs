using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models.ContractEvent
{
	public class CollectionMintedEvent : AbstractContractEvent
	{
		[JsonProperty("tokenCID")]
		public string TokenCID { get; set; }

		//[JsonProperty("tokenId_decimal")]
		//public int TokenIdDecimal { get; set; }

		[JsonProperty("creator")]
		public string Creator { get; set; }

		[JsonProperty("indexedTokenCID")]
		public string IndexedTokenCID { get; set; }

		
	}
}
