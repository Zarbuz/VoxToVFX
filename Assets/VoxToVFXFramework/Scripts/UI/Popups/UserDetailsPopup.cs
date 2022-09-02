using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.UI.Atomic;

namespace VoxToVFXFramework.Scripts.UI.Popups
{
	public class UserDetailsPopup : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private AvatarImage AvatarImage;
		[SerializeField] private TextMeshProUGUI NameText;
		[SerializeField] private TextMeshProUGUI UserNameText;
		[SerializeField] private TextMeshProUGUI FollowingCountText;
		[SerializeField] private TextMeshProUGUI FollowersCountText;
		[SerializeField] private Button FollowButton;
		[SerializeField] private Button UnFollowButton;

		#endregion

		#region PublicMethods

		public void Initialize(CustomUser user)
		{
			FollowButton.onClick.AddListener(OnFollowClicked);
			UnFollowButton.onClick.AddListener(OnUnFollowClicked);
			if (user != null)
			{
				AvatarImage.Initialize(user);
				NameText.text = user.Name;
				UserNameText.text = "@" + user.UserName;
			}
		}

		#endregion

		#region PrivateMethods

		private void OnFollowClicked()
		{
			//TODO
		}

		private void OnUnFollowClicked()
		{
			//TODO
		}

		#endregion
	}
}
