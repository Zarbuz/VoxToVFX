using System;
using UnityEngine;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Button))]
	public class SwitchController : MonoBehaviour
	{
		#region Fields

		public bool IsOn;

		public event Action<bool> OnValueChanged; 

		private Animator mSwitchAnimator;
		private Button mSwitchButton;

		#endregion

		#region UnityMethods

		private void Start()
		{
			mSwitchAnimator = GetComponent<Animator>();
			mSwitchButton = GetComponent<Button>();
			mSwitchButton.onClick.AddListener(OnSwitchClicked);
		}

		#endregion

		#region PrivateMethods

		private void OnSwitchClicked()
		{
			if (IsOn == true)
			{
				mSwitchAnimator.Play("Switch Off");
				IsOn = false;
			}

			else
			{
				mSwitchAnimator.Play("Switch On");
				IsOn = true;
			}

			OnValueChanged?.Invoke(IsOn);
		}

		#endregion


	}
}
