using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	public class AvatarImage : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Image ProfileImage;
		[SerializeField] private Image NoAvatarImage;

		#endregion

		#region PublicMethods

		public async void Initialize(CustomUser user)
		{
			NoAvatarImage.gameObject.SetActive(true);
			ProfileImage.gameObject.SetActive(false);

			if (!string.IsNullOrEmpty(user.PictureUrl))
			{
				bool success = await ImageUtils.DownloadAndApplyImage(user.PictureUrl, ProfileImage, 256, true, true, true);
				if (success)
				{
					NoAvatarImage.gameObject.SetActive(false);
					ProfileImage.gameObject.SetActive(true);
				}
				else
				{
					NoAvatarImage.gameObject.SetActive(true);
					ProfileImage.gameObject.SetActive(false);
				}
			}
		}

		#endregion
	}
}
