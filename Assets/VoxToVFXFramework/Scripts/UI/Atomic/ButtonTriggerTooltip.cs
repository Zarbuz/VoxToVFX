using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VoxToVFXFramework.Scripts.Localization;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	public class ButtonTriggerTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		#region ScriptParameters

		public string LocalizationKey;

		[SerializeField] private GameObject TooltipPrefab;
		[SerializeField] private GameObject OptionalPanel;

		#endregion

		#region Fields

		private GameObject mTooltip;
		private bool mCanBeVisible = true;

		#endregion

		#region UnityMethods

		private void Update()
		{
			if (OptionalPanel != null)
			{
				mCanBeVisible = !OptionalPanel.activeSelf;

				if (mTooltip != null)
				{
					mTooltip.SetActive(mCanBeVisible);
				}
			}
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

		public void OnPointerEnter(PointerEventData eventData)
		{
			SpawnToolTip();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (mTooltip != null)
			{
				Destroy(mTooltip);
			}
		}

		#endregion

		public void RefreshTooltip(string text)
		{
			LocalizationKey = text;
			if (mTooltip != null)
			{
				DestroyImmediate(mTooltip);
				SpawnToolTip();
				//mTooltip.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationKey.Translate();
			}
		}

		private void SpawnToolTip()
		{
			GameObject tooltip = Instantiate(TooltipPrefab, transform, false);
			mTooltip = tooltip;
			if (transform.localScale.x == -1)
			{
				mTooltip.transform.localScale = new Vector3(-1, 1, 1);
			}

			if (transform.localEulerAngles.z != 0)
			{
				RectTransform rt = GetComponent<RectTransform>();
				Vector2 size = rt.rect.size;

				mTooltip.transform.localEulerAngles = new Vector3(0, 0, -transform.localEulerAngles.z);
				mTooltip.transform.localPosition = new Vector3(size.x / 2 + 4, size.y / 2 + 4);
			}

			tooltip.gameObject.SetActive(mCanBeVisible);
			tooltip.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationKey.Translate();
		}
	}
}
