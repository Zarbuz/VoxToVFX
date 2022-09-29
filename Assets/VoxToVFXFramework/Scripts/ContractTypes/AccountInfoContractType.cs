using System;
using Nethereum.Util;
using Newtonsoft.Json;
using System.Numerics;

namespace VoxToVFXFramework.Scripts.ContractTypes
{
	public class AccountInfoContractType
	{
		[JsonProperty("ethBalance")]
		public BigInteger EthBalance { get; set; }

		[JsonProperty("availableFethBalance")]
		public BigInteger AvailableFethBalance { get; set; }

		[JsonProperty("lockedFethBalance")]
		public BigInteger LockedFethBalance { get; set; }

		[JsonProperty("EnsName")]
		public string EnsName { get; set; }

		[JsonIgnore]
		public decimal AvailableBalance
		{
			get
			{
				try
				{
					return UnitConversion.Convert.FromWei(AvailableFethBalance);
				}
				catch
				{
					return 0;
				}
			}
		}

		[JsonIgnore]
		public decimal Balance
		{
			get
			{
				try
				{
					return UnitConversion.Convert.FromWei(EthBalance);
				}
				catch
				{
					return 0;
				}
			}
		}

		[JsonIgnore]
		public DateTime LastUpdate { get; set; }
	}
}
