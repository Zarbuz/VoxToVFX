//using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace frame8.Logic.Misc.Visual.UI.MonoBehaviours
{
	/// <summary>
	/// <para>Fixes ScrollView inertia when the content grows too big. The default method cuts off the inertia in most cases.</para>
	/// <para>Attach it to the Scrollbar and make sure no scrollbars are assigned to the ScrollRect</para>
	/// <para>It also contains a lot of other silky-smooth features</para>
	/// </summary>
	[RequireComponent(typeof(Scrollbar))]
    public class ScrollbarFixer8 : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler, IScrollRectProxy
	{
        public bool hideWhenNotNeeded = true;
        public bool autoHide = true;

		[Tooltip("A CanvasGroup will be added to the Scrollbar, if not already present, and the fade effect will be achieved by changing its alpha property")]
		public bool autoHideFadeEffect = true;

		[Tooltip("The collapsing effect will change the localScale of the Scrollbar, so the pivot's position decides in what direction it'll grow/shrink.\n " +
				 "Note that sometimes a really nice effect is achieved by placing the pivot slightly outside the rect (the minimized scrollbar will move outside while collapsing)")]
        public bool autoHideCollapseEffect = true;

		[Tooltip("Used if autoHide is on. Duration in seconds")]
        public float autoHideTime = 1f;

		public float autoHideFadeEffectMinAlpha = .8f;
		public float autoHideCollapseEffectMinScale = .2f;

		[Range(0.01f, 1f)]
        public float minSize = .1f;

        [Range(0.015f, 2f)]
		public float sizeUpdateInterval = .05f;

		[Tooltip("Used to prevent updates to be processed too often, in case this is a concern")]
		public int skippedFramesBetweenPositionChanges;

        [Tooltip("If not assigned, will try to find a ScrollRect or an IScrollRectProxy in the parent")]
        public ScrollRect scrollRect;

        [Tooltip("If not assigned, will use the resolved scrollRect")]
        public RectTransform viewport;

		public UnityEvent OnScrollbarSizeChanged;

		/// <summary>
		/// Will be retrieved from the scrollrect. If not found, it can be assigned anytime before the first Update. 
		/// If not assigned, a default proxy will be used. The purpose of this is to allow custom implementations of ScrollRect to be used
		/// </summary>
		public IScrollRectProxy externalScrollRectProxy
		{
			get { return _ExternalScrollRectProxy; }
			set
			{
				_ExternalScrollRectProxy = value;

				if (_ExternalScrollRectProxy != null)
				{
					if (scrollRect)
					{
						scrollRect.onValueChanged.RemoveListener(ScrollRect_OnValueChangedCalled);
						scrollRect = null;
					}
				}
			}
		}
		IScrollRectProxy _ExternalScrollRectProxy;

		#region IScrollRectProxy properties implementation
		public bool IsInitialized { get { return scrollRect != null; } }
		public Vector2 Velocity { get; set; }
		public bool IsHorizontal { get { return scrollRect.horizontal; } }
		public bool IsVertical { get { return scrollRect.vertical; } }
		public RectTransform Content { get { return scrollRect.content; } }
		public RectTransform Viewport { get { return scrollRect.viewport; } }
		#endregion

		public bool IsDragging { get { return _Dragging; } }

		/// <summary> Using Scaled time for a scrollbar's animation doesn't make too much sense, so we're always using unscaledTime</summary>
		float Time { get { return UnityEngine.Time.unscaledTime; } }

		IScrollRectProxy ScrollRectProxy { get { return externalScrollRectProxy == null ? this : externalScrollRectProxy; } }

		const float HIDE_EFFECT_START_DELAY_01 = .4f; // relative to this.autoHideTime

		RectTransform _ScrollViewRT;
        Scrollbar _Scrollbar;
		CanvasGroup _CanvasGroupForFadeEffect;
        bool _HorizontalScrollBar;
        Vector3 _InitialScale = Vector3.one;
        bool _Hidden, _AutoHidden, _HiddenNotNeeded;
		double _LastValue;
        float _TimeOnLastValueChange;
        bool _Dragging;
        IEnumerator _SlowUpdateCoroutine;
		float _TransversalScaleOnLastDrag, _AlphaOnLastDrag;
		bool _FullyInitialized;
		int _FrameCountOnLastPositionUpdate;
		bool _TriedToCallOnScrollbarSizeChangedAtLeastOnce;


		void Awake()
        {
			if (autoHideTime == 0f)
				autoHideTime = 1f;

			_Scrollbar = GetComponent<Scrollbar>();
			_InitialScale = _Scrollbar.transform.localScale;
            _LastValue = _Scrollbar.value;
            _TimeOnLastValueChange = Time;
            _HorizontalScrollBar = _Scrollbar.direction == Scrollbar.Direction.LeftToRight || _Scrollbar.direction == Scrollbar.Direction.RightToLeft;

			if (externalScrollRectProxy == null)
			{
				if (!scrollRect)
				{
					scrollRect = GetComponentInParent<ScrollRect>();
					//if (!scrollRect)
					//    throw new UnityException("Please provide a ScrollRect for ScrollbarFixer8 to work");
				}
			}

            if (scrollRect)
            {
                _ScrollViewRT = scrollRect.transform as RectTransform;
				if (_HorizontalScrollBar)
                {
                    if (!scrollRect.horizontal)
                        throw new UnityException("ScrollbarFixer8: Can't use horizontal scrollbar with non-horizontal scrollRect");

                    if (scrollRect.horizontalScrollbar)
                    {
                        Debug.Log("ScrollbarFixer8: setting scrollRect.horizontalScrollbar to null (the whole point of using ScrollbarFixer8 is to NOT have any scrollbars assigned)");
                        scrollRect.horizontalScrollbar = null;
                    }
                    if (scrollRect.verticalScrollbar == _Scrollbar)
                    {
                        Debug.Log("ScrollbarFixer8: Can't use the same scrollbar for both vert and hor");
                        scrollRect.verticalScrollbar = null;
                    }
                }
                else
                {
                    if (!scrollRect.vertical)
                        throw new UnityException("Can't use vertical scrollbar with non-vertical scrollRect");

                    if (scrollRect.verticalScrollbar)
                    {
                        Debug.Log("ScrollbarFixer8: setting scrollRect.verticalScrollbar to null (the whole point of using ScrollbarFixer8 is to NOT have any scrollbars assigned)");
                        scrollRect.verticalScrollbar = null;
                    }
                    if (scrollRect.horizontalScrollbar == _Scrollbar)
                    {
                        Debug.Log("ScrollbarFixer8: Can't use the same scrollbar for both vert and hor");
                        scrollRect.horizontalScrollbar = null;
                    }
                }

            }
			else
			{

			}

			if (scrollRect)
			{
				scrollRect.onValueChanged.AddListener(ScrollRect_OnValueChangedCalled);

				// May be null
				externalScrollRectProxy = scrollRect.GetComponent(typeof(IScrollRectProxy)) as IScrollRectProxy;
			}
			else
			{
				if (externalScrollRectProxy == null)
				{
					// Start with directly with the parent when searching for IScrollRectProxy, as the scrollbar itself is a IScrollRectProxy and needs to be avoided;
					externalScrollRectProxy = transform.parent.GetComponentInParent(typeof(IScrollRectProxy)) as IScrollRectProxy;
					if (externalScrollRectProxy == null)
					{
						if (enabled)
						{
							Debug.Log(GetType().Name + ": no scrollRect provided and found no " + typeof(IScrollRectProxy).Name + " component among ancestors. Disabling...");
							enabled = false;
						}
						return;
					}
				}

				_ScrollViewRT = externalScrollRectProxy.gameObject.transform as RectTransform;
			}

			if (!viewport)
				viewport = _ScrollViewRT;

			if (autoHide)
				UpdateStartingValuesForAutoHideEffect();
		}

		//void Start()
		//{
		//	// In case useUnscaledTime has changed between Awake and Start, this needs to be updated
		//	_TimeOnLastValueChange = Time;
		//}

		void OnEnable()
        {
            _Dragging = false; // just in case dragging was stuck in true and the object was disabled
			_SlowUpdateCoroutine = SlowUpdate();

			StartCoroutine(_SlowUpdateCoroutine);
		}

		void Update()
		{
			if (!_FullyInitialized)
				InitializeInFirstUpdate();

			if (scrollRect || externalScrollRectProxy != null && externalScrollRectProxy.IsInitialized)
			{
				// Don't override when dragging
				if (_Dragging)
					return;

				var value = ScrollRectProxy.GetNormalizedPosition();

				_Scrollbar.value = (float)value;
				if (autoHide)
				{
					if (value == _LastValue)
					{
						if (!_Hidden)
						{
							float timePassedForHide01 = Mathf.Clamp01((Time - _TimeOnLastValueChange) / autoHideTime);
							if (timePassedForHide01 >= HIDE_EFFECT_START_DELAY_01)
							{
								float hideEffectAmount01 = (timePassedForHide01 - HIDE_EFFECT_START_DELAY_01) / (1f - HIDE_EFFECT_START_DELAY_01);
								hideEffectAmount01 = hideEffectAmount01 * hideEffectAmount01 * hideEffectAmount01; // slow in, fast-out effect
								if (CheckForAudoHideFadeEffectAndInitIfNeeded())
									_CanvasGroupForFadeEffect.alpha = Mathf.Lerp(_AlphaOnLastDrag, autoHideFadeEffectMinAlpha, hideEffectAmount01);

								if (autoHideCollapseEffect)
								{
									Vector3 localScale = transform.localScale;
									localScale[ScrollRectProxy.IsHorizontal ? 1 : 0] = Mathf.Lerp(_TransversalScaleOnLastDrag, autoHideCollapseEffectMinScale, hideEffectAmount01);
									transform.localScale = localScale;
								}
							}

							if (timePassedForHide01 == 1f)
							{
								_AutoHidden = true;
								Hide();
							}
						}
					}
					else
					{
						_TimeOnLastValueChange = Time;
						_LastValue = value;

						if (_Hidden && !_HiddenNotNeeded)
							Show();
					}
				}
				// Handling the case when the scrollbar was hidden but its autoHide property was set to false afterwards 
				// and hideWhenNotNeeded is also false, meaning the scrollbar won't ever be shown
				else if (!hideWhenNotNeeded)
				{
					if (_Hidden)
						Show();
				}
			}
		}

		void OnDisable()
        {
			if (_SlowUpdateCoroutine != null)
				StopCoroutine(_SlowUpdateCoroutine);
		}

		void OnDestroy()
		{
			if (scrollRect)
				scrollRect.onValueChanged.RemoveListener(ScrollRect_OnValueChangedCalled);

			if (externalScrollRectProxy != null)
				externalScrollRectProxy.ScrollPositionChanged -= ExternalScrollRectProxy_OnScrollPositionChanged;
		}

		#region IScrollRectProxy methods implementation (used if external proxy is not manually assigned)
		/// <summary>Not used in this default interface implementation</summary>
#pragma warning disable 0067
		public event System.Action<double> ScrollPositionChanged;
#pragma warning restore 0067
		public void SetNormalizedPosition(double normalizedPosition) { if (_HorizontalScrollBar) scrollRect.horizontalNormalizedPosition = (float)normalizedPosition; else scrollRect.verticalNormalizedPosition = (float)normalizedPosition; }
		public double GetNormalizedPosition() { return _HorizontalScrollBar ? scrollRect.horizontalNormalizedPosition : scrollRect.verticalNormalizedPosition; }
		public double GetContentSize() { return IsHorizontal ? Content.rect.width : Content.rect.height; }
		public double GetViewportSize() { return IsHorizontal ? Viewport.rect.width : Viewport.rect.height; }
		public void StopMovement() { scrollRect.StopMovement(); }
		#endregion

		#region Unity UI event callbacks
		public void OnBeginDrag(PointerEventData eventData) { _Dragging = true; }
		public void OnEndDrag(PointerEventData eventData) { _Dragging = false; }
		public void OnDrag(PointerEventData eventData) { OnScrollRectValueChanged(false); }
		public void OnPointerDown(PointerEventData eventData) { if (externalScrollRectProxy != null && externalScrollRectProxy.IsInitialized) externalScrollRectProxy.StopMovement(); }
		#endregion

		void InitializeInFirstUpdate()
		{
			if (externalScrollRectProxy != null)
				externalScrollRectProxy.ScrollPositionChanged += ExternalScrollRectProxy_OnScrollPositionChanged;
			_FullyInitialized = true;
		}

		IEnumerator SlowUpdate()
		{
			var waitAmount = new WaitForSecondsRealtime(sizeUpdateInterval);

            while (true)
            {
                yield return waitAmount;

                if (!enabled)
                    break;

                if (_ScrollViewRT && (scrollRect && scrollRect.content || externalScrollRectProxy != null && externalScrollRectProxy.IsInitialized))
                {
                    double size, viewportSize, contentSize = ScrollRectProxy.GetContentSize();
                    if (_HorizontalScrollBar)
                        viewportSize = viewport.rect.width;
                    else
                        viewportSize = viewport.rect.height;

                    if (contentSize <= 0d || contentSize == double.NaN || contentSize == double.Epsilon || contentSize == double.NegativeInfinity || contentSize == double.PositiveInfinity)
                        size = 1d;
					else
					{
						size = viewportSize / contentSize;
						size = System.Math.Max(minSize, System.Math.Min(1d, size));
					}

					//Debug.Log(viewportSize + ", ct=" + contentSize);

					float oldSizeFloat = _Scrollbar.size;
                    _Scrollbar.size = (float)size;
					float currentSizeFloat = _Scrollbar.size;

					if (hideWhenNotNeeded)
                    {
                        if (size > .99d)
                        {
                            if (!_Hidden)
                            {
                                _HiddenNotNeeded = true;
                                Hide();
                            }
                        }
                        else
                        {
                            if (_Hidden && !_AutoHidden) // if autohidden, we don't interfere with the process
                            {

                                Show();
                            }
                        }
                    }
                    // Handling the case when the scrollbar was hidden but its hideWhenNotNeeded property was set to false afterwards
                    // and autoHide is also false, meaning the scrollbar won't ever be shown
                    else if (!autoHide)
                    {
                        if (_Hidden)
                            Show();
                    }

					if (!_TriedToCallOnScrollbarSizeChangedAtLeastOnce || oldSizeFloat != currentSizeFloat)
					{
						_TriedToCallOnScrollbarSizeChangedAtLeastOnce = true;
						if (OnScrollbarSizeChanged != null)
							OnScrollbarSizeChanged.Invoke();
					}
                }
            }
        }

        void Hide()
        {
            _Hidden = true;
			if (!autoHide || _HiddenNotNeeded)
				gameObject.transform.localScale = Vector3.zero;
        }

        void Show()
        {
            gameObject.transform.localScale = _InitialScale;
            _HiddenNotNeeded = _AutoHidden = _Hidden = false;
			if (CheckForAudoHideFadeEffectAndInitIfNeeded())
				_CanvasGroupForFadeEffect.alpha = 1f;

			UpdateStartingValuesForAutoHideEffect();
		}

		void UpdateStartingValuesForAutoHideEffect()
		{
			if (CheckForAudoHideFadeEffectAndInitIfNeeded())
				_AlphaOnLastDrag = _CanvasGroupForFadeEffect.alpha;

			if (autoHideCollapseEffect)
				_TransversalScaleOnLastDrag = transform.localScale[ScrollRectProxy.IsHorizontal ? 1 : 0];
		}

		bool CheckForAudoHideFadeEffectAndInitIfNeeded()
		{
			if (autoHideFadeEffect && !_CanvasGroupForFadeEffect)
			{
				_CanvasGroupForFadeEffect = GetComponent<CanvasGroup>();
				if (!_CanvasGroupForFadeEffect)
					_CanvasGroupForFadeEffect = gameObject.AddComponent<CanvasGroup>();
			}

			return autoHideFadeEffect;
		}

		void ScrollRect_OnValueChangedCalled(Vector2 _)
		{
			// Only consider this callback if there's no external proxy provided, which is supposed to call ExternalScrollRectProxy_OnScrollPositionChanged()
			if (externalScrollRectProxy == null)
				OnScrollRectValueChanged(true);
		}

		void ExternalScrollRectProxy_OnScrollPositionChanged(double _) { OnScrollRectValueChanged(true); }

		void OnScrollRectValueChanged(bool fromScrollRect)
		{
			if (!fromScrollRect)
			{
				ScrollRectProxy.StopMovement();

				if (_FrameCountOnLastPositionUpdate + skippedFramesBetweenPositionChanges < UnityEngine.Time.frameCount)
				{
					ScrollRectProxy.SetNormalizedPosition(_Scrollbar.value);
					_FrameCountOnLastPositionUpdate = UnityEngine.Time.frameCount;
				}
			}

			//var normPos = ScrollRectProxy.GetNormalizedPosition();
			//if (_HorizontalScrollBar)
			//	normPos.x = _Scrollbar.value;
			//else
			//	normPos.y = _Scrollbar.value;

			//if (!fromScrollRect)
			//	ScrollRectProxy.SetNormalizedPosition(_Scrollbar.value);

			_TimeOnLastValueChange = Time;
			if (autoHide)
				UpdateStartingValuesForAutoHideEffect();

			if (!_HiddenNotNeeded
				&& _Scrollbar.size < 1f) // is needed
				Show();
		}
	}
}