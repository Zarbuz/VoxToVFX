using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using MoralisUnity.Web3Api.Models;
using System.Collections.Generic;
using VoxToVFXFramework.Scripts.Models;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public class ProfileNFTGridAdaptater : GridAdapter<GridParams, NFTGridItemViewsHolder>
	{
		private List<NftOwnerWithDetails> mData;

		public void Initialize(List<NftOwnerWithDetails> nfts)
		{
			base.Start();
			mData = nfts;
			ResetItems(nfts.Count);
		}

		protected override async void UpdateCellViewsHolder(NFTGridItemViewsHolder viewsHolder)
		{
			NftOwnerWithDetails nft = mData[viewsHolder.ItemIndex];

			viewsHolder.Nft = nft;
			await viewsHolder.Item.Initialize(nft);
		}

		/// <param name="contentPanelEndEdgeStationary">ignored because we override this via <see cref="freezeContentEndEdgeOnCountChange"/></param>
		/// <seealso cref="GridAdapter{TParams, TCellVH}.Refresh(bool, bool)"/>
		public override void Refresh(bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			if (mData != null)
			{
				_CellsCount = mData.Count;
			}

			base.Refresh(contentPanelEndEdgeStationary, keepVelocity);
		}

	}

	public class NFTGridItemViewsHolder : CellViewsHolder
	{
		public ProfileListNFTItem Item;

		public NftOwner Nft;

		public override void CollectViews()
		{
			base.CollectViews();

			Item = root.GetComponent<ProfileListNFTItem>();
		}

	}
}
