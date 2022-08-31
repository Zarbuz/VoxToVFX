using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace VoxToVFXFramework.Scripts.Models
{
	public class MetadataObject
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("image")]
		public string Image { get; set; }

		[JsonProperty("files_url")]
		public List<string> FilesUrl { get; set; }

	}
}
