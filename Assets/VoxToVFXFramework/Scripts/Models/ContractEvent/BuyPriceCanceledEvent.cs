using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models.ContractEvent
{
	public class BuyPriceCanceledEvent : AbstractContractEvent
	{
		[JsonProperty("nftContract")]
		public string NFTContract { get; set; }

		[JsonProperty("tokenId")]
		public string TokenId { get; set; }
	}
}
