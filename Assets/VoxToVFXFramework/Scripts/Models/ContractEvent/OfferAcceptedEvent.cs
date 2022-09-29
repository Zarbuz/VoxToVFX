using System;
using System.Numerics;
using Nethereum.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace VoxToVFXFramework.Scripts.Models.ContractEvent
{
	public class OfferAcceptedEvent : AbstractContractEvent
	{
		[JsonProperty("nftContract")]
		public string NFTContract { get; set; }

		[JsonProperty("tokenId")]
		public string TokenId { get; set; }

		[JsonProperty("buyer")]
		public string Buyer { get; set; }

		[JsonProperty("seller")]
		public string Seller { get; set; }

		[JsonProperty("protocolFee")]
		public BigInteger ProtocolFee { get; set; }

		[JsonProperty("creatorFee")]
		public BigInteger CreatorFee { get; set; }

		[JsonProperty("sellerRev")]
		public BigInteger SellerRev { get; set; }

		[JsonIgnore]
		public BigInteger Total => ProtocolFee + CreatorFee + SellerRev;

		[JsonIgnore]
		public decimal TotalInEther
		{
			get
			{
				try
				{
					return UnitConversion.Convert.FromWei(Total);
				}
				catch (Exception e)
				{
					Debug.LogError(e);
					return 0;
				}
			}
		}
	}
}
