//#define DEBUG_COMPUTE_VISIBILITY

using frame8.Logic.Misc.Other.Extensions;
using UnityEngine;

namespace Com.TheFallenGames.OSA.Core.SubComponents
{
	internal class ReleaseFromPullManager<TParams, TItemViewsHolder>
		where TParams : BaseParams
		where TItemViewsHolder : BaseItemViewsHolder
	{
		public bool inProgress;
		//public RectTransform.Edge pulledEdge;
		public double targetCTInsetFromVPS;


		OSA<TParams, TItemViewsHolder> _Adapter;
		ComputeVisibilityParams _ComputeVisibilityParams_Reusable = new ComputeVisibilityParams();


		public ReleaseFromPullManager(OSA<TParams, TItemViewsHolder> adapter) { _Adapter = adapter; }


		public double CalculateFirstItemTargetInsetFromVPS() { return targetCTInsetFromVPS + _Adapter._InternalState.paddingContentStart; }

		// Only call it if there ARE visible items
		public double CalculateFirstItemInsetFromVPS()
		{
			var firstVH = _Adapter._VisibleItems[0];
			//float firstItemInsetFromVPS = _VisibleItems[0].root.GetInsetFromParentEdge(Parameters.content, _InternalState.startEdge);
			double firstItemInsetFromVPS = firstVH.root.GetInsetFromParentEdge(_Adapter.Parameters.Content, _Adapter._InternalState.startEdge);
			if (firstVH.itemIndexInView > 0)
				firstItemInsetFromVPS -= _Adapter._InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(firstVH.itemIndexInView) - _Adapter._InternalState.paddingContentStart;

			return firstItemInsetFromVPS;
		}

		// Only call this if in progress
		public void FinishNowByDraggingItems(bool computeVisibility)
		{
			//Debug.Log("FinishNowByDraggingItems: " + inProgress);

			if (!inProgress)
				return;

			var abstrDelta = CalculateFirstItemTargetInsetFromVPS() - CalculateFirstItemInsetFromVPS();
			if (abstrDelta != 0d)
				_Adapter.DragVisibleItemsRangeUnchecked(0, _Adapter.VisibleItemsCount, abstrDelta, true, computeVisibility);

			inProgress = false;
		}

		public void FinishNowBySettingContentInset(bool computeVisibility)
		{
			//Debug.Log("FinishNowBySettingContentInset: " + targetCTInsetFromVPS + ", " + _Adapter._InternalState.ctVirtualInsetFromVPS_Cached + ", " + computeVisibility);

			// Don't let it infer the delta, since we already know its value
			_ComputeVisibilityParams_Reusable.overrideDelta = targetCTInsetFromVPS - _Adapter._InternalState.ctVirtualInsetFromVPS_Cached;
			var contentPosChangeParams = new ContentSizeOrPositionChangeParams
			{
				cancelSnappingIfAny = true,
				computeVisibilityParams = computeVisibility ? _ComputeVisibilityParams_Reusable : null,
				fireScrollPositionChangedEvent = true,
				keepVelocity = true,
			};

			bool _;
			_Adapter.SetContentVirtualInsetFromViewportStart(targetCTInsetFromVPS, ref contentPosChangeParams, out _);

			inProgress = false;
		}
	}
}
