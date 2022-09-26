using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	public class AvatarImage : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private RawImage ProfileImage;
		[SerializeField] private Image NoAvatarImage;

		#endregion

		#region PublicMethods

		public async UniTask Initialize(CustomUser user)
		{
			NoAvatarImage.gameObject.SetActive(user == null || string.IsNullOrEmpty(user.PictureUrl));
			ProfileImage.gameObject.SetActive(user != null && !string.IsNullOrEmpty(user.PictureUrl));

			if (user != null && !string.IsNullOrEmpty(user.PictureUrl))
			{
				bool success = await ImageUtils.DownloadAndApplyImageAndCrop(user.PictureUrl, ProfileImage, 256, 256);
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
