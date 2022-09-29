using System;
using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.Utils.Image;

namespace VoxToVFXFramework.Scripts.UI.Collection
{
	public class CollectionPanelItem : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI NameText;
		[SerializeField] private TextMeshProUGUI CounterText;
		[SerializeField] private Button SelectButton;
		[SerializeField] private Image CollectionImage;
		[SerializeField] private Image PlusImage;

		#endregion

		#region PublicMethods

		public async UniTask Initialize(CollectionCreatedEvent collectionCreated, int count, Action<CollectionCreatedEvent> onSelectedCallback)
		{
			Models.CollectionDetails details = await DataManager.Instance.GetCollectionDetailsWithCache(collectionCreated.CollectionContract);
			CollectionImage.gameObject.SetActive(details != null && !string.IsNullOrEmpty(details.LogoImageUrl));
			if (details != null)
			{
				await ImageUtils.DownloadAndApplyImageAndCrop(details.LogoImageUrl, CollectionImage, 60, 60);
			}
			NameText.text = collectionCreated.Name;
			CounterText.text = count + " NFTs";
			PlusImage.gameObject.SetActive(false);
			SelectButton.onClick.AddListener(() =>
			{
				onSelectedCallback?.Invoke(collectionCreated);
			});
		}

		#endregion
	}
}
