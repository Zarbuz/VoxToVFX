using System;
using MoralisUnity.Platform.Objects;

namespace VoxToVFXFramework.Scripts.Models
{
	public class CustomUserDTO : MoralisUser
	{
		public string Name { get; set; }
		public string Bio { get; set; }
		public string PictureUrl { get; set; }

		public CustomUserDTO()
		{

		}

		public CustomUserDTO(MoralisUser baseUser)
		{
			accounts = baseUser.accounts;
			ACL = baseUser.ACL;
			authData = baseUser.authData;
			createdAt = baseUser.createdAt;
			email = baseUser.email;
			ethAddress = baseUser.ethAddress;
			objectId = baseUser.objectId;
			sessionToken = baseUser.sessionToken;
			updatedAt = baseUser.updatedAt;
			username = baseUser.username;
			// Necessary step to make sure the customer user is saved to
			// the _User table and not a new table.
			ClassName = baseUser.ClassName;
		}

	}

	public class _User : CustomUserDTO
	{

	}
}
