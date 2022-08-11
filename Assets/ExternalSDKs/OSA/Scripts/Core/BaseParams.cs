using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using frame8.Logic.Misc.Other.Extensions;
using Com.TheFallenGames.OSA.Core.SubComponents;
using Com.TheFallenGames.OSA.Core.Data;
using Com.TheFallenGames.OSA.Core.Data.Gallery;

namespace Com.TheFallenGames.OSA.Core
{
	/// <summary>
	/// <para>Input params to be passed to <see cref="OSA{TParams, TItemViewsHolder}.Init()"/></para>
	/// <para>This can be used Monobehaviour's field and exposed via inspector (most common case)</para>
	/// <para>Or can be manually constructed, depending on what's easier in your context</para>
	/// </summary>
	[System.Serializable]
	public class BaseParams
	{
		#region Configuration

		#region Core params
		[SerializeField]
		RectTransform _Content = null;
		public RectTransform Content { get { return _Content; } set { _Content = value; } }

		[Tooltip("If null, the scrollRect is considered to be the viewport")]
		[SerializeField]
		[FormerlySerializedAs("viewport")]
		RectTransform _Viewport = null;
		/// <summary>If null, <see cref="ScrollViewRT"/> is considered to be the viewport</summary>
		public RectTransform Viewport { get { return _Viewport; } set { _Viewport = value; } }

		[SerializeField]
		OrientationEnum _Orientation = OrientationEnum.VERTICAL;
		public OrientationEnum Orientation { get { return _Orientation; } set { _Orientation = value; } }

		[SerializeField]
		Scrollbar _Scrollbar = null;
		public Scrollbar Scrollbar { get { return _Scrollbar; } set { _Scrollbar = value; } }

		[Tooltip("The sensivity to the Mouse's scrolling wheel or similar input methods. " +
			"Not related to dragging or scrolling via scrollbar")]
		[SerializeField]
		float _ScrollSensivity = 100f;
		/// <summary>The sensivity to the Mouse's scrolling wheel or similar input methods. Not related to dragging or scrolling via scrollbar</summary>
		public float ScrollSensivity { get { return _ScrollSensivity; } set { _ScrollSensivity = value; } }

		[Tooltip("The sensivity to the Mouse's horizontal scrolling wheel (where supported) or similar input methods that send scroll signals on the horizontal axis. " +
			"Not related to dragging or scrolling via scrollbar. \n" +
			"It's set to a positive value by default to comply with Unity's ScrollRect (which for some reason inverts the left and right directions). \n" +
			"If you'll nest OSA in a regular horizontal ScrollRect, set the ScrollRect's sensivity to a positive value and OSA's ScrollSensivityOnXAxis to a negative value to get an intuitive scroll")]
		[SerializeField]
		float _ScrollSensivityOnXAxis = 100f;
		/// <summary>The sensivity to the Mouse's left/right scrolling wheel (where supported) or similar input methods that send scroll signals on the horizontal axis. Not related to dragging or scrolling via scrollbar</summary>
		public float ScrollSensivityOnXAxis { get { return _ScrollSensivityOnXAxis; } set { _ScrollSensivityOnXAxis = value; } }

		[SerializeField]
		//[HideInInspector]
		[Tooltip("Padding for the 4 edges of the content panel.\n"
				+ "Tip: if using a fixed, constant ItemTransversalSize, also set the paddings in the" +
				" transversal direction to -1 (left/right for vertical ScrollView and vice-versa)." +
				" This aligns the item in the center, transversally")]
		[FormerlySerializedAs("contentPadding")]
		RectOffset _ContentPadding = new RectOffset();
		/// <summary>
		/// Padding for the 4 edges of the content panel. 
		/// <para>Tip: if using a fixed, constant <see cref="ItemTransversalSize"/>, also set the paddings in the 
		/// transversal direction to -1 (left/right for vertical ScrollView and vice-versa). This aligns the item in the center, transversally</para>
		/// </summary>
		public RectOffset ContentPadding { get { return _ContentPadding; } set { _ContentPadding = value; } }

