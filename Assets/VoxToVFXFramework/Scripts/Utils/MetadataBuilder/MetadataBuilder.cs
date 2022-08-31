using System;
using System.Collections.Generic;
using VoxToVFXFramework.Scripts.Models;

namespace VoxToVFXFramework.Scripts.Utils.MetadataBuilder
{
	public static class MetadataBuilder
	{
		public static MetadataObject BuildMetadata(string name, string description, string imageUrl, List<string> files)
		{
			MetadataObject metadata = new MetadataObject
			{
				Description = description,
				Name = name,
				FilesUrl = files,
				Image = imageUrl,
			};
			return metadata;
		}
	}
}
