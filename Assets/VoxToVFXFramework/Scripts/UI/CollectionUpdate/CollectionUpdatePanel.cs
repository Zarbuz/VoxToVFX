using MoralisUnity.Web3Api.Models;
using System;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Popups;
using VoxToVFXFramework.Scripts.UI.Profile;
using VoxToVFXFramework.Scripts.Utils.Extensions;

namespace VoxToVFXFramework.Scripts.UI.CollectionUpdate
{
	public enum eCollectionUpdateTargetType
	{
		BURN
	}

	public class CollectionUpdatePanel : MonoBehaviour
	{
		#region ScriptParameters

		[Header("Panels")]
		[SerializeField] private GameObject SelfDestructPanel;
		[SerializeField] private GameObject CollectionDestroyedPanel;

		[SerializeField] private ProfileCollectionItem CollectionItem;

		[Header("SelfDestruct")]
		[SerializeField] private Button SelfDestructButton;

		[Header("CollectionDestroyed")]
		[SerializeField] private Button OpenProfileButton;

		#endregion

		#region Enum

		private enum eCollectionUpdateState
		{
			BURN,
			COLLECTION_DESTROYED
		}

		#endregion

		#region Fields

		private eCollectionUpdateState mPanelState;
		private CollectionCreatedEvent mCollection;
		private eCollectionUpdateState CollectionUpdateState
		{
			get => mPanelState;
			set
			{
				mPanelState = value;
				SelfDestructPanel.SetActiveSafe(mPanelState == eCollectionUpdateState.BURN);
				CollectionDestroyedPanel.SetActiveSafe(mPanelState == eCollectionUpdateState.COLLECTION_DESTROYED);
			}
		}

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			SelfDestructButton.onClick.AddListener(OnSelfDestructClicked);
			OpenProfileButton.onClick.AddListener(OnOpenProfileClicked);
		}

		private void OnDisable()
		{
			SelfDestructButton.onClick.RemoveListener(OnSelfDestructClicked);
			OpenProfileButton.onClick.RemoveListener(OnOpenProfileClicked);

		}

		#endregion

		#region PublicMethods

		public async void Initialize(eCollectionUpdateTargetType collectionUpdateTargetType, CollectionCreatedEvent collection)
		{
			mCollection = collection;
			switch (collectionUpdateTargetType)
			{
				case eCollectionUpdateTargetType.BURN:
					CollectionUpdateState = eCollectionUpdateState.BURN;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(collectionUpdateTargetType), collectionUpdateTargetType, null);
			}

			await CollectionItem.Initialize(collection);
		}

		#endregion
		
		#region PrivateMethods

		private void OnSelfDestructClicked()
		{
			MessagePopup.ShowConfirmationWalletPopup(NFTManager.Instance.SelfDestruct(mCollection.CollectionContract),
				(transactionId) =>
				{
					MessagePopup.ShowConfirmationBlockchainPopup(
						LocalizationKeys.BURN_COLLECTION_WAITING_TITLE.Translate(),
						LocalizationKeys.BURN_COLLECTION_WAITING_DESCRIPTION.Translate(),
						transactionId,
						OnCollectionDestroyed);
				});
		}

		private void OnCollectionDestroyed(AbstractContractEvent obj)
		{
			CollectionUpdateState = eCollectionUpdateState.COLLECTION_DESTROYED;
		}

		private void OnOpenProfileClicked()
		{
			CanvasPlayerPCManager.Instance.OpenProfilePanel(UserManager.Instance.CurrentUser);
		}
		#endregion
	}
}