		[SerializeField]
		//[HideInInspector]
		[FormerlySerializedAs("contentGravity")]
		ContentGravity _Gravity = ContentGravity.START;
		/// <summary>
		/// The effect of this property can only be seen when the content size is smaller than the viewport, case in which there are 3 possibilities: 
		/// place the content at the start, middle or end. <see cref="ContentGravity.FROM_PIVOT"/> doesn't change the content's position (it'll be preserved from the way you aligned it in edit-mode)
		/// </summary>
		public ContentGravity Gravity { get { return _Gravity; } set { _Gravity = value; } }

		[Tooltip("The space between items")]
		[SerializeField]
		//[HideInInspector]
		[FormerlySerializedAs("contentSpacing")]
		float _ContentSpacing = 0f;
		/// <summary>Spacing between items (horizontal if the ScrollView is horizontal. else, vertical)</summary>
		public float ContentSpacing { get { return _ContentSpacing; } set { _ContentSpacing = value; } }

		[Tooltip("The size of all items for which the size is not specified in CollectItemSizes()")]
		[SerializeField]
		//[HideInInspector]
		float _DefaultItemSize = 60f;
		/// <summary>The size of all items for which the size is not specified</summary>
		public float DefaultItemSize { get { return _DefaultItemSize; } protected set { _DefaultItemSize = value; } }

		[Tooltip("You'll probably need this if the scroll view is a child of another scroll view." +
		" If enabled, the first parent that implements all of IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler and IEndDragHandler," +
		" as well as the first IScrollHandler found in parents (but it should be on the same game object as other handlers, IF they're found), " +
		" will receive these events when they occur on this scroll view. This works both with Unity's ScrollRect and OSA")]
		[SerializeField]
		bool _ForwardDragToParents = false;
		/// <summary>You'll probably need this if the scroll view is a child of another scroll view.
		/// If enabled, the first parent that implements all of IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler and IEndDragHandler, 
		/// as well as the first IScrollHandler found in parents (but it should be on the same game object as other handlers, IF they're found), 
		/// will receive these events when they occur on this scroll view. This works both with Unity's ScrollRect and OSA</summary>
		public bool ForwardDragToParents { get { return _ForwardDragToParents; } protected set { _ForwardDragToParents = value; } }

		[Tooltip("It forwards the drag/scroll in the same direction to the parent when the current scrolled position is at boundary, " +
			"thus allowing scrolling through nested Scroll Views that have the same scroll direction. \n" +
			"ForwardDragToParents also needs to be enabled for this to work.")]
		[SerializeField]
		bool _ForwardDragSameDirectionAtBoundary = false;
		/// <summary>It forwards the drag/scroll events in the same direction to the parent when the current scrolled position is at boundary,
		/// thus allowing scrolling through nested Scroll Views that have the same scroll direction.
		/// <para>ForwardDragToParents also needs to be enabled for this to work.</para>
		/// </summary>
		public bool ForwardDragSameDirectionAtBoundary { get { return _ForwardDragSameDirectionAtBoundary; } protected set { _ForwardDragSameDirectionAtBoundary = value; } }

		[Tooltip("Allows you to click and drag the content directly (enabled by default). The property ForwardDragToParents is not affected by this")]
		[SerializeField]
		bool _DragEnabled = true;
		/// <summary>
		/// Allows you to click and drag the content directly (enabled by default). 
		/// The <see cref="ForwardDragToParents"/> is not affected by this.
		/// NOTE: Do not change this property during a drag event (i.e. when <see cref="OSA{TParams, TItemViewsHolder}.IsDragging"/> is true)
		/// </summary>
		public bool DragEnabled { get { return _DragEnabled; } protected set { _DragEnabled = value; } }

		[Tooltip("Allows you to scroll by mouse wheel or other similar input devices (enabled by default). The property ForwardDragToParents is not affected by this")]
		[SerializeField]
		bool _ScrollEnabled = true;
		/// <summary>
		/// Allows you to scroll by mouse wheel or other similar input devices (enabled by default). 
		/// The <see cref="ForwardDragToParents"/> is not affected by this.
		/// NOTE: Do not change this property during a drag event (i.e. when <see cref="OSA{TParams, TItemViewsHolder}.IsDragging"/> is true)
		/// </summary>
		public bool ScrollEnabled { get { return _ScrollEnabled; } protected set { _ScrollEnabled = value; } }

