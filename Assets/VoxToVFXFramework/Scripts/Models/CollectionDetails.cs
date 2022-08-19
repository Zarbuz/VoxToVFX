using MoralisUnity.Platform.Objects;
using Newtonsoft.Json;

namespace VoxToVFXFramework.Scripts.Models
{
	public class CollectionDetails : MoralisObject
	{
		[JsonProperty("collectionContract")]
		public string CollectionContract { get; set; }

		[JsonProperty("logo_image_url")]
		public string LogoImageUrl { get; set; }

		[JsonProperty("cover_image_url")]
		public string CoverImageUrl { get; set; }
		
		[JsonProperty("description")]
		public string Description { get; set; }
	}
}
