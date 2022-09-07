using MoralisUnity.Web3Api.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.UI.Atomic;

namespace VoxToVFXFramework.Scripts.UI.Popups.Popup.OwnedBy
{
	[RequireComponent(typeof(Button))]
	public class CollectionOwnerItem : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private AvatarImage AvatarImage;
		[SerializeField] private TextMeshProUGUI NameText;
		[SerializeField] private TextMeshProUGUI UsernameText;
		[SerializeField] private Button FollowButton;
		[SerializeField] private Button UnFollowButton;

		#endregion

		#region Fields

		private CustomUser mUser;

		#endregion

		#region PublicMethods

		public async void Initialize(string ethAddress)
		{
			Button button = GetComponent<Button>();
			button.onClick.AddListener(OnButtonClicked);
			mUser = await DataManager.Instance.GetUserWithCache(ethAddress);
			button.interactable = mUser != null;

			FollowButton.gameObject.SetActive(false); //TODO
			UnFollowButton.gameObject.SetActive(false); //TODO
			await AvatarImage.Initialize(mUser);
			if (mUser != null)
			{
				NameText.text = mUser.Name;
				UsernameText.text = "@" + mUser.UserName;
			}
			else
			{
				NameText.text = ethAddress;
				UsernameText.text = string.Empty;
			}
		}

		#endregion

		#region PrivateMethods

		private void OnButtonClicked()
		{
			CanvasPlayerPCManager.Instance.OpenProfilePanel(mUser);
			MessagePopup.CleanDisplayPopup(MessagePopupUnicityTag.OWNED_BY);
		}

		#endregion
	}
}
