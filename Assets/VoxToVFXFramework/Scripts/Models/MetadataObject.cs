﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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