using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.Localization
{
	[ExecuteAlways]
	[DisallowMultipleComponent]
	public class LocalizedText : LocalizedField
	{
		#region ScriptParameters

		[SerializeField]
		private string key;

		#endregion

		#region Fields

		private Text mText;
		private TextMeshProUGUI mTextMeshProUGUI;
		private TextMesh mTextMesh;
		private object[] mValues;
		private string mLocalisedString;

		public string Key
		{
			set => key = value;
		}

		#endregion

		#region UnityMethods

		//[ExecuteInEditMode]
		[ExecuteAlways]
		protected override void Start()
		{
			base.Start();
			mText = GetComponent<Text>();
			if (mText)
			{
				UpdateLocalisation();
				return;
			}

			mTextMeshProUGUI = GetComponent<TextMeshProUGUI>();
			if (mTextMeshProUGUI)
			{
				UpdateLocalisation();
			}

			mTextMesh = GetComponent<TextMesh>();
			if (mTextMesh)
			{
				UpdateLocalisation();
				return;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Update the text after a change (e.g. language change).
		/// </summary>
		public override void UpdateLocalisation()
		{
			RefreshLocalisedString();
			AssignLocalStringToTextGUI();
		}

		/// <summary>
		/// Set a new localisation key.
		/// Text update will be triggered.
		/// </summary>
		public void SetKey(string key)
		{
			this.key = key;
			UpdateLocalisation();
		}

		/// <summary>
		/// Sets new values for composite formatting.
		/// Text update will be triggered.
		/// </summary>
		/// <param name="values">The values to compose must be given in the same order as they appear in the key</param>
		public void SetValues(params object[] values)
		{
			this.mValues = values;
			UpdateLocalisation();
		}

		/// <summary>
		/// Sets a new localisation key and new values for composite formatting.
		/// Text update will be triggered.
		/// </summary>
		/// <param name="values">The values to compose must be given in the same order as they appear in the key</param>
		public void SetKeyAndValues(string key, params string[] values)
		{
			this.key = key;
			this.mValues = values;
			UpdateLocalisation();
		}

		private void RefreshLocalisedString()
		{
			if (mLocalizationManager == null)
			{
				if (!(Application.isPlaying))
				{
					Start();
					//mLocalizationManager = LocalizationManager.Get;
				}
				else
				{
					Debug.LogWarning("LocalizationManager is null!");
					mLocalisedString = key;
					return;
				}
			}

			if (mValues != null && mValues.Length > 0)
			{
				mLocalisedString = string.Format(mLocalizationManager.GetValue(key), mValues);
			}
			else
			{
				mLocalisedString = mLocalizationManager.GetValue(key);
			}
		}

		private void AssignLocalStringToTextGUI()
		{
			if (mText)
			{
				mText.text = mLocalisedString;
			}
			else if (mTextMeshProUGUI)
			{
				mTextMeshProUGUI.SetText(mLocalisedString);
			}
			else if (mTextMesh)
			{
				mTextMesh.text = mLocalisedString;
			}
		}

		#endregion
	}
}
