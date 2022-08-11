using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using frame8.Logic.Misc.Other;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI;

namespace Com.TheFallenGames.OSA.Core
{
	/// <summary>Holds basic info, like padding, content size etc, abstractized from the scrolling direction</summary>
	public class LayoutInfo
	{
		public readonly Vector2 constantAnchorPosForAllItems = new Vector2(0f, 1f); // top-left

		public Vector2 scrollViewSize { get; private set; }

		/// <summary>Assuming vertical scrolling direction, this would be the viewport's Height. Else, the Width</summary>
		public double vpSize { get; private set; }

		public double transversalContentSize { get; private set; }

		/// <summary>Assuming vertical scrolling direction, this would be the Top padding. Else, the Left padding</summary>
		public double paddingContentStart { get; private set; }

		/// <summary>Assuming vertical scrolling direction, this would be the Left padding. Else, the Top padding</summary>
		public double transversalPaddingContentStart { get; private set; }

		public double transversalPaddingContentEnd { get; private set; }
		public double paddingContentEnd { get; private set; } // bottom/right
		public double paddingStartPlusEnd { get; private set; }
		public double transversalPaddingStartPlusEnd { get; private set; }
		public double spacing { get; private set; }

		/// <summary>Assuming vertical scrolling direction, this would be the Width of all items. Else, the Height</summary>
		public double itemsConstantTransversalSize { get; private set; }

		/// <summary>Assuming vertical scrolling direction, this would be the Top edge. Else, the Left edge</summary>
		public RectTransform.Edge startEdge { get; private set; }

		/// <summary>Assuming vertical scrolling direction, this would be the Bottom edge. Else, the Right edge</summary>
		public RectTransform.Edge endEdge { get; private set; }

		/// <summary>Transversal starting edge. Assuming vertical scrolling direction, this would be the Left edge. Else, the Top edge</summary>
		public RectTransform.Edge transvStartEdge { get; private set; }

		/// <summary>0, if it's a horizontal ScrollView. 1, else</summary>
		public int hor0_vert1 { get; private set; }

		/// <summary>1, if it's a horizontal ScrollView. -1, else</summary>
		public int hor1_vertMinus1 { get; private set; }


		internal void CacheScrollViewInfo(BaseParams parameters)
		{
			scrollViewSize = parameters.ScrollViewRT.rect.size;

			RectTransform vpRT = parameters.Viewport;
			Rect vpRect = vpRT.rect;
			Rect ctRect = parameters.Content.rect;

			if (parameters.IsHorizontal)
			{
				startEdge = RectTransform.Edge.Left;
				endEdge = RectTransform.Edge.Right;
				transvStartEdge = RectTransform.Edge.Top;

				hor0_vert1 = 0;
				hor1_vertMinus1 = 1;
				vpSize = vpRect.width;
				paddingContentStart = parameters.ContentPadding.left;
				paddingContentEnd = parameters.ContentPadding.right;
				transversalPaddingContentStart = parameters.ContentPadding.top;
				transversalPaddingContentEnd = parameters.ContentPadding.bottom;
			}
			else
			{
				startEdge = RectTransform.Edge.Top;
				endEdge = RectTransform.Edge.Bottom;
				transvStartEdge = RectTransform.Edge.Left;

				hor0_vert1 = 1;
				hor1_vertMinus1 = -1;
				vpSize = vpRect.height;
				paddingContentStart = parameters.ContentPadding.top;
				paddingContentEnd = parameters.ContentPadding.bottom;
				transversalPaddingContentStart = parameters.ContentPadding.left;
				transversalPaddingContentEnd = parameters.ContentPadding.right;
			}

			transversalContentSize = ctRect.size[1 - hor0_vert1];

			if (transversalPaddingContentStart == -1d || transversalPaddingContentEnd == -1d)
			{
				if (parameters.ItemTransversalSize == 0f)
					throw new OSAException(
						"ItemTransversalSize is 0, meaning the item should fill the available space transversally, " +
						"but the transversal padding is not specified (it's -1, which is a reserved value)"
					);

				itemsConstantTransversalSize =
				transversalPaddingStartPlusEnd =
				transversalPaddingContentStart =
				transversalPaddingContentEnd = -1d;
			}
			else
			{
				transversalPaddingStartPlusEnd = transversalPaddingContentStart + transversalPaddingContentEnd;
				itemsConstantTransversalSize = transversalContentSize - transversalPaddingStartPlusEnd;
			}

			spacing = parameters.ContentSpacing;

			// There's no concept of content start/end padding when looping. instead, the spacing amount is appended before+after the first+last item
			if (parameters.effects.LoopItems && (paddingContentStart != spacing || paddingContentEnd != spacing))
				throw new OSAException(
					"When looping is active, the content padding should be the same as content spacing. " +
					"This is handled automatically in Params.InitIfNeeded(). " +
					"If you overrode method, please call base's implementation first"
				);

			paddingStartPlusEnd = paddingContentStart + paddingContentEnd;
		}
	}
}
