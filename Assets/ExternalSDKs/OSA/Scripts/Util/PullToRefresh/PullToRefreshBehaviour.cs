using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using frame8.Logic.Misc.Visual.UI;

namespace Com.TheFallenGames.OSA.Util.PullToRefresh
{
	/// <summary>
	/// Attach it to your ScrollView where the pull to refresh functionality is needed.
	/// Browse the PullToRefreshExample scene to see how the gizmo should be set up. An image is better than 1k words.
	/// </summary>
	public class PullToRefreshBehaviour : MonoBehaviour, IScrollRectProxy, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
        #region Serialized fields
        /// <summary>The normalized distance relative to screen size. Always between 0 and 1</summary>
        [SerializeField] [Range(.1f, 1f)] [Tooltip("The normalized distance relative to screen size. Always between 0 and 1")] 
        float _PullAmountNormalized = .25f;

        /// <summary>The reference of the gizmo to use. If null, will try to GetComponentInChildren&lt;PullToRefreshGizmo&gt;()</summary>
        [SerializeField] [Tooltip("If null, will try to GetComponentInChildren()")]
        PullToRefreshGizmo _RefreshGizmo = null;

		//[SerializeField]
		//RectTransform.Axis _Axis;

		/// <summary></summary>
		[SerializeField]
		bool _AllowPullFromEnd = false;

		/// <summary>If false, you'll need to call HideGizmo() manually after pull. Subscribe to PullToRefreshBehaviour.OnRefresh event to know when a refresh event occurred. This method is used when the gizmo should do an animation while the refresh is executing (for ex., when some data is downloading)</summary>
		[SerializeField] [Tooltip("If false, you'll need to call HideGizmo() manually after pull. Subscribe to PullToRefreshBehaviour.OnRefresh event to know when a refresh event occurred")]
        bool _AutoHideRefreshGizmo = true;
#pragma warning disable 0649
        /// <summary>Quick way of playing a sound effect when the pull power reaches 1f</summary>
        [SerializeField]
        AudioClip _SoundOnPreRefresh = null;

        /// <summary>Quick way of playing a sound effect when the refresh occurred</summary>
        [SerializeField]
        AudioClip _SoundOnRefresh = null;
		#endregion
#pragma warning restore 0649

		#region Unity events
		[Tooltip("Unity event fired when the pull was released")]
		/// <summary>Unity event (editable in inspector) fired when the refresh occurred</summary>
		public UnityEvent OnRefresh = null;

		[Tooltip("Same as OnRefresh, but also gives you the refresh sign.\n" +
				 "1 = top, -1 = bottom")]
		/// <summary>Same as <see cref="OnRefresh"/>, but also gives you the refresh sign. 1 = top, -1 = bottom</summary>
		public UnityEventFloat OnRefreshWithSign = null;

		[Tooltip("Unity event (editable in inspector) fired when each frame the click/finger is dragged after it has touched the ScrollView.\n" +
				 "Negative values indicate pulling from end")]
		/// <summary>
		/// Unity event (editable in inspector) fired when each frame the click/finger is dragged after it has touched the ScrollView.
		/// Negative values indicate pulling from end
		/// </summary>
		public UnityEventFloat OnPullProgress = null;
		#endregion

		/// <summary>
		/// Will be retrieved from the scrollrect. If not found, it can be assigned anytime before the first Update. 
		/// If not assigned, a default proxy will be used. The purpose of this is to allow custom implementations of ScrollRect to be used
		/// </summary>
		public IScrollRectProxy externalScrollRectProxy;

		#region IScrollRectProxy properties implementation
		public bool IsInitialized { get { return _ScrollRect != null; } }
		public Vector2 Velocity { get; set; }
		public bool IsHorizontal { get { return _ScrollRect.horizontal; } }
		public bool IsVertical { get { return _ScrollRect.vertical; } }
		public RectTransform Content { get { return _ScrollRect.content; } }
		public RectTransform Viewport { get { return _ScrollRect.viewport; } }
		#endregion

		IScrollRectProxy ScrollRectProxy { get { return externalScrollRectProxy == null ? this : externalScrollRectProxy; } }

		ScrollRect _ScrollRect;
        float _ResolvedAVGScreenSize;
        bool _PlayedPreSoundForCurrentDrag;
		//bool _IgnoreCurrentDrag;
		RectTransform _RT;
		int _CurrentDragSign;
		StateEnum _State;


