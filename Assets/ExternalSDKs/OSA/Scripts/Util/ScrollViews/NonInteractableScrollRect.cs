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
	public class NonInteractableScrollRect : ScrollRect
	{
		public override void OnInitializePotentialDrag(PointerEventData eventData) { }
		public override void OnBeginDrag(PointerEventData eventData) { }
		public override void OnDrag(PointerEventData eventData) { }
		public override void OnEndDrag(PointerEventData eventData) { }
		public override void OnScroll(PointerEventData data) { }
	}
}