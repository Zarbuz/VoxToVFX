using System.Numerics;
using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models.ContractEvent
{
	internal class BuyPriceAcceptedEvent : AbstractContractEvent
	{
		[JsonProperty("nftContract")]
		public string NftContract { get; set; }

		[JsonProperty("tokenId")]
		public string TokenId { get; set; }

		[JsonProperty("seller")]
		public string Seller { get; set; }

		[JsonProperty("buyer")]
		public string Buyer { get; set; }

		[JsonProperty("protocolFee")]
		public BigInteger ProtocolFee { get; set; }

		[JsonProperty("creatorFee")]
		public BigInteger CreatorFee { get; set; }

		[JsonProperty("sellerRev")]
		public BigInteger SellerRev { get; set; }
	}
}
