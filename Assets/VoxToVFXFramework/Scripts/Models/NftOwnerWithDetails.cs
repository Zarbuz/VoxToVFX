using System;
using MoralisUnity.Web3Api.Models;
using Newtonsoft.Json;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.Managers.DataManager;

namespace VoxToVFXFramework.Scripts.Models
{
	public class NftOwnerWithDetails : NftOwner
	{
		public NftOwnerWithDetails(NftOwner nft)
		{
			TokenAddress = nft.TokenAddress;
			TokenId = nft.TokenId;
			ContractType = nft.ContractType;
			OwnerOf = nft.OwnerOf;
			Metadata = nft.Metadata;
			Name = nft.Name;
			SyncedAt = nft.SyncedAt;
			Symbol = nft.Symbol;
		}

		public string CollectionName => Name;


		private MetadataObject mMetadataObject;

		public MetadataObject MetadataObject
		{
			get
			{
				if (mMetadataObject == null)
				{
					mMetadataObject = JsonConvert.DeserializeObject<MetadataObject>(Metadata);
				}

				return mMetadataObject;
			}
		}


		public DateTime MintedDate => MetadataObject.MintedUTCDate;

		public decimal BuyPriceInEther
		{
			get
			{
				string key = TokenAddress + "_" + TokenId;
				if (DataManager.Instance.NFTDetailsCache.TryGetValue(key, out NFTDetailsCacheDTO detailsCache))
				{
					return detailsCache.ContractType.BuyPriceInEther;
				}

				return 0;
			}
		}
	}
}
