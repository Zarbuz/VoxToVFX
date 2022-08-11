using System;
using UnityEngine;

namespace frame8.Logic.Misc.Visual.UI
{
	/// <summary> A delegate used to communicate with a ScrollRect-like component, even if it's not derived from UnityEngine.UI.ScrollRect</summary>
	public interface IScrollRectProxy
	{
		/// <summary>The float parameter has the same format as described in <see cref="SetNormalizedPosition(double)"/></summary>
		event Action<double> ScrollPositionChanged;

		/// <summary>Tha game object which represents the scroll rect. Lowercase, so that monobehaviours won't be forced override it, as they already have this property</summary>
		UnityEngine.GameObject gameObject { get; }

		/// <summary>Whether this scroll rect proxy can be used</summary>
		bool IsInitialized { get; }

		/// <summary>The velocity of the content panel in local UI space (from left to right = positive, from bottom to top = positive)</summary>
		UnityEngine.Vector2 Velocity { get; set; }
		bool IsHorizontal { get; }
		bool IsVertical { get; }

		RectTransform Content { get; }

		RectTransform Viewport { get; }

		/// <summary><paramref name="normalizedPosition"/> is exactly the same as the ScrollRect.horizontalNormalizedPosition, if the ScrollRect is horizontal (ScrollRect.verticalNormalizedPosition, else) </summary>
		void SetNormalizedPosition(double normalizedPosition);

		/// <summary>See <see cref="SetNormalizedPosition(double)"/></summary>
		double GetNormalizedPosition();

		/// <summary>The width of the content panel, if the ScrollRect is horizontal (the height, else)</summary>
		double GetContentSize();

		/// <summary>See <see cref="GetContentSize"/></summary>
		double GetViewportSize();

		void StopMovement();
	}


	public static class IScrollRectProxyExtensions
	{
		public static double GetScrollableArea(this IScrollRectProxy proxy)
		{
			return proxy.GetContentSize() - proxy.GetViewportSize();
		}

		public static double GetContentSizeToViewportRatio(this IScrollRectProxy proxy)
		{
			return proxy.GetContentSize() / proxy.GetViewportSize();
		}
		
	}
}
