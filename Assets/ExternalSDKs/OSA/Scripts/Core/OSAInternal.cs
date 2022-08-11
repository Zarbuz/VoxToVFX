//#define ALLOW_DEBUG_OUTSIDE_EDITOR

#if UNITY_EDITOR || ALLOW_DEBUG_OUTSIDE_EDITOR
	//#define DEBUG_COMPUTE_VISIBILITY_TWIN
	//#define DEBUG_CHANGE_COUNT
	//#define DEBUG_UPDATE
	//#define DEBUG_INDICES
	//#define DEBUG_CONTENT_VISUALLY
	//#define DEBUG_ADD_VHS
	//#define DEBUG_LOOPING
	//#define DEBUG_ON_SIZES_CHANGED_EXTERNALLY
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;
using Com.TheFallenGames.OSA.Core.SubComponents;
using Com.TheFallenGames.OSA.Core.Data;
using Com.TheFallenGames.OSA.Core.Data.Gallery;

namespace Com.TheFallenGames.OSA.Core
{
	public abstract partial class OSA<TParams, TItemViewsHolder> : MonoBehaviour, IOSA
	where TParams : BaseParams
	where TItemViewsHolder : BaseItemViewsHolder
	{
#if DEBUG_UPDATE
		public bool debug_Update;
#endif

#if DEBUG_CONTENT_VISUALLY
		public bool debug_ContentVisually;
#endif

#if DEBUG_INDICES
		public bool debug_Indices;
#endif

		ComputeVisibilityParams _ComputeVisibilityParams_Reusable_Empty = new ComputeVisibilityParams(),
								_ComputeVisibilityParams_Reusable_DragUnchecked = new ComputeVisibilityParams();

		void Drag(
			double abstrDeltaInCTSpace,
			AllowContentOutsideBoundsMode allowOutsideBoundsMode,
			bool cancelSnappingIfAny)
		{
			bool _, __;
			Drag(abstrDeltaInCTSpace, allowOutsideBoundsMode, cancelSnappingIfAny, out _, out __);
		}

		/// <summary></summary>
		/// <param name="abstrDeltaInCTSpace">
		/// diff in positions, raw value, local space (content's space). 
		/// Represented as normalized is: 
		///		start=1, end=0
		///		, and translated in local space: 
		///			1) vert: 1=top, 0=bottom; 
		///			2) hor: inversely
		///	</param>
		void Drag(
			double abstrDeltaInCTSpace, 
			AllowContentOutsideBoundsMode allowOutsideBoundsMode, 
			bool cancelSnappingIfAny,
			out bool done,
			out bool looped)
			//bool cancelSnappingIfAny,
			//bool updateCachedCTVirtualInset = true,
			//double? updateCachedCTVirtualInset_ContentInsetOverride = null)
		{
			done = false;
			looped = false;

			//Debug.Log("Dragging by abstrDelta " + abstrDeltaInCTSpace);
			// Commented: Drag functions correctly in theory even if there are no visible items; it'll just update the content's inset, which is what we want in this case
			//if (_VisibleItemsCount == 0)
			//	return false;
			// TODO think if it eases the looping or not to clamp using the last item's inset instead of the content's. 
			// R(after more thinking): yes
			double dragCoefficient = 1d;
			double curPullSignDistance, newPullDistance;
			double absAbstractDelta;
			//bool isVirtualizing = _InternalState.VirtualScrollableArea > 0;
			if (abstrDeltaInCTSpace > 0d) // going to start
			{
				//if (isVirtualizing)
					curPullSignDistance = _InternalState.ctVirtualInsetFromVPS_Cached;
				//else
				//	// Not virtualizing means the content's allowed start edge is not at VPS
				//	curPullSignDistance = _InternalState.GetContentInferredRealInsetFromVPS(_VisibleItems[0]);

				absAbstractDelta = abstrDeltaInCTSpace;
			}
			else // going to end
			{
				//if (isVirtualizing)
					curPullSignDistance = _InternalState.CTVirtualInsetFromVPE_Cached;
				//else
				//	// Not virtualizing means the content's allowed end edge position is not at VPE
				//	curPullSignDistance = _InternalState.GetContentInferredRealInsetFromVPE(_VisibleItems[_VisibleItemsCount-1]);
				absAbstractDelta = -abstrDeltaInCTSpace;
			}

			newPullDistance = curPullSignDistance + absAbstractDelta;

			bool allowOutsideBounds;
			if (newPullDistance >= 0d) // is pulled beyond bounds
			{
				allowOutsideBounds = allowOutsideBoundsMode == AllowContentOutsideBoundsMode.ALLOW 
									|| allowOutsideBoundsMode == AllowContentOutsideBoundsMode.ALLOW_IF_OUTSIDE_AMOUNT_SHRINKS 
													&& newPullDistance < curPullSignDistance;

				bool currentIsAlreadyPulledOutOfBoundsOrIsAtLimit = curPullSignDistance >= 0d;
				if (/*!_Params.elasticMovement || _Params.effects.LoopItems || */!allowOutsideBounds)
				{
					if (currentIsAlreadyPulledOutOfBoundsOrIsAtLimit)
						return; // nothing more to pull

					double maxAllowedAbsDelta = -curPullSignDistance;
					abstrDeltaInCTSpace = Math.Sign(abstrDeltaInCTSpace) * Math.Min(absAbstractDelta, maxAllowedAbsDelta);
				}

				if (_Params.effects.ElasticMovement && currentIsAlreadyPulledOutOfBoundsOrIsAtLimit)
				{
					double curPullSignDistance01 = curPullSignDistance / (_InternalState.vpSize);
					if (curPullSignDistance01 > 1d)
						curPullSignDistance01 = 1d;
					dragCoefficient = _Params.effects.PullElasticity * (1d - curPullSignDistance01);
				}
			}
			else
				allowOutsideBounds = allowOutsideBoundsMode != AllowContentOutsideBoundsMode.DO_NOT_ALLOW;

			double finalAbstractDelta = abstrDeltaInCTSpace * dragCoefficient;
			//DragVisibleItemsRangeUnchecked(0, _VisibleItemsCount, finalAbstractDelta, false);
			//UpdateCTVrtInsetFromVPS(
			//	new ContentSizeOrPositionChangeParams {
			//		cancelSnappingIfAny = true,
			//		computeVisibilityNowIfSuccess = true,
			//		computeVisibilityNowIfSuccess_OverrideDelta = finalAbstractDelta,
			//		fireScrollPositionChangedEvent = true,
			//		keepVelocity = true,
			//		allowOutsideBounds = allowOutsideBounds
			//	}
			//);

			looped = DragVisibleItemsRangeUnchecked(0, _VisibleItemsCount, finalAbstractDelta, true, true, allowOutsideBounds, cancelSnappingIfAny);
			//bool looped = DragVisibleItemsRangeUnchecked(0, _VisibleItemsCount, finalAbstractDelta, updateCachedCTVirtualInset, true, allowOutsideBounds, cancelSnappingIfAny, updateCachedCTVirtualInset_ContentInsetOverride);

			//double newInset = currentInset + finalAbstractDelta;
			//SetContentVirtualInsetFromViewportStart2(newInset, true, false, true, true, true, allowOutsideBounds);
			done = true;
		}

		// Returns whether looped or not
		internal bool DragVisibleItemsRangeUnchecked(
			int vhStartIndex, 
			int vhEndIndexExcl,
			double abstractDelta,
			bool updateCachedCTVirtualInset,
			bool updateCachedCTVirtualInset_ComputeVisibility,
			bool updateCachedCTVirtualInset_AllowOutsideBounds = true,
			bool updateCachedCTVirtualInset_CancelSnappingIfAny = true
			//double? updateCachedCTVirtualInset_ContentInsetOverride = null
		)
		{
			//Debug.Log("DragVisibleItemsRangeUnchecked: (count="+Math.Max(0, vhEndIndexExcl - vhStartIndex)+") start=" + vhStartIndex + ", endExcl=" + vhEndIndexExcl + ", abstrDelta=" + abstractDelta);
			double localDelta = (float)(abstractDelta * _InternalState.hor1_vertMinus1);
			for (int i = vhStartIndex; i < vhEndIndexExcl; ++i)
			{
				var vh = _VisibleItems[i];
				var localPos = vh.root.localPosition;
				//localPos[_InternalState.hor0_vert1] += transformedLocalDelta[_InternalState.hor0_vert1];
				localPos[_InternalState.hor0_vert1] = (float)(localDelta + localPos[_InternalState.hor0_vert1]);
				vh.root.localPosition = localPos;
			}

			if (updateCachedCTVirtualInset)
			{
				_ComputeVisibilityParams_Reusable_DragUnchecked.overrideDelta = abstractDelta;
				double? contentInsetOverride = null;
				if (_VisibleItemsCount == 0) // nothing to infer the content size from => use the cached one
				{
					contentInsetOverride = _InternalState.ctVirtualInsetFromVPS_Cached + abstractDelta;
				}

				var p = new ContentSizeOrPositionChangeParams
				{
					cancelSnappingIfAny = updateCachedCTVirtualInset_CancelSnappingIfAny,
					allowOutsideBounds = updateCachedCTVirtualInset_AllowOutsideBounds,
					computeVisibilityParams = updateCachedCTVirtualInset_ComputeVisibility ? _ComputeVisibilityParams_Reusable_DragUnchecked : null,
					fireScrollPositionChangedEvent = true,
					keepVelocity = true,
					contentInsetOverride = contentInsetOverride
					//contentInsetOverride = updateCachedCTVirtualInset_ContentInsetOverride
				};

				return UpdateCTVrtInsetFromVPS(ref p);
			}

			return false;
		}

		IEnumerator SmoothScrollProgressCoroutine(
			int itemIndex,
			double duration, 
			double normalizedOffsetFromViewportStart = 0f,
			double normalizedPositionOfItemPivotToUse = 0f,
			Func<float, bool> onProgress = null,
			Action onDone = null)
		{
			//Debug.Log("Started routine");
			double vsa = _InternalState.VirtualScrollableArea;
			// Negative/zero values indicate CT is smallerthan/sameas VP, so no scrolling can be done
			if (vsa <= 0d)
			{
				// This is dependent on the case. sometimes is needed, sometimes not
				//if (duration > 0f)
				//{
				//	if (_Params.UseUnscaledTime)
				//		yield return new WaitForSecondsRealtime(duration);
				//	else
				//		yield return new WaitForSeconds(duration);
				//}

				_SmoothScrollCoroutine = null;

				if (onProgress != null)
					onProgress(1f);

				if (onDone != null)
					onDone();
				//Debug.Log("stop 1f");
				yield break;
			}

			// Ignoring OnScrollViewValueChanged during smooth scrolling
			var ignorOnScroll_lastValue = _SkipComputeVisibilityInUpdateOrOnScroll;
			_SkipComputeVisibilityInUpdateOrOnScroll = true;

			StopMovement();

			//Canvas.ForceUpdateCanvases();
			if (_Params.optimization.ForceLayoutRebuildOnBeginSmoothScroll)
				_InternalState.RebuildLayoutImmediateCompat(_Params.ScrollViewRT);

			Func<double> getTargetVrtInset = () =>
			{
				// This needs to be updated regularly (if looping/twin pass, but it doesn't add too much overhead, so it's ok to re-calculate it each time)
				vsa = _InternalState.VirtualScrollableArea;

				return ScrollToHelper_GetContentStartVirtualInsetFromViewportStart_Clamped(
							vsa, 
							itemIndex, 
							normalizedOffsetFromViewportStart, 
							normalizedPositionOfItemPivotToUse
						);
			};

			double initialVrtInsetFromParent = -1d, targetVrtInsetFromParent = -1d; // setting a value because of compiler, but it's initialized at least once in the loop below
			bool needToCalculateInitialInset = true, needToCalculateTargetInset = true, notCanceledByCaller = true;
			double startTime = Time, elapsedTime;
			double  localProgress = 0d, // used in calculations
					reportedProgress, // the "real" progress, as needed for the caller of this function
					value;
			var endOfFrame = new WaitForEndOfFrame();

			var contentPosChangeParams = new ContentSizeOrPositionChangeParams
			{
				computeVisibilityParams = _ComputeVisibilityParams_Reusable_Empty,
				fireScrollPositionChangedEvent = true,
				allowOutsideBounds = true
			};

			bool looped = false;
			Action<double> setInsetAndUpdateLocalsFn = inset =>
			{
				//Debug.Log("vrtinset="+_InternalState.ContentPanelVirtualInsetFromViewportStart + ", i="+ initialVirtualInsetFromParent + ", t="+ targetInsetFromParent + ", v="+value);
				contentPosChangeParams.allowOutsideBounds = _Params.effects.LoopItems && _InternalState.VirtualScrollableArea > 0d;
				SetContentVirtualInsetFromViewportStart(inset, ref contentPosChangeParams, out looped);
			};

			double time;
			double originalStartTime = startTime, originalDuration = duration;
			//bool neededToRecalculateInitialInset;
			bool atLeastOneTwinPassDetected = false;
			Action updateTwinPassLocalVariable = () => atLeastOneTwinPassDetected = atLeastOneTwinPassDetected || _InternalState.lastComputeVisibilityHadATwinPass;

			do
			{
				yield return null;
				updateTwinPassLocalVariable();
				//hadTwinPass = hadTwinPass || _InternalState.lastComputeVisibilityHadATwinPass;
				yield return endOfFrame;
				updateTwinPassLocalVariable();

				time = Time;
				elapsedTime = time - startTime;

				if (elapsedTime >= duration)
					reportedProgress = localProgress = 1d;
				else
				{
					// Normal in, sin slow out
					//progress = (elapsedTime / duration);
					localProgress = Math.Sin((elapsedTime / duration) * Math.PI / 2);
					reportedProgress = Math.Sin(((time - originalStartTime) / originalDuration) * Math.PI / 2);
				}

				//neededToRecalculateInitialInset = needToCalculateInitialInset;
				if (needToCalculateInitialInset)
				{
					initialVrtInsetFromParent = _InternalState.ctVirtualInsetFromVPS_Cached;

					startTime = time;
					duration -= elapsedTime;
				}

				if (needToCalculateTargetInset)
				{
					targetVrtInsetFromParent = getTargetVrtInset();

					//if (!neededToRecalculateInitialInset)
					//{

					//}
				}

				value = initialVrtInsetFromParent * (1d - localProgress) + targetVrtInsetFromParent * localProgress; // Lerp for double
				//Debug.Log(
				//	"t=" + progress.ToString("0.####") +
				//	", i=" + initialVrtInsetFromParent.ToString("0") +
				//	", t=" + targetVrtInsetFromParent.ToString("0") +
				//	", t-i=" + (targetVrtInsetFromParent - initialVrtInsetFromParent).ToString("0") +
				//	", toSet=" + value.ToString("0"));

				
				// If finished earlier => don't make additional unnecesary steps
				if (Math.Abs(targetVrtInsetFromParent - value) < .01d)
				{
					value = targetVrtInsetFromParent;
					reportedProgress = localProgress = 1d;
				}

				// Values that that would cause the ctStart to be placed AFTER vpStart should indicate the scrolling has ended (can't go past it)
				// Only allowed if looping
				if (value > 0d && !_Params.effects.LoopItems)
				{
					reportedProgress = localProgress = 1d; // end; last loop
					value = 0d;
				}
				else
				{
					setInsetAndUpdateLocalsFn(value);
					updateTwinPassLocalVariable();

					if (_Params.effects.LoopItems)
					{
						needToCalculateInitialInset = needToCalculateTargetInset = true;
					}
					else
					{
						needToCalculateInitialInset = needToCalculateTargetInset = atLeastOneTwinPassDetected;
					}

					//if (_InternalState.lastComputeVisibilityHadATwinPass)
					//	Debug.Log(_InternalState.lastComputeVisibilityHadATwinPass);
					//if (false && looped)
					//{
					//	needToCalculateInitialInset = true;
					//	needToCalculateTargetInset = true;
					//}
				}
			}
			while (reportedProgress < 1d && (onProgress == null || (notCanceledByCaller = onProgress((float)reportedProgress))));

			if (notCanceledByCaller)
			{
				updateTwinPassLocalVariable();
				// Assures the end result is the expected one
				setInsetAndUpdateLocalsFn(getTargetVrtInset());
				updateTwinPassLocalVariable();

				// Bugfix when new items request a twin pass which may displace the content, or if the content simply looped (this is the same correction as done in the loop above)
				if (looped || atLeastOneTwinPassDetected)
					setInsetAndUpdateLocalsFn(getTargetVrtInset());

				//if (false && looped)
				//{
				//	needToCalculateInitialInset = true;
				//	needToCalculateTargetInset = true;
				//}

				//// This is a semi-hack-lazy hot-fix because when the duration is 0 (or near 0), sometimes the visibility isn't computed well
				//// Same thing is done in ScrollTo method above
				//ComputeVisibilityForCurrentPosition(false, -.1);
				//ComputeVisibilityForCurrentPosition(true, +.1);
				////ScrollTo(itemIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);

				_SmoothScrollCoroutine = null;

				if (onProgress != null)
					onProgress(1f);

				//Debug.Log("stop natural");

			}
			//else
			//	Debug.Log("routine cancelled");

			// This should be restored even if the scroll was cancelled by the caller. 
			// When the routine is stopped via StopCoroutine, this line won't be executed because the execution point won't pass the previous yield instruction.
			// It's assumed that _SkipComputeVisibilityInUpdateOrOnScroll is manually set to false whenever that happpens
			_SkipComputeVisibilityInUpdateOrOnScroll = ignorOnScroll_lastValue;

			if (notCanceledByCaller)
			{
				if (onDone != null)
					onDone();
			}
		}

		/// <summary> It assumes that the content is bigger than the viewport </summary>
		double ScrollToHelper_GetContentStartVirtualInsetFromViewportStart_Clamped(double vsa, int itemIndex, double normalizedItemOffsetFromStart, double normalizedPositionOfItemPivotToUse)
		{
			double maxContentInsetFromVPAllowed = _Params.effects.LoopItems && vsa > 0d ? _InternalState.vpSize/2d : 0d; // if looping, there's no need to clamp. in addition, clamping would cancel a scrollTo if the content is exactly at start or end
			double minContentVirtualInsetFromVPAllowed = -vsa - maxContentInsetFromVPAllowed;
			int itemViewIdex = _ItemsDesc.GetItemViewIndexFromRealIndexChecked(itemIndex);
			double itemSize = _ItemsDesc[itemViewIdex];
			double insetToAdd = _InternalState.vpSize * normalizedItemOffsetFromStart - itemSize * normalizedPositionOfItemPivotToUse;

			double itemVrtInsetFromStart = _InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(itemViewIdex);
			double ctInsetFromStart_Clamped = Math.Max(
						minContentVirtualInsetFromVPAllowed,
						Math.Min(maxContentInsetFromVPAllowed, -itemVrtInsetFromStart + insetToAdd)
					);

			//Debug.Log("siz=" + itemSize + ", -itemVrtInsetFromStart=" + (-itemVrtInsetFromStart) + ", insetToAdd=" + insetToAdd + ", ctInsetFromStart_Clamped=" + ctInsetFromStart_Clamped);

			return ctInsetFromStart_Clamped;
		}

		//bool DoesInsetDeltaRequireOptimization(double insetDelta)
		//{
		//	return Math.Abs(insetDelta) > OSAConst.RECYCLE_ALL__MIN_DRAG_AMOUNT_AS_FACTOR_OF_VIEWPORT_SIZE * _InternalState.vpSize;
		//}

		/// <summary><paramref name="virtualInset"/> should be a valid value. See how it's clamped in <see cref="ScrollTo(int, float, float)"/></summary>
		internal double SetContentVirtualInsetFromViewportStart(double virtualInset, ref ContentSizeOrPositionChangeParams p, out bool looped)
		{
			_ReleaseFromPull.inProgress = false;

			double insetDelta = virtualInset - _InternalState.ctVirtualInsetFromVPS_Cached;
//			//Debug.Log("insetDelta " + insetDelta + ", maxAllowed " + OSAConst.RECYCLE_ALL__MIN_DRAG_AMOUNT_AS_FACTOR_OF_VIEWPORT_SIZE * _InternalState.vpSize);
			//bool bigJumpOptimization = DoesInsetDeltaRequireOptimization(insetDelta);
//			//Debug.Log("SetInset " + virtualInset.ToString(OSAConst.DEBUG_FLOAT_FORMAT) + (bigJumpOptimization ? ", bigJumpOptimiz." : ""));
//#warning remove this if tested and not needed anymore
//			if (false && bigJumpOptimization)
//			{
//				//Debug.Log("bigJumpOptimization ");
//				//Debug.Log("Opt1 curInset " + _InternalState.ctVirtualInsetFromVPS_Cached + ", insetDelta " + insetDelta + ", vsa " + _InternalState.VirtualScrollableArea);
//				RecycleAllVisibleViewsHolders();

//				p.contentInsetOverride = virtualInset;
//				looped = UpdateCTVrtInsetFromVPS(ref p);

//				// Quick-fix: items not being corrected + computed when jumping large amounts when recycleAll threshold is reached, 
//				// especially if different sizes
//				CorrectPositionsOfVisibleItems(false);
//				ComputeVisibilityForCurrentPositionRawParams(true, false, -.1d);
//				ComputeVisibilityForCurrentPositionRawParams(true, false, .1d);

//				//Debug.Log("Opt2 curInset " + _InternalState.ctVirtualInsetFromVPS_Cached + ", insetDelta " + insetDelta + ", vsa " + _InternalState.VirtualScrollableArea);

//				//// Quick fix for when looping and doing big jumps, which may prevent looping from being executed
//				//if (_Params.effects.LoopItems && _InternalState.VirtualScrollableArea > 0)
//				//{
//				//	Debug.Log("Opt3 vsa " + _InternalState.VirtualScrollableArea + ", insetDelta " + insetDelta);
//				//	Drag(
//				//		1f * Math.Sign(insetDelta),
//				//		_InternalState.VirtualScrollableArea > 0 ? AllowContentOutsideBoundsMode.ALLOW : AllowContentOutsideBoundsMode.ALLOW_IF_OUTSIDE_AMOUNT_SHRINKS,
//				//		p.cancelSnappingIfAny
//				//	);
//				//	Drag(
//				//		1f * -Math.Sign(insetDelta),
//				//		_InternalState.VirtualScrollableArea > 0 ? AllowContentOutsideBoundsMode.ALLOW : AllowContentOutsideBoundsMode.ALLOW_IF_OUTSIDE_AMOUNT_SHRINKS,
//				//		p.cancelSnappingIfAny
//				//	);

//				//	//ComputeVisibilityForCurrentPositionRawParams(true, false, -.1f);
//				//	//ComputeVisibilityForCurrentPositionRawParams(true, false, .1f);
//				//}
//			}
//			else
//			{
				bool _;

			if (!p.keepVelocity)
				StopMovement();

				Drag(
					insetDelta, 
					_Params.effects.LoopItems && _InternalState.VirtualScrollableArea > 0 ? 
						AllowContentOutsideBoundsMode.ALLOW 
						: AllowContentOutsideBoundsMode.ALLOW_IF_OUTSIDE_AMOUNT_SHRINKS, 
					p.cancelSnappingIfAny,
					out looped,
					out _
				);

				CorrectPositionsOfVisibleItems(false, p.fireScrollPositionChangedEvent);
				//if (bigJumpOptimization)
				//{
				//	Debug.Log("bigJumpOptimization");
				//	ClearCachedRecyclableItems();
				//}
			//}

			return insetDelta;
		}

		void RecycleAllVisibleViewsHolders()
		{
			while (_VisibleItemsCount > 0)
				RecycleOrStealViewsHolder(0, false);
		}

		void RecycleOrStealViewsHolder(int vhIndex, bool steal)
		{
			var vh = _VisibleItems[vhIndex];
			if (!steal)
			{
				OnBeforeRecycleOrDisableViewsHolder(vh, -1); // -1 means it'll be disabled, not re-used ATM
				SetViewsHolderDisabled(vh);
			}

			_VisibleItems.RemoveAt(vhIndex);
			--_VisibleItemsCount;
			if (steal)
			{
				//_StolenItems.Add(vh);
			}
			else
				_RecyclableItems.Add(vh);
		}

		bool UpdateCTVrtInsetFromVPS(ref ContentSizeOrPositionChangeParams p)
		{
			double ctInsetBefore = _InternalState.ctVirtualInsetFromVPS_Cached;

			double itemVirtualInset = _InternalState.paddingContentStart;
			double contentVirtualInset = 0d;
			if (p.contentInsetOverride != null)
				contentVirtualInset = p.contentInsetOverride.Value;
			else if (_VisibleItemsCount > 0)
			{
				var vh = _VisibleItems[0];
				int indexInViewOfFirstVisible = vh.itemIndexInView;

				if (indexInViewOfFirstVisible > 0)
					itemVirtualInset += _ItemsDesc.GetItemSizeCumulative(indexInViewOfFirstVisible - 1, false) + indexInViewOfFirstVisible * _InternalState.spacing;

				double itemRealInset = vh.root.GetInsetFromParentEdge(_Params.Content, _InternalState.startEdge);

				contentVirtualInset = itemRealInset - itemVirtualInset;
			}

			var ignoreOnScroll_valueBefore = _SkipComputeVisibilityInUpdateOrOnScroll;
			_SkipComputeVisibilityInUpdateOrOnScroll = true;

			if (p.cancelSnappingIfAny && _Params.Snapper)
				_Params.Snapper.CancelSnappingIfInProgress();

			if (!p.keepVelocity)
				StopMovement();

			_InternalState.UpdateCachedCTVirtInsetFromVPS(contentVirtualInset, p.allowOutsideBounds);
			bool looped = false;
			if (p.computeVisibilityParams != null)
			{
				// Correct positisions only temporarily added to test something
				//CorrectPositionsOfVisibleItems(false);
				looped = ComputeVisibilityForCurrentPosition(p.computeVisibilityParams);
				//CorrectPositionsOfVisibleItems(true);
			}
			if (p.fireScrollPositionChangedEvent)
				OnScrollPositionChangedInternal();

			_SkipComputeVisibilityInUpdateOrOnScroll = ignoreOnScroll_valueBefore;

			if (_Params.effects.HasContentVisual)
			{
				double ctInsetDelta = _InternalState.ctVirtualInsetFromVPS_Cached - ctInsetBefore;
				ctInsetDelta = ctInsetDelta + _Params.effects.ContentVisualParallaxEffect * ctInsetDelta;

				var uvRect = _Params.effects.ContentVisual.uvRect;

				var pos = uvRect.position;
				float curVal = pos[_InternalState.hor0_vert1];
				double dragToVPSizeRatio = ctInsetDelta / _InternalState.vpSize;
				double dragToVPSizeRatio_FractionalPartPositiveLooped = dragToVPSizeRatio - Math.Floor(dragToVPSizeRatio);

				double dragDeltaInUVSpace = dragToVPSizeRatio_FractionalPartPositiveLooped * (-_InternalState.hor1_vertMinus1);
				if (dragDeltaInUVSpace < 0d)
					dragDeltaInUVSpace = 1d + dragDeltaInUVSpace;

				double newUVPos = curVal + dragDeltaInUVSpace;
				newUVPos = newUVPos - Math.Floor(newUVPos);
				if (newUVPos < 0d)
					newUVPos = 1d + newUVPos;

				pos[_InternalState.hor0_vert1] = (float)newUVPos;
				uvRect.position = pos;
				_Params.effects.ContentVisual.uvRect = uvRect;
			}

			return looped;
		}

		void ShiftViewsHolderItemIndexAndFireEvent(TItemViewsHolder vh, int shift, bool wasInsert, int insertOrRemoveIndex)
		{
			int prev = vh.ItemIndex;
			vh.ShiftIndex(shift, _ItemsDesc.itemsCount);
			OnItemIndexChangedDueInsertOrRemove(vh, prev, wasInsert, insertOrRemoveIndex);
		}
		void ShiftViewsHolderItemIndexInView(TItemViewsHolder vh, int shift)
		{ vh.ShiftIndexInView(shift, _ItemsDesc.itemsCount); }

		/// <summary> 
		/// Make sure to only call this from <see cref="ChangeItemsCount(ItemCountChangeMode, int, int, bool, bool)"/>, because implementors may override it to catch the "pre-item-count-change" event
		/// </summary>
		void ChangeItemsCountInternal(
			ItemCountChangeMode changeMode, 
			int count, 
			int indexIfInsertingOrRemoving, 
			bool contentPanelEndEdgeStationary, 
			bool keepVelocity, 
			bool stealInsteadOfRecycle
		){
			if (!_Initialized && !_SkipInitializationChecks) 
				throw new OSAException("ChangeItemsCountInternal: OSA not initialized. Before using it, make sure the GameObject is active in hierarchy, the OSA component is enabled, and Start has been called. If you overrode OnInitialized, please call base.OnInitialized() on the first line of your function");

			int prevCount = _ItemsDesc.itemsCount;
			if (changeMode == ItemCountChangeMode.INSERT)
			{
				if (indexIfInsertingOrRemoving < 0 || indexIfInsertingOrRemoving > prevCount)
					throw new ArgumentOutOfRangeException("indexIfInsertingOrRemoving", indexIfInsertingOrRemoving, "should be >=0 and <= than itemsCount(=" + prevCount + ")");

				long newCountLong = (long)prevCount + count;
				if (newCountLong > OSAConst.MAX_ITEMS)
					throw new ArgumentOutOfRangeException("newCount", newCountLong, "should be <= MAX_COUNT(=" + OSAConst.MAX_ITEMS + ")");
			}
			else if (changeMode == ItemCountChangeMode.REMOVE)
			{
				if (indexIfInsertingOrRemoving < 0 || indexIfInsertingOrRemoving >= prevCount)
					throw new ArgumentOutOfRangeException("indexIfInsertingOrRemoving", indexIfInsertingOrRemoving, "should be >=0 and < than itemsCount(=" + prevCount + ")");

				if (count < 1)
					throw new ArgumentOutOfRangeException("count", count, "should be > 0");

				if (indexIfInsertingOrRemoving + count > prevCount)
					throw new ArgumentOutOfRangeException("indexIfInsertingOrRemoving + count", count, "indexIfInsertingOrRemoving+count = "+(indexIfInsertingOrRemoving + count) +" should be <= itemsCount(=" + _ItemsDesc.itemsCount + ")");
			}

			bool loopItems = _Params.effects.LoopItems;
			if (loopItems)
			{
				if (changeMode != ItemCountChangeMode.RESET)
					throw new OSAException("ChangeItemsCountInternal: At the moment, only ItemCountChangeMode.RESET is supported when looping. Use ResetItems()");

//				if (changeMode == ItemCountChangeMode.REMOVE)
//				{
//					if (count > 1)
//						throw new ArgumentOutOfRangeException(
//							"count", 
//							count, 
//							"Looping is enabled. Removing more than 1 item at once is not yet supported. " +
//								"Use ResetItems instead, or simply remove them 1 by 1 (if feasible)"
//						);
//				}

//				if (contentPanelEndEdgeStationary)
//				{
//#if UNITY_EDITOR
//					Debug.Log("OSA.ChangeItemsCountInternal: When looping is active, contentPanelEndEdgeStationary parameter is ignored");
//#endif
//					contentPanelEndEdgeStationary = false;
//				}
			}

			//OnItemsCountWillChange(itemsCount);
			CancelAnimationsIfAny();

			if (_ReleaseFromPull.inProgress)
			{
				// Bugfix 15-Jul.2019: if there are no items visible, there's nothing to drag
				if (_VisibleItemsCount > 0)
					_ReleaseFromPull.FinishNowByDraggingItems(false);
				else
					_ReleaseFromPull.FinishNowBySettingContentInset(false);
			}

			//if (_ReleaseFromPullCurrentState.inProgress && changeMode != ItemCountChangeMode.RESET)
			//{
			//	Debug.Log("ChangeItemsCountInternal: _ReleaseFromPullCurrentState.inProgress and removing/inserting. TODO clamp current items before, in case of negative VSA");
			//}

			//_ReleasingFromOutsideBoundsPull = false;

			var ignoreOnScroll_valueBefore = _SkipComputeVisibilityInUpdateOrOnScroll;
			_SkipComputeVisibilityInUpdateOrOnScroll = true;

			int indexInViewIfInsertingOrRemoving = -1;
			if (prevCount > 0 && changeMode != ItemCountChangeMode.RESET)
			{
				if (indexIfInsertingOrRemoving == prevCount)
					// If inserting at end, GetItemViewIndexFromRealIndex(<count>) will return 0, since the item count was not yet changed
					indexInViewIfInsertingOrRemoving = _ItemsDesc.GetItemViewIndexFromRealIndexChecked(indexIfInsertingOrRemoving-1) + 1;
				else
					indexInViewIfInsertingOrRemoving = _ItemsDesc.GetItemViewIndexFromRealIndexChecked(indexIfInsertingOrRemoving);
			}

			var velocity = _Velocity;
			if (!keepVelocity)
				StopMovement();

			double ctSizeBefore = _InternalState.CalculateContentVirtualSize();
			//if (_InternalState.layoutRebuildPendingDueToScrollViewSizeChangeEvent)
			//	Canvas.ForceUpdateCanvases();

#if DEBUG_INDICES
			string debugIndicesString;
			if (GetDebugIndicesString(out debugIndicesString))
				Debug.Log("ChangeCountBef vhs " + _VisibleItemsCount + ". Indices: " + debugIndicesString);
#endif
			//int oldCount = _ItemsDesc.itemsCount;
			CollectItemsSizes(changeMode, count, indexIfInsertingOrRemoving, _ItemsDesc);
			int newCount = _ItemsDesc.itemsCount;

			double newCTSize = _InternalState.CalculateContentVirtualSize();
			double deltaSize = newCTSize - ctSizeBefore;
			double? _;
			double additionalCTDragAbstrDelta = 0d; // only provided if shrinking
			_InternalState.CorrectParametersOnCTSizeChange(contentPanelEndEdgeStationary, out _, ref additionalCTDragAbstrDelta, newCTSize, deltaSize);
			double emptyAreaWhenCTSmallerThanVP = -_InternalState.VirtualScrollableArea;
			bool vrtContentPanelIsAtOrBeforeEnd = _InternalState.CTVirtualInsetFromVPE_Cached >= 0d;

			// Re-build the content: mark all currentViews as recyclable
			// _RecyclableItems.Count must be zero;
			if (GetNumExcessRecycleableItems() > 0)
				throw new OSAException("ChangeItemsCountInternal: GetNumExcessObjects() > 0 when calling ChangeItemsCountInternal(); this may be due ComputeVisibility not being finished executing yet");

			// TODO see if it makes sense to optimize by keeping the items that will continue to be visible, in case of insert/remove. Currently, all of them are being recycled
			// , case in which the more of the items will be dragged, not only the first one for ctinset calculation

			// DragVisibleItemsRangeUnchecked is called only to compute content inset start accordingly, as all the vhs are made into recyclable after, anyway
			double? reportedScrollDeltaOverride = null;
			double? ctInsetFromVPSOverrideToPassAsParam = null;
			////bool allowOutsideBounds = Parameters.elasticMovement;
			//// Outside bounds should be always allowed in case of insert/remove, because otherwise the content's inset is clamped due to its pivot when smaller than vp, but
			//// in ComputeVisibility that's called below the item's inset from CTS is being inferred from the new value 
			//// (thus, placing newly added items at tail over the existing ones, the more the pivot is towards bottom)
			bool allowOutsideBounds = false;

			TItemViewsHolder vh;
			// TODO see if setting ctInsetFromVPSOverrideToPassAsParam instead of auto-inferring-from-first-vh is necessary (maybe it does more harm than good)
			//Debug.Log("TODO see if setting ctInsetFromVPSOverrideToPassAsParam for REMOVE/INSERT instead of auto-inferring-from-first-vh is necessary (maybe it does more harm than good)");
			bool recycleAllViewsHolders = false, correctionMayBeNeeded = false;
			int vhIndexForInsertOrRemove = -1;
			if (_VisibleItemsCount > 0 && changeMode != ItemCountChangeMode.RESET)
			{
				var firstVH = _VisibleItems[0];
				int firstVHIndexInViewBeforeShifting = firstVH.itemIndexInView;
				vhIndexForInsertOrRemove = indexInViewIfInsertingOrRemoving - firstVHIndexInViewBeforeShifting;
			}

			switch (changeMode)
			{
				// IMGDOC <!image url="$(SolutionDir)\frame8\Docs\img\OSA\Insert-Remove-Items.jpg" scale=".86"/>
				case ItemCountChangeMode.INSERT:
					{
						int vhIndex;

						if (_VisibleItemsCount > 0)
						{
							// TODO test
							// Items with higer indices can be BEFORE the insertIndex, if looping. They need to be increased.
							// Increasing indices that are bigger, i.e. ignoring the HEAD(if looping) and items after.
							// This covers both the looping case and the normal case, to avoid multiple loops. vCount is not that big anyway
							// The indexInView of items before the inserted ones should remain the same
							for (vhIndex = 0; vhIndex < _VisibleItemsCount; ++vhIndex)
							{
								vh = _VisibleItems[vhIndex];
								if (vh.itemIndexInView >= indexInViewIfInsertingOrRemoving)
								{
									//if (loopItems && (vhIndexForInsertOrRemove < 0 || indexIfInsertingOrRemoving == oldCount))
									//{
									//	// If inserting before viewport when looping, the items will preserve their indexInView and they will also remain stationary
									//	// This is a easier way of handling looping in this case and avoiding some edge cases
									//}
									//else
										ShiftViewsHolderItemIndexInView(vh, count);
								}

								if (vh.ItemIndex >= indexIfInsertingOrRemoving)
									ShiftViewsHolderItemIndexAndFireEvent(vh, count, true, indexIfInsertingOrRemoving);
							}
						}

						//allowOutsideBounds = true;
						//// no looping 
						if (contentPanelEndEdgeStationary)
						{
							// commented: additionalCTDragAbstrDelta is 0 if expanding the size
							//ctInsetFromVPSOverrideToPassAsParam = _InternalState.contentPanelVirtualInsetFromViewportStart_Cached - deltaSize + additionalCTDragAbstrDelta;

							// TODO see if setting ctInsetFromVPSOverrideToPassAsParam should be done here instead of only inside one if branch
							if (emptyAreaWhenCTSmallerThanVP > 0)
							{
								ctInsetFromVPSOverrideToPassAsParam = _InternalState.ctVirtualInsetFromVPS_Cached - deltaSize;
								allowOutsideBounds = true;
							}

							if (_VisibleItemsCount > 0)
							{
								// Important: if you insert at X, that translates to inserting BEFORE x, meaning between x and x-1, which
								// means X should stay in place and only items before it should be shifted towards start.

								int vhEndIndex, vhEndIndexMinus1;
								vhEndIndex = vhIndexForInsertOrRemove;
								vhEndIndexMinus1 = vhEndIndex - 1;

								reportedScrollDeltaOverride = -.1d;

								// TODO test
								//// Only shifting indices that are bigger, i.e. ignoring the HEAD(if looping) and items after.
								//// Items with higer indices can be BEFORE the insertIndex, if looping. They need to be increased.
								//// This covers both the looping case and the normal case, to avoid multiple loops. vCount is not that big anyway
								//for (vhIndex = 0; vhIndex < _VisibleItemsCount; ++vhIndex)
								//{
								//	vh = _VisibleItems[vhIndex];
								//	if (vh.ItemIndex < indexIfInsertingOrRemoving)
								//		continue;
								//	ShiftViewsHolderIndex(vh, count, true, indexIfInsertingOrRemoving);
								//}

								if (vhEndIndexMinus1 < 0)
								{
									// The views holders to be shifted are all before vp => only shift the indices of all visible items

									reportedScrollDeltaOverride = .1d;

									//for (vhIndex = 0; vhIndex < _VisibleItemsCount; ++vhIndex)
									//{
									//	vh = _VisibleItems[vhIndex];
									//	if (loopItems && vh.ItemIndex < indexIfInsertingOrRemoving)
									//		continue;
									//	ShiftViewsHolderIndex(vh, count, true, indexIfInsertingOrRemoving);
									//}
								}
								else
								{
									//if (__t_OverrideCTInsetWhenInsertRemove)
									//	ctInsetFromVPSOverrideToPassAsParam = _InternalState.ctVirtualInsetFromVPS_Cached - deltaSize;
									//else if (emptyAreaWhenCTSmallerThanVP > 0)
									//{
									//	ctInsetFromVPSOverrideToPassAsParam = _InternalState.ctVirtualInsetFromVPS_Cached - deltaSize;
									//	allowOutsideBounds = true;
									//}

									if (vhEndIndex >= _VisibleItemsCount)
									{
										// The new items will be added after LV, so all the currently visible ones will be shifted towards start

										DragVisibleItemsRangeUnchecked(0, _VisibleItemsCount, -deltaSize, false, false);

										//// If looping, only shifting items with bigger ItemIndex, which can be before insertIndex, i.e. shifting indices of the TAIL and everything before it
										//if (loopItems)
										//{
										//	for (vhIndex = 0; vhIndex < _VisibleItemsCount; ++vhIndex)
										//	{
										//		vh = _VisibleItems[vhIndex];
										//		if (vh.ItemIndex < indexIfInsertingOrRemoving)
										//			continue;
										//		ShiftViewsHolderIndex(vh, count, true, indexIfInsertingOrRemoving);
										//	}
										//}
										////for (vhIndex = 0; vhIndex < _VisibleItemsCount; ++vhIndex)
										////	ShiftViewsHolderIndex(_VisibleItems[vhIndex], count, true, indexIfInsertingOrRemoving);
									}
									else
									{
										double insetFromEndForNextNewVH = _VisibleItems[vhEndIndexMinus1].root.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge);

										// Drag towards start the ones before the new items
										DragVisibleItemsRangeUnchecked(0, vhEndIndex, -deltaSize, false, false);

										//for (vhIndex = 0; vhIndex < _VisibleItemsCount; ++vhIndex)
										//{
										//	vh = _VisibleItems[vhIndex];
										//	if (vh.ItemIndex < indexIfInsertingOrRemoving) 
										//		continue;
										//	ShiftViewsHolderIndex(vh, count, true, indexIfInsertingOrRemoving);
										//}

										// Find the next before viewport (to be recycled, and all vhs before it)
										double vhInsetFromEnd;
										int idxOfFirstVHToRecycle;
										for (idxOfFirstVHToRecycle = vhEndIndexMinus1; idxOfFirstVHToRecycle >= 0; --idxOfFirstVHToRecycle)
										{
											vh = _VisibleItems[idxOfFirstVHToRecycle];
											vhInsetFromEnd = vh.root.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge);

											if (vhInsetFromEnd > _InternalState.vpSize)
												break;
										}

										if (idxOfFirstVHToRecycle >= 0) // at least 1 item to recycle that went before VP (otherwise, this would be -1)
											vhEndIndex -= idxOfFirstVHToRecycle + 1; // since the vhs from the beginning will be recycled, their position changes in the _VisibleItems array

										// Recycle all items that now are before viewport
										while (idxOfFirstVHToRecycle >= 0)
											RecycleOrStealViewsHolder(idxOfFirstVHToRecycle--, stealInsteadOfRecycle);

										// Extract from the recycler or create new items, until the viewport is filled (not necesarily <count> new items will be shown)
										int indexInViewOfFirstItemToBeInserted = indexInViewIfInsertingOrRemoving - 1 + count;
										double sizeAddedToContent = AddViewsHoldersAndMakeVisible(insetFromEndForNextNewVH, _InternalState.endEdge, vhEndIndex, indexInViewOfFirstItemToBeInserted, count, 0, -1);

										// The content needs to be shifted towards start with the same amout it grew, so its end edge will be stationary
										//if (true || _InternalState.computeVisibilityTwinPassScheduled)
										//{
											if (ctInsetFromVPSOverrideToPassAsParam == null)
												ctInsetFromVPSOverrideToPassAsParam = _InternalState.ctVirtualInsetFromVPS_Cached - sizeAddedToContent;
											else
												ctInsetFromVPSOverrideToPassAsParam -= sizeAddedToContent;
											allowOutsideBounds = true;
										//}
									}
								}
							}
						}
						//// possible looping
						else
						{
							if (emptyAreaWhenCTSmallerThanVP > 0)
							{
								ctInsetFromVPSOverrideToPassAsParam = _InternalState.ctVirtualInsetFromVPS_Cached;
								allowOutsideBounds = true;
							}

							if (_VisibleItemsCount > 0)
							{
								int vhStartIndex;
								vhStartIndex = vhIndexForInsertOrRemove;

								//// Shift items having their ItemIndex >= insertIndex
								//for (vhIndex = 0; vhIndex < _VisibleItemsCount; ++vhIndex)
								//{
								//	vh = _VisibleItems[vhIndex];
								//	if (vh.ItemIndex < indexIfInsertingOrRemoving)
								//		continue;
								//	ShiftViewsHolderIndex(vh, count, true, indexIfInsertingOrRemoving);
								//}

								if (vhStartIndex < _VisibleItemsCount)
								{
									if (vhStartIndex < 0)
									{
										// The first item will be inserted before the first VH

										reportedScrollDeltaOverride = .1d;
										//if (loopItems) 
										//{
										//	// Keeping items stationary when looping. Their indexInView is also preserved at the begining of this switch case
										//}
										//else
										//{
											// The first inserted item may not become visible => shift the existing items towards end and the ComputeVisibility will fill the gaps that'll form at start

											DragVisibleItemsRangeUnchecked(0, _VisibleItemsCount, deltaSize, false, false);
										//}

										//for (vhIndex = 0; vhIndex < _VisibleItemsCount; ++vhIndex)
										//{
										//	vh = _VisibleItems[vhIndex];
										//	if (loopItems && vh.ItemIndex < indexIfInsertingOrRemoving)
										//		continue;
										//	ShiftViewsHolderIndex(vh, count, true, indexIfInsertingOrRemoving);
										//}
									}
									else
									{
										//if (loopItems && vhStartIndex == 0 && indexIfInsertingOrRemoving == oldCount)
										//{

										//}
										//else
										//{
											//if (loopItems)
											//{
											//	for (vhIndex = 0; vhIndex < vhStartIndex; ++vhIndex)
											//	{
											//		vh = _VisibleItems[vhIndex];
											//		if (vh.ItemIndex < indexIfInsertingOrRemoving)
											//			continue;
											//		ShiftViewsHolderIndex(vh, count, true, indexIfInsertingOrRemoving);
											//	}
											//}

											double insetFromStartForNextNewVH = _VisibleItems[vhStartIndex].root.GetInsetFromParentEdge(_Params.Content, _InternalState.startEdge);
											DragVisibleItemsRangeUnchecked(vhStartIndex, _VisibleItemsCount, deltaSize, false, false);
											// Find the next after viewport (to be recycled, and all vhs after it) 
											// (update: this is now done for all cases at once at the begining)while also shifting indices of the ones that will remain visible
											double vhInsetFromStart;
											for (vhIndex = vhStartIndex; vhIndex < _VisibleItemsCount; ++vhIndex)
											{
												vh = _VisibleItems[vhIndex];
												vhInsetFromStart = vh.root.GetInsetFromParentEdge(_Params.Content, _InternalState.startEdge);

												if (vhInsetFromStart > _InternalState.vpSize)
													break; // the current and all after it will be recycled.

												//// If looping and found an item with ItemIndex<insertIndex, we've eached the "head", and all the following items will have ItemIndex smaller than insertIndex
												//if (loopItems && vh.ItemIndex < indexIfInsertingOrRemoving)
												//	continue;

												//ShiftViewsHolderIndex(vh, count, true, indexIfInsertingOrRemoving);
											}

											// Include all items after viewport into the recycle bin
											while (vhIndex < _VisibleItemsCount)
												RecycleOrStealViewsHolder(vhIndex, stealInsteadOfRecycle);

											// Extract from the recycler or create new items, until the viewport is filled (not necesarily <count> new items will be shown)
											AddViewsHoldersAndMakeVisible(insetFromStartForNextNewVH, _InternalState.startEdge, vhStartIndex, indexInViewIfInsertingOrRemoving, count, 1, 1);
										//}
									}
								}
								else
								{
									// All items to be added are after vp

									//// Visible items may have indices bigger than insertIndex, if looping
									//if (loopItems)
									//{
									//	for (vhIndex = 0; vhIndex < _VisibleItemsCount; ++vhIndex)
									//	{
									//		vh = _VisibleItems[vhIndex];
									//		if (vh.ItemIndex < indexIfInsertingOrRemoving)
									//			continue;
									//		ShiftViewsHolderIndex(vh, count, true, indexIfInsertingOrRemoving);
									//	}
									//}

									reportedScrollDeltaOverride = -.1d;
								}
							}
						}
					}
					break;

				// IMGDOC <!image url="$(SolutionDir)\frame8\Docs\img\OSA\Insert-Remove-Items-Remove.jpg" scale=".81"/>
				case ItemCountChangeMode.REMOVE:
					{
						//if (emptyAreaWhenCTSmallerThanVP > 0)
						//{
						//	ctInsetFromVPSOverrideToPassAsParam = _InternalState.ctVirtualInsetFromVPS_Cached;
						//	allowOutsideBounds = true;
						//}

						//allowOutsideBounds = true;
						if (_VisibleItemsCount > 0)
						{
							int startVHIndex = vhIndexForInsertOrRemove;
							int endVHIndexExcl = startVHIndex + count;
							int endVHIndex = endVHIndexExcl - 1;
							int vhsToRemove = 0; // guaranteed to be >= 0
							int itemsOutsideVPToBeRemoved = 0;
							int vhStationaryStartIndex, vhStationaryEndIndexExcl; // stationary in the sense that they don't get moved by deltaSize, only by the correction amount

							//// No looping
							if (contentPanelEndEdgeStationary)
							{
								if (emptyAreaWhenCTSmallerThanVP > 0)
								{
									ctInsetFromVPSOverrideToPassAsParam = _InternalState.ctVirtualInsetFromVPS_Cached - deltaSize + additionalCTDragAbstrDelta;
									allowOutsideBounds = true;
								}

								//int endVHIndexClamped; // last to be removed
								int startVHIndexClamped;
								vhStationaryEndIndexExcl = _VisibleItemsCount;

								if (endVHIndexExcl > _VisibleItemsCount) // some are after vp
								{
									if (startVHIndex < 0) // the rest are some inside + some before vp => all vhs will be recycled =>  treat is as the RESET case
										goto case ItemCountChangeMode.RESET;

									reportedScrollDeltaOverride = .1d;

									if (startVHIndex >= _VisibleItemsCount) // all are after vp
									{
										startVHIndexClamped = _VisibleItemsCount;
										vhsToRemove = 0;
									}
									else // the rest are inside
									{
										startVHIndexClamped = startVHIndex;
										vhsToRemove = _VisibleItemsCount - startVHIndexClamped;

										correctionMayBeNeeded = true;
									}
								}
								else // none are after vp
								{
									if (startVHIndex < 0) // some of items are before vp
									{
										startVHIndexClamped = 0;
										if (endVHIndex < 0) // all are before vp
										{
											vhsToRemove = 0;
										}
										else // .. and some are inside vp
										{
											vhsToRemove = endVHIndexExcl;

											reportedScrollDeltaOverride = .1d;
											correctionMayBeNeeded = true;
										}
									}
									else // all items are inside
									{
										startVHIndexClamped = startVHIndex;
										vhsToRemove = endVHIndexExcl - startVHIndexClamped;

										reportedScrollDeltaOverride = .1d;
										correctionMayBeNeeded = true;
									}
								}
								//endVHIndexClamped = startVHIndexClamped + vhsToRemove - 1;

								// Recycle the removed items 
								// Note: after this, <startVHIndexClamped> will be the index of the first item after the removed ones
								while (vhsToRemove-- > 0)
									RecycleOrStealViewsHolder(startVHIndexClamped, stealInsteadOfRecycle);

								// Drag the ones before the removed items
								DragVisibleItemsRangeUnchecked(0, startVHIndexClamped, -deltaSize + additionalCTDragAbstrDelta, false, false);

								// Drag any 'stationary' items (i.e. not affected by sizeDelta change - the items after the ones removed) to account for the correction, if any, + shift their indices
								if (startVHIndexClamped < _VisibleItemsCount)
								{
									if (additionalCTDragAbstrDelta != 0d)
										DragVisibleItemsRangeUnchecked(startVHIndexClamped, _VisibleItemsCount, additionalCTDragAbstrDelta, false, false);

									//// TODO remove this check temporarily, until removing with endStat while looping will be implemented
									//if (!loopItems) // looping is handled for the whole REMOVE case, at the very end of the switch
									//{
										for (int i = startVHIndexClamped; i < _VisibleItemsCount; ++i)
										{
											vh = _VisibleItems[i];
											ShiftViewsHolderItemIndexInView(vh, -count);
											ShiftViewsHolderItemIndexAndFireEvent(vh, -count, false, indexIfInsertingOrRemoving);
										}
									//}
								}
							}
							//// Possible looping
							else
							{
								if (emptyAreaWhenCTSmallerThanVP > 0d)
								{
									ctInsetFromVPSOverrideToPassAsParam = _InternalState.ctVirtualInsetFromVPS_Cached + additionalCTDragAbstrDelta;
									allowOutsideBounds = true;
								}

								int startVHIndexClamped; // first to be removed
								vhStationaryStartIndex = 0;

								bool atLeastOneItemsToBeRemovedIsBeforeViewport = startVHIndex < 0;
								if (atLeastOneItemsToBeRemovedIsBeforeViewport) // some items are before vp
								{
									startVHIndexClamped = 0;
									if (endVHIndex < 0) // all are before vp
										itemsOutsideVPToBeRemoved = count;
									else
									{
										if (endVHIndex < _VisibleItemsCount) // the rest are inside vp
										{
											itemsOutsideVPToBeRemoved = -startVHIndex;
											correctionMayBeNeeded = true;
										}
										else // the rest are some inside + some after vp => all vhs will be recycled =>  treat is as the RESET case
											goto case ItemCountChangeMode.RESET;
									}

									reportedScrollDeltaOverride = -.1d;
								}
								else // none are before vp
								{
									if (startVHIndex < _VisibleItemsCount) // some are inside vp
									{
										startVHIndexClamped = startVHIndex;
										if (endVHIndexExcl > _VisibleItemsCount) // .. and some are after vp
											itemsOutsideVPToBeRemoved = endVHIndexExcl - _VisibleItemsCount;
										else // are all inside
										{

										}

										reportedScrollDeltaOverride = -.1d;
										correctionMayBeNeeded = true;
									}
									else // all are after vp
									{
										itemsOutsideVPToBeRemoved = count;
										startVHIndexClamped = _VisibleItemsCount; // no vh will be removed
									}
								}
								vhStationaryEndIndexExcl = startVHIndexClamped;

								// Add the removed visible vhs to recycle bin
								vhsToRemove = count - itemsOutsideVPToBeRemoved;
								while (vhsToRemove-- > 0)
									RecycleOrStealViewsHolder(startVHIndexClamped, stealInsteadOfRecycle);

								// Drag the stationary items by the correction amount
								if (additionalCTDragAbstrDelta != 0d)
									DragVisibleItemsRangeUnchecked(vhStationaryStartIndex, vhStationaryEndIndexExcl, additionalCTDragAbstrDelta, false, false);

								// Drag the items following the removed ones by the size delta + the correction amount, if any, & shift their indices
								// The one after the last vh to be removed will have the same vhIndex as the first removed
								if (startVHIndexClamped < _VisibleItemsCount)
								{
									DragVisibleItemsRangeUnchecked(startVHIndexClamped, _VisibleItemsCount, deltaSize + additionalCTDragAbstrDelta, false, false);

									//if (!loopItems) // looping is handled for the whole REMOVE case, at the very end of the switch
									//{
										for (int i = startVHIndexClamped; i < _VisibleItemsCount; i++)
										{
											vh = _VisibleItems[i];
											ShiftViewsHolderItemIndexInView(vh, -count);
											ShiftViewsHolderItemIndexAndFireEvent(vh, -count, false, indexIfInsertingOrRemoving);
										}
									//}
								}
							}

							//if (loopItems)
							//{
							//	// Decrementing bigger indices, if present
							//	// Keeping indexInView for items before the removal indexInView
							//	for (int i = 0; i < _VisibleItemsCount; i++)
							//	{
							//		vh = _VisibleItems[i];
							//		if (vh.itemIndexInView > indexInViewIfInsertingOrRemoving)
							//			ShiftViewsHolderItemIndexInView(vh, -count);

							//		if (vh.ItemIndex < indexIfInsertingOrRemoving)
							//			continue;

							//		ShiftViewsHolderItemIndexAndFireEvent(vh, -count, false, indexIfInsertingOrRemoving);
							//	}

							//	//correctionMayBeNeeded = true;
							//}
						}
					}
					break;

				case ItemCountChangeMode.RESET:
					recycleAllViewsHolders = true;
					if (contentPanelEndEdgeStationary)
						ctInsetFromVPSOverrideToPassAsParam = _InternalState.ctVirtualInsetFromVPS_Cached - deltaSize + additionalCTDragAbstrDelta;
					else
						ctInsetFromVPSOverrideToPassAsParam = _InternalState.ctVirtualInsetFromVPS_Cached + additionalCTDragAbstrDelta;
					break;
			}

#if DEBUG_CHANGE_COUNT
			double ctInsetEndBefOnSizeChange = _InternalState.CTVirtualInsetFromVPE_Cached;
#endif

#if DEBUG_INDICES
			if (GetDebugIndicesString(out debugIndicesString))
				Debug.Log("ChangeCountAft vhs " + _VisibleItemsCount + (recycleAllViewsHolders ? "(allWillBeRecycled)" : "") + ". Indices: " + debugIndicesString);
#endif

