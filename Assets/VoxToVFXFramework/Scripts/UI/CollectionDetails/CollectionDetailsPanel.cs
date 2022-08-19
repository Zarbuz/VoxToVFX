using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.UI.Popups;

namespace VoxToVFXFramework.Scripts.UI.CollectionDetails
{
	public class CollectionDetailsPanel : MonoBehaviour
	{
		#region Enum

		private enum eCollectionDetailsState
		{
			NFT,
			ACTIVITY
		}

		#endregion

		#region ScriptParameters

		[SerializeField] private Image MainImage;
		[SerializeField] private TextMeshProUGUI CollectionNameText;
		[SerializeField] private OpenUserProfileButton OpenUserProfileButton;
		[SerializeField] private Button EditCollectionButton;
		[SerializeField] private TextMeshProUGUI CollectionOfCountText;
		[SerializeField] private TextMeshProUGUI OwnedByCountText;
		[SerializeField] private TextMeshProUGUI FloorPriceText;
		[SerializeField] private TextMeshProUGUI TotalSalesText;
		[SerializeField] private Button NFTTabButton;
		[SerializeField] private Button ActivityTabButton;
		[SerializeField] private GameObject NoItemFoundPanel;
		[SerializeField] private Button MintNftButton;

		[SerializeField] private Button OpenSymbolButton;
		[SerializeField] private TextMeshProUGUI CollectionSymbolText; 

		[SerializeField] private GameObject NFTPanel;
		[SerializeField] private GameObject ActivityPanel;

		#endregion

		#region Fields

		private CollectionCreatedEvent mCollectionCreated;
		private eCollectionDetailsState mCollectionDetailsState;

		private eCollectionDetailsState CollectionDetailsState
		{
			get => mCollectionDetailsState;
			set
			{
				mCollectionDetailsState = value;
				NFTPanel.gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.NFT);
				ActivityPanel.gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.ACTIVITY);
				NFTTabButton.transform.GetChild(0).gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.NFT);
				ActivityTabButton.transform.GetChild(0).gameObject.SetActive(mCollectionDetailsState == eCollectionDetailsState.ACTIVITY);
			}
		}

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			OpenSymbolButton.onClick.AddListener(OnSymbolClicked);
			NFTTabButton.onClick.AddListener(() => OnSwitchTabClicked(eCollectionDetailsState.NFT));
			ActivityTabButton.onClick.AddListener(() => OnSwitchTabClicked(eCollectionDetailsState.ACTIVITY));
			EditCollectionButton.onClick.AddListener(OnEditCollectionClicked);
		}
		

		private void OnDisable()
		{
			OpenSymbolButton.onClick.RemoveListener(OnSymbolClicked);
			NFTTabButton.onClick.RemoveAllListeners();
			ActivityTabButton.onClick.RemoveAllListeners();
			EditCollectionButton.onClick.RemoveListener(OnEditCollectionClicked);
		}

		#endregion

		#region PublicMethods

		public async UniTask Initialize(CollectionCreatedEvent collection)
		{
			mCollectionCreated = collection;
			CustomUser creatorUser = await UserManager.Instance.LoadUserFromEthAddress(collection.Creator);
			MintNftButton.gameObject.SetActive(collection.Creator == UserManager.Instance.CurrentUser.EthAddress);
			EditCollectionButton.gameObject.SetActive(collection.Creator == UserManager.Instance.CurrentUser.EthAddress);
			CollectionNameText.text = collection.Name;
			CollectionSymbolText.text = collection.Symbol;
			CollectionDetailsState = eCollectionDetailsState.NFT;
			OpenUserProfileButton.Initialize(creatorUser);

		}

		#endregion

		#region PrivateMethods

		private void OnSymbolClicked()
		{
			string url = ConfigManager.Instance.EtherScanBaseUrl + "address/" + mCollectionCreated.CollectionContract;
			Application.OpenURL(url);
		}

		private void OnSwitchTabClicked(eCollectionDetailsState collectionDetailsState)
		{
			CollectionDetailsState = collectionDetailsState;
		}

		private void OnEditCollectionClicked()
		{
			MessagePopup.ShowEditCollectionPopup();
		}

		#endregion
	}
}
