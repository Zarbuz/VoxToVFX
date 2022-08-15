using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Collection;
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
			}
		}

		private CustomUser mCustomUser;
		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			CreatedButton.onClick.AddListener(() => OnSwitchTabClicked(eProfileListingState.CREATED));
			CollectionButton.onClick.AddListener(() => OnSwitchTabClicked(eProfileListingState.COLLECTION));
			OwnedButton.onClick.AddListener(() => OnSwitchTabClicked(eProfileListingState.OWNED));

			OnSwitchTabClicked(eProfileListingState.CREATED);
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
			await RefreshCreatedTab();
			ShowSpinnerImage(false);
		}

		#endregion

		#region PrivateMethods

		private void OnSwitchTabClicked(eProfileListingState profileListingState)
		{
			CreatedButton.transform.GetChild(0).gameObject.SetActive(profileListingState == eProfileListingState.CREATED);
			CollectionButton.transform.GetChild(0).gameObject.SetActive(profileListingState == eProfileListingState.COLLECTION);
			OwnedButton.transform.GetChild(0).gameObject.SetActive(profileListingState == eProfileListingState.OWNED);

			ProfileListingState = profileListingState;
		}

		private void ShowSpinnerImage(bool showSpinner)
		{
			LoadingSpinner.gameObject.SetActive(showSpinner);
			CollectionPanel.SetActive(!showSpinner && ProfileListingState == eProfileListingState.COLLECTION);
			CreatedPanel.SetActive(!showSpinner && ProfileListingState == eProfileListingState.CREATED);
			OwnedPanel.SetActive(!showSpinner && ProfileListingState == eProfileListingState.OWNED);
		}

		private async UniTask RefreshCreatedTab()
		{
			CreatedGridTransform.DestroyAllChildren();
			List<CollectionCreatedEvent> list = await CollectionFactoryManager.Instance.GetUserListContract(mCustomUser);
			foreach (CollectionCreatedEvent collection in list.OrderByDescending(c => c.createdAt))
			{
				List<CollectionMintedEvent> listNfTsForContract = await NFTManager.Instance.FetchNFTsForContract(mCustomUser.EthAddress, collection.CollectionContract);
				foreach (CollectionMintedEvent nft in listNfTsForContract.OrderBy(t => t.createdAt))
				{
					ProfileListNFTItem item = Instantiate(ProfileListNftItemPrefab, CreatedGridTransform, false);
					bool initSuccess = await item.Initialize(nft, mCustomUser);
					item.gameObject.SetActive(initSuccess);
				}
			}

			CreatedCountText.text = CreatedGridTransform.CountActiveChild().ToString();
		}

		#endregion
	}
}
