using System;
using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Visual.UI;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;

namespace Com.TheFallenGames.OSA.Core
{
	/// <summary>
	/// Script that enables snapping on a <see cref="OSA{TParams, TItemViewsHolder}"/>. Attach it to the ScrollView's game object.
	/// </summary>
	public class Snapper8 : MonoBehaviour
	{
		public float snapWhenSpeedFallsBelow = 50f;
		public float viewportSnapPivot01 = .5f;
		public float itemSnapPivot01 = .5f;
		//public float snapOnlyIfSpeedIsAbove = 20f;
		public float snapDuration = .3f;
		public float snapAllowedError = 1f;
		//[Tooltip("This will be disabled during snapping animation")]
		public Scrollbar scrollbar;
		//public int maxNeighborsToSnapToRegardlessOfSpeed;
		[Tooltip("If the current drag distance is not enough to change the currently centered item, " +
				"snapping to the next item will still occur if the current speed is bigger than this. " +
				"Set to a negative value to disable (default). This was initially useful for things like page views")]
		public float minSpeedToAllowSnapToNext = -1;
		public bool skipIfReachedExtremity = true;

		public event Action SnappingStarted;
		public event Action SnappingEndedOrCancelled;

		/// <summary>This needs to be set externally</summary>
		public IOSA Adapter
		{
			set
			{
				if (_Adapter == value)
					return;

				if (_Adapter != null)
				{
					_Adapter.ScrollPositionChanged -= OnScrolled;
					_Adapter.ItemsRefreshed -= OnItemsRefreshed;
				}
				_Adapter = value;
				if (_Adapter != null)
				{
					_Adapter.ScrollPositionChanged += OnScrolled;
					_Adapter.ItemsRefreshed += OnItemsRefreshed;
				}
			}
		}
		public bool SnappingInProgress { get; private set; }

		//bool IsPointerDraggingOnScrollRect { get { return _ScrollRect != null && Utils.GetPointerEventDataWithPointerDragGO(_ScrollRect.gameObject, false) != null; } }
		bool IsPointerDraggingOnScrollRect { get { return _Adapter != null && _Adapter.IsDragging; } }
		//bool IsPointerDraggingOnScrollbar { get { return scrollbar != null && Utils.GetPointerEventDataWithPointerDragGO(scrollbar.gameObject, false) != null; } }
		bool IsPointerDraggingOnScrollbar { get { return _ScrollbarFixer != null && _ScrollbarFixer.IsDragging; } }
		
		IOSA _Adapter;
		ScrollbarFixer8 _ScrollbarFixer;
		bool _SnappingDoneAndEndSnappingEventPending;
		bool _SnapNeeded; // a new snap will only start if after the las snap the scrollrect's scroll position has changed
		bool _SnappingCancelled;
		//bool _PointerDown;
		Func<float> _GetSignedAbstractSpeed;
		int _LastSnappedItemIndex = -1;
		bool _SnapToNextOnlyEnabled;
		bool _StartCalled;
		Canvas _Canvas;
		RectTransform _CanvasRT;
		bool _TriedToGetCanvasAtLeastOnce;
		int _LastItemIndexUnFinishedSnap = -1;


		void Start()
		{
			//if (maxNeighborsToSnapToRegardlessOfSpeed < 0)
			//	maxNeighborsToSnapToRegardlessOfSpeed = 0;
			if (minSpeedToAllowSnapToNext < 0)
				minSpeedToAllowSnapToNext = float.MaxValue;
			_SnapToNextOnlyEnabled = minSpeedToAllowSnapToNext != float.MaxValue;

			if (scrollbar)
			{
				_ScrollbarFixer = scrollbar.GetComponent<ScrollbarFixer8>();
				if (!_ScrollbarFixer)
					throw new OSAException("ScrollbarFixer8 should be attached to Scrollbar");
			}

			if (scrollbar)
				scrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
			_StartCalled = true;
		}

		void OnDisable() { CancelSnappingIfInProgress(); }

		void OnDestroy()
		{
			if (scrollbar)
				scrollbar.onValueChanged.RemoveListener(OnScrollbarValueChanged);
			//if (_ScrollRect)
			//	_ScrollRect.onValueChanged.RemoveListener(OnScrolled);

			Adapter = null; // will unregister listeners

			SnappingStarted = null;
			SnappingEndedOrCancelled = null;
		}

