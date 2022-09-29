using UnityEngine;
using System;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine.EventSystems;
using frame8.Logic.Misc.Other;
using UnityEngine.Serialization;

namespace Com.TheFallenGames.OSA.Util
{
	public class RectTransformEdgeDragger : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
	{
		public event Action TargetDragged;

		[FormerlySerializedAs("draggedRectTransform")]
		[SerializeField]
		RectTransform _DraggedRectTransform = null;
		[FormerlySerializedAs("draggedEdge")]
		[SerializeField]
		RectTransform.Edge _DraggedEdge = RectTransform.Edge.Left;
		[SerializeField]
		RectTransform _StartPoint = null;
		[SerializeField]
		RectTransform _EndPoint = null;
		[SerializeField]
		[Tooltip("Set to false if the dragger will be automatically dragged by the exact same amount, as a result of being a direct child of the dragged recttransform")]
		bool _DragSelf = true;
		[SerializeField]
		float _DraggedRectTransformMinSize = 1f;
		[SerializeField]
		float _DraggedRectTransformMaxSize = 0f;

		public RectTransform DraggedRectTransform { get { return _DraggedRectTransform; } }
		public float DragNormalizedAmount { get { return GetNormPosOnDraggingSegment(GetVEndpointStartToMe()); } }

		float DragAreaSize { get { return Vector3.Distance(_StartPoint.localPosition, _EndPoint.localPosition); } }

		RectTransform _RT;
		RectTransform _MyParent;
		Vector2 _StartDragPosInMySpace;
		//Vector2 _MyInitialLocalPos;
		float _MyInitialInset;
		float _DraggedRTInitialInset;
		//float _DraggedRTStartInset;
		//float _DraggedRTStartSize;
		float _DraggedRTInitialSize;
		Canvas _Canvas;
		bool _Dragging;


		void Awake()
		{
			_RT = (transform as RectTransform);
			_MyParent = _RT.parent as RectTransform;

			if (_StartPoint.parent != _MyParent || _EndPoint.parent != _MyParent)
				throw new UnityException("_StartPoint and _EndPoint should have the same parent as the dragger");

			// Get the root canvas
			var c = _Canvas = transform.parent.GetComponentInParent<Canvas>();
			while (c && c.transform.parent)
			{
				_Canvas = c;
				c = c.transform.parent.GetComponentInParent<Canvas>();
			}
		}

		void Start()
		{
			//_MyInitialLocalPos = _RT.localPosition;
			Reinitialize();

			//SetNormalizedPosition(0);
		}

		public void Reinitialize()
		{
			_MyInitialInset = GetMyCurrentInsetFromDraggedEdge();
			_DraggedRTInitialInset = GetDraggedRTCurrentInsetFromDraggedEdge();
			_DraggedRTInitialSize = GetRTSize(_DraggedRectTransform);
		}

		void IPointerDownHandler.OnPointerDown(PointerEventData ped)
		{
			var localPos = UIUtils8.Instance.WorldToCanvasLocalPosition(_Canvas, _RT.parent as RectTransform, Camera.main, _RT.position);
			_Dragging = localPos != null;
			if (!_Dragging)
				return;

			_StartDragPosInMySpace = localPos.Value;

			if (!_DragSelf)
			{
				Reinitialize();
			}
			//_DraggedRTStartInset = GetDraggedRTCurrentInsetFromDraggedEdge();
			//_DraggedRTStartSize = GetRTSize(_DraggedRectTransform);
		}

		void IDragHandler.OnDrag(PointerEventData ped)
		{
			if (!_Dragging)
				return;

			var cam = ped.pressEventCamera;
			Vector2 posInMySpace;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_MyParent, ped.position, cam, out posInMySpace))
				return;

