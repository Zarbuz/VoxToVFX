using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.UI.Popups;
using static UnityEngine.UI.GridLayoutGroup;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	[RequireComponent(typeof(Button))]
	public class ButtonTriggerUserDetailsPopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		#region ScriptParameters

		[SerializeField] private UserDetailsPopup UserDetailsPopupPrefab;

		#endregion

		#region Fields

		private GameObject mTooltip;
		private CustomUser mCustomUser;

		#endregion

		#region UnityMethods

		private void Start()
		{
			Button button = GetComponent<Button>();
			button.onClick.AddListener(OnButtonClicked);
		}

		private void OnDisable()
		{
			if (mTooltip != null)
			{
				Destroy(mTooltip);
			}
		}



		#endregion
		#region PublicMethods

		public async void Initialize(string userAddress)
		{
			mCustomUser = await DataManager.Instance.GetUserWithCache(userAddress);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (mCustomUser != null)
			{
				SpawnUserDetailsPopup();
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (mTooltip != null)
			{
				Destroy(mTooltip);
			}
		}

		#endregion

		#region PrivateMethods

		private void OnButtonClicked()
		{
			if (mCustomUser != null)
			{
				CanvasPlayerPCManager.Instance.OpenProfilePanel(mCustomUser);
			}
		}

		private void SpawnUserDetailsPopup()
		{
			UserDetailsPopup tooltip = Instantiate(UserDetailsPopupPrefab, transform, false);
			tooltip.Initialize(mCustomUser);
			mTooltip = tooltip.gameObject;
			if (transform.localEulerAngles.z != 0)
			{
				RectTransform rt = GetComponent<RectTransform>();
				Vector2 size = rt.rect.size;

				mTooltip.transform.localEulerAngles = new Vector3(0, 0, -transform.localEulerAngles.z);
				mTooltip.transform.localPosition = new Vector3(size.x / 2 + 4, size.y / 2 + 4);
			}
		}


		#endregion
	}
}
