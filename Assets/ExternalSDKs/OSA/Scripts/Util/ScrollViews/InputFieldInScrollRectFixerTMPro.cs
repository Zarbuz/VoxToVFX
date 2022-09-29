// Uncomment this if you have TMPro installed and want to use this script
//#define TMPRO_AVAILABLE


using System;
using System.Reflection;

namespace Com.TheFallenGames.OSA.AdditionalComponents
{
	public class InputFieldInScrollRectFixerTMPro : InputFieldInScrollRectFixerBase
	{
		MethodInfo _ActivateInputFieldMI;
		PropertyInfo _isFocusedPI;


		/// <summary>Using reflection so you won't get compile-time errors</summary>
		protected override void CacheMethods()
		{
			var type = _InputField.GetType();
			string reqComp = "TMPro.TMP_InputField";
			if (type.FullName != reqComp)
				throw new InvalidOperationException("This script can only be attached to a GameObject containing a " + reqComp);

			_ActivateInputFieldMI = type.GetMethod("ActivateInputField");
			_isFocusedPI = type.GetProperty("isFocused");
		}

		protected override void ActivateInputField()
		{
			if (_ActivateInputFieldMI != null)
				_ActivateInputFieldMI.Invoke(_InputField, null);
		}

		protected override bool IsInputFieldFocused() { return (bool)_isFocusedPI.GetValue(_InputField, null); }
	}
}