			var p = new ContentSizeOrPositionChangeParams
			{
				keepVelocity = keepVelocity,
				contentEndEdgeStationary = contentPanelEndEdgeStationary,
				contentInsetOverride = ctInsetFromVPSOverrideToPassAsParam,
				allowOutsideBounds = allowOutsideBounds,
				// Commented: this is done by ComputeVisibility below
				//fireScrollPositionChangedEvent = true
			};
			OnCumulatedSizesOfAllItemsChanged(ref p);


			// If the itemsCount is 0, then it makes sense to destroy all the views, instead of marking them as recyclable. Maybe the ChangeItemCountTo(0) was called in order to clear the current contents
			if (newCount == 0)
			{
				ClearVisibleItems();
				ClearCachedRecyclableItems();
			}
			else if (recycleAllViewsHolders)
				RecycleAllVisibleViewsHolders();

			// TODO check this, the same way as for when changing item's size
			double reportedScrollDelta;
			if (reportedScrollDeltaOverride != null)
			{
				reportedScrollDelta = reportedScrollDeltaOverride.Value;
			}
			else
			{
				if (prevCount == 0)
					reportedScrollDelta = 0d; // helps with the initial displacement of the content when using CSF and preferEndEdge=false
				else if (contentPanelEndEdgeStationary)
					reportedScrollDelta = .1d;
				else
				{
					// If start edge is stationary, either if the content shrinks or expands the reportedDelta should be negative, 
					// indicating that a fake "slight scroll towards end" was done. This triggers a virtualization of the the content's position correctly to compensate for the new ctEnd 
					// and makes any item after it be visible again (in the shirnking case) if it was after viewport
					reportedScrollDelta = -.1d;

					// ..but if the ctEnd is fully visible, the content will act as it was shrinking with itemEndEdgeStationary=true, because the content's end can't go before vpEnd
					if (vrtContentPanelIsAtOrBeforeEnd)
						reportedScrollDelta = .1d;
				}
			}

