using System;
using System.Numerics;
using Nethereum.Util;
using Newtonsoft.Json;
using VoxToVFXFramework.Scripts.Localization;

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

		[JsonIgnore]
		public string TargetAction
		{
			get
			{
				if (BuyPrice != 0)
				{
					return LocalizationKeys.PROFILE_BUY_NOW.Translate();
				}

				//TODO To COMPLETE
				return string.Empty;
			}
		}

		[JsonIgnore]
		public decimal BuyPriceInEther
		{
			get
			{
				if (BuyPrice != 0)
				{
					try
					{
						return UnitConversion.Convert.FromWei(BuyPrice);
					}
					catch
					{
						return 0;
					}
				}

				return 0;
			}
		}

		[JsonIgnore]
		public string BuyPriceInEtherFixedPoint => BuyPriceInEther.ToString("F2");
	}

	public class NFTDetailsCacheDTO
	{
		public NFTDetailsContractType ContractType { get; set; }
		public DateTime LastTimeUpdated { get; set; }
	}
}