		[Tooltip("If enabled, will use Time.unscaledTime instead of Time.time, which means the animations, inertia etc. won't be affected by the Time.timeScale")]
		[SerializeField]
		bool _UseUnscaledTime = true;
		/// <summary>If enabled, will use <see cref="Time.unscaledTime"/> and <see cref="Time.unscaledDeltaTime"/> for animations, inertia etc. instead of <see cref="Time.time"/> and <see cref="Time.deltaTime"/></summary>
		public bool UseUnscaledTime { get { return _UseUnscaledTime; } set { _UseUnscaledTime = value; } }

		[Tooltip(
			"The item size in the direction perpendicular to scrolling direction. Width for vertical ScrollViews, and vice-versa.\n" +
			"-1 = items won't have their widths (heights for horizontal ScrollViews) changed at all - you'll be responsible for them\n" +
			"0 = fill to available space, taking ContentPadding into account\n" +
			">0 = fixed size; in this case, it's better to also set transversal padding to -1 so the item will be centered\n")]
		[SerializeField]
		float _ItemTransversalSize = 0f;
		/// <summary>
		/// The item size in the direction perpendicular to scrolling direction. Width for vertical ScrollViews, and vice-versa
		/// <para>-1 = items won't have their widths (heights for horizontal ScrollViews) changed at all - you'll be responsible for them</para>
		/// <para>0 = fill to available space, taking <see cref="ContentPadding"/> into account</para>
		/// <para>any positive value = fixed size; in this case, it's better to also set transversal padding to -1 so the item will be centered</para>
		/// </summary>
		public float ItemTransversalSize { get { return _ItemTransversalSize; } set { _ItemTransversalSize = value; } }
		#endregion

		public Effects effects = new Effects();

		public Optimization optimization = new Optimization();
		#endregion


		public bool IsHorizontal { get { return _Orientation == OrientationEnum.HORIZONTAL; } }
		public RectTransform ScrollViewRT { get { return _ScrollViewRT; } }
		public Snapper8 Snapper { get { return _Snapper; } }

		RectTransform _ScrollViewRT;
		Snapper8 _Snapper;


		/// <summary>It's here just so the class can be serialized by Unity when used as a MonoBehaviour's field</summary>
		public BaseParams() { }


		public void PrepareForInit(bool firstTime)
		{
			if (!Content)
				throw new OSAException("Content cannot be null");
			Content.MatchParentSize(true);

			if (firstTime)
				Canvas.ForceUpdateCanvases();
		}

		/// <summary>
		/// Called internally in <see cref="OSA{TParams, TItemViewsHolder}.Init()"/> and every time the scrollview's size changes. 
		/// This makes sure the content and viewport have valid values. It can also be overridden to initialize custom data
		/// </summary>
		public virtual void InitIfNeeded(IOSA iAdapter)
        {
			_ScrollViewRT = iAdapter.AsMonoBehaviour.transform as RectTransform;
			LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollViewRT);

			AssertValidWidthHeight(ScrollViewRT);

			var sr = ScrollViewRT.GetComponent<ScrollRect>();
			if (sr && sr.enabled)
				throw new OSAException("The ScrollRect is not needed anymore starting with v4.0. Remove or disable it!");

			if (!Content)
				throw new OSAException("Content not set!");
			if (!Viewport)
			{
				Viewport = ScrollViewRT;
				if (Content.parent != ScrollViewRT)
					throw new OSAException("Content's parent should be the ScrollView itself if there's no viewport specified!");
			}
			if (!_Snapper)
				_Snapper = ScrollViewRT.GetComponent<Snapper8>();

			if (ForwardDragSameDirectionAtBoundary && !ForwardDragToParents)
				Debug.Log("OSA: ForwardDragSameDirectionAtBoundary is true, but ForwardDragToParents is false. This will have no effect");

			if ((ContentPadding.left == -1) != (ContentPadding.right == -1))
			{
				Debug.Log("OSA: ContentPadding.right and .left should either be both zero or positive or both -1. Setting both to 0...");
				ContentPadding.left = ContentPadding.right = 0;
			}

