using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;
using UnityEngine.EventSystems;

namespace Com.TheFallenGames.OSA.Util.ScrollViews
{
	/// <summary>
	/// Provides access to a Unity's ScrollRect through <see cref="IScrollRectProxy"/>.
	/// For example, it can be added to a regular ScrollRect so <see cref="ScrollbarFixer8"/> can communicate with it, in case you want to use the <see cref="ScrollbarFixer8"/> without OSA.
	/// </summary>
	[RequireComponent(typeof(ScrollRect))]
	public class UnityScrollRectProxy : MonoBehaviour, IScrollRectProxy
	{
		#region IScrollRectProxy properties implementation
		public bool IsInitialized { get { return ScrollRect != null; } }
		public Vector2 Velocity { get { return ScrollRect.velocity; } set { ScrollRect.velocity = value; } }
		public bool IsHorizontal { get { return ScrollRect.horizontal; } }
		public bool IsVertical { get { return ScrollRect.vertical; } }
		public RectTransform Content { get { return ScrollRect.content; } }
		public RectTransform Viewport { get { return ScrollRect.viewport; } }
		#endregion

		ScrollRect ScrollRect { get { if (!_ScrollRect) _ScrollRect = GetComponent<ScrollRect>(); return _ScrollRect; } }
		ScrollRect _ScrollRect;


		void Awake()
		{
			if (ScrollRect == null)
				throw new UnityException(GetType().Name + ": No ScrollRect component found");
		}


		#region IScrollRectProxy methods implementation
#pragma warning disable 0067
		public event System.Action<double> ScrollPositionChanged;
#pragma warning restore 0067
		public void SetNormalizedPosition(double normalizedPosition) { if (IsHorizontal) ScrollRect.horizontalNormalizedPosition = (float)normalizedPosition; else ScrollRect.verticalNormalizedPosition = (float)normalizedPosition; }
		public double GetNormalizedPosition() { return IsHorizontal ? ScrollRect.horizontalNormalizedPosition : ScrollRect.verticalNormalizedPosition; }
		public double GetContentSize() { return IsHorizontal ? Content.rect.width : Content.rect.height; }
		public double GetViewportSize() { return IsHorizontal ? Viewport.rect.width : Viewport.rect.height; }
		public void StopMovement() { ScrollRect.StopMovement(); }
		#endregion
	}
}