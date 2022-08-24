using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models.ContractEvent
{
	public class CollectionMintedEvent : AbstractContractEvent
	{
		[JsonProperty("tokenCID")]
		public string TokenCID { get; set; }

		[JsonProperty("tokenId")]
		public string TokenID { get; set; }

		[JsonProperty("creator")]
		public string Creator { get; set; }

		[JsonProperty("indexedTokenCID")]
		public string IndexedTokenCID { get; set; }

		[JsonProperty("address")]
		public string Address { get; set; }
	}


}
