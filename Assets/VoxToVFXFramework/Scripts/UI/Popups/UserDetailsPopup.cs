using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Managers;
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
		[SerializeField] private ButtonFollowUser FollowButton;

		#endregion

		#region Fields

		private CustomUser mUser;

		#endregion

		#region PublicMethods

		public async void Initialize(CustomUser user)
		{
			if (user != null)
			{
				mUser = user;
				NameText.text = user.Name;
				UserNameText.text = "@" + user.UserName;
				await AvatarImage.Initialize(user);
				FollowButton.gameObject.SetActive(UserManager.Instance.CurrentUserAddress != user.EthAddress);

				int followers = await DataManager.Instance.GetCountUserFollowers(user.EthAddress);
				int following = await DataManager.Instance.GetCountUserFollowings(user.EthAddress);
				FollowersCountText.text = followers.ToString();
				FollowingCountText.text = following.ToString();

				bool isFollowing = DataManager.Instance.IsUserFollowing(user.EthAddress);
				FollowButton.Initialize(isFollowing, user.EthAddress, RefreshFollowers);
			}
		}

		private async void RefreshFollowers()
		{
			int followers = await DataManager.Instance.GetCountUserFollowers(mUser.EthAddress);
			int following = await DataManager.Instance.GetCountUserFollowings(mUser.EthAddress);
			FollowersCountText.text = followers.ToString();
			FollowingCountText.text = following.ToString();
		}

		#endregion


	}
}
