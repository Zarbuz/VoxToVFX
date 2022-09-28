using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.Localization;
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

		[SerializeField] private RawImage MainImage;
		[SerializeField] private RawImage CollectionLogoImage;
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

		public bool IsReadyOnly { get; set; }

		private NftWithDetails mNft;

		#endregion

		#region PublicMethods

		public async UniTask Initialize(NftWithDetails nft)
		{
			mNft = nft;
			Button.onClick.AddListener(OnItemClicked);
			Models.CollectionDetails collectionDetails = await DataManager.Instance.GetCollectionDetailsWithCache(nft.TokenAddress);
			NFTDetailsContractType details = await DataManager.Instance.GetNFTDetailsWithCache(nft.TokenAddress, nft.TokenId);
			RefreshActionPrice(details);
			RefreshOwner(details);

			CollectionNameText.text = nft.Name;

			string ethAddress = await DataManager.Instance.GetCreatorOfCollection(nft.TokenAddress);
			CustomUser creatorUser = await DataManager.Instance.GetUserWithCache(ethAddress);
			CreatorUsernameText.text = "@" + creatorUser.UserName;
			UniTask task1 = CreatorAvatarImage.Initialize(creatorUser);
			CreatorTrigger.Initialize(creatorUser.EthAddress);
			Title.text = nft.MetadataObject.Name;
			UniTask<bool> task2 = ImageUtils.DownloadAndApplyImageAndCrop(nft.MetadataObject.Image, MainImage, 512, 512);

			if (collectionDetails == null || string.IsNullOrEmpty(collectionDetails.LogoImageUrl))
			{
				CollectionLogoImage.gameObject.SetActive(false);
				await UniTask.WhenAll(task1, task2);
			}
			else
			{
				CollectionLogoImage.gameObject.SetActive(true);
				UniTask<bool> task3 = ImageUtils.DownloadAndApplyImageAndCrop(collectionDetails.LogoImageUrl, CollectionLogoImage, 32, 32);
				await UniTask.WhenAll(task1, task2, task3);
			}
		}

		#endregion
		#region PrivateMethods

		private async void RefreshOwner(NFTDetailsContractType details)
		{
			if (details.OwnerInLowercase == UserManager.Instance.CurrentUserAddress)
			{
				OwnerAvatarImage.gameObject.SetActive(false);
				OwnerUsernameText.text = string.Empty;
			}
			else
			{
				CustomUser ownerUser = await DataManager.Instance.GetUserWithCache(details.OwnerInLowercase);
				OwnerTrigger.Initialize(details.OwnerInLowercase);
				if (ownerUser != null)
				{
					OwnerUsernameText.text = "@" + ownerUser.UserName;
					OwnerAvatarImage.gameObject.SetActive(true);
					await OwnerAvatarImage.Initialize(ownerUser);
				}
				else
				{
					OwnerUsernameText.text = details.OwnerInLowercase.FormatEthAddress(6);
					await OwnerAvatarImage.Initialize(null);
				}
			}
		}

		private async void RefreshActionPrice(NFTDetailsContractType details)
		{
			if (details.BuyPriceInEther != 0)
			{
				ActionText.text = LocalizationKeys.PROFILE_BUY_NOW.Translate();
				PriceText.text = details.BuyPriceInEtherFixedPoint + " " + Moralis.CurrentChain.Symbol;
			}
			else
			{
				decimal lastSoldPrice = await DataManager.Instance.GetLastPriceForNFT(mNft.TokenAddress, mNft.TokenId, details.Owner);
				if (lastSoldPrice > 0)
				{
					ActionText.text = LocalizationKeys.LAST_SOLD_LABEL.Translate();
					PriceText.text = lastSoldPrice.ToString("F2") + " " + Moralis.CurrentChain.Symbol;
				}
				else
				{
					ActionText.text = string.Empty;
					PriceText.text = string.Empty;
				}
			}
		}

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