			if ((ContentPadding.top == -1) != (ContentPadding.bottom == -1))
			{
				Debug.Log("OSA: ContentPadding.top and .bottom should either be both zero or positive or both -1. Setting both to 0...");
				ContentPadding.top = ContentPadding.bottom = 0;
			}


			effects.InitIfNeeded();

			// There's no concept of content padding when looping. spacing should be used instead
			if (effects.LoopItems)
			{
				bool showLog = false;
				int ctSp = (int)ContentSpacing;
				if (IsHorizontal)
				{
					if (ContentPadding.left != ctSp)
					{
						showLog = true;
						ContentPadding.left = ctSp;
					}

					if (ContentPadding.right != ctSp)
					{
						showLog = true;
						ContentPadding.right = ctSp;
					}
				}
				else
				{
					if (ContentPadding.top != ctSp)
					{
						showLog = true;
						ContentPadding.top = ctSp;
					}

					if (ContentPadding.bottom != ctSp)
					{
						showLog = true;
						ContentPadding.bottom = ctSp;
					}
				}

				if (showLog)
					Debug.Log("OSA: setting conteng padding to be the same as content spacing (" + ContentSpacing.ToString("#############.##")+"), because looping is enabled");
			}
		}

		/// <summary>See <see cref="ContentGravity"/></summary>
		public void UpdateContentPivotFromGravityType()
		{
			if (Gravity != ContentGravity.FROM_PIVOT)
			{
				int v1_h0 = IsHorizontal ? 0 : 1;

				var piv = Content.pivot;

				// The transversal position is at the center
				piv[1 - v1_h0] = .5f;

				int contentGravityAsInt = ((int)Gravity);
				float pivotInScrollingDirection_IfVerticalScrollView;
				if (contentGravityAsInt < 3)
					// 1 = TOP := 1f;
					// 2 = CENTER := .5f;
					pivotInScrollingDirection_IfVerticalScrollView = 1f / contentGravityAsInt;
				else
					// 3 = BOTTOM := 0f;
					pivotInScrollingDirection_IfVerticalScrollView = 0f;

				piv[v1_h0] = pivotInScrollingDirection_IfVerticalScrollView;
				if (v1_h0 == 0) // i.e. if horizontal
					piv[v1_h0] = 1f - piv[v1_h0];

				Content.pivot = piv;
			}
		}

		public void ApplyScrollSensitivityTo(ref Vector2 vec)
		{
			vec.x *= OSAConst.SCROLL_DIR_X_MULTIPLIER * ScrollSensivityOnXAxis;
			vec.y *= OSAConst.SCROLL_DIR_Y_MULTIPLIER * ScrollSensivity;
		}

		internal void AssertValidWidthHeight(RectTransform rt)
		{
			var rectSize = rt.rect.size;
			string widthOfHeightErr = null;
			float sizErr;
			if ((sizErr = rectSize.x) < 1f)
				widthOfHeightErr = "width";
			else if ((sizErr = rectSize.y) < 1f)
				widthOfHeightErr = "height";
			if (widthOfHeightErr != null)
				throw new OSAException("OSA: '" + rt.name + "' reports a zero or negative " + widthOfHeightErr + "(" + sizErr + "). " +
					"\nThis can happen if you don't have a Canvas component in the OSA's parents or if you accidentally set an invalid size in editor. " +
					"\nIf you instantiate the ScrollView at runtime, make sure you use the version of Object.Instantiate(..) that also takes the parent so it can be directly instantiated in it. The parent should be a Canvas or a descendant of a Canvas"
					);
		}


		public enum OrientationEnum
		{
			VERTICAL,
			HORIZONTAL
		}


		/// <summary> Represents how often or when the optimizer does his core loop: checking for any items that need to be created, destroyed, disabled, displayed, recycled</summary>
		public enum ContentGravity
		{
			/// <summary>you set it up manually</summary>
			FROM_PIVOT,

			/// <summary>top if vertical scrollview, else left</summary>
			START,

			/// <summary>top if vertical scrollview, else left</summary>
			CENTER,

			/// <summary>bottom if vertical scrollview, else right</summary>
			END

		}