			// Update 18.02.2019: added "emptyAreaWhenCTSmallerThanVP_After >= 0" because when this is true, additionalCTDragAbstrDelta will be 0, which
			// caused some items to disappear if the suddenly a great number of items were removed from start from index 1 or after (For example, having 128 items, and using RemoveItems(1, 125), 
			// would not compute visibility as needed)
			double emptyAreaWhenCTSmallerThanVP_After = -_InternalState.VirtualScrollableArea;
			bool computeBothWays = correctionMayBeNeeded && (additionalCTDragAbstrDelta != 0d || emptyAreaWhenCTSmallerThanVP_After >= 0d);

			if (_InternalState.computeVisibilityTwinPassScheduled)
			{
				//if(_Params.effects.LoopItems && changeMode != ItemCountChangeMode.RESET)
				//{
				//	throw new OSAException(
				//		"OSA.ChangeItemCountInternal: Looping is enabled and twin pass scheduled (you're probably using ContentSizeFitter or similar), but changeMode is " + changeMode +
				//		". In this particular case, only ResetItems can be used to change the count"
				//		);
				//}

				_InternalState.computeVisibilityTwinPassScheduled = false;
				bool preferEndStat = _InternalState.preferKeepingCTEndEdgeStationaryInNextComputeVisibilityTwinPass;

#if DEBUG_CHANGE_COUNT
				double ctInsetBeforeTwinPass = _InternalState.ctVirtualInsetFromVPS_Cached;
				//double ctSizeBeforeTwinPass = _InternalState.ctVirtualSize;
				string str =
					"ctInsetEndBefOnSizeChange " + ctInsetEndBefOnSizeChange.ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
					", ctInsetFromVPSOverrideToPassAsParam " + ctInsetFromVPSOverrideToPassAsParam +
					", allowOutsideBounds " + allowOutsideBounds +
					", lastVHInsetEnd " + _VisibleItems[_VisibleItemsCount - 1].root.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge).ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
					", ctInsetEnd " + _InternalState.CTVirtualInsetFromVPE_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
#endif
				ComputeVisibilityTwinPass(preferEndStat);

#if DEBUG_CHANGE_COUNT
				double ctInsetDeltaFromTwinPass = _InternalState.ctVirtualInsetFromVPS_Cached - ctInsetBeforeTwinPass;
				double ctSizeDeltaFromTwinPass = _InternalState.ctVirtualSize;
				str += ", lastVHInsetEnd aft " + _VisibleItems[_VisibleItemsCount - 1].root.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge).ToString(OSAConst.DEBUG_FLOAT_FORMAT);
				str += ", ctInsetEnd aft " + _InternalState.CTVirtualInsetFromVPE_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
				str +=
					"\n(ctInsetDelta " + ctInsetDeltaFromTwinPass.ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
					", ctSizeDelta " + ctSizeDeltaFromTwinPass.ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
					"), reportedDelta " + reportedScrollDelta.ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
					", computeBothWays " + computeBothWays;

				Debug.Log(str);
