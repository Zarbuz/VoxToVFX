using UnityEngine;

namespace Com.TheFallenGames.OSA.Util.PullToRefresh
{
	/// <summary> Base class for gizmos that can be used with <see cref="PullToRefreshBehaviour"/> (see it for more details). Attach it to your ScrollView </summary>
	public class PullToRefreshGizmo : MonoBehaviour
    {
        /// <summary>Property that can be overriden by the inheritors. The default implementation is to set whether the game object is active or not</summary>
        public virtual bool IsShown
        {
            get { return _IsShown; }
            set
            {
                _IsShown = value;

                gameObject.SetActive(_IsShown);
            }
        }

        bool _IsShown;


		public virtual void Awake()
        {}


		/// <summary>
		/// <para>Called for each OnDrag event on the ScrollView. In other words, it's called continuously during moving the mouse/finger after the click</para>
		/// </summary>
		/// <param name="power">0d = didn't drag at all; .5d = dragged half-way from start; 1d = dragged from start exactly at the minimum needed point in 
		/// order for a refresh event to occur; values will exceed 1f after this minimum drag amount is exceeded 
		/// (which can be used to visualize the fact that after the click/finger is released, the refresh will occur).
		/// Negative values indicate a pull from the end instead of start, and the same rules apply
		/// </param>
		public virtual void OnPull(double power)
        {}

		/// <summary> Called when the refresh did occur (dragged with at least 1f power and released)</summary>
		/// <param name="autoHide">A hint for the gizmo to know whether it should hide itself or something will hide it externally by setting <see cref="IsShown"/>=false </param>
		public virtual void OnRefreshed(bool autoHide)
        {
            if (autoHide)
                IsShown = false;
        }

		/// <summary> Called when the click/finger was released before the pullPower reached 1f</summary>
		public virtual void OnRefreshCancelled()
        { IsShown = false; }
    }
}