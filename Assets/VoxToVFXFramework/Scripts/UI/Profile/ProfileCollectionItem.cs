using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public class ProfileCollectionItem : AbstractCardItem
	{
		#region ScriptParameters

		[SerializeField] private RawImage CollectionCoverImage;
		[SerializeField] private RawImage CollectionLogoImage;
		[SerializeField] private TextMeshProUGUI CollectionNameText;
		[SerializeField] private OpenUserProfileButton OpenUserProfileButton;
		[SerializeField] private TextMeshProUGUI CollectionSymbolText;
		[SerializeField] private Button Button;

		#endregion

		#region PublicMethods

		public async UniTask Initialize(CollectionCreatedEvent collection)
		{
			Button.onClick.AddListener(() => CanvasPlayerPCManager.Instance.OpenCollectionDetailsPanel(collection));
			TransparentButton[] transparentButtons = GetComponentsInChildren<TransparentButton>();

			Models.CollectionDetails collectionDetails = await DataManager.Instance.GetCollectionDetailsWithCache(collection.CollectionContract);

			foreach (TransparentButton transparentButton in transparentButtons)
			{
				transparentButton.ImageBackgroundActive = collectionDetails != null && !string.IsNullOrEmpty(collectionDetails.CoverImageUrl);
			}

			CollectionLogoImage.transform.parent.gameObject.SetActive(collectionDetails != null && !string.IsNullOrEmpty(collectionDetails.LogoImageUrl));
			CollectionCoverImage.gameObject.SetActive(collectionDetails != null && !string.IsNullOrEmpty(collectionDetails.CoverImageUrl));
			if (collectionDetails != null)
			{
				await ImageUtils.DownloadAndApplyImageAndCrop(collectionDetails.CoverImageUrl, CollectionCoverImage, 398, 524);
				await ImageUtils.DownloadAndApplyImageAndCrop(collectionDetails.LogoImageUrl, CollectionLogoImage, 100, 100);
			}
			CollectionNameText.color = collectionDetails != null && !string.IsNullOrEmpty(collectionDetails.CoverImageUrl) ? Color.white : Color.black;
			CollectionNameText.text = collection.Name;
			CollectionSymbolText.text = collection.Symbol;
			OpenUserProfileButton.Initialize(collection.Creator);
		}

		#endregion

	}
}
