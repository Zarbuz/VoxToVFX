using MoralisUnity.Platform.Objects;
using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models
{
	public class MintedEvent : MoralisObject
	{
		[JsonProperty("address")]
		public string Address { get; set; }

		[JsonProperty("tokenId")]
		public int TokenId { get; set; }

		[JsonProperty("indexedTokenCID")]
		public string IndexedTokenCID { get; set; }

		[JsonProperty("tokenCID")]
		public string TokenCID { get; set; }

		public MintedEvent()
		{
			ClassName = "Minted";
		}
	}
}