		/// <summary>Not used in this default interface implementation</summary>
#pragma warning disable 0067
		public event Action<double> ScrollPositionChanged = delegate { };
#pragma warning restore 0067


		void Awake()
        {
			_RT = transform as RectTransform;
            _ResolvedAVGScreenSize = (Screen.width + Screen.height) / 2f;
            _ScrollRect = GetComponent<ScrollRect>();
            _RefreshGizmo = GetComponentInChildren<PullToRefreshGizmo>(); // self or children
			if (_ScrollRect)
			{
				// May be null
				externalScrollRectProxy = _ScrollRect.GetComponent(typeof(IScrollRectProxy)) as IScrollRectProxy;
			}
			else
			{
				externalScrollRectProxy = GetComponentInParent(typeof(IScrollRectProxy)) as IScrollRectProxy;
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
		}

		#region IScrollRectProxy methods implementation (used if external proxy is not manually assigned)
		public void SetNormalizedPosition(double normalizedPosition) { }

		public double GetNormalizedPosition()
		{
			if (_ScrollRect.horizontal)
				return _ScrollRect.horizontalNormalizedPosition;
			return _ScrollRect.verticalNormalizedPosition;
		}

		public double GetContentSize() { return _RT.rect.size[_ScrollRect.horizontal ? 0 : 1]; }
		public double GetViewportSize() { return Viewport.rect.size[_ScrollRect.horizontal ? 0 : 1]; }

		public void StopMovement() { _ScrollRect.StopMovement(); }
		#endregion

		#region UI callbacks
		public void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (!isActiveAndEnabled)
				return;

			if (_State != StateEnum.NONE)
				return;

			if (_RefreshGizmo.IsShown)
				return;

			if (!ScrollRectProxy.IsInitialized)
				return;

			double dragAmountNorm, _;
			GetDragAmountNormalized(eventData, out dragAmountNorm, out _);
			if (!_AllowPullFromEnd && dragAmountNorm < 0d)
				return;

			int curDragSign = Math.Sign(dragAmountNorm);
			_CurrentDragSign = curDragSign;
			_PlayedPreSoundForCurrentDrag = false;

			_State = StateEnum.DRAGGING_WAITING_FOR_PULL;
		}

        public void OnDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (!isActiveAndEnabled)
				return;

			if (!ScrollRectProxy.IsInitialized)
				return;


			switch (_State)
			{
				case StateEnum.DRAGGING_WAITING_FOR_PULL:
					if (!IsContentBiggerThanViewport() || IsScrollRectAtTarget(_CurrentDragSign))
					{
						_State = StateEnum.PULLING_WAITING_FOR_RELEASE;
						goto case StateEnum.PULLING_WAITING_FOR_RELEASE;
					}

					return;

				case StateEnum.PULLING_WAITING_FOR_RELEASE:
					if (IsContentBiggerThanViewport() && !IsScrollRectAtTarget(_CurrentDragSign))
					{
						HideGizmoInternal();
						_State = StateEnum.DRAGGING_WAITING_FOR_PULL;
						return;
					}

					double dragAmountNorm, deltaNorm;
					GetDragAmountNormalized(eventData, out dragAmountNorm, out deltaNorm);
					if (Math.Sign(dragAmountNorm) != _CurrentDragSign)
					{
						return;
					}

					//if (!_AllowPullFromEnd && dragAmountNorm < 0d)
					//{
					//	HideGizmoInternal();
					//	return;
					//}

					double pullPower = dragAmountNorm;

					ShowGizmoIfNeeded();
					if (_RefreshGizmo)
						_RefreshGizmo.OnPull(pullPower);

					if (OnPullProgress != null)
						OnPullProgress.Invoke((float)pullPower);
			
					if (Math.Abs(pullPower) >= 1d && !_PlayedPreSoundForCurrentDrag)
					{
						_PlayedPreSoundForCurrentDrag = true;

						if (_SoundOnPreRefresh)
							AudioSource.PlayClipAtPoint(_SoundOnPreRefresh, Camera.main.transform.position);
					}

					return;
			}