#endif
			}
			else
				_InternalState.lastComputeVisibilityHadATwinPass = false;

			//Debug.Log("correctionMayBeNeeded " + correctionMayBeNeeded +
			//	", computeBothWays " + computeBothWays +
			//	", ctDelta " + additionalCTDragAbstrDelta +
			//	", reportedScrollDelta " + reportedScrollDelta
			//	);

			// Bugfix items displaced when cutting huge amounts from content, bringing it smaller than vp, and gravity != start
			if (correctionMayBeNeeded)
				CorrectPositionsOfVisibleItems(true, false);
			//CorrectPositionsOfVisibleItemsUsingDefaultSizeRetrievingMethod(true);

			ComputeVisibilityForCurrentPositionRawParams(false, true, reportedScrollDelta);
			if (computeBothWays)
				ComputeVisibilityForCurrentPositionRawParams(false, true, -reportedScrollDelta);
			//Debug.Log(str);

			// Correcting & firing PosChanged event
			CorrectPositionsOfVisibleItems(true, true);

			//if (changeMode == ItemCountChangeMode.INSERT || changeMode == ItemCountChangeMode.REMOVE)
			//	CorrectPositionsOfVisibleItemsUsingDefaultSizeRetrievingMethod(true);

			if (keepVelocity)
				_Velocity = velocity;

			OnItemsRefreshed(prevCount, newCount);
			if (ItemsRefreshed != null)
				ItemsRefreshed(prevCount, newCount);

			_SkipComputeVisibilityInUpdateOrOnScroll = ignoreOnScroll_valueBefore;
		}

		/// <summary>Called by MonoBehaviour.Update</summary>
		void MyUpdate()
		{
			if (_InternalState.computeVisibilityTwinPassScheduled)
				throw new OSAException(OSAConst.EXCEPTION_SCHEDULE_TWIN_PASS_CALL_ALLOWANCE);

			bool scrollviewSizeChanged = _InternalState.HasScrollViewSizeChanged;
			if (scrollviewSizeChanged)
			{
				//_InternalState.layoutIsBeingRebuildDueToScrollViewSizeChangeEvent = true;
				OnScrollViewSizeChangedBase(); 
				RebuildLayoutDueToScrollViewSizeChange();
				Refresh(false, true);
				PostRebuildLayoutDueToScrollViewSizeChange();
				//ChangeItemsCount(ItemCountChangeMode.RESET, GetItemsCount(), -1, false, true); // keeping velocity
				return;
				//_InternalState.updateRequestPending = true;
			}

			////bool startSnappingIfNeeded = !IsDragging && !_SkipComputeVisibilityInUpdateOrOnScroll && _Params.Snapper;
			//if (_InternalState.updateRequestPending)
			//{
			//	// TODO See if need to skip modifying updateRequestPending if _SkipComputeVisibility is true

			//	// ON_SCROLL is the only case when we don't regularly update and are using only onScroll event to ComputeVisibility
			//	_InternalState.updateRequestPending = _Params.optimization.updateMode != BaseParams.UpdateModeEnum.ON_SCROLL;
			//	if (!_SkipComputeVisibilityInUpdateOrOnScroll)
			//	{
			//		ComputeVisibilityForCurrentPosition(false, false);

			//		//startSnappingIfNeeded = _Params.Snapper != null;
			//		//if (_Params.Snapper)// && !scrollviewSizeChanged)
			//		//	_Params.Snapper.StartSnappingIfNeeded();
			//	}
			//}
			////if (startSnappingIfNeeded)
			////	_Params.Snapper.StartSnappingIfNeeded();
		}


