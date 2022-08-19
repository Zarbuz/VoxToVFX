using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.UI.Popups.Descriptor;
using VoxToVFXFramework.Scripts.UI.Popups.Popup;

public class EditCollectionPopup : PopupWithAlpha<EditCollectionDescriptor>
{
	#region ScriptParameters

	[SerializeField] private SelectImage SelectLogoImage;
	[SerializeField] private SelectImage SelectCoverImage;
	[SerializeField] private TMP_InputField DescriptionInputField;
	[SerializeField] private Button OkButton;
	[SerializeField] private Button CancelButton;

	#endregion

	#region Fields

	private Action<CollectionDetails> mOnConfirmCallback;
	private Action mOnCancelCallback;

	#endregion

	#region PublicMethods

	public override void Init(EditCollectionDescriptor descriptor)
	{
		base.Init(descriptor);
		SelectCoverImage.Initialize(descriptor.CoverImageUrl);
		SelectLogoImage.Initialize(descriptor.LogoImageUrl);
		DescriptionInputField.text = descriptor.Description;

		mOnConfirmCallback = descriptor.OnConfirmAction;
		mOnCancelCallback = descriptor.OnCancelAction;

		OkButton.onClick.RemoveAllListeners();
		OkButton.onClick.AddListener(OnConfirmClicked);

		CancelButton.onClick.RemoveAllListeners();
		CancelButton.onClick.AddListener(OnCancelClicked);
	}

	#endregion

	#region PrivateMethods

	private void OnConfirmClicked()
	{
		CollectionDetails collectionDetails = new CollectionDetails
		{
			Description = DescriptionInputField.text,
			CoverImageUrl = SelectCoverImage.ImageUrl,
			LogoImageUrl = SelectLogoImage.ImageUrl,
		};

		mOnConfirmCallback?.Invoke(collectionDetails);
		Hide();
	}

	private void OnCancelClicked()
	{
		mOnCancelCallback?.Invoke();
		Hide();
	}
	
	#endregion
}
