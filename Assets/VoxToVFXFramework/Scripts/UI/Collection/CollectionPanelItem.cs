using System;
using MoralisUnity.Web3Api.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Models;

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

		public void Initialize(CollectionCreatedEvent collectionCreated, NftOwnerCollection nftOwnerCollection, Action<CollectionCreatedEvent> onSelectedCallback)
		{
			NameText.text = collectionCreated.Name;
			if (nftOwnerCollection.Total != null)
			{
				CounterText.text = nftOwnerCollection.Total.Value + " NFTs";
			}
			else
			{
				CounterText.gameObject.SetActive(false);
			}
			PlusImage.gameObject.SetActive(false);
			SelectButton.onClick.AddListener(() =>
			{
				onSelectedCallback?.Invoke(collectionCreated);
			});
		}

		#endregion
	}
}
