using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MoralisUnity.Web3Api.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.Utils.Extensions;

namespace VoxToVFXFramework.Scripts.UI.Profile
{
	public class ProfileListingPanel : MonoBehaviour
	{
		#region Enum

		private enum eProfileListingState
		{
			CREATED,
			COLLECTION,
			OWNED
		}

		#endregion

		#region ScriptParameters

		[Header("Tabs")]
		[SerializeField] private Button CreatedButton;
		[SerializeField] private Button CollectionButton;
		[SerializeField] private Button OwnedButton;

		[SerializeField] private TextMeshProUGUI CreatedCountText;
		[SerializeField] private TextMeshProUGUI CollectionCountText;
		[SerializeField] private TextMeshProUGUI OwnedCountText;

		[Header("Panels")]
		[SerializeField] private GameObject CreatedPanel;
		[SerializeField] private GameObject CollectionPanel;
		[SerializeField] private GameObject OwnedPanel;

		[SerializeField] private Transform CreatedGridTransform;
		[SerializeField] private Transform CollectionGridTransform;
		[SerializeField] private Transform OwnedGridTransform;

		[Header("ProfileListNFTItem")]
		[SerializeField] private ProfileListNFTItem ProfileListNftItemPrefab;
		[SerializeField] private ProfileCollectionItem ProfileCollectionItemPrefab;

		[SerializeField] private Image LoadingSpinner;

		#endregion

		#region Fields

		private eProfileListingState mEProfileListingState;

		private eProfileListingState ProfileListingState
		{
			get => mEProfileListingState;
			set
			{
				mEProfileListingState = value;
				CreatedPanel.SetActive(mEProfileListingState == eProfileListingState.CREATED);
				CollectionPanel.SetActive(mEProfileListingState == eProfileListingState.COLLECTION);
				OwnedPanel.SetActive(mEProfileListingState == eProfileListingState.OWNED);

				CreatedButton.transform.GetChild(0).gameObject.SetActive(mEProfileListingState == eProfileListingState.CREATED);
				CollectionButton.transform.GetChild(0).gameObject.SetActive(mEProfileListingState == eProfileListingState.COLLECTION);
				OwnedButton.transform.GetChild(0).gameObject.SetActive(mEProfileListingState == eProfileListingState.OWNED);
			}
		}

		private readonly List<ProfileListNFTItem> mItems = new List<ProfileListNFTItem>();
		private CustomUser mCustomUser;
		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			CreatedButton.onClick.AddListener(() => OnSwitchTabClicked(eProfileListingState.CREATED));
			CollectionButton.onClick.AddListener(() => OnSwitchTabClicked(eProfileListingState.COLLECTION));
			OwnedButton.onClick.AddListener(() => OnSwitchTabClicked(eProfileListingState.OWNED));

			ProfileListingState = eProfileListingState.CREATED;
		}


		private void OnDisable()
		{
			CreatedButton.onClick.RemoveAllListeners();
			CollectionButton.onClick.RemoveAllListeners();
			OwnedButton.onClick.RemoveAllListeners();
		}

		#endregion

		#region PublicMethods

		public async void Initialize(CustomUser user)
		{
			mCustomUser = user;
			ShowSpinnerImage(true);
			UniTask task1 = RefreshCreatedTab();
			UniTask task2 = RefreshCollectionTab();

			await (task1, task2);
			ShowSpinnerImage(false);
		}

		#endregion

		#region PrivateMethods

		private void OnSwitchTabClicked(eProfileListingState profileListingState)
		{
			ProfileListingState = profileListingState;
		}

		private void ShowSpinnerImage(bool showSpinner)
		{
			LoadingSpinner.gameObject.SetActive(showSpinner);
			CollectionPanel.SetActive(!showSpinner && ProfileListingState == eProfileListingState.COLLECTION);
			CreatedPanel.SetActive(!showSpinner && ProfileListingState == eProfileListingState.CREATED);
			OwnedPanel.SetActive(!showSpinner && ProfileListingState == eProfileListingState.OWNED);

			CreatedButton.interactable = !showSpinner;
			CollectionButton.interactable = !showSpinner;
			OwnedButton.interactable = !showSpinner;
		}

		private async UniTask RefreshCreatedTab()
		{
			CreatedGridTransform.DestroyAllChildren();

			mItems.Clear();
			List<UniTask> tasks = new List<UniTask>();
			List<CollectionCreatedEvent> list = await DataManager.Instance.GetUserListContractWithCache(mCustomUser.EthAddress);
			foreach (CollectionCreatedEvent collection in list.OrderByDescending(c => c.createdAt))
			{
				var nftCollection = await DataManager.Instance.GetNftCollectionWithCache(collection.CollectionContract);

				//List<CollectionMintedEvent> listNfTsForContract = await DataManager.Instance.GetNFTForContractWithCache(mCustomUser.EthAddress, collection.CollectionContract);
				foreach (Nft nft in nftCollection.NftCollection.Result.Where(t => !string.IsNullOrEmpty(t.Metadata)))
				{
					ProfileListNFTItem item = Instantiate(ProfileListNftItemPrefab, CreatedGridTransform, false);
					NftOwner nftOwner = nftCollection.NftOwnerCollection.Result.FirstOrDefault(t => t.TokenId == nft.TokenId);
					tasks.Add(item.Initialize(nft,nftOwner, mCustomUser));
					mItems.Add(item);
				}
			}

			await UniTask.WhenAll(tasks);
			CreatedCountText.text = mItems.Count(i => i.InitSuccess).ToString();
		}

		private async UniTask RefreshCollectionTab()
		{
			CollectionGridTransform.DestroyAllChildren();
			List<CollectionCreatedEvent> list = await DataManager.Instance.GetUserListContractWithCache(mCustomUser.EthAddress);

			List<UniTask> tasks	= new List<UniTask>();
			foreach (CollectionCreatedEvent collection in list.OrderByDescending(c => c.createdAt))
			{
				ProfileCollectionItem item = Instantiate(ProfileCollectionItemPrefab, CollectionGridTransform, false);
				tasks.Add(item.Initialize(collection));
			}
			await UniTask.WhenAll(tasks);
			CollectionCountText.text = list.Count.ToString();
		}

		#endregion
	}
}
