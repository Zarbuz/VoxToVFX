using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Com.TheFallenGames.OSA.Util
{
	/// <summary>
	/// Utility to expand an item when it's clicked, dispatching the size change request via <see cref="ISizeChangesHandler"/> for increased flexibility
	/// Known issue when used with OSA: when during collapsing the item goes outside viewport, the animation stales, since the views are recycled. 
	/// This can be solved by having a separate resizing utility script that's not attached to a recycling-prone object.
	/// </summary>
	public class ExpandCollapseOnClick : MonoBehaviour
    {
		/// <summary>
		/// The button to whose onClock to subscribe. If not specified, will try to GetComponent&lt;Button&gt; from the GO containing this script 
		/// </summary>
		[Tooltip("will be taken from this object, if not specified")]
        public Button button = null;

        /// <summary>When expanding, the initial size will be <see cref="nonExpandedSize"/> and the target size will be <see cref="nonExpandedSize"/> x <see cref="expandFactor"/>; opposite is true when collapsing</summary>
		[NonSerialized] // must be set through code
        public float expandFactor = 2f;

        /// <summary>The duration of the expand(or collapse) animation</summary>
        public float animDuration = .2f;

		public NonExpandedSizeSource nonExpandedSizeSource = NonExpandedSizeSource.WAIT_FOR_EXTERNAL;

		[Tooltip("Used in conjunction with NonExpandedSizeSource.PREDEFINED")]
		public float nonExpandedSizePredefined = 0f;

		public bool useUnscaledTime = true;

		/// <summary>This is the size from which the item will start expanding</summary>
		[HideInInspector]
        public float nonExpandedSize = -1f;

        /// <summary>This keeps track of the 'expanded' state. If true, on click the animation will set <see cref="nonExpandedSize"/> as the target size; else, <see cref="nonExpandedSize"/> x <see cref="expandFactor"/> </summary>
        [HideInInspector]
        public bool expanded = false;

		[Tooltip("Returns a value between 0 and 1, 1 meaning end of the progress")]
		[FormerlySerializedAs("onExpandAmounChanged")] // correcting typo from pre-4.0 versions
		public UnityFloatEvent onExpandAmountChanged = null;

		[Tooltip("Returns a value between nonExpandedSize and nonExpandedSize * expandFactor")]
		public UnityFloatEvent onExpandSizeChanged = null;

		[Obsolete("Use onExpandAmountChanged, instead", true)]
		[HideInInspector] // just to be sure unity's serialization system won't behave buggy
		public UnityFloatEvent onExpandAmounChanged { get { return onExpandAmountChanged; } }

		float Time { get { return useUnscaledTime ? UnityEngine.Time.unscaledTime : UnityEngine.Time.time; } }

		float startSize;
        float endSize;
        float animStart;
        //float animEnd;
        bool animating = false;
        RectTransform rectTransform;

        public ISizeChangesHandler sizeChangesHandler;

		public enum NonExpandedSizeSource
		{
			WAIT_FOR_EXTERNAL,
			SELF_HEIGHT,
			SELF_WIDTH,
			PREDEFINED
		}


		void Awake()
        {
            rectTransform = transform as RectTransform;

			if (button == null)
                button = GetComponent<Button>();

			if (button)
				button.onClick.AddListener(OnClicked);
        }

		void Start()
		{
			if (nonExpandedSizeSource == NonExpandedSizeSource.SELF_HEIGHT)
				nonExpandedSize = rectTransform.rect.height;
			else if (nonExpandedSizeSource == NonExpandedSizeSource.SELF_WIDTH)
				nonExpandedSize = rectTransform.rect.width;
			else if (nonExpandedSizeSource == NonExpandedSizeSource.PREDEFINED)
				nonExpandedSize = nonExpandedSizePredefined;
		}

        public void OnClicked()
        {
            if (animating)
                return;

            if (nonExpandedSize < 0f)
                return;

            animating = true;
            animStart = Time;
            //animEnd = animStart + animDuration;

            if (expanded) // shrinking
            {
                startSize = nonExpandedSize * expandFactor;
                endSize = nonExpandedSize;
            }
            else // expanding
            {
                startSize = nonExpandedSize;
                endSize = nonExpandedSize * expandFactor;
            }
        }


        void Update()
        {
            if (animating)
            {
                float elapsedTime = Time - animStart;
                float t01 = elapsedTime / animDuration;
				if (t01 >= 1f) // done
				{
					t01 = 1f; // fill/clamp animation
					animating = false;
				}
				else
					t01 = Mathf.Sqrt(t01); // fast-in, slow-out effect

				float size = Mathf.Lerp(startSize, endSize, t01);
                if (sizeChangesHandler == null)
				{
					//// debug
					//rectTransform.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge.Top, rectTransform.GetInsetFromParentTopEdge(rectTransform.parent as RectTransform), size);
					if (t01 == 0f) // done
						expanded = !expanded;
				}
				else
                {
                    bool accepted = sizeChangesHandler.HandleSizeChangeRequest(rectTransform, size);

                    // Interruption
                    if (!accepted)
                        animating = false;

                    if (!animating) // done; even if it wasn't accepted, wether we should or shouldn't change the "expanded" state depends on the user's requirements. We chose to change it
                    {
                        expanded = !expanded;
                        sizeChangesHandler.OnExpandedStateChanged(rectTransform, expanded);
					}
				}


				if (onExpandAmountChanged != null)
					onExpandAmountChanged.Invoke(t01);

				if (onExpandSizeChanged != null)
					onExpandSizeChanged.Invoke(size);
			}
        }

        /// <summary>Interface to implement by the class that'll handle the size changes when the animation runs</summary>
        public interface ISizeChangesHandler
        {
            /// <summary>Called each frame during animation</summary>
            /// <param name="rt">The animated RectTransform</param>
            /// <param name="newSize">The requested size</param>
            /// <returns>If it was accepted</returns>
            bool HandleSizeChangeRequest(RectTransform rt, float newSize);

            /// <summary>Called when the animation ends and the item successfully expanded (<paramref name="expanded"/> is true) or collapsed (else)</summary>
            /// <param name="rt">The animated RectTransform</param>
            /// <param name="expanded">true if the item expanded. false, if collapsed</param>
            void OnExpandedStateChanged(RectTransform rt, bool expanded);
        }

		[Serializable]
		public class UnityFloatEvent : UnityEvent<float> { }
    }
}