#if DEBUG_UPDATE
		string prev_UpdateDebugString;
#endif

#if DEBUG_INDICES
		string prev_IndicesDebugString;

		bool GetDebugIndicesString(out string debugIndicesString)
		{
			debugIndicesString = 
					//"ctSize " + _InternalState.ctVirtualSize.ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
					"RFirst " + _ItemsDesc.realIndexOfFirstItemInView +
					//", cumuSizeAll " + _ItemsDesc.CumulatedSizeOfAllItems.ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
					"; " + _ItemsDesc.itemsCount + " items: ";

			var phantomVHS = new List<TItemViewsHolder>(_VisibleItems);
			for (int i = 0; i < Math.Min(20, _ItemsDesc.itemsCount); i++)
			{
				bool vis = false;
				//int visIdx = -1;
				int selfR = -1;
				for (int j = 0; j < _VisibleItemsCount; j++)
				{
					var vh = _VisibleItems[j];
					if (vh.itemIndexInView == i)
					{
						phantomVHS.Remove(vh);
						selfR = vh.ItemIndex;
						vis = true;
						//visIdx = j;
						break;
					}
				}

				int r = _ItemsDesc.GetItemRealIndexFromViewIndex(i);
				debugIndicesString += (vis ? "<b>" : "") + i + "R" + r + 
					(vis ? (/*"V"+ visIdx +*/ (selfR == r ? "" : "<color=red>SR</color>" + selfR) + "</b>, ") : ", "
				);
				// + "insetV " + _InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(i).ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
				//", insetR " + _InternalState.GetItemInferredRealInsetFromParentStart(i).ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
				//", size " + _ItemsDesc[i].ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
				//", cumuSize " + _ItemsDesc.GetItemSizeCumulative(i).ToString(OSAConst.DEBUG_FLOAT_FORMAT);
			}
			if (phantomVHS.Count > 0)
			{
				debugIndicesString += "|| PhantomVHs: ";
				for (int i = 0; i < phantomVHS.Count; i++)
					debugIndicesString += phantomVHS[i] + ", ";
			}

			if (debugIndicesString == prev_IndicesDebugString)
			{
				debugIndicesString = null;
				return false;
			}
			prev_IndicesDebugString = debugIndicesString;
			return true;
		}
#endif

#if DEBUG_CONTENT_VISUALLY
		RectTransform _ContentPanelVisualization;
