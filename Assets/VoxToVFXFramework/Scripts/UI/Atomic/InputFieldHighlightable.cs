using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Utils.Extensions;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	public class InputFieldHighlightable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
	{
		#region ScriptParameters

		[SerializeField] private TMP_InputField InputField;
		[SerializeField] private Image Outline;
		[SerializeField] private Sprite SelectedOutline;
		[SerializeField] private TextMeshProUGUI TitleText; // Can be null
		[SerializeField] private TextMeshProUGUI Text;
		[SerializeField] private TextMeshProUGUI Placeholder;
		[SerializeField] private Image Icon; // Can be null
		[SerializeField] private Color HighlighterColor;
		[SerializeField] private string PlaceholderKey;

		#endregion

		#region Fields

		private Sprite mOutlineBaseSprite;
		private Color mOutlineBaseColor;
		private Color mTitleTextBaseColor;
		private Color mPlaceholderBaseColor;
		private Color mIconBaseColor;

		private bool mIsSelected;
		private bool mIsInit;

		#endregion

		#region UnityMethods

		/// <summary>
		/// I use the start and not the awake because of the Text.text not set fast enough.
		/// </summary>
		public void Start()
		{
			Init();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			Init();
			if (mIsSelected)
			{
				return;
			}

			if (TitleText)
			{
				TitleText.color = HighlighterColor;
			}
			Placeholder.color = HighlighterColor;
			if (Icon)
			{
				Icon.color = HighlighterColor;
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			Init();
			if (mIsSelected)
			{
				return;
			}

			if (TitleText)
			{
				TitleText.color = mTitleTextBaseColor;
			}
			Placeholder.color = mPlaceholderBaseColor;
			if (Icon)
				Icon.color = mIconBaseColor;
		}

		public void OnSelect(BaseEventData eventData)
		{
			Init();
			TitleText?.gameObject.SetActiveSafe(true);

			mIsSelected = true;

			Outline.sprite = SelectedOutline;
			Outline.color = HighlighterColor;
			if (TitleText)
			{
				TitleText.color = HighlighterColor;
			}
			Placeholder.text = string.Empty;
			if (Icon)
			{
				Icon.color = HighlighterColor;
			}
		}

		public void OnDeselect(BaseEventData eventData)
		{
			Init();
			TitleText?.gameObject.SetActiveSafe(Text.text.Length > 1); // I check this because of a weird char that get at the end of the string

			mIsSelected = false;

			Outline.sprite = mOutlineBaseSprite;
			Outline.color = mOutlineBaseColor;
			if (TitleText)
			{
				TitleText.color = mTitleTextBaseColor;
			}

			Placeholder.text = PlaceholderKey.Translate();
			Placeholder.color = mPlaceholderBaseColor;
			if (Icon)
			{
				Icon.color = mIconBaseColor;
			}
		}

		#endregion

		#region PublicMethods
	

		public void ClearText()
		{
			InputField.text = string.Empty;
			InputField.DeactivateInputField();
			TitleText?.gameObject.SetActiveSafe(false);
		}

		#endregion

		#region PrivateMethods

		private void Init()
		{
			if (mIsInit)
			{
				return;
			}

			TitleText?.gameObject.SetActiveSafe(Text.text.Length > 1); // I check this because of a weird char that get at the end of the string
			Placeholder.text = PlaceholderKey.Translate();

			mOutlineBaseSprite = Outline.sprite;
			mOutlineBaseColor = Outline.color;
			if (TitleText)
			{
				mTitleTextBaseColor = TitleText.color;
			}
			mPlaceholderBaseColor = Placeholder.color;
			if (Icon)
			{
				mIconBaseColor = Icon.color;
			}
			mIsInit = true;
		}

		#endregion

		
	}
}
