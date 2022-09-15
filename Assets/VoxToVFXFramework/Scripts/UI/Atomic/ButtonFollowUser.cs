using System;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.UI.Atomic;

[RequireComponent(typeof(Button))]
public class ButtonFollowUser : TransparentButton
{
	#region ScriptParameters

	[SerializeField] private Image Spinner;

	#endregion

	#region Fields

	private bool mIsFollowing;
	private string mTargetUser;
	private Action mOnRefreshCallback;

	#endregion

	#region UnityMethods

	protected override void Awake()
	{
		base.Awake();
		Spinner.gameObject.SetActive(false);
		mButton.onClick.AddListener(OnButtonClicked);
		FrameImage.color = FrameColorDisable;
	}

	#endregion

	#region PublicMethods

	public void Initialize(bool isFollowing, string targetUser, Action onRefreshCallback)
	{
		mIsFollowing = isFollowing;
		mTargetUser = targetUser;
		mOnRefreshCallback = onRefreshCallback;
		Refresh();
	}

	#endregion

	#region PrivateMethods

	private void Refresh()
	{
		if (mIsFollowing)
		{
			ButtonText.text = LocalizationKeys.FOLLOWING.Translate();
			ImageBackgroundActive = true;
		}
		else
		{
			ButtonText.text = LocalizationKeys.FOLLOW.Translate();
			ImageBackgroundActive = false;
		}
	}

	private async void OnButtonClicked()
	{
		SetLockedState(true);

		if (!mIsFollowing)
		{
			await DataManager.Instance.FollowUser(mTargetUser);
			mIsFollowing = true;
		}
		else
		{
			await DataManager.Instance.UnFollowUser(mTargetUser);
			mIsFollowing = false;
		}

		Refresh();
		SetLockedState(false);
		mOnRefreshCallback?.Invoke();
	}

	private void SetLockedState(bool locked)
	{
		Spinner.gameObject.SetActive(locked);
		mButton.interactable = !locked;
	}

	#endregion
}