#endif

		void MyLateUpdate()
		{
			bool releasingFromOutsideBoundsPull_wasInProgress = _ReleaseFromPull.inProgress;
			double vsa = _InternalState.VirtualScrollableArea;
			double emptyAreaWhenCTSmallerThanVP = -vsa;
			bool ctSmallerThanVP = emptyAreaWhenCTSmallerThanVP > 0d;
			double emptyAreaWhenCTSmallerThanVPClamped = Math.Max(0d, emptyAreaWhenCTSmallerThanVP);

			var snapper = _Params.Snapper;
			bool startSnappingIfNeeded = !ctSmallerThanVP && !releasingFromOutsideBoundsPull_wasInProgress && !IsDragging && !_SkipComputeVisibilityInUpdateOrOnScroll && snapper;
			bool isSnapping = false;
			if (startSnappingIfNeeded)
			{
				snapper.StartSnappingIfNeeded();
				isSnapping = snapper.SnappingInProgress;
			}

			UpdateGalleryEffectIfNeeded(true);

			float dt = DeltaTime;
			bool canChangeVelocity = true;
			float velocity = _Velocity[_InternalState.hor0_vert1];
			var allowOutsideBoundsMode = AllowContentOutsideBoundsMode.DO_NOT_ALLOW;
			bool dragUnchecked = false;
			// TODO think if it eases the looping or not to clamp using the last item's inset instead of the content's. 

#if DEBUG_UPDATE
			string debugString = null;
			if (debug_Update)
			{
				debugString = 
					"vNormPos " + GetVirtualAbstractNormalizedScrollPosition() +
					"vsa " + vsa +
					", ctSize " + _InternalState.ctVirtualSize.ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
					", ctInsetCached " + _InternalState.ctVirtualInsetFromVPS_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
					", realIndexOfFirst " + _ItemsDesc.realIndexOfFirstItemInView +
					", cumuSizeAll " + _ItemsDesc.CumulatedSizeOfAllItems.ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
					", " + _ItemsDesc.itemsCount + " items: ";

				for (int i = 0; i < Math.Min(20, _ItemsDesc.itemsCount); i++)
				{
					debugString +=
						"\n" + i + "(R" + _ItemsDesc.GetItemRealIndexFromViewIndex(i) +
						"): insetV " + _InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(i).ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
						", insetR " + _InternalState.GetItemInferredRealInsetFromParentStart(i).ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
						", size " + _ItemsDesc[i].ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
						", cumuSize " + _ItemsDesc.GetItemSizeCumulative(i).ToString(OSAConst.DEBUG_FLOAT_FORMAT);
				}

				debugString += "\n-- "+ _VisibleItemsCount + " vhs: ";
				if (_VisibleItemsCount > 0)
				{
					for (int i = 0; i < Math.Min(10, _VisibleItemsCount); i++)
					{
						var itemIndexInView = _VisibleItems[i].itemIndexInView;
						debugString += 
							"\n" + itemIndexInView + "(R" + _ItemsDesc.GetItemRealIndexFromViewIndex(itemIndexInView) + 
							"): insetV " + _InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(itemIndexInView).ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
							", insetR " + _InternalState.GetItemInferredRealInsetFromParentStart(itemIndexInView).ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
							", size " + _ItemsDesc[itemIndexInView].ToString(OSAConst.DEBUG_FLOAT_FORMAT);
					}
				}
			}
#endif

#if DEBUG_INDICES
			string debugIndicesString = null;
			if (debug_Indices)
			{
				if (GetDebugIndicesString(out debugIndicesString))
					Debug.Log(debugIndicesString);
			}
#endif

			if (ctSmallerThanVP)
			{
				_ReleaseFromPull.targetCTInsetFromVPS = _InternalState.GetTargetCTVirtualInsetFromVPSWhenCTSmallerThanVP(emptyAreaWhenCTSmallerThanVPClamped);

				if (_IsDragging)
				{
					_ReleaseFromPull.inProgress = false;
					_Velocity[_InternalState.hor0_vert1] = 0f;
				}
				else
				{
					if (_VisibleItemsCount > 0)
					{
						dragUnchecked = true;
						//var firstVH = _VisibleItems[0];
						//float firstItemInsetFromVPS = _VisibleItems[0].root.GetInsetFromParentEdge(Parameters.content, _InternalState.startEdge);
						double firstItemInsetFromVPS = _ReleaseFromPull.CalculateFirstItemInsetFromVPS();
						double firstItemTargetInsetFromVPS = _ReleaseFromPull.CalculateFirstItemTargetInsetFromVPS();
						_ReleaseFromPull.inProgress = Math.Abs(firstItemInsetFromVPS - firstItemTargetInsetFromVPS) >= 1d;

						if (_Params.effects.ElasticMovement)
						{
							float velocityAbstr = velocity * _InternalState.hor1_vertMinus1;

							/*float nextCTInsetF = */Mathf.SmoothDamp((float)firstItemInsetFromVPS, (float)firstItemTargetInsetFromVPS, ref velocityAbstr, _Params.effects.ReleaseTime, float.PositiveInfinity, dt);
							//Debug.Log(velocity);
							// End if the drag distance would be close to zero
							if (_ReleaseFromPull.inProgress)
								velocity = velocityAbstr * _InternalState.hor1_vertMinus1;
							else
								velocity = 0f;
						}
						else
						{
							if (_ReleaseFromPull.inProgress)
								_ReleaseFromPull.FinishNowByDraggingItems(
									true // bugfix for disappearing items that are outside vp on pointer up
								);
							velocity = 0f;
						}
					}
					else
						_ReleaseFromPull.inProgress = false;

					_Velocity[_InternalState.hor0_vert1] = velocity;
				}
			}
			else
			{
				if (_IsDragging || isSnapping)
					_ReleaseFromPull.inProgress = false;
				else
				{
					double currentInset = _InternalState.ctVirtualInsetFromVPS_Cached;
					double absDisplacement;
					bool displacedFromStart = currentInset > 0d;
					if (displacedFromStart)
					{
						absDisplacement = currentInset;
						_ReleaseFromPull.targetCTInsetFromVPS = 0d;
					}
					else
					{
						double currentInsetEnd = _InternalState.CTVirtualInsetFromVPE_Cached;
						bool displacedFromEnd = currentInsetEnd > 0d;
						if (displacedFromEnd)
						{
							absDisplacement = currentInsetEnd;
							_ReleaseFromPull.targetCTInsetFromVPS = -vsa;
						}
						else
							absDisplacement = 0d;
					}
					bool displacementExists = absDisplacement > 0d;

					bool clampManually = false;
					_ReleaseFromPull.inProgress = displacementExists;
					if (_Params.effects.ElasticMovement)
					{
						bool zeroVelocity = false;
						if (_ReleaseFromPull.inProgress)
						{
							allowOutsideBoundsMode = AllowContentOutsideBoundsMode.ALLOW_IF_OUTSIDE_AMOUNT_SHRINKS;
							canChangeVelocity = false;

							// If statement commented: This wasn't necessary in testing (it also cut the release-from-pull animation),
							// but can be uncommented back if future bugs are found
							//if (_VisibleItemsCount > 0)
							//{
								double pullDistanceF = absDisplacement > float.MaxValue ? float.MaxValue : absDisplacement;
								double smoothDampCurrentValueToGive;
								if (displacedFromStart)
								{
									// Exemplifying the horizontal with pull from start: the content needs to be shifted to the left => velocity decrease => smoothDampCurrentValueToGive is 
									// set as positive in order to obtain a negative velocity with SmoothDamp (from positive to 0 you need a negative velocity)
									smoothDampCurrentValueToGive = pullDistanceF * _InternalState.hor1_vertMinus1;
								}
								else
								{
									smoothDampCurrentValueToGive = -pullDistanceF * _InternalState.hor1_vertMinus1;
								}

								float nextPullDistanceF = Mathf.SmoothDamp((float)smoothDampCurrentValueToGive, 0f, ref velocity, _Params.effects.ReleaseTime, float.PositiveInfinity, dt);

								// Clamp to zero inset start or end if the distance is close to zero
								_ReleaseFromPull.inProgress = Mathf.Abs(nextPullDistanceF) >= 1f;
								if (_ReleaseFromPull.inProgress)
									_Velocity[_InternalState.hor0_vert1] = velocity;
								else
									clampManually = zeroVelocity = true;
							//}
							//else
							//{
							//	clampManually = zeroVelocity = true;
							//}
						}
						else
						{
							zeroVelocity = releasingFromOutsideBoundsPull_wasInProgress;
							// In case the applied velocity made the pull distance too negative (ideally, it'll be 0)
							clampManually = releasingFromOutsideBoundsPull_wasInProgress && absDisplacement < .1d;
							canChangeVelocity = !clampManually;
							if (!clampManually)
								allowOutsideBoundsMode = AllowContentOutsideBoundsMode.ALLOW;
						}

						if (zeroVelocity)
							_Velocity[_InternalState.hor0_vert1] = velocity = 0f;
					}
					else if (_Params.effects.LoopItems)
					{
						allowOutsideBoundsMode = AllowContentOutsideBoundsMode.ALLOW;
						if (displacementExists)
						{
							// All items are visible and they're scrollable (the ct size bigger than vp => don't clamp them)
							if (_VisibleItemsCount == _ItemsDesc.itemsCount && vsa > 0d)
							{

							}
							else
								//// Bugfix: on fast scrolling and/or on low-framerate, sometimes all vhs go outside the vp, 
								//// so even if looping, the content needs to be clamped & computevisibility needs to correct the positions
								//if (_VisibleItemsCount == 0 && _ItemsDesc.itemsCount > 0)
								clampManually = true;
						}
					}
					else
					{
						if (displacementExists)
							clampManually = true;
					}

					if (clampManually)
					{
						//canDrag = false; // no dragging
						_ReleaseFromPull.FinishNowBySettingContentInset(true);
					}
				}
			}

			if (_Params.effects.Inertia && !isSnapping)
			{
				float velocityFactor = Mathf.Pow(1f - _Params.effects.InertiaDecelerationRate, dt);
				if (_IsDragging)
				{
					// The longer the drag lasts, the less previous velocity will be added up to the curent on drag end
					_VelocityToAddOnDragEnd *= velocityFactor;

					var magVelocityToAdd = _VelocityToAddOnDragEnd.magnitude;
					if (magVelocityToAdd < 1f)
						_VelocityToAddOnDragEnd = Vector2.zero;
					else
					{
						var magVelocityToAddToMaxVelocity = magVelocityToAdd / _Params.effects.MaxSpeed;
						if (magVelocityToAddToMaxVelocity > 1f)
							_VelocityToAddOnDragEnd /= magVelocityToAddToMaxVelocity;
					}
				}
				else if (canChangeVelocity)
				{
					if (Mathf.Abs(velocity) < 2f)
					{
						_Velocity[_InternalState.hor0_vert1] = 0f;

						// The content's speed decreases with each second, according to inertiaDecelerationRate
						int transvIdx = 1 - _InternalState.hor0_vert1;
						_Velocity[transvIdx] *= velocityFactor;
					}
					else
					{
						// The content's speed decreases with each second, according to inertiaDecelerationRate
						_Velocity *= velocityFactor;
					}
				}
			}
#if DEBUG_UPDATE
			if (debug_Update)
			{
				float velocityAbstr = velocity * _InternalState.hor1_vertMinus1;
				float dragPerFrame = velocityAbstr * dt;
				debugString +=
					"\n_IsDragging " + _IsDragging +
					", velocityAbstr " + velocityAbstr +
					", dragPerFrame " + dragPerFrame +
					", nvelocityToAddOnDragEnd " + _VelocityToAddOnDragEnd;
			}
#endif
			Velocity = _Velocity; // will clamp it
			velocity = _Velocity[_InternalState.hor0_vert1];

			if (!_IsDragging && !isSnapping && velocity != 0f)
			{
				float velocityAbstr = velocity * _InternalState.hor1_vertMinus1;
				double dragPerFrame = velocityAbstr * (double)dt;
				//if (Math.Abs(dragPerFrame) > .001d)
				if (Math.Abs(velocityAbstr) > .001d)
				{
#if DEBUG_UPDATE
					if (debug_Update)
					{
						debugString +=
							"\nvelocityAbstr " + velocityAbstr.ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
							", dragPerFrame " + dragPerFrame.ToString(OSAConst.DEBUG_FLOAT_FORMAT) + 
							", dragUnchecked " + dragUnchecked;
					}
#endif

					if (dragUnchecked)
					{
						DragVisibleItemsRangeUnchecked(0, _VisibleItemsCount, dragPerFrame,
							true, true); // bugfix for disappearing items that are outside vp on pointer up
					}
					else
					{
						bool _, done;
						Drag(dragPerFrame, allowOutsideBoundsMode, false, out _, out done);
					}
				}
			}

			//// Bugfix: when removing large amounts of items and gravity != start, the remaining items are displaced
			//if (releasingFromOutsideBoundsPull_wasInProgress)
			//	CorrectPositionsOfVisibleItemsUsingDefaultSizeRetrievingMethod(true);

#if DEBUG_UPDATE
			if (debug_Update && debugString != prev_UpdateDebugString)
				Debug.Log(prev_UpdateDebugString =  debugString);
#endif

#if DEBUG_CONTENT_VISUALLY
			if (debug_ContentVisually)
			{
				if (_ContentPanelVisualization == null)
				{
					_ContentPanelVisualization = new GameObject("ContentVisualization").AddComponent<RectTransform>();
					var img = _ContentPanelVisualization.gameObject.AddComponent<Image>();
					img.CrossFadeAlpha(.15f, 1f, true);
					_ContentPanelVisualization.SetParent(_Params.ScrollViewRT, false);
					_ContentPanelVisualization.SetAsFirstSibling();
				}
				else if (!_ContentPanelVisualization.gameObject.activeSelf)
					_ContentPanelVisualization.gameObject.SetActive(true);

				var ins = _InternalState.ctVirtualInsetFromVPS_Cached + 
					(_Params.Viewport == _Params.ScrollViewRT ? 
						0d
						: _Params.Viewport.GetInsetFromParentEdge(_Params.ScrollViewRT, _InternalState.startEdge)
					);
				_ContentPanelVisualization.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(
					_InternalState.startEdge,
					ins > float.MaxValue ? float.MaxValue : (float)ins,
					_InternalState.ctVirtualSize > float.MaxValue ? float.MaxValue : (float)_InternalState.ctVirtualSize
				);
			}
			else if (_ContentPanelVisualization != null && _ContentPanelVisualization.gameObject.activeSelf)
				_ContentPanelVisualization.gameObject.SetActive(false);
#endif
		}

		void MyOnDisable()
		{
			// Bugfix 11.04.2019 (thanks justtime (Unity forum)).
			// Disabling the GameObject or the script should clear the animation coroutines and other types of animations
			CancelAnimationsIfAny();

			// Bugfix: if the routine is stopped, this is not restored back. Setting it to false is the best thing we can do
			_SkipComputeVisibilityInUpdateOrOnScroll = false;
		}

		void OnScrollPositionChangedInternal()
		{
			UpdateGalleryEffectIfNeeded(false);

			var normPos = GetNormalizedPosition();
			OnScrollPositionChanged(normPos);

			if (ScrollPositionChanged != null)
				ScrollPositionChanged(normPos);
		}

		double GetDeltaForComputeVisibility() { return _InternalState.ctVirtualInsetFromVPS_Cached - _InternalState.lastProcessedCTVirtualInsetFromVPS; }

		bool ComputeVisibilityForCurrentPosition(ComputeVisibilityParams p)
		{
			if (p.overrideDelta != null)
				return ComputeVisibilityForCurrentPositionRawParams(p.forceFireScrollPositionChangedEvent, p.potentialTwinPassCTEndStationaryPrioritizeUserPreference, p.overrideDelta.Value);

			return ComputeVisibilityForCurrentPosition(p.forceFireScrollPositionChangedEvent, p.potentialTwinPassCTEndStationaryPrioritizeUserPreference);
		}

		bool ComputeVisibilityForCurrentPositionRawParams(bool forceFireScrollViewPositionChangedEvent, bool potentialTwinPassCTEndStationaryPrioritizeUserPreference, double overrideScrollingDelta)
		{
			double curInset = _InternalState.ctVirtualInsetFromVPS_Cached;
			_InternalState.lastProcessedCTVirtualInsetFromVPS = curInset - overrideScrollingDelta;
			return ComputeVisibilityForCurrentPosition(forceFireScrollViewPositionChangedEvent, potentialTwinPassCTEndStationaryPrioritizeUserPreference);
		}

		bool ComputeVisibilityForCurrentPosition(bool forceFireScrollViewPositionChangedEvent, bool potentialTwinPassCTEndStationaryPrioritizeUserPreference)
		{
			if (_InternalState.computeVisibilityTwinPassScheduled)
				throw new OSAException(OSAConst.EXCEPTION_SCHEDULE_TWIN_PASS_CALL_ALLOWANCE);

			double delta = GetDeltaForComputeVisibility();

			//if (forcePreTwinPass)
			//{
			//	ComputeVisibilityTwinPass(delta);
			//	GetDeltaForComputeVisibility();
			//}

			var velocityToSet = _Velocity;

			bool looped = false;
			if (_Params.effects.LoopItems)
				looped = LoopIfNeeded(delta);

			_ComputeVisibilityManager.ComputeVisibility(delta);

			if (_InternalState.computeVisibilityTwinPassScheduled)
			{
				_InternalState.computeVisibilityTwinPassScheduled = false;

				bool preferEndStat = _InternalState.preferKeepingCTEndEdgeStationaryInNextComputeVisibilityTwinPass;
				bool contentEndEdgeStationary = potentialTwinPassCTEndStationaryPrioritizeUserPreference || delta == 0d ? preferEndStat : delta > 0d;

#if DEBUG_COMPUTE_VISIBILITY_TWIN
				string debugString =
					"preferEndStat " + preferEndStat +
					", endEdgeStatFinal " + contentEndEdgeStationary + (preferEndStat != contentEndEdgeStationary ? "(delta " + delta.ToString(OSAConst.DEBUG_FLOAT_FORMAT) + ")" : "") +
					", ctInsetBef " + _InternalState.ctVirtualInsetFromVPS_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
				Debug.Log("|---PreTwinPass: " + debugString);
#endif
				if (delta == 0d)
					delta = -.1d;

				bool ctSizeChanged;
				int maxIterations = 20;
				int iter = 0;
				do
				{
					ComputeVisibilityTwinPass(contentEndEdgeStationary);
					ctSizeChanged = false;
					double ctSizeBef = _InternalState.ctVirtualSize;
					_ComputeVisibilityManager.ComputeVisibility(delta);
					_ComputeVisibilityManager.ComputeVisibility(-delta);
					if (_InternalState.computeVisibilityTwinPassScheduled)
					{
						ComputeVisibilityTwinPass(contentEndEdgeStationary);
						// Ignore subsequent twin pass requests for the current function call
						_InternalState.computeVisibilityTwinPassScheduled = false;
					}

					ctSizeChanged = ctSizeBef != _InternalState.ctVirtualSize;

					++iter;
					if (iter == maxIterations)
						throw new OSAException(
							"Max iterations (" + maxIterations + ") reached for TwinPass. \n" +
							"If you're using ContentSizeFitter, make sure the DefaultItemSize is smaller than the size of any generated item.\n" +
							"If you're also using BaseParamsWithPrefab for the params, DefaultItemSize will be automatically set to the prefab's size, " +
							"so in this case make the prefab as small as possible instead."
						);
				} while (ctSizeChanged);
			}
			else
				_InternalState.lastComputeVisibilityHadATwinPass = false;

			_InternalState.UpdateLastProcessedCTVirtualInsetFromVPStart();

			if (!IsDragging) // if dragging, the velocity is not needed
				_Velocity = velocityToSet;

			if (forceFireScrollViewPositionChangedEvent || delta != 0d)
				OnScrollPositionChangedInternal();

			return looped;
		}

		void ComputeVisibilityTwinPass(bool contentEndEdgeStationary)
		{
			if (_VisibleItemsCount == 0)
				throw new OSAException("computeVisibilityTwinPassScheduled, but there are no visible items." + OSAConst.EXCEPTION_SCHEDULE_TWIN_PASS_CALL_ALLOWANCE);

			int itCount = GetItemsCount();
			if (_Params.effects.LoopItems && itCount > OSAConst.MAX_ITEMS_WHILE_LOOPING_TO_ALLOW_TWIN_PASS)
				throw new OSAException(
					"If looping is enabled, ComputeVisibilityTwinPass can only be used if item count is less than " + OSAConst.MAX_ITEMS_WHILE_LOOPING_TO_ALLOW_TWIN_PASS + 
					" (currently having "+ itCount + "). This prevents UI overlaps due to rounding errors"
				);

			// Prevent onValueChanged callbacks from being processed when setting inset and size of content
			var ignoreOnScroll_valueBefore = _SkipComputeVisibilityInUpdateOrOnScroll;
			_SkipComputeVisibilityInUpdateOrOnScroll = true;

			//Canvas.ForceUpdateCanvases();

			// Caching the sizes before disabling the CSF, because Unity 2017.2 suddenly decided that's a good idea to resize the item to its original size after the CSF is disabled
			double[] sizes = new double[_VisibleItemsCount];
			TItemViewsHolder v;
			Action<TItemViewsHolder> sizeChangeCallback;

			if (_Params.IsHorizontal)
				sizeChangeCallback = OnItemWidthChangedPreTwinPass;
			else
				sizeChangeCallback = OnItemHeightChangedPreTwinPass;

#if DEBUG_COMPUTE_VISIBILITY_TWIN
			string debugString = "|---TwinPass: ";
#endif
			for (int i = 0; i < _VisibleItemsCount; ++i)
			{
				v = _VisibleItems[i];
#if DEBUG_COMPUTE_VISIBILITY_TWIN
				debugString += "\n" + i + ": " + v.root.rect.size[_InternalState.hor0_vert1].ToString(OSAConst.DEBUG_FLOAT_FORMAT) + " -> ";
#endif
				sizes[i] = UpdateItemSizeOnTwinPass(v);
#if DEBUG_COMPUTE_VISIBILITY_TWIN
				debugString += sizes[i].ToString(OSAConst.DEBUG_FLOAT_FORMAT);
#endif
				sizeChangeCallback(v);
			}

			////bool endEdgeStationary = delta > 0d;
			//bool preferEndStat = _InternalState.preferKeepingCTEndEdgeStationaryInNextComputeVisibilityTwinPass;
			////bool contentEndEdgeStationary = delta == 0d ? preferEndStat : delta > 0d;
			//bool contentEndEdgeStationary = preferEndStat;

#if DEBUG_COMPUTE_VISIBILITY_TWIN
			debugString +=
				"\ncontentEndEdgeStationary " + contentEndEdgeStationary +
				", ctInsetBef " + _InternalState.ctVirtualInsetFromVPS_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
				", ctInsetEndBef " + _InternalState.CTVirtualInsetFromVPE_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
#endif

			OnItemsSizesChangedExternally(_VisibleItems, sizes, contentEndEdgeStationary);

#if DEBUG_COMPUTE_VISIBILITY_TWIN
			debugString +=
				", ctInsetAft " + _InternalState.ctVirtualInsetFromVPS_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
				", ctInsetEndAft " + _InternalState.CTVirtualInsetFromVPE_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
			Debug.Log(debugString);
#endif

			_SkipComputeVisibilityInUpdateOrOnScroll = ignoreOnScroll_valueBefore;

			_InternalState.lastComputeVisibilityHadATwinPass = true;
		}

		/// <summary>
		/// Should only be called once, in ComputeVisibilityForCurrentPosition()!
		/// Assigns pev to the pointer event data, if a pointer was touching the scroll view before virtualizing. 
		/// Will return false if it did not try to retrieve the pev
		/// </summary>
		bool LoopIfNeeded(double delta)
		{
			if (delta == 0d)
				return false;

			double vsa = _InternalState.VirtualScrollableArea;
			if (vsa <= 0d) // nothing to loop through, since ctsize<=vpsize
				return false;

			ContentSizeOrPositionChangeParams p;
			if (_VisibleItemsCount == 0)
			{
				if (_ItemsDesc.itemsCount == 0)
					return false;

				//double ctAmountOutside = -_InternalState.ctVirtualInsetFromVPS_Cached;

				// Because of high jumps that are optimized by recycling all visible items (confirmed) or a very high speed (not confirmed, but seems similar), 
				// vhs can end up being outside the viewport. In this case, we wait for them to appear in next frames
				return false;

				//double targetCTInsetFromVPS = _InternalState.paddingContentStart;
				//p = new ContentSizeOrPositionChangeParams
				//{
				//	allowOutsideBounds = true,
				//	contentInsetOverride = targetCTInsetFromVPS,
				//	keepVelocity = true
				//};
			}
			else
			{
				p = new ContentSizeOrPositionChangeParams
				{
					allowOutsideBounds = true,
					keepVelocity = true,
					// Commented: this is done by CorrectPositions at the end of this method
					//fireScrollPositionChangedEvent = true
				};

				bool negativeScroll = delta <= 0d;
				var firstVH = _VisibleItems[0];
				var lastVH = _VisibleItems[_VisibleItemsCount - 1];
				int firstVH_IndexInView = firstVH.itemIndexInView, lastVH_IndexInView = lastVH.itemIndexInView;
				bool firstVHIsFirstInView = firstVH_IndexInView == 0;
				bool lastVHIsLastInView = lastVH_IndexInView == _ItemsDesc.itemsCount - 1;

				double firstVisibleItemAmountOutside = 0d, lastVisibleItemAmountOutside = 0d;
				int newRealIndexOfFirstItemInView;

				if (negativeScroll) // going towards end
				{
					// There are more items after the last
					if (!lastVHIsLastInView)
						return false;

					//// Commented: this blocks scrolling when there's a high speed drag
					// Only loop if there's at least 1 item that's not visible
					if (firstVHIsFirstInView)
					{
						// Even if the first vh is last in view, it may be outside the viewport completely due to a high speed drag
						firstVisibleItemAmountOutside = -firstVH.root.GetInsetFromParentEdge(_Params.Content, _InternalState.startEdge);
						if (firstVisibleItemAmountOutside <= 0d)
							return false;
					}

					// Only loop after the last item is completely inside the viewport
					lastVisibleItemAmountOutside = -lastVH.root.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge);
					if (lastVisibleItemAmountOutside > 0d)
						return false;

					newRealIndexOfFirstItemInView = firstVH.ItemIndex;
					//newRealIndexOfFirstItemInView = _InternalState.GetItemRealIndexFromViewIndex(0);

					// Adjust the itemIndexInView for the visible items. they'll be the last ones, so the last one of them will have, for example, viewIndex = itemsCount-1
					for (int i = 0; i < _VisibleItemsCount; ++i)
						_VisibleItems[i].itemIndexInView = i;
				}
				else // going towards start
				{
					// There are more items before the first
					if (!firstVHIsFirstInView)
						return false;

					// Only loop if there's at least 1 item that's entirely not visible
					if (lastVHIsLastInView)
					{
						// Even if the last vh is last in view, it may be outside the viewport completely due to a high speed drag
						lastVisibleItemAmountOutside = -lastVH.root.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge);
						if (lastVisibleItemAmountOutside <= 0d)
							return false;
					}

					// Only loop after the first item is completely inside the viewport
					firstVisibleItemAmountOutside = -firstVH.root.GetInsetFromParentEdge(_Params.Content, _InternalState.startEdge);
					if (firstVisibleItemAmountOutside > 0d)
						return false;

					// The next item after this will become the first one in view
					newRealIndexOfFirstItemInView = _ItemsDesc.GetItemRealIndexFromViewIndex(lastVH_IndexInView + 1);
					//newRealIndexOfFirstItemInView = _InternalState.GetItemRealIndexFromViewIndex(_ItemsDescriptor.itemsCount - 1);

					// Adjust the itemIndexInView for the visible items
					for (int i = 0; i < _VisibleItemsCount; ++i)
						_VisibleItems[i].itemIndexInView = _ItemsDesc.itemsCount - _VisibleItemsCount + i;
				}

				_ItemsDesc.RotateItemsSizesOnScrollViewLooped(newRealIndexOfFirstItemInView);

