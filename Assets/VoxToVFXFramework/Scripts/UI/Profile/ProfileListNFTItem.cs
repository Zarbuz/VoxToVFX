using System;
using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
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
		[SerializeField] private AvatarImage OwnerAvatarImage;
		[SerializeField] private TextMeshProUGUI OwnerUsernameText;

		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private TextMeshProUGUI CollectionNameText;

		[SerializeField] private ButtonTriggerUserDetailsPopup CreatorTrigger;
		[SerializeField] private ButtonTriggerUserDetailsPopup OwnerTrigger;

		#endregion

		#region Fields

		public bool InitSuccess { get; private set; }
		public bool IsReadyOnly { get; set; }
		public decimal BuyPriceInEther { get; private set; }
		public string CollectionName { get; private set; }
		public DateTime MintedDate { get; private set; }

		private NftOwner mNft;

		#endregion

		#region PublicMethods

		public async UniTask Initialize(NftOwner nft)
		{
			mNft = nft;
			Button.onClick.AddListener(OnItemClicked);
			Models.CollectionDetails collectionDetails = await DataManager.Instance.GetCollectionDetailsWithCache(nft.TokenAddress);
			NFTDetailsContractType details = await DataManager.Instance.GetNFTDetailsWithCache(nft.TokenAddress, nft.TokenId);

			if (nft.OwnerOf == UserManager.Instance.CurrentUserAddress)
			{
				OwnerAvatarImage.gameObject.SetActive(false);
				OwnerUsernameText.text = string.Empty;
			}
			else
			{
				CustomUser ownerUser = await DataManager.Instance.GetUserWithCache(nft.OwnerOf);
				OwnerTrigger.Initialize(nft.OwnerOf);
				if (ownerUser != null)
				{

					OwnerUsernameText.text = "@" + ownerUser.UserName;
					OwnerAvatarImage.gameObject.SetActive(true);
					await OwnerAvatarImage.Initialize(ownerUser);
				}
				else
				{
					OwnerUsernameText.text = nft.OwnerOf.FormatEthAddress(6);
					await OwnerAvatarImage.Initialize(null);
				}
			}


			ActionText.text = details != null ? details.TargetAction : string.Empty;
			BuyPriceInEther = details?.BuyPriceInEther ?? 0;
			PriceText.text = details != null && details.BuyPriceInEther != 0 ? details.BuyPriceInEtherFixedPoint + " ETH" : string.Empty;
			CollectionNameText.text = nft.Name;
			CollectionName = nft.Name;

			string ethAddress = await DataManager.Instance.GetCreatorOfCollection(nft.TokenAddress);
			CustomUser creatorUser = await DataManager.Instance.GetUserWithCache(ethAddress);
			CreatorUsernameText.text = "@" + creatorUser.UserName;
			UniTask task1 = CreatorAvatarImage.Initialize(creatorUser);
			CreatorTrigger.Initialize(creatorUser.EthAddress);

			MetadataObject metadataObject = JsonConvert.DeserializeObject<MetadataObject>(nft.Metadata);
			Title.text = metadataObject.Name;
			MintedDate = metadataObject.MintedUTCDate;
			UniTask<bool> task2 = ImageUtils.DownloadAndApplyImageAndCropAfter(metadataObject.Image, MainImage, 512, 512);

			if (collectionDetails == null || string.IsNullOrEmpty(collectionDetails.LogoImageUrl))
			{
				CollectionLogoImage.gameObject.SetActive(false);
				await UniTask.WhenAll(task1, task2);
			}
			else
			{
				CollectionLogoImage.gameObject.SetActive(true);
				UniTask<bool> task3 = ImageUtils.DownloadAndApplyImageAndCropAfter(collectionDetails.LogoImageUrl,
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
				CanvasPlayerPCManager.Instance.OpenNftDetailsPanel(mNft);
			}
		}

		#endregion
	}
}
