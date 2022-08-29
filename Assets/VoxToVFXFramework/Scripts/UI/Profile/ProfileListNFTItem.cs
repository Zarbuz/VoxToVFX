using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using Newtonsoft.Json;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public class ProfileListNFTItem : AbstractCardItem
	{
		#region ScriptParameters

		[SerializeField] private Image MainImage;
		[SerializeField] private Image CollectionLogoImage;
		[SerializeField] private Button Button;
		[SerializeField] private AvatarImage CreatorAvatarImage;
		[SerializeField] private TextMeshProUGUI CreatorUsernameText;
		[SerializeField] private TextMeshProUGUI ActionText;
		[SerializeField] private TextMeshProUGUI PriceText;
		[SerializeField] private AvatarImage BuyerAvatarImage;
		[SerializeField] private TextMeshProUGUI BuyerUsernameText;

		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private TextMeshProUGUI CollectionNameText;

		#endregion

		#region Fields

		public bool InitSuccess { get; private set; }
		public bool IsReadyOnly { get; set; }

		private CollectionMintedEvent mCollectionMintedEvent;
		private Nft mMetadata;
		private Models.CollectionDetails mCollectionDetails;

		#endregion

		#region PublicMethods

		public async UniTask Initialize(CollectionMintedEvent nft)
		{
			mCollectionMintedEvent = nft;
			Button.onClick.AddListener(OnItemClicked);
			Nft tokenIdMetadata = await DataManager.Instance.GetTokenIdMetadataWithCache(address: nft.Address, tokenId: nft.TokenID);
			if (tokenIdMetadata == null)
			{
				gameObject.SetActive(false);
				return;
			}
			mCollectionDetails = await DataManager.Instance.GetCollectionDetailsWithCache(nft.Address);

			NFTDetailsContractType details = await DataManager.Instance.GetNFTDetailsWithCache(nft.Address, nft.TokenID);

			BuyerAvatarImage.gameObject.SetActive(false);
			BuyerUsernameText.text = string.Empty;
			if (details != null)
			{
				ActionText.text = details.TargetAction;
				if (details.BuyPriceInEther != 0)
				{
					PriceText.text = details.BuyPriceInEtherFixedPoint + " ETH";
				}
				else
				{
					PriceText.text = string.Empty;
					ActionText.text = string.Empty;// Sure ?
				}
			}
			else
			{
				ActionText.text = string.Empty;
				PriceText.text = string.Empty;
			}

			mMetadata = tokenIdMetadata;
			CollectionNameText.text = tokenIdMetadata.Name;
			
			if (tokenIdMetadata.Metadata == null)
			{
				gameObject.SetActive(false);
				return;
			}

			CustomUser creatorUser = await DataManager.Instance.GetUserWithCache(nft.Creator);
			CreatorUsernameText.text = "@" + creatorUser.UserName;
			UniTask task1 = CreatorAvatarImage.Initialize(creatorUser);

			MetadataObject metadataObject = JsonConvert.DeserializeObject<MetadataObject>(tokenIdMetadata.Metadata);
			Title.text = metadataObject.Name;
			UniTask<bool> task2 = ImageUtils.DownloadAndApplyImageAndCropAfter(metadataObject.Image, MainImage, 512, 512);

			if (mCollectionDetails == null || string.IsNullOrEmpty(mCollectionDetails.LogoImageUrl))
			{
				CollectionLogoImage.gameObject.SetActive(false);
				await UniTask.WhenAll(task1, task2);
			}
			else
			{
				CollectionLogoImage.gameObject.SetActive(true);
				UniTask<bool> task3 = ImageUtils.DownloadAndApplyImageAndCropAfter(mCollectionDetails.LogoImageUrl,
					CollectionLogoImage, 32, 32);
				await UniTask.WhenAll(task1, task2, task3);
			}

			InitSuccess = true;
		}


		#endregion

		#region PrivateMethods

		private void OnItemClicked()
		{
			if (!IsReadyOnly)
			{
				CanvasPlayerPCManager.Instance.OpenNftDetailsPanel(mCollectionMintedEvent, mMetadata);
			}
		}

		#endregion
	}
}
