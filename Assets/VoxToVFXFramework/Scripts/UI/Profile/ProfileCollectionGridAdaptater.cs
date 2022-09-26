using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using frame8.Logic.Misc.Other.Extensions;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public class ProfileCollectionGridAdaptater : GridAdapter<GridParams, CollectionGridItemViewsHolder>
	{
		private List<CollectionCreatedEvent> mData;

		public void Initialize(List<CollectionCreatedEvent> data)
		{
			mData = data;
			ResetItems(data.Count, true);
		}

		protected override void OnCellViewsHolderCreated(CollectionGridItemViewsHolder cellVH, CellGroupViewsHolder<CollectionGridItemViewsHolder> cellGroup)
		{
			cellVH.SetOnClickItem(OnItemClicked);
			base.OnCellViewsHolderCreated(cellVH, cellGroup);
		}

		protected override async void UpdateCellViewsHolder(CollectionGridItemViewsHolder viewsHolder)
		{
			CollectionCreatedEvent collection = mData[viewsHolder.ItemIndex];
			viewsHolder.Collection = collection;
			Models.CollectionDetails collectionDetails = await DataManager.Instance.GetCollectionDetailsWithCache(collection.CollectionContract);

			foreach (TransparentButton transparentButton in viewsHolder.TransparentButtons)
			{
				transparentButton.ImageBackgroundActive = collectionDetails != null && !string.IsNullOrEmpty(collectionDetails.CoverImageUrl);
			}

			viewsHolder.CollectionLogoImage.transform.parent.gameObject.SetActive(collectionDetails != null && !string.IsNullOrEmpty(collectionDetails.LogoImageUrl));
			viewsHolder.CollectionCoverImage.gameObject.SetActive(collectionDetails != null && !string.IsNullOrEmpty(collectionDetails.CoverImageUrl));
			if (collectionDetails != null)
			{
				await ImageUtils.DownloadAndApplyImageAndCrop(collectionDetails.CoverImageUrl, viewsHolder.CollectionCoverImage, 398, 524);
				await ImageUtils.DownloadAndApplyImageAndCrop(collectionDetails.LogoImageUrl, viewsHolder.CollectionLogoImage, 100, 100);
			}
			viewsHolder.CollectionNameText.color = collectionDetails != null && !string.IsNullOrEmpty(collectionDetails.CoverImageUrl) ? Color.white : Color.black;
			viewsHolder.CollectionNameText.text = collection.Name;
			viewsHolder.CollectionSymbolText.text = collection.Symbol;
			viewsHolder.OpenUserProfileButton.Initialize(collection.Creator);
		}

		public override void Refresh(bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			if (mData != null)
			{
				_CellsCount = mData.Count;
			}

			base.Refresh(contentPanelEndEdgeStationary, keepVelocity);
		}

		private void OnItemClicked(CollectionGridItemViewsHolder item)
		{
			CanvasPlayerPCManager.Instance.OpenCollectionDetailsPanel(item.Collection);
		}
	}

	public class CollectionGridItemViewsHolder : CellViewsHolder
	{
		public CollectionCreatedEvent Collection;
		public RawImage CollectionCoverImage;
		public RawImage CollectionLogoImage;
		public TextMeshProUGUI CollectionNameText;
		public OpenUserProfileButton OpenUserProfileButton;
		public TextMeshProUGUI CollectionSymbolText;
		public Button Button;

		public TransparentButton[] TransparentButtons;


		public override void CollectViews()
		{
			base.CollectViews();


			Button = views.GetComponent<Button>();
			views.GetComponentAtPath("Content/CollectionImage", out CollectionCoverImage);
			views.GetComponentAtPath("Content/CollectionLogoMask/CollectionLogoImage", out CollectionLogoImage);
			views.GetComponentAtPath("Content/CollectionNameText", out CollectionNameText);
			views.GetComponentAtPath("Content/OpenCreatorProfileButton", out OpenUserProfileButton);
			views.GetComponentAtPath("Content/OpenSymbolButton/SymbolText", out CollectionSymbolText);
			TransparentButtons = views.GetComponentsInChildren<TransparentButton>();
		}

		public void SetOnClickItem(Action<CollectionGridItemViewsHolder> onClickItem)
		{
			Button.onClick.RemoveAllListeners();
			if (onClickItem != null)
			{
				Button.onClick.AddListener(() => onClickItem(this));
			}
		}
	}
}
