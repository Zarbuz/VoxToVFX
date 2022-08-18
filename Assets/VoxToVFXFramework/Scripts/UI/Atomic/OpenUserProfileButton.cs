using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Models;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	[RequireComponent(typeof(Button))]
	public class OpenUserProfileButton : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI CreatorUsername;
		[SerializeField] private AvatarImage CreatorImage;

		#endregion

		#region Fields

		private Button mButton;
		private CustomUser mUser;

		#endregion

		#region UnityMethods

		private void Awake()
		{
			mButton = GetComponent<Button>();
			mButton.onClick.AddListener(OnOpenUserProfileClicked);
		}

		#endregion

		#region PublicMethods

		public async void Initialize(CustomUser user)
		{
			mUser = user;
			CreatorUsername.text = user.UserName;
			await CreatorImage.Initialize(user);
		}

		#endregion

		#region PrivateMethods

		private void OnOpenUserProfileClicked()
		{
			CanvasPlayerPCManager.Instance.OpenProfilePanel(mUser);

		}

		#endregion
	}
}
