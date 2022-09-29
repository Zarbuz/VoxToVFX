//#define DEBUG_COMPUTE_VISIBILITY

using System;
using UnityEngine;

namespace Com.TheFallenGames.OSA.Core.SubComponents
{
	internal class ComputeVisibilityManager<TParams, TItemViewsHolder>
		where TParams : BaseParams
		where TItemViewsHolder : BaseItemViewsHolder
	{
		OSA<TParams, TItemViewsHolder> _Adapter;
		TParams _Params;
		InternalState<TItemViewsHolder> _InternalState;
		ItemsDescriptor _ItemsDesc;

		#region Per-ComputeVisibility call params
		string debugString;
		bool negativeScroll;
		double vpSize;
		RectTransform.Edge negStartEdge_posEndEdge;
		TItemViewsHolder nlvHolder = null;
		int endItemIndexInView,
				neg1_posMinus1,
				neg1_pos0,
				neg0_pos1;
		double ctVrtInsetFromVPS;
		int itemsCount;
		//int estimatedAVGVisibleItems;
		bool thereWereVisibletems;
		//int currentLVItemIndexInView;
		double currentItemRealInset_negStart_posEnd;
		bool negNLVCandidateIsBeforeVP_posNLVCandidateIsAfterVP;
		// The item that was the last in the _VisibleItems (first, if pos scroll); We're inferring the positions of the other ones after(below/to the right, depending on hor/vert scroll) it this way, since the heights(widths for hor scroll) are known
		TItemViewsHolder startingLVHolder = null;
		double negCTVrtInsetFromVPS_posCTVrtInsetFromVPE;
		int nlvIndexInView;
		double nlvSize, nlvSizePlusSpacing;
		double currentRealInsetToUseForNLV_negFromCTS_posFromCTE;
		#endregion


		public ComputeVisibilityManager(OSA<TParams, TItemViewsHolder> adapter)
		{
			_Adapter = adapter;
			_Params = _Adapter.Parameters;
			_InternalState = _Adapter._InternalState;
			_ItemsDesc = _Adapter._ItemsDesc;
		}


		/// <summary>The very core of <see cref="OSA{TParams, TItemViewsHolder}"/>. You must be really brave if you think about trying to understand it :)</summary>
		public void ComputeVisibility(double abstractDelta)
		{
			//Debugging
			//Debug.Log("ad " + abstractDelta.ToString("######0000.0000") + ", c " + _Adapter.GetItemsCount() + ", vc " + _Adapter.VisibleItemsCount);

			#region visualization & info
			// ALIASES:
			// scroll down = the content goes down(the "view point" goes up); scroll up = analogue
			// the notation "x/y" means "x, if vertical scroll; y, if horizontal scroll"
			// positive scroll = down/right; negative scroll = up/left
			// [start] = usually refers to some point above/to-left-of [end], if negativeScroll; 
			//          else, [start] is below/to-right-of [end]; 
			//          for example: -in context of _VisibleItems, [start] is 0 for negativeScroll and <_VisibleItemsCount-1> for positiveScroll;
			//                       -in context of an item, [start] is its top for negativeScroll and bottom for positiveScroll;
			//                       - BUT for ct and vp, they have fixed meaning, regardless of the scroll sign. they only depend on scroll direction (if vert, start = top, end = bottom; if hor, start = left, end = right)
			// [end] = inferred from definition of [start]
			// LV = last visible (the last item that was closest to the negVPEnd_posVPStart in the prev call of this func - if applicable)
			// NLV = new last visible (the next one closer to the negVPEnd_posVPStart than LV)
			// neg = negative scroll (down or right)
			// pos =positive scroll (up or left)
			// ch = child (i.e. ctChS = content child start(first child) (= ct.top - ctPaddingTop, in case of vertical scroll))

			// So, again, this is the items' start/end notions! Viewport's and Content's start/end are constant throughout the session
			// Assume the following scroll direction (hor) and sign (neg) (where the VIEWPORT+SCROLLBAR goes, opposed to where the CONTENT goes):
			// hor, negative:
			// O---------------->
			//      [vpStart]  [start]item[end] .. [start]item2[end] .. [start]LVItem[end] [vpEnd]
			// hor, positive:
			// <----------------O
			//      [vpStart]  [end]item[start] .. [end]item2[start] .. [end]LVItem[start] [vpEnd]
			#endregion

			debugString = null;
			negativeScroll = abstractDelta <= 0d;

			// Viewport constant values
			vpSize = _InternalState.vpSize;

			// Items variable values

			if (negativeScroll)
			{
				neg1_posMinus1 = 1;
				negStartEdge_posEndEdge = _InternalState.startEdge;
			}
			else
			{
				neg1_posMinus1 = -1;
				negStartEdge_posEndEdge = _InternalState.endEdge;
			}
			neg1_pos0 = (neg1_posMinus1 + 1) / 2;
			neg0_pos1 = 1 - neg1_pos0;

			thereWereVisibletems = _Adapter.VisibleItemsCount > 0;

			itemsCount = _ItemsDesc.itemsCount;

			// _InternalParams.itemsCount - 1, if negativeScroll
			// 0, else
			endItemIndexInView = neg1_pos0 * (itemsCount - 1);

			ctVrtInsetFromVPS = _InternalState.ctVirtualInsetFromVPS_Cached;
			negCTVrtInsetFromVPS_posCTVrtInsetFromVPE = negativeScroll ? ctVrtInsetFromVPS : (-_InternalState.ctVirtualSize + _InternalState.vpSize - ctVrtInsetFromVPS);

#if DEBUG_COMPUTE_VISIBILITY
		debugString += "\n : ctVirtualInsetFromVPS_Cached=" + ctVrtInsetFromVPS + 
			" negCTVrtInsetFromVPS_posCTVrtInsetFromVPE=" + negCTVrtInsetFromVPS_posCTVrtInsetFromVPE +
			", vpSize=" + vpSize;
#endif

			// _VisibleItemsCount is always 0 in the first call of this func after the list is modified.

			// IF _VisibleItemsCount == 0:
			//		-1, if negativeScroll
			//		_InternalParams.itemsCount, else
			// ELSE
			//		indexInView of last visible, if negativeScroll
			//		indexInView of first visible, else
			int currentLVItemIndexInView;

			// Get a list of items that are before(if neg)/after(if pos) viewport and move them from 
			// _VisibleItems to itemsOutsideViewport; they'll be candidates for recycling
			if (thereWereVisibletems)
				PrepareOutsideItemsForPotentialRecycleAndGetNextFirstVisibleIndexInView(out currentLVItemIndexInView);
			else
				//currentLVItemIndexInView = neg0_pos1 * (itemsCount - 1) - neg1_posMinus1;
				currentLVItemIndexInView = neg0_pos1 * (itemsCount - 1);

			if (itemsCount > 0)
			{
				// Optimization: saving a lot of computations (especially visible when fast-scrolling using SmoothScrollTo or dragging the scrollbar) by skipping 
				// GetItemVirtualOffsetFromParent[Start/End]UsingItemIndexInView() calls and instead, inferring the offset along the way after calling that method only for the first item
				// Optimization2: trying to estimate the new FIRST visible item by the current scroll position when jumping large distances
				bool inferredDifferentFirstVisibleVH = false;
				// TODO see if using double instead of int breaks anything, since avg should be double usually
				//int estimatedAVGVisibleItems = -1;
				double estimatedAVGVisibleItems = -1d;
				bool forceInferFirstLVIndexAndInset = !thereWereVisibletems; // always infer if there were no items, because there's not item to use as a base for usual inferring
				if ((forceInferFirstLVIndexAndInset
						|| Math.Abs(abstractDelta) > _InternalState.vpSize * OSAConst.OPTIMIZE_JUMP__MIN_DRAG_AMOUNT_AS_FACTOR_OF_VIEWPORT_SIZE // huge jumps need optimization
					)
					//&& (estimatedAVGVisibleItems = (int)Math.Round(Math.Min(_InternalState.vpSize / (_Params.DefaultItemSize + _InternalState.spacing), _Adapter._AVGVisibleItemsCount)))
					&& (estimatedAVGVisibleItems = Math.Min(_InternalState.vpSize / ((double)_Params.DefaultItemSize + _InternalState.spacing), _Adapter._AVGVisibleItemsCount))
						< itemsCount
				)
				{
					//if (thereWereVisibletems)
					//	Debug.Log(thereWereVisibletems + ", " + _Adapter.VisibleItemsCount + ", " + _Adapter.RecyclableItemsCount);
					inferredDifferentFirstVisibleVH = InferFirstVisibleVHIndexInViewAndInset(ref currentLVItemIndexInView, estimatedAVGVisibleItems);
				}

				// This check is the same as in the loop inside. there won't be no "next" item if the current LV is the last in view (first if positive scroll) 
				//if (currentLVItemIndexInView != endItemIndexInView + neg1_posMinus1)
				if (negativeScroll && currentLVItemIndexInView <= endItemIndexInView || !negativeScroll && currentLVItemIndexInView >= endItemIndexInView)
				{
					// Infinity means it was not set; if set, it means the position was inferred due to big jumps in dragging
					if (!inferredDifferentFirstVisibleVH)
					{
						if (negativeScroll)
							currentItemRealInset_negStart_posEnd = _InternalState.GetItemInferredRealInsetFromParentStart(currentLVItemIndexInView);
						else
							currentItemRealInset_negStart_posEnd = _InternalState.GetItemInferredRealInsetFromParentEnd(currentLVItemIndexInView);

#if DEBUG_COMPUTE_VISIBILITY
					debugString += "\n First: currentItemRealInset_negStart_posEnd=" + currentItemRealInset_negStart_posEnd;
#endif
					}

					// Searching for next item(s) that might get visible in order to update them: towards vpEnd on negativeScroll OR towards vpStart else
					do
					{
						bool b = FindCorrectNLVFromCurrent(currentLVItemIndexInView);
						//if (iterations > 100)
						//	Debug.Log(iterations + ", delta " + abstractDelta + ", found " + b + ", estVisible " + estimatedAVGVisibleItems + ", nlvIndexInView " + nlvIndexInView + ", endItemIndexInView " + endItemIndexInView + ", negScroll " + negativeScroll);
						if (!b)
							break;

						int nlvRealIndex = _ItemsDesc.GetItemRealIndexFromViewIndex(nlvIndexInView);

						// Search for a recyclable holder for current NLV
						// This block remains the same regardless of <negativeScroll> variable, because the items in <itemsOutsideViewport> were already added in an order dependent on <negativeScroll>
						// (they are always from <closest to [start]> to <closest to [end]>)
						nlvHolder = _Adapter.ExtractRecyclableViewsHolderOrCreateNew(nlvRealIndex, nlvSize);

						int vhIndex = neg1_pos0 * _Adapter.VisibleItemsCount;

#if DEBUG_COMPUTE_VISIBILITY
					debugString += "\n InsertVH at #" + vhIndex + " (itemIndex=" + nlvRealIndex + "): nlvSize=" + nlvSize + ", realInset_negStart_posEnd=" + currentItemRealInset_negStart_posEnd;
#endif

						_Adapter.AddViewsHolderAndMakeVisible(nlvHolder, vhIndex, nlvRealIndex, nlvIndexInView, currentItemRealInset_negStart_posEnd, negStartEdge_posEndEdge, nlvSize);

						currentLVItemIndexInView = nlvIndexInView + neg1_posMinus1;
						currentItemRealInset_negStart_posEnd += nlvSizePlusSpacing;
					}
					// Loop until:
					//		- negativeScroll vert/hor: there are no items below/to-the-right-of-the current LV that might need to be made visible
					//		- positive vert/hor: there are no items above/to-the-left-of-the current LV that might need to be made visible
					//while (currentLVItemIndexInView != endItemIndexInView + neg1_posMinus1);
					while (negativeScroll && currentLVItemIndexInView <= endItemIndexInView || !negativeScroll && currentLVItemIndexInView >= endItemIndexInView);

				}
				if (debugString != null)
					Debug.Log("OSA.ComputeVisibility(" + abstractDelta + "): " + debugString);
			}

			// Keep track of the <maximum number of items that were visible since last scroll view size change>, so we can optimize the object pooling process
			// by destroying objects in recycle bin only if the aforementioned number is less than <numVisibleItems + numItemsInRecycleBin>,
			// and of course, making sure at least 1 item is in the bin all the time
			if (_Adapter.VisibleItemsCount > _ItemsDesc.maxVisibleItemsSeenSinceLastScrollViewSizeChange)
				_ItemsDesc.maxVisibleItemsSeenSinceLastScrollViewSizeChange = _Adapter.VisibleItemsCount;

			PostComputeVisibilityCleanRecyclableItems();

			// Last result weighs 9x more than the current result in calculating the AVG to prevent "outliers"
			_Adapter._AVGVisibleItemsCount = _Adapter._AVGVisibleItemsCount * .9d + _Adapter.VisibleItemsCount * .1d;
		}

		//int iterations;
		bool FindCorrectNLVFromCurrent(int currentLVItemIndexInView)
		{
			//iterations = 0;

			nlvIndexInView = currentLVItemIndexInView;
			do
			{
				nlvSize = _ItemsDesc[nlvIndexInView];
				nlvSizePlusSpacing = nlvSize + _InternalState.spacing;
				currentRealInsetToUseForNLV_negFromCTS_posFromCTE = currentItemRealInset_negStart_posEnd;
				negNLVCandidateIsBeforeVP_posNLVCandidateIsAfterVP = currentRealInsetToUseForNLV_negFromCTS_posFromCTE <= -nlvSize;
				if (negNLVCandidateIsBeforeVP_posNLVCandidateIsAfterVP)
				{
					if (nlvIndexInView == endItemIndexInView) // all items are outside viewport => abort
						return false;
				}
				else
				{
					// Next item is after vp(if neg) or before vp (if pos) => no more items will become visible 
					// (this happens usually in the first iteration of this loop inner loop, i.e. negNLVCandidateBeforeVP_posNLVCandidateAfterVP never being true)
					if (currentRealInsetToUseForNLV_negFromCTS_posFromCTE > vpSize)
						return false;

					// At this point, we've found the real nlv: nlvIndex, nlvH and currentTopToUseForNLV(if negativeScroll)/currentBottomToUseForNLV(if upScroll) were correctly assigned
					return true;
				}
				currentItemRealInset_negStart_posEnd += nlvSizePlusSpacing;
				//++iterations;
				nlvIndexInView += neg1_posMinus1;
			}
			while (true);
		}

		void PrepareOutsideItemsForPotentialRecycleAndGetNextFirstVisibleIndexInView(out int startingVHIndexInView)
		{
			// startingLV means the item in _VisibleItems that's the closest to the next one that'll spawn

			int startingLVHolderIndex;

			// startingLVHolderIndex will be:
			// _VisibleItemsCount - 1, if negativeScroll
			// 0, if upScroll
			startingLVHolderIndex = neg1_pos0 * (_Adapter.VisibleItemsCount - 1);
			startingLVHolder = _Adapter._VisibleItems[startingLVHolderIndex];
			//startingLVRT = startingLVHolder.root;

			// Approach name(will be referenced below): (%%%)
			// currentStartToUseForNLV will be:
			// NLV top (= LV bottom - spacing), if negativeScroll
			// NLV bottom (= LV top + spacing), else
			//---
			// More in depth: <down0up1 - startingLVRT.pivot.y> will be
			// -startingLVRT.pivot.y, if negativeScroll
			// 1 - startingLVRT.pivot.y, else
			//---
			// And: 
			// ctSpacing will be subtracted from the value, if negativeScroll
			// added, if upScroll


			// Items variable values; initializing them to the current LV
			startingVHIndexInView = startingLVHolder.itemIndexInView + neg1_posMinus1;

#if DEBUG_COMPUTE_VISIBILITY
			debugString += "\n ThereAreVisibleItems: (starting)currentLVItemIndexInView=" + startingVHIndexInView;
#endif

			bool currentIsOutside;
			//RectTransform curRecCandidateRT;
			double curRecCandidateSizePlusSpacing;

			// vItemHolder is:
			// first in _VisibleItems, if negativeScroll
			// last in _VisibleItems, else
			int curRecCandidateVHIndex = neg0_pos1 * (_Adapter.VisibleItemsCount - 1);
			TItemViewsHolder curRecCandidateVH = _Adapter._VisibleItems[curRecCandidateVHIndex];
			double curVrtInsetFromParentEdge = negativeScroll ? _InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(curRecCandidateVH.itemIndexInView)
															: _InternalState.GetItemVirtualInsetFromParentEndUsingItemIndexInView(curRecCandidateVH.itemIndexInView);
			while (true)
			{
				//// vItemHolder is:
				//// first in _VisibleItems, if negativeScroll
				//// last in _VisibleItems, else
				//int curRecCandidateVHIndex = neg0_pos1 * (_VisibleItemsCount - 1);

				curRecCandidateSizePlusSpacing = _ItemsDesc[curRecCandidateVH.itemIndexInView] + _InternalState.spacing; // major bugfix: 18.12.2016 1:20: must use vItemHolder.ItemIndex INSTEAD of currentLVItemIndex

				// Commented: avoiding some potential loss in precision
				//currentIsOutside = negCTVrtInsetFromVPS_posCTVrtInsetFromVPE + (curVrtInsetFromParentEdge + curRecCandidateSizePlusSpacing) <= 0d;
				currentIsOutside = negCTVrtInsetFromVPS_posCTVrtInsetFromVPE <= -(curVrtInsetFromParentEdge + curRecCandidateSizePlusSpacing);

#if DEBUG_COMPUTE_VISIBILITY
				var realInsetFromParentEdge = negativeScroll ? _InternalState.GetItemInferredRealInsetFromParentStart(curRecCandidateVH.itemIndexInView)
															: _InternalState.GetItemInferredRealInsetFromParentEnd(curRecCandidateVH.itemIndexInView);
				debugString += "\n |---: curRecCandidateVHIndex=" + curRecCandidateVHIndex + 
					", itemIdxView=" + curRecCandidateVH.itemIndexInView + 
					", vrtIinsetFromPar=" + curVrtInsetFromParentEdge + 
					", realIinsetFromPar=" + realInsetFromParentEdge + 
					", outside=" + currentIsOutside;
#endif
				if (currentIsOutside)
				{
					_Adapter._RecyclableItems.Add(curRecCandidateVH);
					_Adapter._VisibleItems.RemoveAt(curRecCandidateVHIndex);
					--_Adapter.VisibleItemsCount;

					if (_Adapter.VisibleItemsCount == 0) // all items that were considered visible are now outside viewport => will need to seek even more below 
						break;
				}
				else
					break; // the current item is INside(not necessarily completely) the viewport

				// if negative, VIs will be removed from start, so the index of the "next" stays constantly at 0; 
				// if positive, the index of the "next" is decremented by one, because it starts at end and the list is always shortened by 1
				curRecCandidateVHIndex -= neg0_pos1;

				curVrtInsetFromParentEdge += curRecCandidateSizePlusSpacing;
				curRecCandidateVH = _Adapter._VisibleItems[curRecCandidateVHIndex];
			}
		}

		//bool InferFirstVisibleVHIndexInViewAndInset(ref int currentInferredFirstVisibleVHIndexInView, int estimatedAVGVisibleItems)
		bool InferFirstVisibleVHIndexInViewAndInset(ref int currentInferredFirstVisibleVHIndexInView, double estimatedAVGVisibleItems)
		{
			int initialEstimatedIndexInViewOfNewFirstVisible = (int)
				Math.Round(
					(1d - _InternalState.GetVirtualAbstractNormalizedScrollPosition()) * ((itemsCount - 1) - neg1_pos0 * estimatedAVGVisibleItems)
				);
			initialEstimatedIndexInViewOfNewFirstVisible = Math.Max(0, Math.Min(itemsCount - 1, initialEstimatedIndexInViewOfNewFirstVisible));

			int index = initialEstimatedIndexInViewOfNewFirstVisible;
			double itemSize = _ItemsDesc.GetItemSizeOrDefault(index);
			double negRealInsetStart_posRealInsetEnd =
				negativeScroll ?
					_InternalState.GetItemInferredRealInsetFromParentStart(index)
					: _InternalState.GetItemInferredRealInsetFromParentEnd(index);
				
			int firstOutsideBoundsIndex = itemsCount * neg0_pos1 - neg1_pos0; // -1 if neg, itemsCount if pos

			// Go down/right until a visible item is found
			while (negRealInsetStart_posRealInsetEnd <= -itemSize)
			{
				int nextPotentialIndex = index + neg1_posMinus1;
				if (nextPotentialIndex == firstOutsideBoundsIndex)
					break;

				index = nextPotentialIndex;
				itemSize = _ItemsDesc.GetItemSizeOrDefault(index);
				negRealInsetStart_posRealInsetEnd += itemSize + _InternalState.spacing;
			}

			// If the previous loop didnt' execute at all, it means there's a possibility that the searched item may be after the next item (next=to end if neg scrolling)
			if (index == initialEstimatedIndexInViewOfNewFirstVisible)
			{
				// Go up/left until the FIRST visible item is found (i.e. no one visible before it (after it, if positive scroll))
				do
				{
					int nextPotentialIndex = index - neg1_posMinus1; // next actually means before if neg, after if pos
					if (nextPotentialIndex == firstOutsideBoundsIndex)
						break;

					double nextPotentialItemSize = _ItemsDesc.GetItemSizeOrDefault(nextPotentialIndex);
					double nextPotential_negRealInsetStart_posRealInsetEnd = negRealInsetStart_posRealInsetEnd - (nextPotentialItemSize + _InternalState.spacing);

					if (nextPotential_negRealInsetStart_posRealInsetEnd <= -nextPotentialItemSize) // the next is outside VP => the current one is the first visible
						break;
					index = nextPotentialIndex;
					itemSize = nextPotentialItemSize;
					negRealInsetStart_posRealInsetEnd = nextPotential_negRealInsetStart_posRealInsetEnd;
				}
				while (true);

				if (index < 0)
					throw new OSAException("index " + index + ", currentInferredFirstVisibleVHIndexInView " + currentInferredFirstVisibleVHIndexInView);
				if (index >= itemsCount)
					throw new OSAException("index " + index + " >= itemsCount " + itemsCount + ", currentInferredFirstVisibleVHIndexInView " + currentInferredFirstVisibleVHIndexInView);
			}

			if (!thereWereVisibletems ||
				negativeScroll && index >= currentInferredFirstVisibleVHIndexInView || // the index should be bigger if going down/right. if the inferred one is <=, then startingLV.itemIndexInview is reliable 
				!negativeScroll && index <= currentInferredFirstVisibleVHIndexInView // analogous explanation for pos scroll
				// update: also using "=" to prevent caller from calculating the inset himself
			)
			{
				//Debug.Log("est=" + estimatedIndexInViewOfNewFirstVisible + ", def=" + currentLVItemIndexInView + ", actual=" + index);
#if DEBUG_COMPUTE_VISIBILITY
				debugString += "\nOptimizing big jump: currentInferred "+ currentInferredFirstVisibleVHIndexInView.ToString(DEBUG_FLOAT_FORMAT) + 
					" resolvedIndexItemBeforeNLV=" + index + ", initialEstimatedNLVIndex=" + initialEstimatedIndexInViewOfNewFirstVisible;
#endif

				currentInferredFirstVisibleVHIndexInView = index;
				currentItemRealInset_negStart_posEnd = negRealInsetStart_posRealInsetEnd;

				return true;
			}
#if DEBUG_COMPUTE_VISIBILITY
			debugString += "\nOptimizing big jump: already in bounds";
#endif
			return false;
		}

		void PostComputeVisibilityCleanRecyclableItems()
		{
			// Disable all recyclable views
			// Destroy remaining unused views, BUT keep one, so there's always a reserve, instead of creating/destroying very frequently
			// + keep <numVisibleItems + numItemsInRecycleBin> above <_InternalParams.maxVisibleItemsSeenSinceLastScrollViewSizeChange>
			// See GetNumExcessObjects()
			//GameObject go;
			TItemViewsHolder vh;
			var recItems = _Adapter._RecyclableItems;
			//for (int i = 0; i < recItems.Count;)
			int i = recItems.Count;
			while (i-- > 0)
			{
				vh = recItems[i];
				if (_Adapter.IsViewsHolderEnabled(vh))
				{
					_Adapter.InternalOnBeforeRecycleOrDisableViewsHolder(vh, -1); // -1 means it'll be disabled, not re-used ATM
					_Adapter.SetViewsHolderDisabled(vh);
				}

				recItems.RemoveAt(i);
				int excessCount = _Adapter.GetNumExcessRecycleableItems();
				if (_Adapter.InternalShouldDestroyRecyclableItem(vh, excessCount > 0))
				{
					_Adapter.InternalDestroyRecyclableItem(vh, false);
					++_ItemsDesc.destroyedItemsSinceLastScrollViewSizeChange;
				}
				else
				{
					//++i;
					_Adapter.AddBufferredRecycleableItem(vh);
				}
			}
		}
	}
}
