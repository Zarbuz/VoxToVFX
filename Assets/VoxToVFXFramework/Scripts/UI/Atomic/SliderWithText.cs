using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderWithText : MonoBehaviour
{
	#region ScriptParameters

	[SerializeField] private TextMeshProUGUI Text;

	#endregion

	#region Fields

	private Slider mSlider;

	#endregion

	#region UnityMethods

	private void Awake()
	{
		mSlider = GetComponent<Slider>();
		mSlider.onValueChanged.AddListener(OnSliderValueChanged);
		OnSliderValueChanged(mSlider.value);
	}


	private void OnDestroy()
	{
		mSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
	}

	#endregion

	#region PrivateMethods

	private void OnSliderValueChanged(float value)
	{
		if (mSlider.wholeNumbers)
		{
			int val = (int)value;
			Text.text = val.ToString();
		}
		else
		{
			Text.text = value.ToString(CultureInfo.InvariantCulture);
		}

	}

	#endregion
}
