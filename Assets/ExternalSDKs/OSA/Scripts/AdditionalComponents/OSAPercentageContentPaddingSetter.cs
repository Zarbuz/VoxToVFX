using System.Collections;
using UnityEngine;
using Com.TheFallenGames.OSA.Core;
using UnityEngine.UI;

namespace Com.TheFallenGames.OSA.AdditionalComponents
{
	/// <summary>
	/// Use this when the content padding should be a function of the viewport size, rather than a constant decided at edit-time. 
	/// In other words, use if you want to specify the padding as a percentage rather than in pixels. It also allows for fine adjustments of the first/last item, mostly useful for centering them.
	/// A use case is keeping the last/first element in the middle when the content's extremity is reached. This can be done by setting a constant padding, 
	/// but having a percentage-specified padding allows for seamless screen size changes
	/// </summary>
	public class OSAPercentageContentPaddingSetter : MonoBehaviour
	{
		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("0 = none, .5f = half of the viewport, 1f = the entire viewport's size will be used for padding")]
		float _PaddingStartPercent = .5f;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("Same rules as for PaddingStartPercent")]
		float _PaddingEndPercent = .5f;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("After setting the padding, how much will this item approach the viewport's edge based on its size?. 0=none, i.e. full padding. 1=a distance equal to its size. \n" +
			"For example, a 0.5 value could be used along with PaddingStartPercent and PaddingEndPercent also set to 0.5, resulting in first/last items arriving exactly in the middle when you scroll in the extremities.\n" +
			"ItemSizeSource must also be set for this to be accurate. Otherwise, OSA.Parameters.DefaultItemSize will be used")]
		float _FirstLastItemsInsidePercent = .5f;

		[SerializeField]
		[Tooltip("This object's width or height will be used to calculate the most accurate position to satisfy the FirstLastItemsInsidePercent property")]
		RectTransform _ItemSizeCustomSource = null;

		IOSA _IOSA;
		bool _UseDefaultItemSize;
		bool _IsHorizontal;
		//float _LastItemSize = float.MinValue * 1.1f;
		//float _LastVPSize = float.MinValue * 1.1f;


		#region Unity
		void Awake()
		{
			enabled = false;
			_IOSA = GetComponent(typeof(IOSA)) as IOSA;
			if (_IOSA == null)
			{
				Debug.Log(typeof(OSAPercentageContentPaddingSetter).Name + " needs to be attached to a game object containing an OSA component");
				return;
			}

			if (_IOSA.IsInitialized)
			{
				Debug.Log(typeof(OSAPercentageContentPaddingSetter).Name + " needs the OSA component to not be initialized before it");
				return;
			}

			var parameters = _IOSA.BaseParameters;
			_UseDefaultItemSize = _ItemSizeCustomSource == null;
			_IsHorizontal = _IOSA.BaseParameters.IsHorizontal;

			_IOSA.ScrollViewSizeChanged += OnScrollViewSizeChanged;
			//_IOSA.ItemsRefreshed += OnItemsRefreshed;

			parameters.PrepareForInit(true);
			parameters.InitIfNeeded(_IOSA);

			UpdatePadding();
		}
		#endregion

		/// <summary>Each time the ScrollView's size changes, the padding needs to be recalculated. <see cref="OSA{TParams, TItemViewsHolder}.ScrollViewSizeChanged"/></summary>
		void OnScrollViewSizeChanged()
		{
			if (!_IOSA.IsInitialized)
			{
				Debug.LogError(typeof(OSAPercentageContentPaddingSetter).Name + ".OnScrollViewSizeChanged() called, but OSA not initialized. This shouldn't happen if implemented correctly");
				return;
			}

			_IOSA.BaseParameters.PrepareForInit(false);
			_IOSA.BaseParameters.InitIfNeeded(_IOSA);

			UpdatePadding();
		}

		//void OnItemsRefreshed(int _, int __)
		//{
		//	float curSize;
		//	if (_UseDefaultItemSize)
		//		curSize = _IOSA.BaseParameters.DefaultItemSize;
		//	else
		//		curSize = GetSourceItemSize();

		//	if (Mathf.Abs(curSize - _LastItemSize) < .01f)
		//		return;

		//	float curVPSize = GetVPSize();
		//	if (Mathf.Abs(curVPSize - _LastVPSize) < .01f)
		//		return;

		//	SetPaddingWith(curVPSize, curSize);

		//	_IOSA.asdasdas
		//	_IOSA.Refresh(false, true);
		//}

		void UpdatePadding()
		{
			if (_UseDefaultItemSize)
				SetPaddingFromDefaultItemSize();
			else
				SetPaddingFromCustomItemSource();
		}

		void SetPaddingFromCustomItemSource()
		{
			SetPaddingWith(GetVPSize(), GetSourceItemSize());
		}

		void SetPaddingFromDefaultItemSize()
		{
			SetPaddingWith(GetVPSize(), _IOSA.BaseParameters.DefaultItemSize);
		}

		void SetPaddingWith(float vpSize, float itemSizeToUse)
		{
			var parameters = _IOSA.BaseParameters;
			float firstLastItemInsideAmount = itemSizeToUse * _FirstLastItemsInsidePercent;
			var pad = parameters.ContentPadding;
			int padStart = (int)(vpSize * _PaddingStartPercent - firstLastItemInsideAmount + .5f);
			int padEnd = (int)(vpSize * _PaddingEndPercent - firstLastItemInsideAmount + .5f);
			if (parameters.IsHorizontal)
			{
				pad.left = padStart;
				pad.right = padEnd;
			}
			else
			{
				pad.top = padStart;
				pad.bottom = padEnd;
			}

			//_LastItemSize = itemSizeToUse;
			//_LastVPSize = vpSize;
		}

		float GetSourceItemSize() { var itemRect = _ItemSizeCustomSource.rect; return _IsHorizontal ? itemRect.width : itemRect.height; }
		float GetVPSize() { var vpRect = _IOSA.BaseParameters.Viewport.rect; return _IsHorizontal ? vpRect.width : vpRect.height; }
	}
}
