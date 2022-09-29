using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models.ContractEvent
{
	internal class BuyPriceInvalidatedEvent : AbstractContractEvent
	{
		[JsonProperty("nftContract")]
		public string NFTContract { get; set; }

		[JsonProperty("tokenId")]
		public string TokenId { get; set; }
	}
}
