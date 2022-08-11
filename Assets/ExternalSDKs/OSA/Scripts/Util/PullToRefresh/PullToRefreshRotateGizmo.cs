using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Com.TheFallenGames.OSA.Util.PullToRefresh
{
    /// <summary>
    /// <para> Implementation of <see cref="PullToRefreshGizmo"/> that uses a rotating image to show the pull progress. </para>
    /// <para>The image is rotated by the amount of distance traveled by the click/finger.</para>
    /// <para>When enough pulling distance is covered the gizmo enters the "ready to refresh" state,</para>
    /// <para>the rotation amount applied is damped by <see cref="_ExcessPullRotationDamping"/> (i.e. a value of 1f won't apply any furter rotation, </para>
    /// <para>while a value of 0f will apply the same amount of rotation per distance traveled by the click/finger as before the "ready to refresh" state).</para>
    /// <para>When <see cref="OnRefreshed(bool)"/> is called with true, the gizmo will disappear; if it'll be called with false, </para>
    /// <para>it'll start auto-rotating with a speed of <see cref="_AutoRotationDegreesPerSec"/> degrees per second, until <see cref="IsShown"/> is set to false.</para>
    /// <para>This last use-case is very common for when the refresh event actually takes time (i.e. retrieving items from a server).</para>
    /// </summary>
    public class PullToRefreshRotateGizmo : PullToRefreshGizmo
    {
#pragma warning disable 0649
		[SerializeField]
		[FormerlySerializedAs("_StartingPoint")]
		RectTransform _PullFromStartInitial = null;
		[SerializeField]
		[FormerlySerializedAs("_EndingPoint")]
		RectTransform _PullFromStartTarget = null;

		[SerializeField]
		RectTransform _PullFromEndInitial = null;
		[SerializeField]
		RectTransform _PullFromEndTarget = null;
#pragma warning restore 0649

		//[Tooltip("When pulling is done from the end, this gizmo will also appear at the end, not at the start. " +
		//	"\nThe gizmo's position in this case is inferred using the parent's size and _StartingPoint & _EndingPoint ")]
		//[SerializeField]
		//bool _AllowAppearingFromEnd = true;

        [SerializeField]
        [Range(0f, 1f)]
        float _ExcessPullRotationDamping = .95f;

		[SerializeField]
		float _AutoRotationDegreesPerSec = 200;

		[Tooltip("Will also interpolate its own scale between the Initial's and Target's scale")]
		[SerializeField]
		bool _ScaleWithTarget = true;

		[Tooltip("If true, it won't be affected by Time.timeScale")]
		[SerializeField]
		bool _UseUnscaledTime = true;

		bool _WaitingForManualHide;


        /// <summary>Calls base implementation + resets the rotation to default each time is assigned, regardless if true or false</summary>
        public override bool IsShown
        {
            get { return base.IsShown; }

            set
            {
                base.IsShown = value;

                // Reset to default rotation
                transform.localRotation = Quaternion.Euler(_InitialLocalRotation);

                if (!value)
                    _WaitingForManualHide = false;
            }
        }



        Vector3 _InitialLocalRotation, _InitialLocalScale;
		Transform _TR;


		public override void Awake()
        {
            base.Awake();
			_TR = transform;

			_InitialLocalRotation = _TR.localRotation.eulerAngles;
			_InitialLocalScale = _TR.localScale;
        }


        void Update()
        {
            if (_WaitingForManualHide)
            {
                SetLocalZRotation((_TR.localEulerAngles.z - (_UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * _AutoRotationDegreesPerSec) % 360);
            }
        }

		public override void OnPull(double power)
        {
            base.OnPull(power);

			double powerAbs = Math.Abs(power);
			int powerSign = Math.Sign(power);
			float powerAbsClamped01 = Mathf.Clamp01((float)powerAbs);
            float excess = Mathf.Max(0f, (float)powerAbs - 1f);

            float dampedExcess = excess * (1f - _ExcessPullRotationDamping);

            SetLocalZRotation((_InitialLocalRotation.z - 360 * (powerAbsClamped01 + dampedExcess)) % 360);

			//_TR.position = LerpUnclamped(_StartingPoint.position, _EndingPoint.position, power <= 1f ? (power - (1f - power/2)*(1f-power/2)) : (1 - 1/(1 + excess) ));
			Vector3 start, end;
			Vector3 scaleStart, scaleEnd;
			if (powerSign < 0 && _PullFromEndInitial && _PullFromEndTarget)
			{
				start = _PullFromEndInitial.position;
				end = _PullFromEndTarget.position;
				scaleStart = _PullFromEndInitial.localScale;
				scaleEnd = _PullFromEndTarget.localScale;
			}
			else
			{
				start = _PullFromStartInitial.position;
				end = _PullFromStartTarget.position;
				scaleStart = _PullFromStartInitial.localScale;
				scaleEnd = _PullFromStartTarget.localScale;
			}

			var t01Unclamped = 2 - 2 / (1 + powerAbsClamped01);
			_TR.position = LerpUnclamped(start, end, t01Unclamped);
			if (_ScaleWithTarget)
				_TR.localScale = LerpUnclamped(scaleStart, scaleEnd, t01Unclamped);
			else
				_TR.localScale = _InitialLocalScale;
        }

		public override void OnRefreshCancelled()
        {
            base.OnRefreshCancelled();

            _WaitingForManualHide = false;
        }

		public override void OnRefreshed(bool autoHide)
        {
            base.OnRefreshed(autoHide);

            _WaitingForManualHide = !autoHide;
        }

        Vector3 LerpUnclamped(Vector3 from, Vector3 to, float t) { return (1f - t) * from + t * to ; }

        void SetLocalZRotation(float zRotation)
        {
            var rotE = _TR.localRotation.eulerAngles;
            rotE.z = zRotation;
			_TR.localRotation = Quaternion.Euler(rotE);
        }
    }
}