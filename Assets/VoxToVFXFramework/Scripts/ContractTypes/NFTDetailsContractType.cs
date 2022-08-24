using System.Numerics;
using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.ContractTypes
{
	public class NFTDetailsContractType
	{
		[JsonProperty("owner")]
		public string Owner { get; set; }

		[JsonProperty("isInEscrow")]
		public bool IsInEscrow { get; set; }

		[JsonProperty("auctionBidder")]
		public string AuctionBidder { get; set; }

		[JsonProperty("auctionEndTime")]
		public BigInteger AuctionEndTime { get; set; }

		[JsonProperty("auctionPrice")]
		public BigInteger AuctionPrice { get; set; }

		[JsonProperty("auctionId")]
		public BigInteger AuctionId { get; set; }

		[JsonProperty("buyPrice")]
		public BigInteger BuyPrice { get; set; }

		[JsonProperty("offerAmount")]
		public BigInteger OfferAmount { get; set; }

		[JsonProperty("offerBuyer")]
		public string OfferBuyer { get; set; }
		
		[JsonProperty("offerExpiration")]
		public BigInteger OfferExpiration { get; set; }
	}
}