		[Serializable]
		public class Effects
		{
			public const float DEFAULT_MAX_SPEED = 10 * 1000f;
			public const float MAX_SPEED = DEFAULT_MAX_SPEED * 100;
			public const float MAX_SPEED_IF_LOOPING = DEFAULT_MAX_SPEED;


			[Tooltip("This RawImage will be scrolled together with the content. \n" +
				"The content is always stationary (this is the way the recycling process works), so the scrolling effect is faked by scrolling the texture's x/y.\n" +
				"Tip: use a seamless/looping background texture for best visual results")]
			[FormerlySerializedAs("contentVisual")]
			[SerializeField]
			RawImage _ContentVisual = null;
			[Obsolete("Use ContentVisual instead", true)]
			public RawImage contentVisual { get { return ContentVisual; } set { ContentVisual = value; } }
			public RawImage ContentVisual { get { return _ContentVisual; } set { _ContentVisual = value; } }

			[FormerlySerializedAs("elasticMovement")]
			[SerializeField]
			bool _ElasticMovement = true;
			[Obsolete("Use ElasticMovement instead", true)]
			public bool elasticMovement { get { return ElasticMovement; } set { ElasticMovement = value; } }
			public bool ElasticMovement { get { return _ElasticMovement; } set { _ElasticMovement = value; } }

			[FormerlySerializedAs("pullElasticity")]
			[SerializeField]
			float _PullElasticity = .3f;
			[Obsolete("Use PullElasticity instead", true)]
			public float pullElasticity { get { return PullElasticity; } set { PullElasticity = value; } }
			public float PullElasticity { get { return _PullElasticity; } set { _PullElasticity = value; } }

			[FormerlySerializedAs("releaseTime")]
			[SerializeField]
			float _ReleaseTime = .1f;
			[Obsolete("Use ReleaseTime instead", true)]
			public float releaseTime { get { return ReleaseTime; } set { ReleaseTime = value; } }
			public float ReleaseTime { get { return _ReleaseTime; } set { _ReleaseTime = value; } }

			[FormerlySerializedAs("inertia")]
			[SerializeField]
			bool _Inertia = true;
			[Obsolete("Use Inertia instead", true)]
			public bool inertia { get { return Inertia; } set { Inertia = value; } }
			public bool Inertia { get { return _Inertia; } set { _Inertia = value; } }
			
			[Tooltip("What percent (0=0%, 1=100%) of the velociy will be lost per second after the drag ended. 1=all(immediate stop), 0=none(maitain constant scrolling speed indefinitely)")]
			[Range(0f, 1f)]
			[FormerlySerializedAs("inertiaDecelerationRate")]
			[SerializeField]
			float _InertiaDecelerationRate = 1f - .135f;
			[Obsolete("Use InertiaDecelerationRate instead", true)]
			public float inertiaDecelerationRate { get { return InertiaDecelerationRate; } set { InertiaDecelerationRate = value; } }
			/// <summary>What amount of the velociy will be lost per second after the drag ended</summary>
			// Fun fact: Unity's original ScrollRect mistakenly names "deceleration rate" the amount that should REMAIN, 
			// not the one that will be REMOVED from the velocity. And its deault value is 0.135. 
			// Here, we're setting the correct default value. A 0 deceleration rate should mean no deceleration
			public float InertiaDecelerationRate { get { return _InertiaDecelerationRate; } set { _InertiaDecelerationRate = value; } }

			[SerializeField]
			[Tooltip("Stop any movement from inertia or scrolling animations when a mouse click/touch begins")]
			bool _CutMovementOnPointerDown = true;
			/// <summary>
			/// Stop any movement from inertia or scrolling animations when a mouse click/touch begins
			/// </summary>
			public bool CutMovementOnPointerDown { get { return _CutMovementOnPointerDown; } set { _CutMovementOnPointerDown = value; } }

			[FormerlySerializedAs("maxSpeed")]
			[SerializeField]
			float _MaxSpeed = DEFAULT_MAX_SPEED;
			[Obsolete("Use MaxSpeed instead", true)]
			public float maxSpeed { get { return MaxSpeed; } set { MaxSpeed = value; } }
			public float MaxSpeed { get { return _MaxSpeed; } set { _MaxSpeed = value; } }