		internal void CancelSnappingIfInProgress()
		{
			//Debug.Log(
			//	"CancelSnappingIfInProgress:\n" +
			//	"_SnappingDoneAndEndSnappingEventPending=" + _SnappingDoneAndEndSnappingEventPending +
			//	", _SnapNeeded=" + _SnapNeeded +
			//	", SnappingInProgress=" + SnappingInProgress);

			_SnappingDoneAndEndSnappingEventPending = false;
			_SnapNeeded = false;

			//Debug.Log("cancel: inProg=" + SnappingInProgress);
			if (!SnappingInProgress)
				return;

			_SnappingCancelled = true;
			SnappingInProgress = false;
		}

		internal void StartSnappingIfNeeded()
		{
			if (!_StartCalled)
				return;

			// Disabling the script should make it unoperable
			if (!enabled)
				return;

			if (_SnappingDoneAndEndSnappingEventPending)
			{
				OnSnappingEndedOrCancelled();
				return;
			}

			if (_Adapter == null || !_Adapter.IsInitialized)
				return;

			// Commented: this now works
			//if (_Adapter.GetItemsCount() > OSAConst.MAX_ITEMS_TO_SUPPORT_SMOOTH_SCROLL_AND_ITEM_RESIZING)
			//	return;

			// Initializing it here, because in Start the adapter may not be initialized
			if (_GetSignedAbstractSpeed == null)
			{
				// _ScrollRect.velocity doesn't reflect <curNormPos-prevNormPos>, as it would be expected, but the opposite of that (opposite sign)
				// Returning: negative, if towards end; positive, else.
				if (_Adapter.BaseParameters.IsHorizontal)
					_GetSignedAbstractSpeed = () => _Adapter.Velocity[0];
				else
					_GetSignedAbstractSpeed = () => -_Adapter.Velocity[1];
			}
			float signedSpeed = _GetSignedAbstractSpeed();
			float speed = Mathf.Abs(signedSpeed);

			//Debug.Log(
			//	"StartSnappingIfNeeded:\n" +
			//	"SnappingInProgress=" + SnappingInProgress +
			//	", _SnapNeeded=" + _SnapNeeded +
			//	", magnitude=" + _Adapter.Velocity.magnitude +
			//	", IsPointerDraggingOnScrollRect=" + IsPointerDraggingOnScrollRect +
			//	", IsPointerDraggingOnScrollbar=" + IsPointerDraggingOnScrollbar +
			//	", signedSpeed " + signedSpeed + 
			//	", snapWhenSpeedFallsBelow " + snapWhenSpeedFallsBelow
			//	);
			if (SnappingInProgress || !_SnapNeeded || speed >= snapWhenSpeedFallsBelow || IsPointerDraggingOnScrollRect || IsPointerDraggingOnScrollbar)
				return;

			if (skipIfReachedExtremity)
			{
				double maxAllowedDistFromExtremity = Mathf.Clamp(snapAllowedError, 1f, 20f);
				double insetStartOrEnd = Math.Max(_Adapter.ContentVirtualInsetFromViewportStart, _Adapter.ContentVirtualInsetFromViewportEnd);
				if (Math.Abs(insetStartOrEnd) <= maxAllowedDistFromExtremity) // Content is at start/end => don't force any snapping
					return;
			}
			
			float distanceToTarget;
			var middle = GetMiddleVH(out distanceToTarget);
			if (middle == null)
				return;

			_SnapNeeded = false;
			if (distanceToTarget <= snapAllowedError)
				return;

			//Debug.Log(middle.ItemIndex);

			int indexToSnapTo = middle.ItemIndex;
			bool snapToNextOnly = 
				speed >= minSpeedToAllowSnapToNext
				// Not allowed to skip neighbors. Snapping to neigbors is only allowed if the current middle is the previous middle
				&& (indexToSnapTo == _LastSnappedItemIndex
					// Update: Allowing skipping neighbors if the previous snap didn't finish naturally (most probably, the user swapped again fast, with the sole intent of skipping an item )
					|| _LastItemIndexUnFinishedSnap != -1
				); 
			if (snapToNextOnly)				
			{
				bool loopingEnabled = _Adapter.BaseParameters.effects.LoopItems && _Adapter.GetContentSizeToViewportRatio() > 1d;
				int count = _Adapter.GetItemsCount();
				if (signedSpeed < 0) // going towards end => snap to bigger indexInView
				{
					if (indexToSnapTo == count - 1 && !loopingEnabled)
						return;
					indexToSnapTo = (indexToSnapTo + 1) % count;
				}
				else // going towards start => snap to smaller indexInView
				{
					if (indexToSnapTo == 0 && !loopingEnabled)
						return;
					indexToSnapTo = ((indexToSnapTo + count)/*adding count to prevent a negative dividend*/ - 1) % count;
				}
			}
			else
				indexToSnapTo = middle.ItemIndex;

			//Debug.Log("start: " + s);
			_SnappingCancelled = false;
			bool continuteAnimation;
			bool cancelledOrEnded = false; // used to check if the scroll was cancelled immediately after calling SmoothScrollTo (i.e. without first setting SnappingInProgress = true)
			bool doneNaturally = false;
			_LastItemIndexUnFinishedSnap = indexToSnapTo;
			_Adapter.SmoothScrollTo(
				indexToSnapTo,
				snapDuration,
				viewportSnapPivot01,
				itemSnapPivot01,
				progress =>
				{
					continuteAnimation = true;
					doneNaturally = progress == 1f;
					if (doneNaturally || _SnappingCancelled || IsPointerDraggingOnScrollRect || IsPointerDraggingOnScrollbar) // done. last iteration
					{
						cancelledOrEnded = true;
						continuteAnimation = false;

						//Debug.Log("received end callback: SnappingInProgress=" + SnappingInProgress);
						if (SnappingInProgress)
						{
							_LastSnappedItemIndex = indexToSnapTo;
							SnappingInProgress = false;
							_SnappingDoneAndEndSnappingEventPending = true;

							if (doneNaturally)
							{
								_LastItemIndexUnFinishedSnap = -1;
							}
						}
					}

					// If the items were refreshed while the snap animation was playing or if the user touched the scrollview, don't continue;
					return continuteAnimation;
				},
				null,
				true
			);

			// The scroll was cancelled immediately after calling SmoothScrollTo => cancel
			if (cancelledOrEnded)
			{
				if (doneNaturally)
				{
					_LastItemIndexUnFinishedSnap = -1;
				}

				return;
			}

			SnappingInProgress = true; //always true, because we're overriding the previous scroll

			if (SnappingInProgress)
				OnSnappingStarted();
		}

