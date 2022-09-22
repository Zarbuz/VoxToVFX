using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Utils.Extensions;

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

		public async void Initialize(string address)
		{
			mUser = await DataManager.Instance.GetUserWithCache(address);
			CreatorUsername.text = mUser != null ? mUser.UserName : address.FormatEthAddress(3);
			await CreatorImage.Initialize(mUser);
		}

		#endregion

		#region PrivateMethods

		private void OnOpenUserProfileClicked()
		{
			if (mUser != null)
			{
				CanvasPlayerPCManager.Instance.OpenProfilePanel(mUser);
			}
		}

		#endregion
	}
}