			[Tooltip("If enabled, multiple drags in the same direction will lead to greater speeds")]
			[SerializeField]
			bool _TransientSpeedBetweenDrags = true;
			/// <summary>If enabled, multiple drags in the same direction will lead to greater speeds</summary>
			public bool TransientSpeedBetweenDrags { get { return _TransientSpeedBetweenDrags; } protected set { _TransientSpeedBetweenDrags = value; } }

			[Tooltip("If true: When the last item is reached, the first one appears after it, basically allowing you to scroll infinitely.\n" +
				" Initially intended for things like spinners, but it can be used for anything alike.\n" +
				" It may interfere with other functionalities in some very obscure/complex contexts/setups, so be sure to test the hell out of it.\n" +
				" Also please note that sometimes during dragging the content, the actual looping changes the Unity's internal PointerEventData for the current click/touch pointer id, so if you're also externally tracking the current click/touch, in this case only 'PointerEventData.pointerCurrentRaycast' and 'PointerEventData.position'(current position) are preserved, the other ones are reset to defaults to assure a smooth loop transition. Sorry for the long decription. Here's an ASCII potato: @")]
			[FormerlySerializedAs("loopItems")]
			[SerializeField]
			bool _LoopItems = false;
			[Obsolete("Use LoopItems instead", true)]
			public bool loopItems { get { return LoopItems; } set { LoopItems = value; } }
			/// <summary>
			/// <para>If true: When the last item is reached, the first one appears after it, basically allowing you to scroll infinitely.</para>
			/// <para>Initially intended for things like spinners, but it can be used for anything alike. It may interfere with other functionalities in some very obscure/complex contexts/setups, so be sure to test the hell out of it.</para>
			/// <para>Also please note that sometimes during dragging the content, the actual looping changes the Unity's internal PointerEventData for the current click/touch pointer id, </para>
			/// <para>so if you're also externally tracking the current click/touch, in this case only 'PointerEventData.pointerCurrentRaycast' and 'PointerEventData.position'(current position) are </para>
			/// <para>preserved, the other ones are reset to defaults to assure a smooth loop transition</para>
			/// </summary>
			public bool LoopItems { get { return _LoopItems; } set { _LoopItems = value; } }

			[Tooltip("The contentVisual's additional drag factor. Examples:\n" +
				"-2: the contentVisual will move exactly by the same amount as the items, but in the opposite direction\n" +
				"-1: no movement\n" +
				" 0: same speed (together with the items)\n" +
				" 1: 2x faster in the same direction\n" +
				" 2: 3x faster etc.")]
			[Range(-5f, 5f)]
			[SerializeField]
			float _ContentVisualParallaxEffect = -.85f;
			public float ContentVisualParallaxEffect { get { return _ContentVisualParallaxEffect; } set { _ContentVisualParallaxEffect = value; } }

			[SerializeField]
			GalleryEffectParams _Gallery = new GalleryEffectParams();
			public GalleryEffectParams Gallery { get { return _Gallery; } set { _Gallery = value; } }

			[Range(0f, 1f)]
			[FormerlySerializedAs("galleryEffectAmount")]
			[SerializeField]
			[HideInInspector]
			float _GalleryEffectAmount = 0f;
			[Obsolete("Use GalleryEffectAmount instead", true)]
			public float galleryEffectAmount { get { return Gallery.OverallAmount; } set { Gallery.OverallAmount = value; } }
			[Obsolete("Use Gallery.OverallAmount instead")]
			public float GalleryEffectAmount { get { return Gallery.OverallAmount; } set { Gallery.OverallAmount = value; } }

			[Range(0f, 1f)]
			[FormerlySerializedAs("galleryEffectViewportPivot")]
			[SerializeField]
			[HideInInspector]
			float _GalleryEffectViewportPivot = .5f;
			[Obsolete("Use GalleryEffectViewportPivot instead", true)]
			public float galleryEffectViewportPivot { get { return Gallery.Scale.ViewportPivot; } set { Gallery.Scale.ViewportPivot = value; } }
			[Obsolete("Use Gallery.Scale.ViewportPivot instead")]
			public float GalleryEffectViewportPivot { get { return Gallery.Scale.ViewportPivot; } set { Gallery.Scale.ViewportPivot = value; } }

