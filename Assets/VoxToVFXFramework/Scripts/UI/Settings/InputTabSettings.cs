using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;

namespace VoxToVFXFramework.Scripts.UI.Settings
{
	public class InputTabSettings : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Transform ListParent;
		[SerializeField] private InputTabSettingsItem ItemPrefab;
		[SerializeField] private Button ResetSettingsButton;

		#endregion

		#region Fields

		private bool mIsWaitingKey;
		private Event mKeyEvent;
		private KeyCode mNewKey;
		private InputTabSettingsItem mSelectedItem;
		private readonly List<InputTabSettingsItem> mItems = new List<InputTabSettingsItem>();
		private InputTabSettingsItem mItemWithWarning;
		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			ResetSettingsButton.onClick.AddListener(OnResetSettingsClicked);
		}

		private void OnDisable()
		{
			HidePreviousWarning();
			ResetSettingsButton.onClick.RemoveListener(OnResetSettingsClicked);
		}

		private void Start()
		{
			//string previousCategory = string.Empty;

			foreach (InputInfo inputInfo in InputManager.Instance.InputSettings.Settings)
			{
				//if (previousCategory != inputInfo.KeyCategory.ToString())
				//{
				//	TextMeshProUGUI subTitle = Instantiate(SubCategoryTitle, ListParent, false);
				//	subTitle.gameObject.SetActive(true);
				//	subTitle.text = ("[SETTINGS_KEY_SUBTITLE_" + inputInfo.KeyCategory.ToString().ToUpperInvariant() + "]").Translate();
				//	previousCategory = inputInfo.KeyCategory.ToString();
				//}
				InputTabSettingsItem item = Instantiate(ItemPrefab, ListParent, false);
				item.Initialize(inputInfo, OnButtonClicked);
				mItems.Add(item);
			}
		}


		private void OnGUI()
		{
			mKeyEvent = Event.current;
			if ((mKeyEvent.isKey || mKeyEvent.isMouse) && mIsWaitingKey)
			{
				mNewKey = mKeyEvent.isKey
					? mKeyEvent.keyCode
					: (KeyCode)Enum.Parse(typeof(KeyCode), "Mouse" + mKeyEvent.button);
				mIsWaitingKey = false;
			}
		}

		#endregion

		#region PublicMethods

		public void StartAssignment(string keyName)
		{
			if (!mIsWaitingKey)
			{
				StartCoroutine(AssignKey(keyName));
			}
		}

		#endregion

		#region PrivateMethods

		private void RefreshAllKeys()
		{
			foreach (InputTabSettingsItem item in mItems)
			{
				item.RefreshKey();
			}
		}

		private void OnResetSettingsClicked()
		{
			InputManager.Instance.ResetSettings();
			RefreshAllKeys();
		}

		private void OnButtonClicked(InputTabSettingsItem item)
		{
			if (!mIsWaitingKey)
			{
				mSelectedItem = item;
				StartAssignment(item.InputInfo.KeyName);
			}
		}

		private IEnumerator WaitForKey()
		{
			while (mIsWaitingKey)
			{
				yield return null;
			}
		}

		private IEnumerator AssignKey(string keyName)
		{
			mIsWaitingKey = true;
			HidePreviousWarning();
			yield return WaitForKey();

			if (InputManager.Instance.ConfigKeys[keyName].Key != mNewKey && mNewKey != KeyCode.Escape)
			{
				if (InputManager.Instance.SetKey(keyName,/* mSelectedItem.InputInfo.KeyCategory,*/ mNewKey))
				{
					mSelectedItem.RefreshKey();
				}
				else
				{
					DisplayWarning();
				}
			}
			else
			{
				mSelectedItem.RefreshKey();
			}
		}

		private void HidePreviousWarning()
		{
			if (mItemWithWarning == null) return;

			mItemWithWarning.HideWarningIcon();
			mItemWithWarning = null;
		}

		private void DisplayWarning()
		{
			mSelectedItem.DisplayWarningIcon(mNewKey);
			mItemWithWarning = mSelectedItem;
		}

		#endregion
	}
}
