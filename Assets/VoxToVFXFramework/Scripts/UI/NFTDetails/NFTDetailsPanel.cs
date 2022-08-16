using System;
using MoralisUnity.Web3Api.Models;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.NFTDetails
{
	public class NFTDetailsPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private VerticalLayoutGroup VerticalLayoutGroup;
		[SerializeField] private Image MainImage;
		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private TextMeshProUGUI DescriptionLabel;
		[SerializeField] private TextMeshProUGUI Description;
		[SerializeField] private Button OpenTransactionButton;
		[SerializeField] private TextMeshProUGUI MintedDateText;
		[SerializeField] private TextMeshProUGUI CreatorUsername;
		[SerializeField] private AvatarImage CreatorImage;
		[SerializeField] private TextMeshProUGUI CollectionNameText;
		[SerializeField] private AvatarImage CollectionImage;
		[SerializeField] private Button ViewEtherscanButton;
		[SerializeField] private Button ViewMetadataButton;
		[SerializeField] private Button ViewIpfsButton;

		#endregion

		#region Fields

		private CollectionMintedEvent mCollectionMinted;
		private Nft mNft;
		private MetadataObject mMetadataObject;
		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			OpenTransactionButton.onClick.AddListener(OnOpenTransactionClicked);
			ViewEtherscanButton.onClick.AddListener(OnViewEtherscanClicked);
			ViewMetadataButton.onClick.AddListener(OnViewMetadataClicked);
			ViewIpfsButton.onClick.AddListener(OnViewIpfsClicked);
		}

		private void OnDisable()
		{
			OpenTransactionButton.onClick.RemoveListener(OnOpenTransactionClicked);
			ViewEtherscanButton.onClick.RemoveListener(OnViewEtherscanClicked);
			ViewMetadataButton.onClick.RemoveListener(OnViewMetadataClicked);
			ViewIpfsButton.onClick.RemoveListener(OnViewIpfsClicked);
		}



		#endregion

		#region PublicMethods

		public async void Initialize(CollectionMintedEvent collectionMinted, Nft metadata)
		{
			mCollectionMinted = collectionMinted;
			mNft = metadata;
			CustomUser creatorUser = await UserManager.Instance.LoadUserFromEthAddress(collectionMinted.Creator);
			mMetadataObject = JsonConvert.DeserializeObject<MetadataObject>(metadata.Metadata);
			Title.text = mMetadataObject.Name;
			DescriptionLabel.gameObject.SetActive(!string.IsNullOrEmpty(mMetadataObject.Description));
			Description.gameObject.SetActive(!string.IsNullOrEmpty(mMetadataObject.Description));
			Description.text = mMetadataObject.Description;
			CollectionNameText.text = metadata.Name;
			CreatorUsername.text = creatorUser.UserName;
			await CreatorImage.Initialize(creatorUser);
			if (collectionMinted.createdAt != null)
			{
				MintedDateText.text = string.Format(LocalizationKeys.MINTED_ON_DATE.Translate(), collectionMinted.createdAt.Value.ToShortDateString());
			}
			await ImageUtils.DownloadAndApplyImage(mMetadataObject.Image, MainImage, 512);
			LayoutRebuilder.ForceRebuildLayoutImmediate(VerticalLayoutGroup.GetComponent<RectTransform>());
		}

		#endregion

		#region PrivateMethods

		private void OnViewEtherscanClicked()
		{
			string url = ConfigManager.Instance.EtherScanBaseUrl + "nft/" + mCollectionMinted.Address + "/" + mCollectionMinted.TokenID;
			Application.OpenURL(url);
		}

		private void OnViewMetadataClicked()
		{
			Application.OpenURL(mNft.TokenUri);
		}

		private void OnViewIpfsClicked()
		{
			Application.OpenURL(mMetadataObject.Image);
		}

		private void OnOpenTransactionClicked()
		{
			string url = ConfigManager.Instance.EtherScanBaseUrl + "tx/" + mCollectionMinted.TransactionHash;
			Application.OpenURL(url);
		}

		#endregion
	}
}
