using MoralisUnity.Platform.Objects;

namespace VoxToVFXFramework.Scripts.Models
{
	public class CollectionDetails : MoralisObject
	{
		public string CollectionContract { get; set; }
		public string LogoImageUrl { get; set; }
		public string CoverImageUrl { get; set; }
		public string Description { get; set; }
	}
}
