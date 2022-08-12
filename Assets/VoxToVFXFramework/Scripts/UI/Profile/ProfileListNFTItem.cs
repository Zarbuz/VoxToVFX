using System;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public class ProfileListNFTItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		#region ScriptParameters

		[SerializeField] private Image MainImage;
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

		#endregion

		#region PublicMethods

		public async UniTask Initialize(Nft nft)
		{
			try
			{
				CollectionNameText.text = nft.Name;
				//await Moralis.Web3Api.Token.ReSyncMetadata(address:nft.TokenAddress, tokenId:nft.TokenId, ConfigManager.Instance.ChainList);
				Nft tokenIdMetadata = await Moralis.Web3Api.Token.GetTokenIdMetadata(address: nft.TokenAddress, tokenId: nft.TokenId, ConfigManager.Instance.ChainList);
				Debug.Log(tokenIdMetadata.TokenUri);
				Debug.Log(tokenIdMetadata.Metadata);
				if (tokenIdMetadata.Metadata != null)
				{
					MetadataObject metadataObject = JsonConvert.DeserializeObject<MetadataObject>(tokenIdMetadata.Metadata);
					Title.text = metadataObject.Name;
					await ImageUtils.DownloadAndApplyImage(metadataObject.Image, MainImage, 512, true, true, true);
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
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
	}
}
