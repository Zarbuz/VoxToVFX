using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models.ContractEvent
{
	public class BuyPriceSetEvent : AbstractContractEvent
	{
		[JsonProperty("price")]
		public string Price { get; set; }

		[JsonProperty("seller")]
		public string Seller { get; set; }

		[JsonProperty("tokenId")]
		public string TokenId { get; set; }

		[JsonProperty("nftContract")]
		public string NFTContract { get; set; }
	}
}
