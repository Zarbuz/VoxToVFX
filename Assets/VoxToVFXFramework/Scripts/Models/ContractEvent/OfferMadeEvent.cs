using System.Numerics;
using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models.ContractEvent
{
	public class OfferMadeEvent : AbstractContractEvent
	{
		[JsonProperty("nftContract")]
		public string NFTContract { get; set; }

		[JsonProperty("tokenId")]
		public string TokenId { get; set; }

		[JsonProperty("buyer")]
		public string Buyer { get; set; }

		[JsonProperty("amount")]
		public BigInteger Amount { get; set; }

		[JsonProperty("expiration")]
		public BigInteger Expiration { get; set; }
	}
}