            //Debug.Log("eventData.pressPosition=" + eventData.pressPosition + "\n eventData.position=" + eventData.position + "\neventData.scrollDelta="+ eventData.scrollDelta);
        }

        public void OnEndDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (!ScrollRectProxy.IsInitialized)
				return;

			if (_State != StateEnum.PULLING_WAITING_FOR_RELEASE)
			{
				if (_State == StateEnum.DRAGGING_WAITING_FOR_PULL)
				{
					HideGizmoInternal();
					_State = StateEnum.NONE;
				}

				return;
			}

			bool proceedRefresh = false;
			if (isActiveAndEnabled)
			{
				proceedRefresh = true;
				if (IsContentBiggerThanViewport())
					proceedRefresh = IsScrollRectAtTarget(_CurrentDragSign);

				if (proceedRefresh)
				{
					double dragAmount, _;
					GetDragAmountNormalized(eventData, out dragAmount, out _);
					if (Math.Sign(dragAmount) != _CurrentDragSign)
						proceedRefresh = false;
					else if (Math.Abs(dragAmount) < 1d)
						proceedRefresh = false;
				}
			}

			if (proceedRefresh)
			{
				if (OnRefresh != null)
					OnRefresh.Invoke();

				if (OnRefreshWithSign != null)
					OnRefreshWithSign.Invoke(_CurrentDragSign);

				if (_RefreshGizmo)
					_RefreshGizmo.OnRefreshed(_AutoHideRefreshGizmo);

				if (_SoundOnRefresh)
					AudioSource.PlayClipAtPoint(_SoundOnRefresh, Camera.main.transform.position);
			}
			else
			{
				if (_RefreshGizmo)
					_RefreshGizmo.OnRefreshCancelled();
				_State = StateEnum.NONE;
			}

			if (_RefreshGizmo && _RefreshGizmo.IsShown)
			{
				if (_AutoHideRefreshGizmo)
				{
					HideGizmoInternal();
					_State = StateEnum.NONE;
				}
				else
				{
					_State = StateEnum.AFTER_RELEASE_WAITING_FOR_GIZMO_TO_HIDE;
				}
			}
        }
		#endregion

		// sign: 1=start(top or left), -1=end(bottom or right); 
		bool IsScrollRectAtTarget(int targetDragSign)
		{
			double normPos = ScrollRectProxy.GetNormalizedPosition();
			if (ScrollRectProxy.IsHorizontal)
				normPos = 1d - normPos;

			if (targetDragSign == 1 && normPos >= 1d)
				return true;
			if (targetDragSign == -1 && normPos <= 0d)
				return true;

			return false;
		}

		bool IsContentBiggerThanViewport() { return ScrollRectProxy.GetContentSize() > _RT.rect.size[ScrollRectProxy.IsHorizontal ? 0 : 1]; }

		public void ShowGizmoIfNeeded()
        {
			if (_RefreshGizmo && !_RefreshGizmo.IsShown)
                _RefreshGizmo.IsShown = true;
        }

        public void HideGizmo()
		{
			HideGizmoInternal();
			if (_State == StateEnum.AFTER_RELEASE_WAITING_FOR_GIZMO_TO_HIDE)
				_State = StateEnum.NONE;
		}

		void HideGizmoInternal()
		{
			if (_RefreshGizmo)
				_RefreshGizmo.IsShown = false;
		}

		void GetDragAmountNormalized(PointerEventData eventData, out double total, out double delta)
        {
			total = 0d;
			delta = 0d;
			float pos;
			float maxPullAmount = _PullAmountNormalized * _ResolvedAVGScreenSize;
			if (ScrollRectProxy.IsVertical)
            {
				pos = eventData.position.y;
				float worldDragVec = pos - eventData.pressPosition.y;
				total = -worldDragVec;
				delta = -eventData.delta.y;
            }
            else
			{
				pos = eventData.position.x;
				float worldDragVec = pos - eventData.pressPosition.x;
				total = worldDragVec;
				delta = eventData.delta.x;
			}
			total /= maxPullAmount;
			delta /= maxPullAmount;
        }


		enum StateEnum
		{
			NONE,
			DRAGGING_WAITING_FOR_PULL,
			PULLING_WAITING_FOR_RELEASE,
			AFTER_RELEASE_WAITING_FOR_GIZMO_TO_HIDE
		}


		[Serializable]
        public class UnityEventFloat : UnityEvent<float> { }
    }
}