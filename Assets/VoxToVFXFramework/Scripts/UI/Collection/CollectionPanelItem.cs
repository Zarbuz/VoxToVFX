using System;
using MoralisUnity.Web3Api.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;

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

		public void Initialize(CollectionCreatedEvent collectionCreated, int count, Action<CollectionCreatedEvent> onSelectedCallback)
		{
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
