using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public enum eFilterState
	{
		NONE,
		LIVE_AUCTION,
		BUY_NOW,
		RESERVE_PRICE,
		ACTIVE_OFFER
	}

	public enum eFilterOrderBy
	{
		MOST_ACTIVE,
		PRICE_HIGHEST_FIRST,
		PRICE_LOWEST_FIRST,
		NEWEST,
		OLDEST
	}

	public interface IFilterPanelListener
	{
		void OnFilterStateChanged(eFilterState state);
		void OnFilterOrderByChanged(eFilterOrderBy orderBy);
	}

	public class ProfileFilterPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Toggle LiveAuctionFilterButton;
		[SerializeField] private TextMeshProUGUI LiveAuctionCountText;
		[SerializeField] private Toggle BuyNowFilterButton;
		[SerializeField] private TextMeshProUGUI BuyNowCountText;
		[SerializeField] private Toggle ReservePriceButton;
		[SerializeField] private TextMeshProUGUI ReservePriceCountText;
		[SerializeField] private Toggle ActiveOfferButton;
		[SerializeField] private TextMeshProUGUI ActiveOfferCountText;
		[SerializeField] private TMP_Dropdown CollectionDropdown;
		[SerializeField] private TMP_Dropdown OrderByDropdown;

		#endregion

		#region Fields

		public IFilterPanelListener FilterPanelListener { get; private set; }

		#endregion

		#region UnityMethods

		private void Start()
		{
			LiveAuctionFilterButton.onValueChanged.AddListener((b) => OnFilterStateChanged(b,eFilterState.LIVE_AUCTION));
			BuyNowFilterButton.onValueChanged.AddListener((b) => OnFilterStateChanged(b, eFilterState.BUY_NOW));
			ReservePriceButton.onValueChanged.AddListener((b) => OnFilterStateChanged(b, eFilterState.RESERVE_PRICE));
			ActiveOfferButton.onValueChanged.AddListener((b) => OnFilterStateChanged(b, eFilterState.ACTIVE_OFFER));
			OrderByDropdown.onValueChanged.AddListener(OnOrderByValueChanged);
			OrderByDropdown.ClearOptions();
			OrderByDropdown.AddOptions(new List<TMP_Dropdown.OptionData>()
			{
				new(("[PROFILE_ORDER_BY_" + eFilterOrderBy.MOST_ACTIVE +"]").Translate()),
				new(("[PROFILE_ORDER_BY_" + eFilterOrderBy.PRICE_HIGHEST_FIRST +"]").Translate()),
				new(("[PROFILE_ORDER_BY_" + eFilterOrderBy.PRICE_LOWEST_FIRST +"]").Translate()),
				new(("[PROFILE_ORDER_BY_" + eFilterOrderBy.NEWEST +"]").Translate()),
				new(("[PROFILE_ORDER_BY_" + eFilterOrderBy.OLDEST +"]").Translate())
			});
		}

		#endregion

		#region PublicMethods

		public void Initialize(IFilterPanelListener filterPanelListener)
		{
			FilterPanelListener = filterPanelListener;
		}

		#endregion

		#region PrivateMethods

		private void OnFilterStateChanged(bool active, eFilterState filterState)
		{
			FilterPanelListener?.OnFilterStateChanged(active ? filterState : eFilterState.NONE);
		}

		private void OnOrderByValueChanged(int index)
		{
			switch (index)
			{
				case 0:
					FilterPanelListener?.OnFilterOrderByChanged(eFilterOrderBy.MOST_ACTIVE);
					break;
				case 1:
					FilterPanelListener?.OnFilterOrderByChanged(eFilterOrderBy.PRICE_LOWEST_FIRST);
					break;
				case 2:
					FilterPanelListener?.OnFilterOrderByChanged(eFilterOrderBy.PRICE_HIGHEST_FIRST);
					break;
				case 3:
					FilterPanelListener?.OnFilterOrderByChanged(eFilterOrderBy.NEWEST);
					break;
				case 4:
					FilterPanelListener?.OnFilterOrderByChanged(eFilterOrderBy.OLDEST);
					break;
			}
		}
		#endregion
	}
}
