using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Visual.UI;
using Com.TheFallenGames.OSA.Core;

namespace Com.TheFallenGames.OSA.Util
{
	[RequireComponent(typeof(Scrollbar))]
	public class ScrollbarRotateOnPull : MonoBehaviour
	{
		[SerializeField]
		float _DegreesOfFreedom = 5f;
		[SerializeField]
		float _RotationSensivity = .5f;

		IOSA _Adapter;
		RectTransform _HandleRT;
		Vector2 _SrollbarPivotOnInit;


		void Start()
		{
			_Adapter = GetComponentInParent<IOSA>();
			_HandleRT = transform as RectTransform;
			//_HandleRT = _Scrollbar.handleRect;
			_SrollbarPivotOnInit = _HandleRT.pivot;
		}


        void Update()
        {
			if (_Adapter == null)
				return;

			float pullAmount01 = 0f;
			var piv = _SrollbarPivotOnInit;
			int sign = 1;
			if (_Adapter.GetContentSizeToViewportRatio() > 1d)
			{
				var insetStart = _Adapter.ContentVirtualInsetFromViewportStart;
				if (insetStart > 0d)
				{
					if (_Adapter.IsHorizontal)
					{
						pullAmount01 = (float)(insetStart / _Adapter.BaseParameters.Viewport.rect.width);
						piv.x = 0f;
					}
					else
					{
						pullAmount01 = (float)(insetStart / _Adapter.BaseParameters.Viewport.rect.height);
						piv.y = 1f;
					}
				}
				else
				{
					var insetEnd = _Adapter.ContentVirtualInsetFromViewportEnd;
					if (insetEnd > 0d)
					{
						sign = -1;
						pullAmount01 = (float)(insetEnd / _Adapter.GetContentSize());
						if (_Adapter.IsHorizontal)
						{
							pullAmount01 = (float)(insetEnd / _Adapter.BaseParameters.Viewport.rect.width);
							piv.x = 1f;
						}
						else
						{
							pullAmount01 = (float)(insetEnd / _Adapter.BaseParameters.Viewport.rect.height);
							piv.y = 0f;
						}
					}
				}
			}
			if (_HandleRT.pivot != piv)
				_HandleRT.pivot = piv;

			var euler = _HandleRT.localEulerAngles;
			// Multiplying argument by _Speed to speed up sine function growth
			euler.z = Mathf.Sin(pullAmount01 * _RotationSensivity * Mathf.PI) * _DegreesOfFreedom * sign;
			_HandleRT.localEulerAngles = euler;
        }
    }
}