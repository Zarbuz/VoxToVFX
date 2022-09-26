using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;
using Com.TheFallenGames.OSA.Core;

namespace Com.TheFallenGames.OSA.Util
{
	/// <summary>
	/// Important note: if used with ScrollbarFixer8 (which is true in the most cases, 
	/// make sure <see cref="ScrollbarFixer8.minSize"/> is not too small
	/// </summary>
	public class DiscreteScrollbar : MonoBehaviour 
	{
		public RectTransform slotPrefab;
		public RectTransform slotsParent;
		public UnityIntEvent OnSlotSelected;
		public Func<int> getItemsCountFunc;

		Scrollbar _Scrollbar;
		RectTransform[] slots = new RectTransform[0];
		RectTransform _ScrollbarPanelRT;
		IScrollRectProxy _ScrollRectProxy;
		int _OneIfVert_ZeroIfHor;

		const int MAX_COUNT = 100;
		bool _UpdatePending;


		void Awake()
		{
			// Get in parent, but ignore self
			_ScrollRectProxy = transform.parent.GetComponentInParent<IScrollRectProxy>();
			if (_ScrollRectProxy == null)
				throw new OSAException(GetType().Name + ": No IScrollRectProxy component found in parent");

			_Scrollbar = GetComponent<Scrollbar>();
			_ScrollbarPanelRT = _Scrollbar.transform as RectTransform;
			_OneIfVert_ZeroIfHor = _ScrollRectProxy.IsHorizontal ? 0 : 1;

		}

		void OnEnable() { _UpdatePending = false; }

		public void OnScrollbarSizeChanged()
		{
			StartCoroutine(UpdateSize());
		}

		IEnumerator UpdateSize()
		{
			while (_UpdatePending) // wait for prev request to complete
				yield return null;

			_UpdatePending = true;
			yield return null;

			if (getItemsCountFunc == null)
				throw new OSAException(GetType().Name + "getItemsCountFunc==null. Please specify a count provider");

			_UpdatePending = true;
			int count = getItemsCountFunc(); 
			if (count > MAX_COUNT)
				throw new OSAException(GetType().Name + ": count is " + count + ". Bigger than MAX_COUNT=" + MAX_COUNT + ". Are you sure you want to use a discrete scrollbar?");

			Rebuild(count);
			_UpdatePending = false;
		}

		public void Rebuild(int numSlots)
		{
			slotPrefab.gameObject.SetActive(true);

			// Clear prev
			if (slots != null)
				foreach (var slot in slots)
					Destroy(slot.gameObject);

			// Add new
			slots = new RectTransform[numSlots];
			float sizesCumu = 0;
			float slotSize = _ScrollbarPanelRT.rect.size[_OneIfVert_ZeroIfHor] / numSlots; // not using the handle's size because of rounding errors with higher <numSlots>
			RectTransform.Edge edgeToInsetFrom = _OneIfVert_ZeroIfHor == 1 ? RectTransform.Edge.Top : RectTransform.Edge.Left;
			for (int i = 0; i < numSlots; i++)
			{
				var slot = (Instantiate(slotPrefab.gameObject) as GameObject).GetComponent<RectTransform>();
				slots[i] = slot;
				slot.SetParent(slotsParent, false);
				slot.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(edgeToInsetFrom, sizesCumu, slotSize);
				sizesCumu += slotSize;
				int copyOfI = i;
				slot.GetComponentInChildren<Button>().onClick.AddListener(() => { if (OnSlotSelected != null) OnSlotSelected.Invoke(copyOfI); });
			}
			slotPrefab.gameObject.SetActive(false);
		}


		[Serializable]
		public class UnityIntEvent : UnityEvent<int> { }
	}
}