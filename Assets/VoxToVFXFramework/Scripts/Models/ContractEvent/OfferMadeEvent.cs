using System;
using System.Numerics;
using Nethereum.Util;
using Newtonsoft.Json;
using UnityEngine;

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

		[JsonIgnore]
		public decimal TotalInEther
		{
			get
			{
				try
				{
					return UnitConversion.Convert.FromWei(Amount);
				}
				catch (Exception e)
				{
					Debug.LogError(e);
					return 0;
				}
			}
		}

		public string TotalInEtherFixedPoint => TotalInEther.ToString("F2");
	}
}
