//#define DEBUG_EVENTS

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Com.TheFallenGames.OSA.Util.ItemDragging
{
	/// <summary>
	/// </summary>
	[RequireComponent(typeof(Graphic))]
	public class DraggableItem : MonoBehaviour, IPointerDownHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler//, ICancelHandler
	{
		public float longClickTime = .7f;

		public IDragDropListener dragDropListener;
		public StateEnum State { get { return _State; } }
		public RectTransform RT { get { return _RT; } }
		public OrphanedItemBundle OrphanedBundle { get { return _OrphanedBundle; } }
		public Vector2 CurrentOnDragEventWorldPosition { get { return _CurrentOnDragEventWorldPosition; } }
		public Vector2 DistancePointerToDraggedInCanvasSpace { get { return _DistancePointerToDraggedInCanvasSpace; } }
		public Camera CurrentPressEventCamera { get { return _CurrentPressEventCamera; } }

		IInitializePotentialDragHandler _ParentToDelegateDragEventsTo;
		Vector2 _CurrentOnDragEventWorldPosition;
		Vector2 _DistancePointerToDraggedInCanvasSpace;
		Camera _CurrentPressEventCamera;
		RectTransform _RT;
		Canvas _Canvas;
		GraphicRaycaster _GraphicRaycaster;
		RectTransform _CanvasRT;
		Vector2 _CurrentPressEventWorldPosition;
		float _PressedTime;
		StateEnum _State;
		OrphanedItemBundle _OrphanedBundle;
		//int _PointerID;
		EventSystem _EventSystem;

		EventSystem GetOrFindEventSystem()
		{
			if (_EventSystem == null)
				_EventSystem = FindObjectOfType<EventSystem>();

			return _EventSystem;
		}

		void Start()
		{
			_RT = transform as RectTransform;
		}

		void Update()
		{
			if (_State == StateEnum.PRESSING__WAITING_FOR_LONG_CLICK)
			{
				if (Time.unscaledTime - _PressedTime >= longClickTime)
					OnLongClick();
			}
		}

		void OnLongClick()
		{
			EnterState_AfterLongClickDragAccepted_WaitingToBeginDrag();
			var evSystem = GetOrFindEventSystem();
			if (!evSystem)
			{
				EnterState_WaitingForPress();
				return;
			}

			var canvas = GetComponentInParent<Canvas>();
			var raycaster = canvas.GetComponentInParent<GraphicRaycaster>();
			var raycastResults = new List<RaycastResult>();
			var pev = new PointerEventData(evSystem);
			pev.position = _CurrentPressEventWorldPosition;
			raycaster.Raycast(pev, raycastResults);
			bool foundThis = false;
			foreach (var res in raycastResults)
			{
				if (res.gameObject == gameObject)
				{
					foundThis = true;
					break;
				}
			}

			// Happens if the object is moved externally while the pointer remains still
			if (!foundThis)
			{
				EnterState_AfterLongClickDragDeclined_WaitingToBeginDrag();
				return;
			}

			_Canvas = canvas;
			_CanvasRT = _Canvas.transform as RectTransform;
			_GraphicRaycaster = raycaster;
			var pos = RT.position;
			RT.SetParent(_CanvasRT, false);
			RT.position = pos; // preserving the pos

			SetVisualMode(VisualMode.OVER_OWNER_OR_OUTSIDE);
			if (dragDropListener != null && !dragDropListener.OnPrepareToDragItem(this))
			{
				EnterState_AfterLongClickDragDeclined_WaitingToBeginDrag();
			}
		}

		public void CancelDragSilently()
		{
			EnterState_WaitingForPress();
		}

		void SetVisualMode(VisualMode mode)
		{
			int intMode = (int)mode;
			var euler = RT.localEulerAngles;
			euler.x = 10f * intMode;
			euler.z = 4f * intMode;
			RT.localEulerAngles = euler;
		}

		void EnterState_WaitingForPress()
		{
			_ParentToDelegateDragEventsTo = null;
			_DistancePointerToDraggedInCanvasSpace = Vector2.zero;
			_CurrentPressEventCamera = null;
			_Canvas = null;
			_CanvasRT = null;
			_GraphicRaycaster = null;
			SetVisualMode(VisualMode.NONE);
			_State = StateEnum.WAITING_FOR_PRESS;
		}

		void EnterState_AfterLongClickDragAccepted_WaitingToBeginDrag()
		{
			_State = StateEnum.AFTER_LONG_CLICK_DRAG_ACCEPTED__WAITING_TO_BEGIN_DRAG;
		}

		void EnterState_AfterLongClickDragDeclined_WaitingToBeginDrag()
		{
			_State = StateEnum.AFTER_LONG_CLICK_DRAG_DECLINED__WAITING_TO_BEGIN_DRAG;
		}
		
		void EnterState_BusyDelegatingDragEventToParent()
		{
			_State = StateEnum.BUSY_DELEGATING_DRAG_TO_PARENT;
		}

		#region Callbacks from Unity UI event handlers
		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
#if DEBUG_EVENTS
			Debug.Log("OnPointerDown: " + _State);
#endif
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (_State != StateEnum.WAITING_FOR_PRESS)
				return;

			//_PointerID = eventData.pointerId;

			_CurrentPressEventWorldPosition = eventData.position;
			_State = StateEnum.PRESSING__WAITING_FOR_LONG_CLICK;
			_PressedTime = Time.unscaledTime;
		}
		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
#if DEBUG_EVENTS
			Debug.Log("OnPointerUp: " + _State);
#endif
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (_State == StateEnum.PRESSING__WAITING_FOR_LONG_CLICK)
			{
				EnterState_WaitingForPress();
			}
			else if (_State == StateEnum.DRAGGING || _State == StateEnum.AFTER_LONG_CLICK_DRAG_ACCEPTED__WAITING_TO_BEGIN_DRAG)
			{
				var raycaster = _GraphicRaycaster;
				EnterState_WaitingForPress();
				DropAndCheckForOrphaned(eventData, raycaster);
			}
		}

		void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
		{
#if DEBUG_EVENTS
			Debug.Log("OnInitializePotentialDrag: " + _State);
#endif
			_ParentToDelegateDragEventsTo = null;
			if (!RT.parent)
				return;
			_ParentToDelegateDragEventsTo = RT.parent.GetComponentInParent(typeof(IInitializePotentialDragHandler)) as IInitializePotentialDragHandler;
			if (_ParentToDelegateDragEventsTo != null)
				_ParentToDelegateDragEventsTo.OnInitializePotentialDrag(eventData);
		}

		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{
#if DEBUG_EVENTS
			Debug.Log("OnBeginDrag: " + _State);
#endif
			if (eventData.button != PointerEventData.InputButton.Left
				|| _State != StateEnum.AFTER_LONG_CLICK_DRAG_ACCEPTED__WAITING_TO_BEGIN_DRAG)
			{
				
				if (
					// A child was pressed, which forwarded the event to us
					_State == StateEnum.WAITING_FOR_PRESS
					// Long-click canceled
					|| _State == StateEnum.PRESSING__WAITING_FOR_LONG_CLICK
					// The OnPrepareToDragItem returned false (the listener declined the drag when the long click happened) or the item could not be dragged due to other reasons
					|| _State == StateEnum.AFTER_LONG_CLICK_DRAG_DECLINED__WAITING_TO_BEGIN_DRAG
				)
				{
					var casted = _ParentToDelegateDragEventsTo as IBeginDragHandler;
					if (casted != null)
					{
						EnterState_BusyDelegatingDragEventToParent(); // keep sending the current started drag event
						casted.OnBeginDrag(eventData);
					}
					else
						EnterState_WaitingForPress();
				} 

				return;
			}

			_CurrentPressEventCamera = eventData.pressEventCamera;
			Vector2 draggedVHPosScreen = frame8.Logic.Misc.Other.UIUtils8.Instance.WorldToScreenPointForCanvas(_Canvas, eventData.pressEventCamera, RT.position);
			_DistancePointerToDraggedInCanvasSpace = draggedVHPosScreen - eventData.position;

			_State = StateEnum.DRAGGING;
			if (dragDropListener == null)
			{
				if (_OrphanedBundle == null)
					Debug.Log("OnBeginDrag: dragDropListener is null, but the item is not orphaned (_OrphanedBundle is null)");

				return;
			}

			dragDropListener.OnBeginDragItem(eventData);
		}

		void IDragHandler.OnDrag(PointerEventData eventData)
		{
			//Debug.Log("OnDrag" + eventData.button);
			if (eventData.button != PointerEventData.InputButton.Left
				|| _State != StateEnum.DRAGGING)
			{
				if (_State != StateEnum.BUSY_DELEGATING_DRAG_TO_PARENT)
					return;

				var casted = _ParentToDelegateDragEventsTo as IDragHandler;
				if (casted != null)
					casted.OnDrag(eventData);

				return;
			}

			_CurrentOnDragEventWorldPosition = eventData.position;

			Vector3 worldPoint;
			RectTransformUtility.ScreenPointToWorldPointInRectangle(
				_CanvasRT,
				CurrentOnDragEventWorldPosition + DistancePointerToDraggedInCanvasSpace,
				eventData.pressEventCamera,
				out worldPoint
			);
			RT.position = worldPoint;


			if (dragDropListener == null && _OrphanedBundle == null)
			{
				Debug.Log("OnBeginDrag: dragDropListener is null, but the item is not orphaned (_OrphanedBundle is null)");
				return;
			}

			var results = RaycastForDragDropListeners(_GraphicRaycaster, eventData);
			if (results.Count > 0)
			{
				// Just a visual feedback that another listener may accept this item
				if (dragDropListener == null || !results.Contains(dragDropListener))
					SetVisualMode(VisualMode.OVER_POTENTIAL_NEW_OWNER);
				else
					SetVisualMode(VisualMode.OVER_OWNER_OR_OUTSIDE);
			}
			else
				SetVisualMode(VisualMode.OVER_OWNER_OR_OUTSIDE);

			if (dragDropListener != null)
				dragDropListener.OnDraggedItem(eventData);
		}

		void IEndDragHandler.OnEndDrag(PointerEventData eventData)
		{
#if DEBUG_EVENTS
			Debug.Log("OnEndDrag: " + _State);
#endif
			if (eventData.button != PointerEventData.InputButton.Left
				|| _State != StateEnum.DRAGGING)
			{
				if (_State != StateEnum.BUSY_DELEGATING_DRAG_TO_PARENT)
					return;

				var casted = _ParentToDelegateDragEventsTo as IEndDragHandler;
				EnterState_WaitingForPress(); // prevent setting _ParentToDelegateDragEventsTo to null
				if (casted != null)
					casted.OnEndDrag(eventData);

				return;
			}

			var raycaster = _GraphicRaycaster;
			EnterState_WaitingForPress();
			DropAndCheckForOrphaned(eventData, raycaster);
		}
		#endregion


		void DropAndCheckForOrphaned(PointerEventData eventData, GraphicRaycaster raycaster)
		{
			if (dragDropListener == null && _OrphanedBundle == null)
				Destroy(gameObject);

			var wasOrphanedBeforeDrag = dragDropListener == null;
			if (wasOrphanedBeforeDrag || (_OrphanedBundle = dragDropListener.OnDroppedItem(eventData)) != null)
			{
				if (dragDropListener != null)
					throw new InvalidOperationException("When orphaned, dragDropListener should be set to null");

				// Find a listener among the raycasted ones, other that the current listener (since this is the listener that has orphaned the item anyway)
				var results = RaycastForDragDropListeners(raycaster, eventData);
				bool accepted = false;
				foreach (var listener in results)
				{
					if (!wasOrphanedBeforeDrag && _OrphanedBundle.previousOwner != null && listener == _OrphanedBundle.previousOwner)
						continue;
					accepted = listener.OnDroppedExternalItem(eventData, this);
					if (!accepted)
						continue;
				}

				if (accepted)
				{
					if (dragDropListener == null)
						throw new InvalidOperationException("When adopting an orphaned item, dragDropListener should be set to the new owner");

					_OrphanedBundle = null;
				}
				else
				{
					// Just wait for another press event
				}
			}
		}

		List<IDragDropListener> RaycastForDragDropListeners(GraphicRaycaster raycaster, PointerEventData eventData)
		{
			List<IDragDropListener> listeners = new List<IDragDropListener>();
			List<RaycastResult> results = new List<RaycastResult>();
			raycaster.Raycast(eventData, results);
			// Find a listener among the raycasted ones
			IDragDropListener listener;
			foreach (var res in results)
			{
				listener = res.gameObject.GetComponent(typeof(IDragDropListener)) as IDragDropListener;
				if (listener == null)
					continue;
				listeners.Add(listener);
			}

			return listeners;
		}


		public enum StateEnum
		{
			WAITING_FOR_PRESS,
			BUSY_DELEGATING_DRAG_TO_PARENT,
			PRESSING__WAITING_FOR_LONG_CLICK,
			AFTER_LONG_CLICK_DRAG_DECLINED__WAITING_TO_BEGIN_DRAG,
			AFTER_LONG_CLICK_DRAG_ACCEPTED__WAITING_TO_BEGIN_DRAG,
			DRAGGING,
		}

		enum VisualMode
		{
			NONE = 0,
			OVER_OWNER_OR_OUTSIDE = 1,
			OVER_POTENTIAL_NEW_OWNER = -1
		}


		public class OrphanedItemBundle
		{
			public IDragDropListener previousOwner;
			public object views;
			public object model;
		}


		/// <summary>Interface to implement by the class that'll handle the drag events</summary>
		public interface IDragDropListener
		{
			/// <summary> Returns if the item drag was accepted </summary>
			bool OnPrepareToDragItem(DraggableItem item);
			void OnBeginDragItem(PointerEventData eventData);
			void OnDraggedItem(PointerEventData eventData);
			/// <summary> Returns null if the object was accepted. Otherwise, an <see cref="OrphanedItemBundle"/> </summary>
			OrphanedItemBundle OnDroppedItem(PointerEventData eventData);
			/// <summary> Returns if the item was accepted </summary>
			bool OnDroppedExternalItem(PointerEventData eventData, DraggableItem orphanedItemWithBundle);
		}
	}
}