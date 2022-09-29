using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System.Collections.Generic;
using VoxToVFXFramework.Scripts.Models.ContractEvent;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public class ProfileCollectionGridAdaptater : GridAdapter<GridParams, CollectionGridItemViewsHolder>
	{
		private List<CollectionCreatedEvent> mData;

		public void Initialize(List<CollectionCreatedEvent> data)
		{
			mData = data;
			ResetItems(data.Count);
		}

		protected override async void UpdateCellViewsHolder(CollectionGridItemViewsHolder viewsHolder)
		{
			CollectionCreatedEvent collection = mData[viewsHolder.ItemIndex];
			await viewsHolder.ProfileCollectionItem.Initialize(collection);
		}

		public override void Refresh(bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			if (mData != null)
			{
				_CellsCount = mData.Count;
			}

			base.Refresh(contentPanelEndEdgeStationary, keepVelocity);
		}
	}

	public class CollectionGridItemViewsHolder : CellViewsHolder
	{
		public ProfileCollectionItem ProfileCollectionItem;

		public override void CollectViews()
		{
			base.CollectViews();
			ProfileCollectionItem = root.GetComponent<ProfileCollectionItem>();
		}

	}
}
