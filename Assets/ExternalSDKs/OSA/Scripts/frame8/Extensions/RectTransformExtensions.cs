using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.Logic.Misc.Other.Extensions
{
    public static class RectTransformExtensions
	{
		static Func<RectTransform, RectTransform, float>[] _GetInsetFromParentEdge_MappedActions =
			new Func<RectTransform, RectTransform, float>[]
		{
			GetInsetFromParentLeftEdge,
			GetInsetFromParentRightEdge,
			GetInsetFromParentTopEdge,
			GetInsetFromParentBottomEdge,
		};

		//static Action<RectTransform, float, float>[] _SetInsetAndSizeFromParentEdgeWithCurrentAnchors_MappedActions =
		//	new Action<RectTransform, float, float>[]
		//{
		//	SetInsetAndSizeFromParentLeftEdgeWithCurrentAnchors,
		//	SetInsetAndSizeFromParentRightEdgeWithCurrentAnchors,
		//	SetInsetAndSizeFromParentTopEdgeWithCurrentAnchors,
		//	SetInsetAndSizeFromParentBottomEdgeWithCurrentAnchors,
		//};

		public static bool IsLocalPointInRect(this RectTransform rt, Vector2 localPoint)
		{
			localPoint = rt.ConvertLocalPointToPointNormalizedBySize(localPoint);
			return localPoint.HasComponentsWithin01();
		}

		/// <summary>Only tested with overlay canvases. It assumes it haves a parent</summary>
		public static Vector2 GetBottomLeftCornerDisplacementFromParentBottomLeftCorner(this RectTransform rt)
		{
			Rect rect = rt.rect;
			Rect parentRect = (rt.parent as RectTransform).rect;
			Vector3 locPos = rt.localPosition;
			return new Vector2(rect.x - parentRect.x + locPos.x, rect.y - parentRect.y + locPos.y);
		}

		/// <summary>
		/// Takes local point in <paramref name="rt"/>
		/// And returns a vector which has:
		/// x: 0=left, 1=right
		/// y: 0=bottom, 1=top
		/// </summary>
		public static Vector2 ConvertLocalPointToPointNormalizedBySize(this RectTransform rt, Vector2 localPoint)
		{
			var size = rt.rect.size;

			var vBottomLeftToPivotLocal = rt.pivot;
			vBottomLeftToPivotLocal.x *= size.x;
			vBottomLeftToPivotLocal.y *= size.y;
			var vBottomLeftToPositionLocal = vBottomLeftToPivotLocal + localPoint;

			// Transforming to normalized position ([0,0] bottom-left; [1,1] top-right)
			vBottomLeftToPositionLocal.x /= size.x;
			vBottomLeftToPositionLocal.y /= size.y;

			return vBottomLeftToPositionLocal;
		}

		/// <summary>
		/// Takes a vector which has:
		/// x: 0=left, 1=right
		/// y: 0=bottom, 1=top
		/// And returns it as local point in <paramref name="rt"/>
		/// </summary>
		public static Vector2 ConvertPointNormalizedBySizeToLocalPoint(this RectTransform rt, Vector2 normalizedBySize01)
		{
			var size = rt.rect.size;

			var vBottomLeftToPoint = size;
			vBottomLeftToPoint.x *= normalizedBySize01.x;
			vBottomLeftToPoint.y *= normalizedBySize01.y;

			var vBottomLeftToPivotLocal = rt.pivot;
			vBottomLeftToPivotLocal.x *= size.x;
			vBottomLeftToPivotLocal.y *= size.y;
			var localPoint = -vBottomLeftToPivotLocal + vBottomLeftToPoint;

			return localPoint;
		}

		
//#warning TODO uncomment when these work in all canvas render modes
//		public static float GetWorldTop(
//			this RectTransform rt,
//			Canvas canvas,
//			RectTransform canvasRectTransform
//		)
//		{ return rt.position.y + (1f - rt.pivot.y) * rt.rect.height; }

//        public static float GetWorldBottom(
//			this RectTransform rt,
//			Canvas canvas,
//			RectTransform canvasRectTransform
//		)
//		{ return rt.position.y - rt.pivot.y * rt.rect.height; }

//        public static float GetWorldLeft(
//			this RectTransform rt,
//			Canvas canvas,
//			RectTransform canvasRectTransform
//		)
//		{ return rt.position.x - rt.pivot.x * rt.rect.width; }

//        public static float GetWorldRight(
//			this RectTransform rt,
//			Canvas canvas,
//			RectTransform canvasRectTransform
//		)
//		{ return rt.position.x + (1f - rt.pivot.x) * rt.rect.width; }

		public static float GetInsetFromParentLeftEdge(this RectTransform child, RectTransform parentHint)
		{
			var piv = parentHint.pivot;
			var rect = parentHint.rect;
			return child.GetInsetFromParentLeftEdge(ref piv, ref rect);
		}
        public static float GetInsetFromParentLeftEdge(this RectTransform child, ref Vector2 parentPivot, ref Rect parentRect)
        {
            float parentPivotXDistToParentLeft = parentPivot.x * parentRect.width;
            float childLocPosX = child.localPosition.x;

            return parentPivotXDistToParentLeft + child.rect.xMin + childLocPosX;
		}
		public static float GetInsetFromParentRightEdge(this RectTransform child, RectTransform parentHint)
		{
			var piv = parentHint.pivot;
			var rect = parentHint.rect;
			return child.GetInsetFromParentRightEdge(ref piv, ref rect);
		}
		public static float GetInsetFromParentRightEdge(this RectTransform child, ref Vector2 parentPivot, ref Rect parentRect)
        {
            float parentPivotXDistToParentRight = (1f - parentPivot.x) * parentRect.width;
            float childLocPosX = child.localPosition.x;

            return parentPivotXDistToParentRight - child.rect.xMax - childLocPosX;
		}
		public static float GetInsetFromParentTopEdge(this RectTransform child, RectTransform parentHint)
		{
			var piv = parentHint.pivot;
			var rect = parentHint.rect;
			return child.GetInsetFromParentTopEdge(ref piv, ref rect);
		}
		public static float GetInsetFromParentTopEdge(this RectTransform child, ref Vector2 parentPivot, ref Rect parentRect)
		{
			float parentPivotYDistToParentTop = (1f - parentPivot.y) * parentRect.height;
			float childLocPosY = child.localPosition.y;

			return parentPivotYDistToParentTop - child.rect.yMax - childLocPosY;
		}
		public static float GetInsetFromParentBottomEdge(this RectTransform child, RectTransform parentHint)
		{
			var piv = parentHint.pivot;
			var rect = parentHint.rect;
			return child.GetInsetFromParentBottomEdge(ref piv, ref rect);
		}
		public static float GetInsetFromParentBottomEdge(this RectTransform child, ref Vector2 parentPivot, ref Rect parentRect)
        {
            float parentPivotYDistToParentBottom = parentPivot.y * parentRect.height;
            float childLocPosY = child.localPosition.y;

            return parentPivotYDistToParentBottom + child.rect.yMin + childLocPosY;
        }

		/// <summary>
		/// It assumes the transform has a parent
		/// </summary>
		/// <param name="child"></param>
		/// <param name="parentHint"> the parent of child. used in order to prevent casting, in case the caller already has the parent stored in a variable</param>
		/// <returns></returns>
		public static float GetInsetFromParentEdge(this RectTransform child, RectTransform parentHint, RectTransform.Edge parentEdge)
		{ return _GetInsetFromParentEdge_MappedActions[(int)parentEdge](child, parentHint); }

		public static void SetInsetAndSizeFromParentLeftEdgeWithCurrentAnchors(this RectTransform child, float newInset, float newSize)
		{ child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge.Left, newInset, newSize); }
		public static void SetInsetAndSizeFromParentRightEdgeWithCurrentAnchors(this RectTransform child, float newInset, float newSize)
		{ child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge.Right, newInset, newSize); }
		public static void SetInsetAndSizeFromParentTopEdgeWithCurrentAnchors(this RectTransform child, float newInset, float newSize)
		{ child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge.Top, newInset, newSize); }
		public static void SetInsetAndSizeFromParentBottomEdgeWithCurrentAnchors(this RectTransform child, float newInset, float newSize)
		{ child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge.Bottom, newInset, newSize); }

		static void AfterSetInsetAndSizeFromParentEdgeWithCurrentAnchors_RestoreAnchorsIfNeeded(
			this RectTransform child, 
			int axisIndex,
			Vector2 origSizeDelta, // no ref, bc we need to re-use it
			ref Vector2 origAnchorMin,
			ref Vector2 origAnchorMax, 
			float sizeChange
		)
		{
			// Since the anchors may change, we need to restore them, but keeping the local pos and size
			Vector3 localPos = child.localPosition;
			bool restoreNeeded = false;
			if (child.anchorMin != origAnchorMin)
			{
				child.anchorMin = origAnchorMin;
				restoreNeeded = true;
			}
			if (child.anchorMax != origAnchorMax)
			{
				child.anchorMax = origAnchorMax;
				restoreNeeded = true;
			}
			if (restoreNeeded)
			{
				child.localPosition = localPos;
				origSizeDelta[axisIndex] += sizeChange;
				child.sizeDelta = origSizeDelta;
			}
		}

		/// <summary> Optimized version of SetSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge fixedEdge, float newSize) when parent is known </summary>
		/// <param name="parentCached"></param>
		/// <param name="fixedEdge"></param>
		/// <param name="newSize"></param>
		public static void SetSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform parentCached, RectTransform.Edge fixedEdge, float newSize)
        {
            child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(fixedEdge, child.GetInsetFromParentEdge(parentCached, fixedEdge), newSize);
        }

        ///// <summary> NOTE: Use the optimized version if parent is known </summary>
        ///// <param name="fixedEdge"></param>
        ///// <param name="newInset"></param>
        ///// <param name="newSize"></param>
        //static void SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform.Edge fixedEdge, float newInset, float newSize)
        //{
        //    child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(child.parent as RectTransform, fixedEdge, newInset, newSize);
        //}

		/// <summary> Same as RectTransform's built-in method, but this preserves the anchors</summary>
		public static void SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform.Edge fixedEdge, float newInset, float newSize)
        {
			// Commented: as of 27.07.2018, a better way was found, which luckily also removes the need of mapped actions + bonus2: no need for parent anymore.
			// The subtle problem with the old approach is that it didn't work for very large distances, like when changing
			// an item's inset/size while it was way out of the parent's rectangle

			//_SetInsetAndSizeFromParentEdgeWithCurrentAnchors_MappedActions[(int)fixedEdge](child, parentHint, newInset, newSize);

			int axisIndex = (int)fixedEdge / 2;
			Vector2 origAnchorMin = child.anchorMin, origAnchorMax = child.anchorMax;
			Vector2 origSizeDelta = child.sizeDelta;
			float sizeChange = newSize - child.rect.size[axisIndex];

			child.SetInsetAndSizeFromParentEdge(fixedEdge, newInset, newSize);
			child.AfterSetInsetAndSizeFromParentEdgeWithCurrentAnchors_RestoreAnchorsIfNeeded(axisIndex, origSizeDelta, ref origAnchorMin, ref origAnchorMax, sizeChange);
		}


		public static void MatchParentSize(this RectTransform rt, bool preservePivot)
		{
			//var aMin = rt.anchorMin;
			//var aMax = rt.anchorMax;
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.sizeDelta = Vector3.zero; // same size as anchors
			var piv = rt.pivot;
			rt.pivot = Vector2.one * .5f; // center pivot
			rt.anchoredPosition = Vector3.zero; // centered at the anchors' center

			if (preservePivot)
				rt.pivot = piv;
		}

		public static void TryClampPositionToParentBoundary(this RectTransform rt)
		{
			Transform tr = rt;
			Canvas rootCanvas = null;
			while (tr = tr.parent)
			{
				var c = tr.GetComponent<Canvas>();
				if (c)
					rootCanvas = c;
			}

			if (!rootCanvas)
				return;

			var rootCanvasRT = rootCanvas.transform as RectTransform;
			Vector3[] canvasCorners = new Vector3[4];
			Vector3[] myCorners = new Vector3[4];
			rootCanvasRT.GetWorldCorners(canvasCorners);
			rt.GetWorldCorners(myCorners);

			if (myCorners[0].y <= canvasCorners[0].y)
			{
				// overlapping with bottom
				rt.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge.Bottom, 20f, rt.rect.size.y);
			}
			else if (myCorners[1].y >= canvasCorners[1].y)
			{
				// overlapping with top
				rt.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge.Top, 20f, rt.rect.size.y);
			}
			else
			{

			}
		}
	}
}
