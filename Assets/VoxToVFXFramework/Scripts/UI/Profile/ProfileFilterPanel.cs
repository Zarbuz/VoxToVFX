using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.Models;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public enum eFilterOrderBy
	{
		//MOST_ACTIVE,
		PRICE_HIGHEST_FIRST,
		PRICE_LOWEST_FIRST,
		NEWEST,
		OLDEST
	}

	public interface IFilterPanelListener
	{
		string UserAddress { get; }
		void OnFilterOrderByChanged(eFilterOrderBy orderBy);
		void OnCollectionFilterChanged(string collectionName);
	}

	public class ProfileFilterPanel : MonoBehaviour
	{
		#region ScriptParameters

		//[SerializeField] private Toggle LiveAuctionFilterButton;
		//[SerializeField] private TextMeshProUGUI LiveAuctionCountText;
		//[SerializeField] private Toggle BuyNowFilterButton;
		//[SerializeField] private TextMeshProUGUI BuyNowCountText;
		//[SerializeField] private Toggle ReservePriceButton;
		//[SerializeField] private TextMeshProUGUI ReservePriceCountText;
		//[SerializeField] private Toggle ActiveOfferButton;
		//[SerializeField] private TextMeshProUGUI ActiveOfferCountText;
		[SerializeField] private TMP_Dropdown CollectionDropdown;
		[SerializeField] private TMP_Dropdown OrderByDropdown;

		#endregion

		#region Fields

		public IFilterPanelListener FilterPanelListener { get; private set; }
		private List<string> mCollectionsList = new List<string>();

		#endregion

		#region UnityMethods

		private void Start()
		{
			//LiveAuctionFilterButton.onValueChanged.AddListener((b) => OnFilterStateChanged(b,eFilterState.LIVE_AUCTION));
			//BuyNowFilterButton.onValueChanged.AddListener((b) => OnFilterStateChanged(b, eFilterState.BUY_NOW));
			//ReservePriceButton.onValueChanged.AddListener((b) => OnFilterStateChanged(b, eFilterState.RESERVE_PRICE));
			//ActiveOfferButton.onValueChanged.AddListener((b) => OnFilterStateChanged(b, eFilterState.ACTIVE_OFFER));

			CollectionDropdown.onValueChanged.AddListener(OnCollectionValueChanged);
			OrderByDropdown.onValueChanged.AddListener(OnOrderByValueChanged);
			OrderByDropdown.ClearOptions();
			OrderByDropdown.AddOptions(new List<TMP_Dropdown.OptionData>()
			{
				//new(("[PROFILE_ORDER_BY_" + eFilterOrderBy.MOST_ACTIVE +"]").Translate()),
				new(("[PROFILE_ORDER_BY_" + eFilterOrderBy.NEWEST +"]").Translate()),
				new(("[PROFILE_ORDER_BY_" + eFilterOrderBy.OLDEST +"]").Translate()),
				new(("[PROFILE_ORDER_BY_" + eFilterOrderBy.PRICE_HIGHEST_FIRST +"]").Translate()),
				new(("[PROFILE_ORDER_BY_" + eFilterOrderBy.PRICE_LOWEST_FIRST +"]").Translate()),
			});
		}

		#endregion

		#region PublicMethods

		public async void Initialize(IFilterPanelListener filterPanelListener)
		{
			FilterPanelListener = filterPanelListener;
			CollectionDropdown.ClearOptions();
			List<CollectionCreatedEvent> list = await DataManager.Instance.GetUserListContractWithCache(filterPanelListener.UserAddress);
			mCollectionsList = list.Select(t => t.Name).ToList();
			mCollectionsList.Insert(0, "-"); 
			CollectionDropdown.AddOptions(mCollectionsList);
			OnOrderByValueChanged(0); //Force refresh
		}

		#endregion

		#region PrivateMethods

		private void OnCollectionValueChanged(int index)
		{
			FilterPanelListener?.OnCollectionFilterChanged(index == 0 ? string.Empty : mCollectionsList[index]);
		}


		private void OnOrderByValueChanged(int index)
		{
			switch (index)
			{
				case 0:
					FilterPanelListener?.OnFilterOrderByChanged(eFilterOrderBy.NEWEST);
					break;
				case 1:
					FilterPanelListener?.OnFilterOrderByChanged(eFilterOrderBy.OLDEST);
					break;
				case 2:
					FilterPanelListener?.OnFilterOrderByChanged(eFilterOrderBy.PRICE_HIGHEST_FIRST);
					break;
				case 3:
					FilterPanelListener?.OnFilterOrderByChanged(eFilterOrderBy.PRICE_LOWEST_FIRST);
					break;
			}
		}
		#endregion
	}
}
