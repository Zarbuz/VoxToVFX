using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.Localization
{
	public class LocalizedDropdown : LocalizedField
	{
		#region Fields

		private Dropdown mDropdown;
		private TMP_Dropdown mTmpDropdown;
		private readonly List<string> mKeyOptions = new List<string>();

		#endregion

		#region UnityMethods

		protected override void Start()
		{
			base.Start();
			mDropdown = GetComponent<Dropdown>();
			if (mDropdown)
			{
				if (InitDictionary())
				{
					mDropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(mDropdown); });
					UpdateLocalisation();
				}
			}
			else
			{
				mTmpDropdown = GetComponent<TMP_Dropdown>();
				if (InitDictionary())
				{
					mTmpDropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(mTmpDropdown); });
					UpdateLocalisation();
				}
			}
		}

		#endregion

		#region Methods

		public override void UpdateLocalisation()
		{
			UpdateOption();
			UpdateLabel();
		}

		#endregion

		#region Implementation

		private void UpdateOption()
		{
			if (mDropdown)
			{
				for (int i = 0; i < mKeyOptions.Count; i++)
				{
					mDropdown.options[i].text = mLocalizationManager.GetValue(mKeyOptions[i]);
				}
			}
			else
			{
				for (int i = 0; i < mKeyOptions.Count; i++)
				{
					mTmpDropdown.options[i].text = mLocalizationManager.GetValue(mKeyOptions[i]);
				}
			}
		}

		private void UpdateLabel()
		{
			if (mDropdown != null)
			{
				mDropdown.captionText.text = mLocalizationManager.GetValue(mKeyOptions[mDropdown.value]);
			}
			else
			{
				mTmpDropdown.captionText.text = mLocalizationManager.GetValue(mKeyOptions[mTmpDropdown.value]);
			}
		}

		private bool InitDictionary()
		{
			if (mDropdown != null)
			{
				foreach (Dropdown.OptionData option in mDropdown.options)
				{
					if (mKeyOptions.Contains(option.text))
					{
						Debug.LogWarningFormat(this, "{0} define two or more time in the same dropdown", option.text);
						mKeyOptions.Clear();
						return false;
					}
					mKeyOptions.Add(option.text);
				}
			}
			else
			{
				foreach (TMP_Dropdown.OptionData option in mTmpDropdown.options)
				{
					if (mKeyOptions.Contains(option.text))
					{
						Debug.LogWarningFormat(this, "{0} define two or more time in the same dropdown", option.text);
						mKeyOptions.Clear();
						return false;
					}
					mKeyOptions.Add(option.text);
				}
			}

			return true;
		}

		private void DropdownValueChanged(Dropdown change)
		{
			UpdateLabel();
		}

		private void DropdownValueChanged(TMP_Dropdown change)
		{
			UpdateLabel();
		}

		#endregion
	}
}
