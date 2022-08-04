using System;
using MoralisUnity.Platform.Objects;

namespace VoxToVFXFramework.Scripts.Models
{
	public class CustomUser : MoralisObject
	{
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string Name { get; set; }
		public string Bio { get; set; }
		public string PictureUrl { get; set; }
		public string BannerUrl { get; set; }
		public string WebsiteUrl { get; set; }
		public string Discord { get; set; }
		public string YoutubeUrl { get; set; }
		public string FacebookUrl { get; set; }
		public string TwitchUsername { get; set; }
		public string TikTokUsername { get; set; }
		public string SnapchatUsername { get; set; }

		public CustomUser()
		{
			ClassName = "CustomUser";
		}
	}

}
