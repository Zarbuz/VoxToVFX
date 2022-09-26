using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine.EventSystems;

namespace Com.TheFallenGames.OSA.Util
{
    /// <summary>
    /// Utility to delegate the "long click" event to <see cref="LongClickListener"/>
    /// It requires a graphic component (can be an image with zero alpha) that can be clicked in order to receive OnPointerDown, OnPointerUp etc.
    /// No other UI elements should be on top of this one in order to receive pointer callbacks
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class LongClickableItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ICancelHandler
    {
        public float longClickTime = .7f;

        public IItemLongClickListener LongClickListener;
		public StateEnum State { get; private set; }

		private float mPressedTime;

		public enum StateEnum
		{
			NOT_PRESSING,
			PRESSING_WAITING_FOR_LONG_CLICK,
			PRESSING_AFTER_LONG_CLICK
		}
		private void Update()
        {
            if (State == StateEnum.PRESSING_WAITING_FOR_LONG_CLICK)
            {
                if (Time.unscaledTime - mPressedTime >= longClickTime)
                {
					State = StateEnum.PRESSING_AFTER_LONG_CLICK;
                    if (LongClickListener != null)
                        LongClickListener.OnItemLongClicked(this);
                }
            }
        }

        #region Callbacks from Unity UI event handlers
        public void OnPointerDown(PointerEventData eventData)
		{
			//Debug.Log("OnPointerDown" + eventData.button);
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			//_PointerID = eventData.pointerId;

			State = StateEnum.PRESSING_WAITING_FOR_LONG_CLICK;
            mPressedTime = Time.unscaledTime;
        }
        public void OnPointerUp(PointerEventData eventData)
		{
			//Debug.Log("OnPointerUp" + eventData.button);
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			State = StateEnum.NOT_PRESSING;
		}
        public void OnCancel(BaseEventData eventData)
		{
			//Debug.Log("OnCancel");
			State = StateEnum.NOT_PRESSING;
		}
        #endregion

        /// <summary>Interface to implement by the class that'll handle the long click events</summary>
        public interface IItemLongClickListener
        {
            void OnItemLongClicked(LongClickableItem longClickedItem);
        }
    }
}