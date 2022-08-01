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

		public CustomUser()
		{
			ClassName = "CustomUser";
		}
	}

}