#if DEBUG_LOOPING
			string debugString = null;
			debugString += 
				"Looped: vhs " + _VisibleItemsCount + 
				(lastVHIsLastInView ? 
					", lastVHIsLastInView, amountOutside " + lastVisibleItemAmountOutside.ToString(OSAConst.DEBUG_FLOAT_FORMAT)  
					: ", firstVHIsFirstInView, amountOutside " + firstVisibleItemAmountOutside.ToString(OSAConst.DEBUG_FLOAT_FORMAT)) +
				", newRealIndexofFirst " + newRealIndexOfFirstItemInView +
				", ctInsetCached_Before " + _InternalState.ctVirtualInsetFromVPS_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT) +
				", cumuSizeAll " + _ItemsDesc.CumulatedSizeOfAllItems.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
#endif
			}


			UpdateCTVrtInsetFromVPS(ref p);

#if DEBUG_LOOPING
			debugString += ", ctInsetCached_After " + _InternalState.ctVirtualInsetFromVPS_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
			Debug.Log(debugString);
#endif

			// The visible items are displaced now, so correct their positions
			//CorrectPositionsOfVisibleItemsUsingDefaultSizeRetrievingMethod(true);
			CorrectPositionsOfVisibleItems(true, true);

			_InternalState.UpdateLastProcessedCTVirtualInsetFromVPStart();

			return true;
		}

		/// <summary>Don't abuse this method. See why in the description of <see cref="InternalState{TItemViewsHolder}.CorrectPositions(List{TItemViewsHolder}, bool)"/></summary>
		void CorrectPositionsOfVisibleItems(bool alsoCorrectTransversalPositioning, bool fireScrollPositionChangedEvent)//bool itemEndEdgeStationary)
		{
			// Update the positions of the visible items so they'll retain their position relative to the viewport
			if (_VisibleItemsCount > 0)
				_InternalState.CorrectPositions(_VisibleItems, alsoCorrectTransversalPositioning);//, itemEndEdgeStationary);

			if (fireScrollPositionChangedEvent)
				OnScrollPositionChangedInternal();
		}

		internal TItemViewsHolder ExtractRecyclableViewsHolderOrCreateNew(int indexOfItemThatWillBecomeVisible, double sizeOfItem)
		{
			// First choice recycleable VHs
			TItemViewsHolder vh = TryExtractRecyclableViewsHolderFrom(_RecyclableItems, indexOfItemThatWillBecomeVisible, sizeOfItem);

			// Second choice: buffered recycleable VHs
			if (vh == null)
				vh = TryExtractRecyclableViewsHolderFrom(_BufferredRecyclableItems, indexOfItemThatWillBecomeVisible, sizeOfItem);

			// The only remaining choice: create it
			if (vh == null)
				vh = CreateViewsHolder(indexOfItemThatWillBecomeVisible);

			return vh;
		}

		TItemViewsHolder TryExtractRecyclableViewsHolderFrom(IList<TItemViewsHolder> vhsToChooseFrom, int indexOfItemThatWillBecomeVisible, double sizeOfItem)
		{
			int i = 0;
			while (i < vhsToChooseFrom.Count)
			{
				var vh = vhsToChooseFrom[i];
				if (IsRecyclable(vh, indexOfItemThatWillBecomeVisible, sizeOfItem))
				{
					OnBeforeRecycleOrDisableViewsHolder(vh, indexOfItemThatWillBecomeVisible);

					// Commented: not needed for now. Current tests show no misplacements
					//// This prepares the item to be further adjusted. If the item is way too far outside the content's panel,
					//// some floatpoint precision can be lost when modifying its anchor[Min/Max]
					//vh.root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(_InternalState.startEdge, 0f, (float)sizeOfItem);

					vhsToChooseFrom.RemoveAt(i);
					return vh;
				}
				++i;
			}

			return null;
		}

		internal void AddViewsHolderAndMakeVisible(TItemViewsHolder vh, int vhIndex, int itemIndex, int itemIndexInView, double realInsetFromEdge, RectTransform.Edge insetEdge, double size)
		{
			// Add it in list at [end]
			_VisibleItems.Insert(vhIndex, vh);
			++_VisibleItemsCount;

			// Update its index
			if (itemIndexInView < 0 || itemIndexInView >= _ItemsDesc.itemsCount)
				throw new OSAException("OSA internal error: itemIndexInView " + itemIndexInView + ", while itemsCount is " + _ItemsDesc.itemsCount);

			vh.ItemIndex = itemIndex;
			vh.itemIndexInView = itemIndexInView;

			// Make sure it's parented to content panel
			RectTransform nlvRT = vh.root;
			//if (size > 190)
			//	Debug.LogWarning("size "+ size +", "+ nlvRT.rect.height + ", " + vh.ItemIndex);
			//if (nlvRT.rect.height > 190)
			//	throw new Exception(nlvRT.rect.height + ", " + vh.ItemIndex);
			nlvRT.SetParent(_Params.Content, false);
			//if (nlvRT.rect.height > 190)
			//	throw new Exception(nlvRT.rect.height + ", " + vh.ItemIndex);

			// Make sure its GO is activated
			SetViewsHolderEnabled(vh);

			// Update its views
			UpdateViewsHolder(vh);

			// GO should remain activated
			if (!IsViewsHolderEnabled(vh))
			{
				string midSentence = _Params.optimization.ScaleToZeroInsteadOfDisable ? "have a zero scale" : "be disabled";
				throw new OSAException(
					"AddViewsHolderAndMakeVisible: VH detected to "+ midSentence 
					+ " after UpdateViewsHolder() was called on it. This is not allowed. " + vh.root);
			}

			// Make sure it's left-top anchored (the need for this arose together with the feature for changind an item's size 
			// (an thus, the content's size) externally, using RequestChangeItemSizeAndUpdateLayout)
			nlvRT.anchorMin = nlvRT.anchorMax = _InternalState.layoutInfo.constantAnchorPosForAllItems;

			// TODO make it as a parameter, turned off as default. Maybe the users want to see the views holders in order in hierarchy
			//if (negativeScroll) nlvRT.SetAsLastSibling();
			//else nlvRT.SetAsFirstSibling();
			if (_Params.optimization.KeepItemsSortedInHierarchy)
			{
				if (vhIndex < _Params.Content.childCount) // even if not found while testing, taking additional measures in case vhIndex may be bigger
					nlvRT.SetSiblingIndex(vhIndex);
			}

			//if (negativeScroll)
			//	currentVirtualInsetFromCTSToUseForNLV = negCurrentVrtInsetFromCTSToUseForNLV_posCurrentVrtInsetFromCTEToUseForNLV;
			//else
			//	currentVirtualInsetFromCTSToUseForNLV = _InternalState.contentPanelVirtualSize - nlvSize - negCurrentVrtInsetFromCTSToUseForNLV_posCurrentVrtInsetFromCTEToUseForNLV;

			//float inset = nlvRT.GetInsetFromParentEdge(_Params.Content, insetEdge);
			//string bef = "inset " + inset+ ", h " + nlvRT.rect.height + ", anchorMin " + nlvRT.anchorMin + ", anchorMax " + nlvRT.anchorMax;
			nlvRT.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(
				insetEdge,
				(float)realInsetFromEdge,
				(float)size
			);

			//inset = nlvRT.GetInsetFromParentEdge(_Params.Content, insetEdge);
			//if (nlvRT.rect.height > 190)
			//	throw new Exception("inset " + inset + ", h " + nlvRT.rect.height + ", " + vh.ItemIndex + ", size " + size + ", anchorMin " + nlvRT.anchorMin + ", anchorMax " + nlvRT.anchorMax +
			//		"\n bef: " + bef);

			// Commented: using cumulative sizes
			//negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV += nlvSizePlusSpacing;
			float tInsetStartToUse;
			float tSizeToUse;
			_InternalState.GetTransversalInsetStartAndSizeToUse(vh, out tInsetStartToUse, out tSizeToUse);
			//Assure transversal size and transversal position based on parent's padding and width settings
			nlvRT.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(
				_InternalState.transvStartEdge,
				tInsetStartToUse,
				tSizeToUse
			);
		}

		/// <summary>Returns the content size delta, even if less than 'maxCount' vhs were added </summary>
		double AddViewsHoldersAndMakeVisible(
			double firstVHInsetFromEdge, 
			RectTransform.Edge insetEdge, 
			int vhStartIndex, 
			int startIndexInView, 
			int maxCount, 
			int vhIndexIncrement, 
			int itemIndexInViewIncrementSign
		){
#if DEBUG_ADD_VHS
			string debugString = "AddingVHs: start View"+ startIndexInView + "VH" + vhStartIndex + ", incr View" + itemIndexInViewIncrementSign + "VH" + vhIndexIncrement + ": ";
#endif
			TItemViewsHolder vhToUse = null;
			int indexOfItemThatWillBecomeVisible;
			//double vhInsetFromEdge = firstVHInsetFromEdge, itemSize = _Params.DefaultItemSize;
			double vhInsetFromEdge = firstVHInsetFromEdge, itemSize;
			double sizeAddedToContent = 0d;
			//int numberOfKnownSizes = 0;
			for (int iAbs = 0, vhIndex = vhStartIndex, iIdxView = startIndexInView; 
				vhInsetFromEdge <= _InternalState.vpSize && iAbs < maxCount; 
				++iAbs, vhIndex += vhIndexIncrement, iIdxView += itemIndexInViewIncrementSign)
			{
				itemSize = _ItemsDesc[iIdxView];

				indexOfItemThatWillBecomeVisible = _ItemsDesc.GetItemRealIndexFromViewIndex(iIdxView);
				vhToUse = ExtractRecyclableViewsHolderOrCreateNew(indexOfItemThatWillBecomeVisible, itemSize);
				AddViewsHolderAndMakeVisible(vhToUse, vhIndex, indexOfItemThatWillBecomeVisible, iIdxView, vhInsetFromEdge, insetEdge, itemSize);
				vhInsetFromEdge += itemSize + _InternalState.spacing;

				//sizeAddedToContent += itemSize + _InternalState.spacing;
				//++numberOfKnownSizes;

#if DEBUG_ADD_VHS
				debugString += "i" + iAbs + "VH" + vhIndex + "View" + iIdxView + "R" + indexOfItemThatWillBecomeVisible;
#endif
			}

#if DEBUG_ADD_VHS
			Debug.Log(debugString);
#endif
			double sizeCumulativeUntilStartIncl = 0d;
			if (startIndexInView > 0)
				sizeCumulativeUntilStartIncl = _ItemsDesc.GetItemSizeCumulative(startIndexInView - 1, false);

			double sizeCumulativeUntilEndIncl = _ItemsDesc.GetItemSizeCumulative(startIndexInView + maxCount - 1, false);
			double itemSizesCumulativeDelta = sizeCumulativeUntilEndIncl - sizeCumulativeUntilStartIncl;

			// Update: now sizes are retrieved from ItemsDesc directly, since non-default item sizes could've been set in CollectItemsSizes
			//// Add the remaining size using the default item size
			//sizeAddedToContent += (_Params.DefaultItemSize + _InternalState.spacing) * (maxCount - numberOfKnownSizes);
			sizeAddedToContent = itemSizesCumulativeDelta + _InternalState.spacing * maxCount;

			return sizeAddedToContent;
		}

		internal int GetNumExcessRecycleableItems()
		{
			// It's important to keep this at 1, because
			// 1. the original reason (at least 1 item cached)
			// 2. because of how the item stealing works (it inserts an item at the head of the list)
			if (_RecyclableItems.Count > 1)
			{
				int maxToKeepInMemory = GetMaxNumObjectsToKeepInMemory();
				int excess = (_RecyclableItems.Count + _VisibleItemsCount) - maxToKeepInMemory;
				if (excess > 0)
					return excess;
			}

			return 0;
		}

		int GetMaxNumObjectsToKeepInMemory()
		{
			int binCapacity = _Params.optimization.RecycleBinCapacity;
			if (binCapacity > 0)
				return binCapacity + _VisibleItemsCount;

			return _ItemsDesc.maxVisibleItemsSeenSinceLastScrollViewSizeChange + _ItemsDesc.destroyedItemsSinceLastScrollViewSizeChange + 1;
		}

		/// <summary>
		/// Utility method to create buffered recycleable items (which aren't directly destroyed).
		/// It simply calls <see cref="CreateViewsHolder(int)"/> <paramref name="count"/> times.
		/// <para><paramref name="indexToPass"/> can be specified in case you want additional 
		/// information to be passed to <see cref="CreateViewsHolder(int)"/> during this. 
		/// Use negative values, to distinguish it from the regular calls OSA does to <see cref="CreateViewsHolder(int)"/>. If not specified, -1 is passed.</para>
		/// <para>Make sure you adapt your code in <see cref="CreateViewsHolder(int)"/> to support a negative index being passed!</para>
		/// <para>An example where different negative values for the index are useful is when you have multiple prefabs and want to distinguish between them</para>
		/// <para>Pass the returned list to <see cref="AddBufferredRecycleableItems(IList{TItemViewsHolder})"/></para>
		/// </summary>
		internal IList<TItemViewsHolder> CreateBufferredRecycleableItems(int count, int indexToPass = -1)
		{
			var vhs = new List<TItemViewsHolder>(count);
			for (int i = 0; i < count; i++)
			{
				var vh = CreateViewsHolder(indexToPass);
				vh.root.SetParent(_Params.Content, false);
				SetViewsHolderDisabled(vh);
				vhs.Add(vh);
			}

			return vhs;
		}

		/// <summary>See <see cref="CreateBufferredRecycleableItems(int, int)"/>. You can also pass the buffered Views Holders directly, if you make sure to initialize them properly</summary>
		internal void AddBufferredRecycleableItems(IList<TItemViewsHolder> vhs)
		{
			foreach (var vh in vhs)
				AddBufferredRecycleableItem(vh);
		}

		/// <summary>Same as <see cref="AddBufferredRecycleableItems(IList{TItemViewsHolder})"/></summary>
		internal void AddBufferredRecycleableItem(TItemViewsHolder vh)
		{
			_BufferredRecyclableItems.Add(vh);
		}

		void UpdateGalleryEffectIfNeeded(bool onlyIfEffectAmountChanged)
		{
			bool sameAmount = _PrevGalleryEffectAmount == _Params.effects.Gallery.OverallAmount;
			if (sameAmount && onlyIfEffectAmountChanged)
				return;

			if (_Params.effects.Gallery.OverallAmount == 0f)
			{
				if (sameAmount)
					return;

				// Make sure the items in the recycle bin don't preserve the local scale from the gallery effect
				RemoveGalleryEffectFromItems(_RecyclableItems);
				RemoveGalleryEffectFromItems(_BufferredRecyclableItems);
				RemoveGalleryEffectFromItems(_VisibleItems);
			}
			else
			{
				if (_VisibleItemsCount == 0 || _ItemsDesc.itemsCount == 0)
					return;

				//double halfVPSize = _InternalState.vpSize / 2;
				//double vpPivotInsetFromStart = _Params.effects.Gallery.Scale.ViewportPivot * _InternalState.vpSize;
				for (int i = 0; i < _VisibleItemsCount; i++)
				{
					var vh = _VisibleItems[i];
					double vhRealInsetStart = vh.root.GetInsetFromParentEdge(_Params.Content, _InternalState.startEdge);
					double vhCenterRealInsetFromStart = vhRealInsetStart + _ItemsDesc[vh.itemIndexInView] / 2d;

					vh.root.localScale = ComposeGalleryEffectFinalAmount(_Params.effects.Gallery.Scale, vhCenterRealInsetFromStart);
					vh.root.localEulerAngles = ComposeGalleryEffectFinalAmount(_Params.effects.Gallery.Rotation, vhCenterRealInsetFromStart);
				}
			}
			_PrevGalleryEffectAmount = _Params.effects.Gallery.OverallAmount;
		}

		Vector3 ComposeGalleryEffectFinalAmount(GalleryAnimation effectParams, double vhCenterRealInsetFromStart)
		{
			double vhCenterRealInsetFromStart01 = vhCenterRealInsetFromStart / _InternalState.vpSize;
			vhCenterRealInsetFromStart01 = Math.Min(1d, Math.Max(0d, vhCenterRealInsetFromStart01));
			float vpPivot = effectParams.ViewportPivot;
			double vhDistFromVPPivot01 = Math.Abs(vhCenterRealInsetFromStart01 - vpPivot);
			vhDistFromVPPivot01 = Math.Min(2d, vhDistFromVPPivot01);

			// vhDistFromVPPivot01 needs to be scaled to [0, 1] space, if it's not already, so it needs a divider.
			// Function for the divider: y = |x - .5| + .5, where x = vpPivot
			// Table:
			// -1 -> 2
			// 0  -> 1
			// .5 -> .5
			// 1  -> 1
			// 2  -> 2
			double divider = Math.Abs(vpPivot - .5d) + .5d;
			vhDistFromVPPivot01 /= divider;
			float effectFactor01 = 1f - (float)vhDistFromVPPivot01;

			float exp = Mathf.Clamp(effectParams.Exponent, 1f, GalleryAnimation.MAX_EFFECT_EXPONENT);
			effectFactor01 = Mathf.Pow(effectFactor01, exp);
			effectFactor01 = Mathf.Clamp01(effectFactor01);
			float effAmount = effectParams.Amount * _Params.effects.Gallery.OverallAmount;
			if (effectParams == _Params.effects.Gallery.Scale)
			{
				var regularValue = Vector3.one;
				var value = effectParams.TransformSpace.Transform(effectFactor01);
				float minValue = _Params.effects.Gallery.Scale.MinValue;
				value = new Vector3(Mathf.Max(minValue, value.x), Mathf.Max(minValue, value.y), Mathf.Max(minValue, value.z));
				return Vector3.Lerp(regularValue, value, effAmount);
			}
			else
			{
				var regularValue = Vector3.zero;
				var value = Quaternion.Lerp(Quaternion.Euler(effectParams.TransformSpace.From), Quaternion.Euler(effectParams.TransformSpace.To), effectFactor01);
				var euler = value.eulerAngles;
				value = Quaternion.Lerp(Quaternion.Euler(regularValue), value, effAmount);
				return value.eulerAngles;
			}
		}

		void RemoveGalleryEffectFromItems(IList<TItemViewsHolder> vhs)
		{
			if (vhs == null)
				return;

			foreach (var vh in vhs)
			{
				if (vh != null && vh.root)
				{
					if (_Params.optimization.ScaleToZeroInsteadOfDisable)
					{
						if (vh.root.localScale != Vector3.zero)
							vh.root.localScale = Vector3.zero;
					}
					else
						vh.root.localScale = Vector3.one;
					vh.root.localEulerAngles = Vector3.zero;
				}
			}
		}

		// TODO merge this with UpdateCTInset...
		void OnCumulatedSizesOfAllItemsChanged(ref ContentSizeOrPositionChangeParams p)
		{
			_InternalState.ctVirtualSize = _InternalState.CalculateContentVirtualSize();

			//Debug.Log("OnCumulatedSizesOfAllItemsChanged: verify _ReleaseFromPullCurrentState.inProgress");
			_ReleaseFromPull.inProgress = false;
			UpdateCTVrtInsetFromVPS(ref p);
		}

		/// <summary><paramref name="viewsHolder"/> will be null if the item is not visible</summary>
		/// <returns>the resolved size, as this may be a bit different than the passed <paramref name="requestedSize"/> for huge data sets (>100k items)</returns>
		double ChangeItemSizeAndUpdateContentSizeAccordingly(TItemViewsHolder viewsHolder, int itemIndexInView, double curSize, double requestedSize, bool itemEndEdgeStationary)
		{
			double deltaSize = requestedSize - curSize;
			double newCTSize = _InternalState.ctVirtualSize + deltaSize;
			double? _;
			double additionalCTDragAbstrDelta = 0d;
			_InternalState.CorrectParametersOnCTSizeChange(itemEndEdgeStationary, out _, ref additionalCTDragAbstrDelta, newCTSize, deltaSize);

			double resolvedSize = requestedSize;
			if (viewsHolder == null)
			{
				//resolvedSize = requestedSize;
			}
			else
			{
				if (viewsHolder.root == null)
					throw new OSAException(
						"ChangeItemSizeAndUpdateContentSizeAccordingly: Unexpected state: ViewsHolder not found among visible items. " +
						"Shouldn't happen if implemented according to documentation/examples"
					); // shouldn't happen if implemented according to documentation/examples

				RectTransform.Edge edge;
				float realInsetToSet;
				if (itemEndEdgeStationary)
				{
					edge = _InternalState.endEdge;
					//realInsetToSet = (float)(_InternalState.GetItemInferredRealInsetFromParentEnd(itemIndexInView) + additionalCTDragAbstrDelta);
					realInsetToSet = (float)(viewsHolder.root.GetInsetFromParentEdge(_Params.Content, edge) - additionalCTDragAbstrDelta);
				}
				else
				{
					edge = _InternalState.startEdge;
					//realInsetToSet = (float)(_InternalState.GetItemInferredRealInsetFromParentStart(itemIndexInView) - additionalCTDragAbstrDelta);
					realInsetToSet = (float)(viewsHolder.root.GetInsetFromParentEdge(_Params.Content, edge) + additionalCTDragAbstrDelta);
				}
				viewsHolder.root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(edge, realInsetToSet, (float)requestedSize);

				//// Even though we know the desired size, the one actually set by the UI system may be different, so we cache that one
				//resolvedSize = _InternalState.getRTCurrentSizeFn(viewsHolder.root);
				////viewsHolder.cachedSize = resolvedSize;
			}

			int indexOfVH = _VisibleItems.IndexOf(viewsHolder);
			// All other items need to be moved(in the mose general case), because most of them won't get recycled
			if (itemEndEdgeStationary)
			{
				DragVisibleItemsRangeUnchecked(0, indexOfVH, -deltaSize + additionalCTDragAbstrDelta, false, false);

				if (additionalCTDragAbstrDelta != 0d)
					DragVisibleItemsRangeUnchecked(indexOfVH + 1, _VisibleItemsCount, additionalCTDragAbstrDelta, false, false);
			}
			else
			{
				DragVisibleItemsRangeUnchecked(indexOfVH + 1, _VisibleItemsCount, deltaSize + additionalCTDragAbstrDelta, false, false);

				if (additionalCTDragAbstrDelta != 0d)
					DragVisibleItemsRangeUnchecked(0, indexOfVH, additionalCTDragAbstrDelta, false, false);
			}

			_ItemsDesc.BeginChangingItemsSizes(itemIndexInView);
			_ItemsDesc[itemIndexInView] = resolvedSize;
			_ItemsDesc.EndChangingItemsSizes();

			var p = new ContentSizeOrPositionChangeParams
			{
				computeVisibilityParams = _ComputeVisibilityParams_Reusable_Empty,
				fireScrollPositionChangedEvent = true,
				keepVelocity = true,
				allowOutsideBounds = true,
				contentEndEdgeStationary = itemEndEdgeStationary
				//contentInsetOverride = ctInsetFromVPSOverride
			};
			OnCumulatedSizesOfAllItemsChanged(ref p);

			return resolvedSize;
		}

		/// <summary>
		/// Assuming that vhs.Count is > 0. IMPORTANT: vhs should be in order (their itemIndexInView 
		/// should be in ascending order - not necesarily consecutive)
		/// </summary>
		void OnItemsSizesChangedExternally(List<TItemViewsHolder> vhs, double[] sizes, bool itemEndEdgeStationary)
		{
			if (_ItemsDesc.itemsCount == 0)
				throw new OSAException("Cannot change item sizes externally if the items count is 0!");

			int vhsCount = vhs.Count;
			int viewIndex;
			TItemViewsHolder vh;
			//var insetEdge = itemEndEdgeStationary ? endEdge : startEdge;
			//float currentSize;
			double ctSizeBefore = _InternalState.CalculateContentVirtualSize();

			int firstVHIndexInView = vhs[0].itemIndexInView;

#if DEBUG_INDICES
			string debugIndicesString;
			if (GetDebugIndicesString(out debugIndicesString))
				Debug.Log("OnExtCh " + vhs.Count + ", firstIdx "+ firstVHIndexInView + ". Indices: " + debugIndicesString);
#endif

			//bool doAnotherPass;
			//int i = 0;
			//int iterations = 0;
			//do
			//{
			//	doAnotherPass = false;
			//	int prevViewIndex = -1;
			//	_ItemsDesc.BeginChangingItemsSizes(firstVHIndexInView + i);
			//	for (; i < vhsCount; ++i)
			//	{
			//		vh = vhs[i];
			//		viewIndex = vh.itemIndexInView;

			//		if (viewIndex < prevViewIndex) // looping and found the HEAD after some of the items at the begining => do another pass
			//		{
			//			//throw new OSAException(
			//			//	"OSA.OnItemsSizesChangedExternally: Internal exception. Please report this. Looping=" + _Params.effects.LoopItems);

			//			doAnotherPass = true;
			//			break;
			//		}
			//		// Commented: adapting to Unity 2017.2 breaking the ContentSizeFitter for us... when it's disabled, the object's size returns to the one before resizing. Pretty bad. Oh well..
			//		// Now the sizes are retrieved before disabling the CSF and passed to this method
			//		//currentSize = _GetRTCurrentSizeFn(vh.root);
			//		//_ItemsDesc[viewIndex] = currentSize;

			//		try
			//		{
			//			_ItemsDesc[viewIndex] = sizes[i];

			//		}
			//		catch
			//		{
			//			int x = 0;
			//			Debug.LogError("asd");
			//		}
			//		prevViewIndex = viewIndex;
			//	}
			//	_ItemsDesc.EndChangingItemsSizes();
			//	++iterations;


			//	if (iterations > 2)
			//		throw new OSAException(
			//			"OSA.OnItemsSizesChangedExternally: Internal exception. Please report this. Done " + iterations +
			//			" iterations for changing items' sizes, while only 2 should've been done. Looping=" + _Params.effects.LoopItems);
			//} while (doAnotherPass);

			_ItemsDesc.BeginChangingItemsSizes(firstVHIndexInView);
			for (int i = 0; i < vhsCount; ++i)
			{
				vh = vhs[i];
				viewIndex = vh.itemIndexInView;
				// Commented: adapting to Unity 2017.2 breaking the ContentSizeFitter for us... when it's disabled, the object's size returns to the one before resizing. Pretty bad. Oh well..
				// Now the sizes are retrieved before disabling the CSF and passed to this method
				//currentSize = _GetRTCurrentSizeFn(vh.root);
				//_ItemsDesc[viewIndex] = currentSize;
				_ItemsDesc[viewIndex] = sizes[i];
			}
			_ItemsDesc.EndChangingItemsSizes();


			double ctSizeAfter = _InternalState.CalculateContentVirtualSize();
			double deltaSize = ctSizeAfter - ctSizeBefore;
			double? _;
			double additionalCTDragAbstrDelta = 0d;
			_InternalState.CorrectParametersOnCTSizeChange(itemEndEdgeStationary, out _, ref additionalCTDragAbstrDelta, ctSizeAfter, deltaSize);

			double newCTInset = _InternalState.ctVirtualInsetFromVPS_Cached;

			// Preparing the first visible item to be used in calculating the new ctInsetStart
			// if ct top is stationary, ctinsetStart won't need to be modified
			if (itemEndEdgeStationary)
			{
				newCTInset -= deltaSize;
				//DragVisibleItemsRangeUnchecked(0, 1, -deltaSize + additionalCTDragAbstrDelta);
			}
			//else
			//{
			//	//DragVisibleItemsRangeUnchecked(0, 1, -deltaSize + additionalCTDragAbstrDelta);
			//}

			newCTInset += additionalCTDragAbstrDelta;

#if DEBUG_ON_SIZES_CHANGED_EXTERNALLY
			string str = "OnSizesChExt: additionalCTDragAbstrDelta " + additionalCTDragAbstrDelta.ToString("###################0.####");
			str += "\nlastVHInsetEnd before OnCumuChanged " + _VisibleItems[_VisibleItemsCount - 1].root.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge).ToString(OSAConst.DEBUG_FLOAT_FORMAT);
			str += "\nctInsetEnd before OnSizesChanged " + _InternalState.CTVirtualInsetFromVPE_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
			str += "\nctInset before OnSizesChanged " + _InternalState.ctVirtualInsetFromVPS_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
			str += "\nnewCTInsetToSet " + newCTInset.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
			str += "\nctSize Before BeginChanging " + ctSizeBefore.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
			str += ", ctSize After EndChanging " + ctSizeAfter.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
