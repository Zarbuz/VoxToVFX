using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public class ProfileListNFTItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		#region ScriptParameters

		[SerializeField] private Image MainImage;
		[SerializeField] private Button Button;
		[SerializeField] private AvatarImage CreatorAvatarImage;
		[SerializeField] private TextMeshProUGUI CreatorUsernameText;
		[SerializeField] private TextMeshProUGUI ActionText;
		[SerializeField] private TextMeshProUGUI PriceText;
		[SerializeField] private AvatarImage BuyerAvatarImage;
		[SerializeField] private TextMeshProUGUI BuyerUsernameText;

		[SerializeField] private CanvasGroup CanvasGroup;
		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private TextMeshProUGUI CollectionNameText;

		#endregion

		#region Fields

		private Coroutine mCoroutineAlphaEnter;
		private Coroutine mCoroutineAlphaExit;
		private CollectionMintedEvent mCollectionMintedEvent;
		[CanBeNull] private Nft mMetadata;

		#endregion

		#region PublicMethods

		public async UniTask<bool> Initialize(CollectionMintedEvent nft, CustomUser creatorUser)
		{
			mCollectionMintedEvent = nft;
			try
			{
				Button.onClick.AddListener(OnItemClicked);
				Nft tokenIdMetadata = await Moralis.Web3Api.Token.GetTokenIdMetadata(address: nft.Address, tokenId: nft.TokenID, ConfigManager.Instance.ChainList);
				mMetadata = tokenIdMetadata;
				CollectionNameText.text = tokenIdMetadata.Name;
				CreatorUsernameText.text = "@" + creatorUser.UserName;

				await CreatorAvatarImage.Initialize(creatorUser);
				if (tokenIdMetadata.Metadata != null)
				{
					Debug.Log(tokenIdMetadata.Metadata);

					MetadataObject metadataObject = JsonConvert.DeserializeObject<MetadataObject>(tokenIdMetadata.Metadata);
					Title.text = metadataObject.Name;
					await ImageUtils.DownloadAndApplyImage(metadataObject.Image, MainImage, 512, true, true, true);
				}
				else
				{
					return false;
				}

				return true;
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				return false;
			}
		}

		#endregion

		#region UnityMethods

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (mCoroutineAlphaExit != null)
			{
				StopCoroutine(mCoroutineAlphaExit);
				mCoroutineAlphaExit = null;
			}

			mCoroutineAlphaEnter = StartCoroutine(CanvasGroup.AlphaFade(1, 0.05f));
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (mCoroutineAlphaEnter != null)
			{
				StopCoroutine(mCoroutineAlphaEnter);
				mCoroutineAlphaEnter = null;
			}
			mCoroutineAlphaExit = StartCoroutine(CanvasGroup.AlphaFade(0, 0.05f));
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
