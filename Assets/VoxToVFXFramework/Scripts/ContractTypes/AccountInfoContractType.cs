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
	}
}