			public bool HasContentVisual { get { return _HasContentVisual; } }

			bool _HasContentVisual;


			public void InitIfNeeded()
			{
				_HasContentVisual = ContentVisual != null;

				float maxAllowed;
				string asString;
				if (LoopItems)
				{
					maxAllowed = MAX_SPEED_IF_LOOPING;
					asString = "MAX_SPEED_IF_LOOPING";
				}
				else
				{
					maxAllowed = MAX_SPEED;
					asString = "MAX_SPEED";
				}
				float maxSpeedClamped = Mathf.Clamp(MaxSpeed, 0f, maxAllowed);
				if (Math.Abs(maxSpeedClamped - MaxSpeed) > 1f)
				{
					Debug.Log("OSA: maxSpeed(" + MaxSpeed.ToString("#########.00") + ") value is negative or exceeded "+ asString + "(" +
						maxAllowed.ToString("#########.00") +
						"). Clamped it to " + maxSpeedClamped.ToString("#########.00")
					);
					MaxSpeed = maxSpeedClamped;
				}

				if (ElasticMovement && LoopItems)
				{
					ElasticMovement = false;
					Debug.Log("OSA: 'elasticMovement' was set to false, because 'loopItems' is true. Elasticity only makes sense when there is an end");
				}

				if (HasContentVisual)
					ContentVisual.rectTransform.MatchParentSize(true);

				InitGalleryEffectMigrations();
			}


			void InitGalleryEffectMigrations()
			{
				float defVal = 0f;
				if (_GalleryEffectAmount != defVal)
				{
					string warn = "OSA: Please go to BaseParams.cs, comment the [HideInInspector] attribute on _GalleryEffectAmount, then set this property to " + defVal + " (the default) in inspector, as this property will be removed in next versions." +
						" Use Gallery.OverallAmount instead to set the gallery effect amount. It's available through inspector." +
						" Will use _GalleryEffectAmount as the active one.";
					if (Gallery.OverallAmount != defVal)
						warn += ". Additional migration warning: Both _GalleryEffectAmount and Gallery.OverallAmount are non-default (non " + defVal + ").";
					Gallery.OverallAmount = _GalleryEffectAmount;
					Debug.Log(warn);
				}
				defVal = .5f;
				if (_GalleryEffectViewportPivot != defVal)
				{
					string warn = "OSA: Please go to BaseParams.cs, comment the [HideInInspector] attribute on _GalleryEffectViewportPivot, then set this property to " + defVal + " (the default) in inspector, as this property will be removed in next versions." +
						" Use Gallery.Scale.ViewportPivot instead to set the gallery effect pivot. It's available through inspector." +
						" Preserving the value of _GalleryEffectViewportPivot as the active one.";
					if (Gallery.Scale.ViewportPivot != defVal)
						warn += ". Additional migration warning: Both _GalleryEffectViewportPivot and Gallery.Scale.ViewportPivot are non-default (non " + defVal + ").";
					Gallery.Scale.ViewportPivot = _GalleryEffectViewportPivot;
					Debug.Log(warn);
				}
			}
		}


		[Serializable]
		public class Optimization
		{
			[Tooltip("How much objects besides the visible ones to keep in memory at max. \n" +
				"By default, no more than the heuristically found \"ideal\" number of items will be held in memory.\n" +
				"Set to a positive integer to limit it. In this case, you'll either use more RAM than needed or more CPU than needed. " +
					"One advantage is that you can get a predictable usage of the resources (for example, by specifying a constant bin size, no item will be destroyed if the actual visible items count won't exceed that).\n" +
				"Note that this doesn't include the 'buffered' items, which by design can't be directly destroyed")]
			[FormerlySerializedAs("recycleBinCapacity")]
			[SerializeField]
			int _RecycleBinCapacity = -1;
			[Obsolete("Use RecycleBinCapacity instead", true)]
			public int recycleBinCapacity { get { return RecycleBinCapacity; } set { RecycleBinCapacity = value; } }
			/// <summary>
			/// <para>How much objects besides the visible ones to keep in memory at max, besides the visible ones</para>
			/// <para>By default, no more than the heuristically found "ideal" number of items will be held in memory</para>
			/// <para>Set to a positive integer to limit it - Not recommended, unless you're OK with more GC calls (i.e. occasional FPS hiccups) in favor of using less RAM</para>
			/// </summary>
			public int RecycleBinCapacity { get { return _RecycleBinCapacity; } set { _RecycleBinCapacity = value; } }

