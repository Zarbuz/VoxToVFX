using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using Newtonsoft.Json;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

		private CollectionMintedEvent mCollectionMintedEvent;
		private Nft mMetadata;
		private Models.CollectionDetails mCollectionDetails;

		#endregion

		#region PublicMethods

		public async UniTask Initialize(CollectionMintedEvent nft, CustomUser creatorUser)
		{
			mCollectionMintedEvent = nft;
			try
			{
				Button.onClick.AddListener(OnItemClicked);
				Nft tokenIdMetadata = await DataManager.Instance.GetTokenIdMetadataWithCache(address: nft.Address, tokenId: nft.TokenID);
				if (tokenIdMetadata == null)
				{
					gameObject.SetActive(false);
					return;
				}
				mCollectionDetails = await DataManager.Instance.GetCollectionDetailsWithCache(nft.Address);

				var nftDetails = await MiddlewareManager.Instance.GetNFTDetails(nft.Address, nft.TokenID);
				mMetadata = tokenIdMetadata;
				CollectionNameText.text = tokenIdMetadata.Name;
				CreatorUsernameText.text = "@" + creatorUser.UserName;

				await CreatorAvatarImage.Initialize(creatorUser);
				if (tokenIdMetadata.Metadata == null)
				{
					gameObject.SetActive(false);
					return;
				}

				MetadataObject metadataObject = JsonConvert.DeserializeObject<MetadataObject>(tokenIdMetadata.Metadata);
				Title.text = metadataObject.Name;
				await ImageUtils.DownloadAndApplyImageAndCropAfter(metadataObject.Image, MainImage, 512, 512);

				if (mCollectionDetails == null || string.IsNullOrEmpty(mCollectionDetails.LogoImageUrl))
				{
					CollectionLogoImage.gameObject.SetActive(false);
				}
				else
				{
					CollectionLogoImage.gameObject.SetActive(true);
					await ImageUtils.DownloadAndApplyImageAndCropAfter(mCollectionDetails.LogoImageUrl,
						CollectionLogoImage, 32, 32);
				}

				InitSuccess = true;
			}
			catch (Exception e)
			{
				gameObject.SetActive(false);
				Debug.LogError(e.Message);
			}
		}

		#endregion

		#region PrivateMethods

		private void OnItemClicked()
		{
			CanvasPlayerPCManager.Instance.OpenNftDetailsPanel(mCollectionMintedEvent, mMetadata);
		}

		#endregion
	}
}
