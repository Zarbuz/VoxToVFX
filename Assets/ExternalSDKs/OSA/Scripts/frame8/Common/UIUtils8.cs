using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine.EventSystems;

namespace frame8.Logic.Misc.Other
{
	public class UIUtils8 : Singleton8<UIUtils8>
	{
		public Vector2 WorldToScreenPointForCanvas(Canvas canvas, Camera camera, Vector3 position)
		{
			var camIfNonOverlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera;
			return RectTransformUtility.WorldToScreenPoint(camIfNonOverlay, position);
		}

		public Vector2? WorldToCanvasLocalPosition(Canvas canvas, RectTransform canvasRect, Camera camera, Vector3 position)
		{
			var camIfNonOverlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera;
			Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camIfNonOverlay, position);
			Vector2 result;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, camIfNonOverlay, out result))
				return null;

			return result;
		}

		public Vector2? ScreenPointToLocalPointInCanvas(Canvas canvas, RectTransform canvasRect, Camera camera, Vector2 screenPoint)
		{
			var camIfNonOverlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera;
			Vector2 result;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, camIfNonOverlay, out result))
				return null;

			return result;
		}

		public Vector3? ScreenPointToWorldPointInCanvas(Canvas canvas, RectTransform canvasRect, Camera camera, Vector2 screenPoint)
		{
			var camIfNonOverlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera;
			Vector3 result;
			if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screenPoint, camIfNonOverlay, out result))
				return null;

			return result;
		}

		//public Vector2? WorldToCanvasAdjustedScreenPosition(Canvas canvas, RectTransform canvasRect, Camera camera, Vector3 position)
		//{
		//	Vector2? result = WorldToCanvasLocalPosition(canvas, canvasRect, camera, position);
		//	if (result == null)
		//		return null;

		//	// Result is in the localSpace of the canvas now. The wanted position should be in "canvas-adjusted" screen space, 
		//	// which for 800x600 is simply [bottom-left: (0, 0), top-right:(800, 600)]-space, which is actually the WORLD space, regardless of canvas.renderMode.
		//	// Transforming to local space first was needed because RectTransformUtility handles 
		//	// RenderMode.ScreenSpaceOverlay differently: RectTransformUtility.ScreenPointToRay with camera=null subtracts 100f from the canvas' world position 
		//	// and some other stuff to bring mathematical calculations to the same form as if render mode was ScreenSpaceCamera
		//	return canvas.transform.TransformPoint(result.Value);
		//}

		public Vector2 ScreenPointToLocalPointInRectangle(RectTransform rt, PointerEventData pointerEventData)
		{
			Vector2 curLocalPos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, pointerEventData.position, pointerEventData.pressEventCamera, out curLocalPos);
			return curLocalPos;
		}

		/// <summary>
		/// Format of pivots:
		/// Vertically (y), 0=bottom, 1=top;
		/// Horizontally (x), 0=left, 1=right
		/// </summary>
		public Vector2 GetWorldVectorBetweenCustomLocalPivots(
			RectTransform firstRT,
			Vector2 customPivotOnFirstRect01,
			RectTransform secondRT,
			Vector2 customPivotOnSecondRect01,
			Canvas canvas,
			RectTransform canvasRectTransform // gives you the opportunity to cache it, useful for frequent calls
		)
		{
			var localPointA = firstRT.ConvertPointNormalizedBySizeToLocalPoint(customPivotOnFirstRect01);
			var localPointB = secondRT.ConvertPointNormalizedBySizeToLocalPoint(customPivotOnSecondRect01);

			var worldA = firstRT.TransformPoint(localPointA);
			var worldB = secondRT.TransformPoint(localPointB);

			Vector2 signedDistance = worldB - worldA;

			return signedDistance;
		}

		public void BringRectToAnchorsFor(Transform[] transforms)
		{
			foreach (Transform tr in transforms)
				BringRectToAnchorsFor(tr);
		}

		public void BringRectToAnchorsFor(Transform transform)
		{
			RectTransform rt = transform as RectTransform;
			if (rt != null)
				rt.offsetMin = rt.offsetMax = Vector2.zero;
		}

		public void BringAnchorsToRectFor(Transform[] transforms)
		{
			foreach (Transform tr in transforms)
				BringAnchorsToRectFor(tr);
		}

		public void BringAnchorsToRectFor(Transform transform)
		{
			RectTransform parentRT;
			RectTransform rt = transform as RectTransform;
			if (transform != null)
			{
				parentRT = rt.parent as RectTransform;
				if (parentRT == null) // rt is the root
				{
					rt.anchorMin = Vector2.zero;
					rt.anchorMax = Vector2.one;
				}
				else
				{
					Rect rect = rt.rect;
					Rect parentRect = parentRT.rect;
					Vector2 dispFromParentBottomLeftCorner =
						rt.GetBottomLeftCornerDisplacementFromParentBottomLeftCorner();

					if (parentRect.width > 0f && parentRect.height > 0f)
					{
						rt.anchorMin =
							new Vector2(
								Mathf.Clamp01(dispFromParentBottomLeftCorner.x / parentRect.width),
								Mathf.Clamp01(dispFromParentBottomLeftCorner.y / parentRect.height));

						rt.anchorMax =
							new Vector2(
								Mathf.Clamp01((dispFromParentBottomLeftCorner.x + rect.width) / parentRect.width),
								Mathf.Clamp01((dispFromParentBottomLeftCorner.y + rect.height) / parentRect.height));
					}
				}
				rt.offsetMin = rt.offsetMax = Vector2.zero;
			}
		}

		// Assuming <rectTransform> has a parent
		public void GrowSizeDirectionally(RectTransform rectTransform, RectTransform.Edge edge, float amount)
		{
			Vector2 currentSize = rectTransform.rect.size;
			//		Debug.Log("currentSize=" + currentSize);
			//RectTransform parentRectTransform = rectTransform.parent as RectTransform;
			//Vector2 parentSize = parentRectTransform.rect.size;
			//		Debug.Log("parentSize=" + parentSize);
			//Vector2 vParentBottomLeftCornerToBottomLeftCorner = rectTransform.GetBottomLeftCornerDisplacementFromParentBottomLeftCorner();
			//		Debug.Log("vParentBottomLeftCornerToBottomLeftCorner=" + vParentBottomLeftCornerToBottomLeftCorner);
			//Vector2 vUpRightCornerToParrentUpRightCorner = parentSize - (vParentBottomLeftCornerToBottomLeftCorner + currentSize);
			//		Debug.Log("vUpRightCornerToParrentUpRightCorner=" + vUpRightCornerToParrentUpRightCorner);
			//Vector2 anchorMin = rectTransform.anchorMin, anchorMax = rectTransform.anchorMax;
			//float inset, finalSize;

			// ... so we'll write "SetInsetAndSizeFromParentEdge" only one time below
			//		switch (edge)
			//		{
			//		// Move the left edge
			//		case RectTransform.Edge.Left:
			//			edge = RectTransform.Edge.Right;
			//			inset = vUpRightCornerToParrentUpRightCorner.x;
			//			finalSize = currentSize.x + amount;
			//			Debug.Log(0);
			//			break;
			//			
			//		case RectTransform.Edge.Top:
			//			edge = RectTransform.Edge.Bottom;
			//			inset = vParentBottomLeftCornerToBottomLeftCorner.y;
			//			finalSize = currentSize.y + amount;
			//			Debug.Log(1);
			//			break;
			//			
			//		case RectTransform.Edge.Right:
			//			edge = RectTransform.Edge.Left;
			//			inset = vParentBottomLeftCornerToBottomLeftCorner.x;
			//			finalSize = currentSize.x + amount;
			//			Debug.Log(2);
			//			break;
			//			
			//		default:
			//			edge = RectTransform.Edge.Top;
			//			inset = vUpRightCornerToParrentUpRightCorner.y;
			//			finalSize = currentSize.y + amount;
			//			Debug.Log(3);
			//			break;
			//		}
			//		Debug.Log(edge+";"+inset+";"+finalSize+";");
			//		rectTransform.SetInsetAndSizeFromParentEdge(edge, inset, finalSize);
			//		rectTransform.anchorMin = anchorMin;
			//		rectTransform.anchorMax = anchorMax;
			bool edgeRight = edge == RectTransform.Edge.Right;
			bool horizontal = edgeRight || edge == RectTransform.Edge.Left;
			bool edgeTop = edge == RectTransform.Edge.Top;
			rectTransform.SetSizeWithCurrentAnchors(horizontal ?
					RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical, horizontal ? currentSize.x + amount : currentSize.y + amount);

			Vector3 localPosDiplacementToAdd = Vector3.zero;
			if (horizontal)
			{
				if (edgeRight)
					localPosDiplacementToAdd.x += amount / 2;
				else
					localPosDiplacementToAdd.x -= amount / 2;
			}
			else
			{
				if (edgeTop)
					localPosDiplacementToAdd.y += amount / 2;
				else
					localPosDiplacementToAdd.y -= amount / 2;
			}

			rectTransform.localPosition += localPosDiplacementToAdd;
		}

		// Taken from https://answers.unity.com/questions/1221847/get-position-of-specific-letter-in-ui-text.html
		// Order: top-left, top-right etc
		public UIVertex[] GetCharQuadVertsInObjectSpaceUnscaledByCanvas(Text textComp, int charIndex)
		{
			UIVertex[] result = null;
			string text = textComp.text;

			if (charIndex < text.Length)
			{
				TextGenerator textGen = textComp.cachedTextGenerator;
				int vertCountPerQuad = 4;
				int indexOfTextQuadFirstVertex = (charIndex * vertCountPerQuad);
				if (indexOfTextQuadFirstVertex < textGen.vertexCount)
				{

					result = new UIVertex[4];
					int i = -1;
					var verts = textGen.verts;
					result[++i] = verts[indexOfTextQuadFirstVertex + i];
					result[++i] = verts[indexOfTextQuadFirstVertex + i];
					result[++i] = verts[indexOfTextQuadFirstVertex + i];
					result[++i] = verts[indexOfTextQuadFirstVertex + i];
				}
			}

			return result;
		}

		public void GetUITextCharQuadVertsPositionsInObjectSpaceScaledByCanvas(Text textComp, int charIndex, out Vector3[] positions)
		{
			var vertices = GetCharQuadVertsInObjectSpaceUnscaledByCanvas(textComp, charIndex);
			if (vertices == null)
			{
				positions = null;
				return;
			}

			positions = new Vector3[vertices.Length];
			var cScaleFactor = textComp.canvas.scaleFactor;
			for (int i = 0; i < positions.Length; i++)
				positions[i] = vertices[i].position / cScaleFactor;
		}

		public Vector3[] GetCharQuadVertsPositionsInObjectSpaceScaledByCanvas(Text textComp, int charIndex)
		{
			Vector3[] positions;
			GetUITextCharQuadVertsPositionsInObjectSpaceScaledByCanvas(textComp, charIndex, out positions);

			return positions;
		}

		public Vector3[] GetCharQuadVertsPositionsInWorldSpaceScaledByCanvas(Text textComp, int charIndex)
		{
			Vector3[] positions = GetCharQuadVertsPositionsInObjectSpaceScaledByCanvas(textComp, charIndex);
			if (positions != null)
			{
				int i = 0;
				var tr = textComp.transform;
				positions[i] = tr.TransformPoint(positions[i++]);
				positions[i] = tr.TransformPoint(positions[i++]);
				positions[i] = tr.TransformPoint(positions[i++]);
				positions[i] = tr.TransformPoint(positions[i++]);
			}

			return positions;
		}

		public Vector3? GetCharQuadCenterPositionInObjectSpaceScaledByCanvas(Text textComp, int charIndex)
		{
			Vector3? res = null;
			Vector3[] positions = GetCharQuadVertsPositionsInObjectSpaceScaledByCanvas(textComp, charIndex);
			if (positions != null)
				res = (positions[0] + positions[1] + positions[2] + positions[3]) / 4f;

			return res;
		}

		public Vector3? GetCharQuadCenterPositionInWorldSpaceScaledByCanvas(Text textComp, int charIndex)
		{
			Vector3? res = null;
			Vector3[] positions = GetCharQuadVertsPositionsInWorldSpaceScaledByCanvas(textComp, charIndex);
			if (positions != null)
				res = (positions[0] + positions[1] + positions[2] + positions[3]) / 4f;

			return res;
		}

		public Vector3? GetCharQuadCenterLeftPositionInWorldSpaceScaledByCanvas(Text textComp, int charIndex)
		{
			Vector3? res = null;
			Vector3[] positions = GetCharQuadVertsPositionsInWorldSpaceScaledByCanvas(textComp, charIndex);
			if (positions != null)
				res = (positions[0] + positions[3]) / 2f;

			return res;
		}

		public override void Init()
		{
		}
	}
}