		Canvas FindOrGetCanvas()
		{
			if (_TriedToGetCanvasAtLeastOnce)
				return _Canvas;
			_TriedToGetCanvasAtLeastOnce = true;

			return _Canvas = GetComponentInParent<Canvas>();
		}

		RectTransform FindOrGetCanvasRT()
		{
			if (_TriedToGetCanvasAtLeastOnce)
				return _CanvasRT;
			_TriedToGetCanvasAtLeastOnce = true;

			return _CanvasRT = FindOrGetCanvas().transform as RectTransform;
		}

		AbstractViewsHolder GetMiddleVH(out float distanceToTarget)
		{
			return _Adapter.GetViewsHolderClosestToViewportLongitudinalNormalizedAbstractPoint(FindOrGetCanvas(), FindOrGetCanvasRT(), viewportSnapPivot01, itemSnapPivot01, out distanceToTarget);
		}

		//void OnScrolled(Vector2 _) { if (!SnappingInProgress) _SnapNeeded = true; }
		void OnScrolled(double _)
		{
			if (!SnappingInProgress)
			{
				_SnapNeeded = true;

				if (_SnapToNextOnlyEnabled && !IsPointerDraggingOnScrollbar && !IsPointerDraggingOnScrollRect)
					UpdateLastSnappedIndexFromMiddleVH();
			}
		} // from adapter

		void OnScrollbarValueChanged(float _) { if (IsPointerDraggingOnScrollbar) CancelSnappingIfInProgress(); } // from scrollbar

		void OnItemsRefreshed(int newCount, int prevCount)
		{
			if (newCount == prevCount)
				return;

			if (_SnapToNextOnlyEnabled)
				UpdateLastSnappedIndexFromMiddleVH();
		}

		void UpdateLastSnappedIndexFromMiddleVH()
		{
			float _;
			var middleVH = GetMiddleVH(out _);
			_LastSnappedItemIndex = middleVH == null ? -1 : middleVH.ItemIndex;
			_LastItemIndexUnFinishedSnap = -1;
		}

		void OnSnappingStarted()
		{
			//Debug.Log("start");
			//if (scrollbar)
			//	scrollbar.interactable = false;

			if (SnappingStarted != null)
				SnappingStarted();
		}

		void OnSnappingEndedOrCancelled()
		{
			//Debug.Log("end");
			//if (scrollbar)
			//	scrollbar.interactable = true;

			_SnappingDoneAndEndSnappingEventPending = false;

			if (SnappingEndedOrCancelled != null)
				SnappingEndedOrCancelled();
		}
	}
}
