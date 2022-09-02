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

		#endregion

		#region Fields

		public bool InitSuccess { get; private set; }
		public bool IsReadyOnly { get; set; }

		private Nft mNft;
		private CustomUser mCreatorUser;

		#endregion

		#region PublicMethods

		public async UniTask Initialize(Nft nft, NftOwner owner, CustomUser creatorUser)
		{
			mNft = nft;
			mCreatorUser = creatorUser;
			Button.onClick.AddListener(OnItemClicked);
			Models.CollectionDetails collectionDetails = await DataManager.Instance.GetCollectionDetailsWithCache(nft.TokenAddress);

			NFTDetailsContractType details = await DataManager.Instance.GetNFTDetailsWithCache(nft.TokenAddress, nft.TokenId);

			if (owner != null)
			{
				if (owner.OwnerOf == UserManager.Instance.CurrentUserAddress)
				{
					OwnerAvatarImage.gameObject.SetActive(false);
					OwnerUsernameText.text = string.Empty;
				}
				else
				{
					CustomUser ownerUser = await DataManager.Instance.GetUserWithCache(owner.OwnerOf);
					if (ownerUser != null)
					{
						OwnerUsernameText.text = "@" + ownerUser.UserName;
						OwnerAvatarImage.gameObject.SetActive(true);
						await OwnerAvatarImage.Initialize(ownerUser);
					}
					else
					{
						OwnerUsernameText.text = owner.OwnerOf.FormatEthAddress(6);
						OwnerAvatarImage.gameObject.SetActive(false);
					}
				}
			}
			else
			{
				OwnerAvatarImage.gameObject.SetActive(false);
				OwnerUsernameText.text = string.Empty;
			}

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

			CollectionNameText.text = nft.Name;

			CreatorUsernameText.text = "@" + creatorUser.UserName;
			UniTask task1 = CreatorAvatarImage.Initialize(creatorUser);

			MetadataObject metadataObject = JsonConvert.DeserializeObject<MetadataObject>(nft.Metadata);
			Title.text = metadataObject.Name;
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
				CanvasPlayerPCManager.Instance.OpenNftDetailsPanel(mNft, mCreatorUser);
			}
		}

		#endregion
	}
}