			[FormerlySerializedAs("scaleToZeroInsteadOfDisable")]
			[SerializeField]
			bool _ScaleToZeroInsteadOfDisable = false;
			[Obsolete("Use ScaleToZeroInsteadOfDisable instead", true)]
			public bool scaleToZeroInsteadOfDisable { get { return ScaleToZeroInsteadOfDisable; } set { ScaleToZeroInsteadOfDisable = value; } }
			/// <summary>
			/// Enables ability to scale out-of-view objects to zero instead of de-activating them, 
			/// since GameObject.SetActive is slightly more expensive to call each frame (especially when scrolling via the scrollbar). 
			/// This is not a major speed improvement, but rather a slight memory improvement. 
			/// It's recommended to use this option if your game/business logic doesn't require the game objects to be de-activated.
			/// </summary>
			public bool ScaleToZeroInsteadOfDisable { get { return _ScaleToZeroInsteadOfDisable; } set { _ScaleToZeroInsteadOfDisable = value; } }

			// WIP
			///// <summary>
			///// <para>The bigger, the more items will be active past the minimum needed to fill the viewport - with a performance cost, of course</para>
			///// <para>1f = generally, the number of visible items + 1 will always be active</para>
			///// <para>2f = twice the number of visible items + 1 will be always active</para>
			///// <para>2.5f = 2.5 * (the number of visible items) + 1 will be always active</para>
			///// </summary>
			//[Range(1f, 5f)]
			//public float recyclingToleranceFactor = 1f;

			[FormerlySerializedAs("forceLayoutRebuildOnBeginSmoothScroll")]
			[SerializeField]
			bool _ForceLayoutRebuildOnBeginSmoothScroll = true;
			[Obsolete("Use ForceLayoutRebuildOnBeginSmoothScroll instead", true)]
			public bool forceLayoutRebuildOnBeginSmoothScroll { get { return ForceLayoutRebuildOnBeginSmoothScroll; } set { ForceLayoutRebuildOnBeginSmoothScroll = value; } }
			/// <summary>Disable only if you see FPS drops when calling <see cref="OSA{TParams, TItemViewsHolder}.SmoothScrollTo(int, float, float, float, Func{float, bool}, Action, bool)"/></summary>
			public bool ForceLayoutRebuildOnBeginSmoothScroll { get { return _ForceLayoutRebuildOnBeginSmoothScroll; } set { _ForceLayoutRebuildOnBeginSmoothScroll = value; } }

			[FormerlySerializedAs("forceLayoutRebuildOnDrag")]
			[SerializeField]
			bool _ForceLayoutRebuildOnDrag = false;
			[Obsolete("Use ForceLayoutRebuildOnDrag instead", true)]
			public bool forceLayoutRebuildOnDrag { get { return ForceLayoutRebuildOnDrag; } set { ForceLayoutRebuildOnDrag = value; } }
			/// <summary>
			/// Enable only if you experience issues with misaligned items. If OSA is correctly implemented, this shouldn't happen (please report if you find otherwise). 
			/// However, we still provide this property if you want a quick fix
			/// </summary>
			public bool ForceLayoutRebuildOnDrag { get { return _ForceLayoutRebuildOnDrag; } set { _ForceLayoutRebuildOnDrag = value; } }

			[SerializeField]
			[Tooltip("Whether to sort the actual GameObjects representing the items under the Content. Only use if needed, as it slightly affects performance. Off by default")]
			bool _KeepItemsSortedInHierarchy = false;
			/// <summary>
			/// Whether to sort the actual GameObjects representing the items under the Content. Only use if needed, as it slightly affects performance. Off by default
			/// </summary>
			public bool KeepItemsSortedInHierarchy { get { return _KeepItemsSortedInHierarchy; } set { _KeepItemsSortedInHierarchy = value; } }
		}
	}
}