#endif

			var p = new ContentSizeOrPositionChangeParams
			{
				cancelSnappingIfAny = true,
				keepVelocity = true,
				allowOutsideBounds = true,
				contentEndEdgeStationary = itemEndEdgeStationary,
				contentInsetOverride = newCTInset,
				// Commented: this is done by CorrectPositionsOfVisibleItems below
				//fireScrollPositionChangedEvent = true
			};
			OnCumulatedSizesOfAllItemsChanged(ref p);

#if DEBUG_ON_SIZES_CHANGED_EXTERNALLY
			str += ", lastVHInsetEnd after OnSizesChanged " + _VisibleItems[_VisibleItemsCount - 1].root.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge).ToString(OSAConst.DEBUG_FLOAT_FORMAT);
			str += ", ctInsetEnd after OnSizesChanged " + _InternalState.CTVirtualInsetFromVPE_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
			str += ", ctInset after OnSizesChanged " + _InternalState.ctVirtualInsetFromVPS_Cached.ToString(OSAConst.DEBUG_FLOAT_FORMAT);
#endif

			//CorrectPositionsOfVisibleItems(true, indexInView => sizes[indexInView - firstVHIndexInView]);
			CorrectPositionsOfVisibleItems(true, true);

#if DEBUG_ON_SIZES_CHANGED_EXTERNALLY
			str += ", lastVHInsetEnd after CorrectPos " + _VisibleItems[_VisibleItemsCount - 1].root.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge);
			Debug.Log(str);
#endif
		}

		/// <summary>Needed so <see cref="ScrollViewSizeChanged"/> is called before everything else</summary>
		void OnScrollViewSizeChangedBase()
		{
			if (ScrollViewSizeChanged != null)
				ScrollViewSizeChanged();

			OnScrollViewSizeChanged();
		}
	}
}
