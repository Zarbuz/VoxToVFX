//#define DEBUG_COMPUTE_VISIBILITY

using UnityEngine;
using UnityEngine.EventSystems;

namespace Com.TheFallenGames.OSA.Core.SubComponents
{
	internal class NestingManager<TParams, TItemViewsHolder> : IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
		where TParams : BaseParams
		where TItemViewsHolder : BaseItemViewsHolder
	{
		public bool SearchedParentAtLeastOnce { get { return _SearchedAtLeastOnce; } }
		public bool CurrentDragCapturedByParent { get { return _CurrentDragCapturedByParent; } }
		public bool CurrentScrollConsumedByParent { get { return _CurrentScrollConsumedByParent; } }


		OSA<TParams, TItemViewsHolder> _Adapter;
		InternalState<TItemViewsHolder> _InternalState;
		IInitializePotentialDragHandler parentInitializePotentialDragHandler;
		IBeginDragHandler parentBeginDragHandler;
		IDragHandler parentDragHandler;
		IEndDragHandler parentEndDragHandler;
		IScrollHandler parentScrollHandler;
		bool _SearchedAtLeastOnce;
		bool _CurrentDragCapturedByParent;
		bool _CurrentScrollConsumedByParent;


		public NestingManager(OSA<TParams, TItemViewsHolder> adapter)
		{
			_Adapter = adapter;
			_InternalState = _Adapter._InternalState;
		}


		public void FindAndStoreNestedParent()
		{
			parentInitializePotentialDragHandler = null;
			parentBeginDragHandler = null;
			parentDragHandler = null;
			parentEndDragHandler = null;
			parentScrollHandler = null;

			var tr = _Adapter.transform;
			// Find the first parent that implements all of the interfaces
			while ((tr = tr.parent) && parentInitializePotentialDragHandler == null)
			{
				parentInitializePotentialDragHandler = tr.GetComponent(typeof(IInitializePotentialDragHandler)) as IInitializePotentialDragHandler;
				if (parentInitializePotentialDragHandler == null)
					continue;

				parentBeginDragHandler = parentInitializePotentialDragHandler as IBeginDragHandler;
				if (parentBeginDragHandler == null)
				{
					parentInitializePotentialDragHandler = null;
					continue;
				}

				parentDragHandler = parentInitializePotentialDragHandler as IDragHandler;
				if (parentDragHandler == null)
				{
					parentInitializePotentialDragHandler = null;
					parentBeginDragHandler = null;
					continue;
				}

				parentEndDragHandler = parentInitializePotentialDragHandler as IEndDragHandler;
				if (parentEndDragHandler == null)
				{
					parentInitializePotentialDragHandler = null;
					parentBeginDragHandler = null;
					parentDragHandler = null;
					continue;
				}
			}

			if (parentInitializePotentialDragHandler == null)
			{
				// Search for the scroll handler separately, if no drag handlers present
				tr = _Adapter.transform;
				while ((tr = tr.parent) && parentScrollHandler == null)
				{
					parentScrollHandler = tr.GetComponent(typeof(IScrollHandler)) as IScrollHandler;
				}
			}
			else
			{
				// Only allow the scroll handler to be taken from the drag handler, if any, so all handlers will come from the same object
				parentScrollHandler = parentInitializePotentialDragHandler as IScrollHandler;
			}

			_SearchedAtLeastOnce = true;
		}

		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
			_CurrentDragCapturedByParent = false;

			if (!_SearchedAtLeastOnce)
				FindAndStoreNestedParent();

			if (parentInitializePotentialDragHandler == null)
				return;

			parentInitializePotentialDragHandler.OnInitializePotentialDrag(eventData);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (parentInitializePotentialDragHandler == null)
				return;

			if (_Adapter.Parameters.DragEnabled)
			{
				var delta = eventData.delta;
				float dyExcess = Mathf.Abs(delta.y) - Mathf.Abs(delta.x);

				_CurrentDragCapturedByParent = _InternalState.hor1_vertMinus1 * dyExcess >= 0f; // parents have priority when dx == dy, since they are supposed to be more important

				if (!_CurrentDragCapturedByParent)
				{
					// The drag direction is bigger in the child adapter's scroll direction than in the perpendicular one,
					// i.e. the drag is 'intended' for the child adapter.
					// But if the child adapter is at boundary and ForwardDragSameDirectionAtBoundary, still forward the event to the parent
					_CurrentDragCapturedByParent = CheckForForwardingToParent(delta);
				}
			}
			else
				// When the child ScrollView has its drag disabled, forward the event to the parent without further checks
				_CurrentDragCapturedByParent = true;

			if (!_CurrentDragCapturedByParent)
				return;

			parentBeginDragHandler.OnBeginDrag(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (parentInitializePotentialDragHandler == null)
				return;

			parentDragHandler.OnDrag(eventData);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (parentInitializePotentialDragHandler == null)
				return;

			parentEndDragHandler.OnEndDrag(eventData);
			_CurrentDragCapturedByParent = false;
		}

		public void OnScroll(PointerEventData eventData)
		{
			_CurrentScrollConsumedByParent = false;

			if (!_SearchedAtLeastOnce)
				FindAndStoreNestedParent();

			if (parentScrollHandler == null)
				return;

			if (_Adapter.Parameters.ScrollEnabled)
			{
				var scrollDeltaRaw = eventData.scrollDelta;

				var scrollDeltaWithoutSensitivity = scrollDeltaRaw;
				scrollDeltaWithoutSensitivity.y *= -1f;
				var scrollDeltaWithSensitivity = scrollDeltaWithoutSensitivity;
				_Adapter.Parameters.ApplyScrollSensitivityTo(ref scrollDeltaWithSensitivity);

				bool scrollInChildDirectionExist_AfterSensitivity = scrollDeltaWithSensitivity[_InternalState.hor0_vert1] != 0f;
				//bool b = scrollDeltaRaw[_InternalState.hor0_vert1] != 0f && scrollDelta[_InternalState.hor0_vert1] == 0f;
				if (scrollInChildDirectionExist_AfterSensitivity)
				{
					// Scrolled in the child's orientation => forward if child adapter is at boundary
					if (!CheckForForwardingToParent(scrollDeltaWithSensitivity))
						return;
				}
				else
				{
					// Sensivity in child's orientation disabled (it's set to 0) => forward the event to parent without further checks
					bool scrollInChildDirectionExist_BeforeSensitivity = scrollDeltaWithoutSensitivity[_InternalState.hor0_vert1] != 0f;
					if (scrollInChildDirectionExist_BeforeSensitivity)
					{

					}
					else
					{
						// No scroll input in the child orientation

						bool scrollInChildTransversalDirectionExist_AfterSensitivity = scrollDeltaWithSensitivity[1 - _InternalState.hor0_vert1] != 0f;
						if (scrollInChildTransversalDirectionExist_AfterSensitivity)
							// Child has priority, since it set a non-zero sensitivity for the received input axis
							return;
					}
				}
			}
			else
			{
				// When the child ScrollView has its scroll disabled, forward the event to the parent without further checks
			}
			_CurrentScrollConsumedByParent = true;

			parentScrollHandler.OnScroll(eventData);
		}

		bool CheckForForwardingToParent(Vector2 delta)
		{
			if (_Adapter.Parameters.ForwardDragSameDirectionAtBoundary)
			{
				float deltaInScrollDir = delta[_InternalState.hor0_vert1];
				float abstrDeltaInScrollDir = deltaInScrollDir * _InternalState.hor1_vertMinus1;

				float acceptedError = 3f; // UI units
				if (abstrDeltaInScrollDir < 0f)
				{
					// Delta would drag the Scroll View in a negative direction (towards end)
					return _Adapter.ContentVirtualInsetFromViewportEnd >= -acceptedError;
				}
				else
				{
					// Postive direction (towards start)
					return _Adapter.ContentVirtualInsetFromViewportStart >= -acceptedError;
				}
			}

			return false;
		}
	}
}
