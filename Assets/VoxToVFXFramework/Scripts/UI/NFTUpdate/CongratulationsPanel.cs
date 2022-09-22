using MoralisUnity.Web3Api.Models;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models.ContractEvent;

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public class CongratulationsPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private TextMeshProUGUI CongratulationsTitle;
		[SerializeField] private TextMeshProUGUI CongratulationsDescription;
		[SerializeField] private Button ViewNFTButton;
		[SerializeField] private Button ViewCollectionButton;

		#endregion

		#region Fields

		private NFTUpdatePanel mNftUpdatePanel;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			ViewNFTButton.onClick.AddListener(OnViewNFTClicked);
			ViewCollectionButton.onClick.AddListener(OnViewCollectionClicked);
		}

		private void OnDisable()
		{
			ViewNFTButton.onClick.RemoveListener(OnViewNFTClicked);
			ViewCollectionButton.onClick.RemoveListener(OnViewCollectionClicked);
		}

		#endregion

		#region PublicMethods

		public void Initialize(NFTUpdatePanel updatePanel, string title, string description, bool viewNFTButton)
		{
			mNftUpdatePanel = updatePanel;
			CongratulationsTitle.text = title;
			CongratulationsDescription.text = description;
			ViewNFTButton.gameObject.SetActive(viewNFTButton);
		}

		#endregion
		#region PrivateMethods

		private void OnViewNFTClicked()
		{
			CanvasPlayerPCManager.Instance.OpenNftDetailsPanel(mNftUpdatePanel.Nft);
		}

		private async void OnViewCollectionClicked()
		{
			CollectionCreatedEvent collection = await DataManager.Instance.GetCollectionCreatedEventWithCache(mNftUpdatePanel.Nft.TokenAddress);
			CanvasPlayerPCManager.Instance.OpenCollectionDetailsPanel(collection);
		}

		#endregion
	}
}
