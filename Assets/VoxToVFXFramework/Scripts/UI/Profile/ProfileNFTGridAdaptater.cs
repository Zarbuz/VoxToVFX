using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Cysharp.Threading.Tasks;
using frame8.Logic.Misc.Other.Extensions;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public class ProfileNFTGridAdaptater : GridAdapter<GridParams, NFTGridItemViewsHolder>
	{
		private List<NftOwnerWithDetails> mData;

		public void Initialize(List<NftOwnerWithDetails> nfts)
		{
			mData = nfts;
			ResetItems(nfts.Count, true);
		}

		protected override void OnCellViewsHolderCreated(NFTGridItemViewsHolder cellVH, CellGroupViewsHolder<NFTGridItemViewsHolder> cellGroup)
		{
			cellVH.SetOnClickItem(OnItemClicked);
			base.OnCellViewsHolderCreated(cellVH, cellGroup);
		}

		protected override async void UpdateCellViewsHolder(NFTGridItemViewsHolder viewsHolder)
		{
			NftOwnerWithDetails nft = mData[viewsHolder.ItemIndex];

			viewsHolder.Nft = nft;
			Models.CollectionDetails collectionDetails = await DataManager.Instance.GetCollectionDetailsWithCache(nft.TokenAddress);
			NFTDetailsContractType details = await DataManager.Instance.GetNFTDetailsWithCache(nft.TokenAddress, nft.TokenId);

			if (details.OwnerInLowercase == UserManager.Instance.CurrentUserAddress)
			{
				viewsHolder.OwnerAvatarImage.gameObject.SetActive(false);
				viewsHolder.OwnerUsernameText.text = string.Empty;
			}
			else
			{
				CustomUser ownerUser = await DataManager.Instance.GetUserWithCache(details.OwnerInLowercase);
				viewsHolder.OwnerTrigger.Initialize(details.OwnerInLowercase);
				if (ownerUser != null)
				{
					viewsHolder.OwnerUsernameText.text = "@" + ownerUser.UserName;
					viewsHolder.OwnerAvatarImage.gameObject.SetActive(true);
					await viewsHolder.OwnerAvatarImage.Initialize(ownerUser);
				}
				else
				{
					viewsHolder.OwnerUsernameText.text = details.OwnerInLowercase.FormatEthAddress(6);
					await viewsHolder.OwnerAvatarImage.Initialize(null);
				}
			}


			viewsHolder.ActionText.text = details.TargetAction;
			viewsHolder.PriceText.text = details.BuyPriceInEther != 0 ? details.BuyPriceInEtherFixedPoint + "  " + Moralis.CurrentChain.Symbol : string.Empty;
			viewsHolder.CollectionNameText.text = nft.Name;

			string ethAddress = await DataManager.Instance.GetCreatorOfCollection(nft.TokenAddress);
			CustomUser creatorUser = await DataManager.Instance.GetUserWithCache(ethAddress);
			viewsHolder.CreatorUsernameText.text = "@" + creatorUser.UserName;
			UniTask task1 = viewsHolder.CreatorAvatarImage.Initialize(creatorUser);
			viewsHolder.CreatorTrigger.Initialize(creatorUser.EthAddress);

			viewsHolder.Title.text = nft.MetadataObject.Name;
			UniTask<bool> task2 = ImageUtils.DownloadAndApplyImageAndCrop(nft.MetadataObject.Image, viewsHolder.MainImage, 512, 512);

			if (collectionDetails == null || string.IsNullOrEmpty(collectionDetails.LogoImageUrl))
			{
				viewsHolder.CollectionLogoImage.gameObject.SetActive(false);
				await UniTask.WhenAll(task1, task2);
			}
			else
			{
				viewsHolder.CollectionLogoImage.gameObject.SetActive(true);
				UniTask<bool> task3 = ImageUtils.DownloadAndApplyImageAndCrop(collectionDetails.LogoImageUrl, viewsHolder.CollectionLogoImage, 32, 32);
				await UniTask.WhenAll(task1, task2, task3);
			}
		}

		/// <param name="contentPanelEndEdgeStationary">ignored because we override this via <see cref="freezeContentEndEdgeOnCountChange"/></param>
		/// <seealso cref="GridAdapter{TParams, TCellVH}.Refresh(bool, bool)"/>
		public override void Refresh(bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			if (mData != null)
			{
				_CellsCount = mData.Count;
			}

			base.Refresh(true, keepVelocity);
		}

		private void OnItemClicked(NFTGridItemViewsHolder item)
		{
			CanvasPlayerPCManager.Instance.OpenNftDetailsPanel(item.Nft);
		}
	}

	public class NFTGridItemViewsHolder : CellViewsHolder
	{
		public RawImage MainImage;
		public RawImage CollectionLogoImage;
		public Button Button;
		public AvatarImage CreatorAvatarImage;
		public TextMeshProUGUI CreatorUsernameText;
		public TextMeshProUGUI ActionText;
		public TextMeshProUGUI PriceText;
		public AvatarImage OwnerAvatarImage;
		public TextMeshProUGUI OwnerUsernameText;

		public TextMeshProUGUI Title;
		public TextMeshProUGUI CollectionNameText;

		public ButtonTriggerUserDetailsPopup CreatorTrigger;
		public ButtonTriggerUserDetailsPopup OwnerTrigger;

		public NftOwner Nft;

		public override void CollectViews()
		{
			base.CollectViews();

			Button = views.GetComponent<Button>();
			views.GetComponentAtPath("Content/Mask/MainImage", out MainImage);
			views.GetComponentAtPath("Content/CreatorAvatarImage", out CreatorAvatarImage);
			views.GetComponentAtPath("Content/ActionText", out ActionText);
			views.GetComponentAtPath("Content/PriceText", out PriceText);
			views.GetComponentAtPath("Content/CreatorText", out CreatorUsernameText);
			views.GetComponentAtPath("Content/OwnerUsernameText", out OwnerUsernameText);
			views.GetComponentAtPath("Content/OwnerUsernameText/OwnerAvatarImage", out OwnerAvatarImage);
			views.GetComponentAtPath("Content/OwnerUsernameText/OwnerAvatarImage", out OwnerAvatarImage);
			views.GetComponentAtPath("Content/CanvasGroup/Top/CollectionLogoImage", out CollectionLogoImage);
			views.GetComponentAtPath("Content/CanvasGroup/Top/CollectionName", out CollectionNameText);
			views.GetComponentAtPath("Content/CanvasGroup/Title", out Title);

			CreatorTrigger = CreatorUsernameText.GetComponent<ButtonTriggerUserDetailsPopup>();
			OwnerTrigger = OwnerUsernameText.GetComponent<ButtonTriggerUserDetailsPopup>();
		}


		public void SetOnClickItem(Action<NFTGridItemViewsHolder> onClickQuestion)
		{
			Button.onClick.RemoveAllListeners();
			if (onClickQuestion != null)
				Button.onClick.AddListener(() => onClickQuestion(this));
		}
	}
}
