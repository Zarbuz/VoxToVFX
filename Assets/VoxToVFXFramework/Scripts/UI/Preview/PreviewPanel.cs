using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;

namespace VoxToVFXFramework.Scripts.UI.Preview
{
	public class PreviewPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private TextMeshProUGUI Description;
		[SerializeField] private Button BackNFTCreation;

		#endregion

		#region Fields

		private Action mOnBackCallback;

		#endregion

		#region PublicMethods

		public void Initialize(string title, string description, Action onBackCallback)
		{
			gameObject.SetActive(true);
			mOnBackCallback = onBackCallback;
			Title.text = string.IsNullOrEmpty(title) ? LocalizationKeys.PREVIEW_MISSING_TITLE.Translate() : title;
			Description.text = string.IsNullOrEmpty(description) ? LocalizationKeys.PREVIEW_MISSING_DESCRIPTION.Translate(): description;

			BackNFTCreation.onClick.RemoveAllListeners();
			BackNFTCreation.onClick.AddListener(OnBackClicked);
		}

		#endregion

		#region PrivateMethods

		private void OnBackClicked()
		{
			gameObject.SetActive(false);
			mOnBackCallback?.Invoke();
		}

		#endregion
	}
}