			Vector2 pressPosInMySpace;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_MyParent, ped.pressPosition, cam, out pressPosInMySpace))
				return;

			var dragVectorInMySpace = posInMySpace - pressPosInMySpace;

			var parentOfDragged = _DraggedRectTransform.parent as RectTransform;
			Vector2 posInDraggedRTSpace;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentOfDragged, ped.position, cam, out posInDraggedRTSpace))
				return;

			Vector2 pressPosInDraggedRTSpace;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentOfDragged, ped.pressPosition, cam, out pressPosInDraggedRTSpace))
				return;

			//var dragVectorInDraggedRTSpace = posInDraggedRTSpace - pressPosInDraggedRTSpace;

			var rtNewPos = _StartDragPosInMySpace;
			var rtNewPosUnclamped = _StartDragPosInMySpace;
			rtNewPosUnclamped += dragVectorInMySpace;

			//float amount;
			//float rectMoveAmount;
			float _DraggedRTInsetDelta;
			if (_DraggedEdge == RectTransform.Edge.Left || _DraggedEdge == RectTransform.Edge.Right)
			{
				rtNewPos.x += dragVectorInMySpace.x;
				_DraggedRTInsetDelta = dragVectorInMySpace.x * (_DraggedEdge == RectTransform.Edge.Left ? 1f : -1f);
			}
			else
			{
				rtNewPos.y += dragVectorInMySpace.y;
				_DraggedRTInsetDelta = dragVectorInMySpace.y * (_DraggedEdge == RectTransform.Edge.Bottom ? 1f : -1f);
			}
			float normPos = GetNormPosOnDraggingSegment(GetVEndPointStartTo(rtNewPosUnclamped));
			if (_DragSelf)
			{
				SetNormalizedPosition(normPos, true);
			}
			else
			{
				//Debug.Log(normPos);
				//// TODO see why normPos reports at boundary when it actually isn't
				//if (normPos == 0f)
				//{
				//	if (_DraggedRTInsetDelta > 0)
				//		return;
				//}
				//else if (normPos == 1f)
				//{
				//	if (_DraggedRTInsetDelta < 0)
				//		return;
				//}

				float newInset = _DraggedRTInitialInset + _DraggedRTInsetDelta;
				float newSize = _DraggedRTInitialSize - _DraggedRTInsetDelta;
				if (newSize < _DraggedRectTransformMinSize)
				{
					float excess = _DraggedRectTransformMinSize - newSize;
					newInset -= excess;
					newSize += excess;
				}

				if (_DraggedRectTransformMaxSize != 0f && newSize > _DraggedRectTransformMaxSize)
				{
					float excess = newSize - _DraggedRectTransformMaxSize;
					newInset += excess;
					newSize -= excess;
				}

				SetDraggedRTInsetAndSize(newInset, newSize);
			}

			if (TargetDragged != null)
				TargetDragged();
		}

		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
			if (!_Dragging)
				return;
			// TODO test if this is still needed
			//Reinitialize();
		}

		Vector2 GetVEndpointStartToMe() { return GetVEndPointStartTo(_RT.localPosition); }

		Vector2 GetVEndPointStartTo(Vector2 localPoint)
		{
			Vector2 endV2 = _EndPoint.localPosition;
			return localPoint - endV2;
		}

		/// <summary>
		/// Segment start is the end point (it was at the bottom on the moment of the implementation and it was easier to visualize this way)
		/// </summary>
		float GetNormPosOnDraggingSegment(Vector2 vSegmentStartToPoint)
		{
			Vector2 endV2 = _EndPoint.localPosition;
			Vector2 startV2 = _StartPoint.localPosition;

			// O = end point, A = my pos, B = start point
			var oa = vSegmentStartToPoint;
			var ob = startV2 - endV2;
			return GetNormPosOnSegment(ob, oa);
		}

		float GetNormPosOnSegment(Vector2 segmentVector, Vector2 vSegmentStartToPoint)
		{
			var oa = vSegmentStartToPoint;
			var ob = segmentVector;
			var obNorm = ob / ob.magnitude;
			var oaInOBSpace = oa / ob.magnitude; // i.e. considering ob as unit vector

			//float dot = Vector2.Dot(oaNorm, obNorm);
			float dot = Vector2.Dot(obNorm, oaInOBSpace);

			float normPos = 1f - Mathf.Clamp01(dot);

			return normPos;
		}

		public void SetNormalizedPosition(float normalizedPos, bool updateDraggedRT)
		{
			//var prevLocalPos = transform.localPosition;
			//Debug.Log("SetNormalizedPosition " + normalizedPos);
			transform.position = Vector3.Lerp(_StartPoint.position, _EndPoint.position, normalizedPos);
			if (updateDraggedRT)
				UpdateDraggedRTFromDraggerPos();

			// Commented: doesn't work very well in the current form
			//if (!_DragSelf)
			//	transform.localPosition = prevLocalPos;
		}

		void UpdateDraggedRTFromDraggerPos()
		{
			float myCurrentInset = GetMyCurrentInsetFromDraggedEdge();
			float deltaInset = myCurrentInset - _MyInitialInset;
			SetDraggedRTInsetAndSize(_DraggedRTInitialInset + deltaInset, _DraggedRTInitialSize - deltaInset);
		}

		float GetDraggedRTCurrentInsetFromDraggedEdge() { return GetRTCurrentInsetFromDraggedEdge(_DraggedRectTransform); }
		float GetMyCurrentInsetFromDraggedEdge() { return GetRTCurrentInsetFromDraggedEdge(_RT); }
		float GetRTCurrentInsetFromDraggedEdge(RectTransform rt) { return rt.GetInsetFromParentEdge(rt.parent as RectTransform, _DraggedEdge); }
		float GetRTSize(RectTransform rt)
		{
			float s;
			if (_DraggedEdge == RectTransform.Edge.Left || _DraggedEdge == RectTransform.Edge.Right)
				s = _DraggedRectTransform.rect.width;
			else
				s = _DraggedRectTransform.rect.height;

			return s;
		}

		void SetDraggedRTInsetAndSize(float inset, float size)
		{
			_DraggedRectTransform.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(_DraggedEdge, inset, size);
		}
	}
}
