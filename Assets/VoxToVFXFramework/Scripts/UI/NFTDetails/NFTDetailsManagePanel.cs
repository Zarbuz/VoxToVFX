using MoralisUnity.Web3Api.Models;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.NFTUpdate;

namespace VoxToVFXFramework.Scripts.UI.NFTDetails
{
	public class NFTDetailsManagePanel : MonoBehaviour
	{
		#region ScriptParameters

		[Header("BuyNow")]
		[SerializeField] private Button SetPriceButton;
		[SerializeField] private Button TransferNFTButton;
		[SerializeField] private Button BurnNFTButton;
		[SerializeField] private GameObject SetBuyNowPanel;
		[SerializeField] private GameObject ChangePricePanel;
		[SerializeField] private TextMeshProUGUI CurrentPriceText;
		[SerializeField] private Button ChangePriceButton;
		[SerializeField] private Button RemoveBuyNowButton;

		[Header("Auction")]
		[SerializeField] private Button ListButton;


		[Header("Manager")]
		[SerializeField] private Toggle ManageToggle;
		#endregion


		#region Fields

		private Nft mNft;
		
		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			SetPriceButton.onClick.AddListener(OnSetPriceClicked);
			ListButton.onClick.AddListener(OnSetListClicked);
			TransferNFTButton.onClick.AddListener(OnTransferNFTClicked);
			BurnNFTButton.onClick.AddListener(OnBurnNFTClicked);
			ChangePriceButton.onClick.AddListener(OnChangePriceClicked);
			RemoveBuyNowButton.onClick.AddListener(OnRemoveBuyNowClicked);
		}

		private void OnDisable()
		{
			SetPriceButton.onClick.RemoveListener(OnSetPriceClicked);
			ListButton.onClick.RemoveListener(OnSetListClicked);
			TransferNFTButton.onClick.RemoveListener(OnTransferNFTClicked);
			BurnNFTButton.onClick.RemoveListener(OnBurnNFTClicked);
			ChangePriceButton.onClick.RemoveListener(OnChangePriceClicked);
			RemoveBuyNowButton.onClick.RemoveListener(OnRemoveBuyNowClicked);

		}

		#endregion

		#region PublicMethods

		public async void Initialize(Nft nft, CustomUser creatorUser)
		{
			mNft = nft;
			BurnNFTButton.gameObject.SetActive(creatorUser.EthAddress == UserManager.Instance.CurrentUserAddress);
			NFTDetailsContractType details = await DataManager.Instance.GetNFTDetailsWithCache(nft.TokenAddress, nft.TokenId);
			ManageToggle.gameObject.SetActive(details is not { IsInEscrow: true });
			SetBuyNowPanel.SetActive(details is not { IsInEscrow: true });
			ChangePricePanel.SetActive(details is { IsInEscrow: true });
			CurrentPriceText.text = details != null && details.BuyPriceInEther != 0 ? details.BuyPriceInEtherFixedPoint + " ETH" : string.Empty;
		}

		#endregion

		#region PrivateMethods

		private void OnSetPriceClicked()
		{
			CanvasPlayerPCManager.Instance.OpenUpdateNftPanel(eNFTUpdateTargetType.SET_BUY_PRICE, mNft);
		}

		private void OnSetListClicked()
		{

		}

		private void OnTransferNFTClicked()
		{
			CanvasPlayerPCManager.Instance.OpenUpdateNftPanel(eNFTUpdateTargetType.TRANSFER_NFT, mNft);
		}

		private void OnBurnNFTClicked()
		{

		}

		private void OnChangePriceClicked()
		{
			CanvasPlayerPCManager.Instance.OpenUpdateNftPanel(eNFTUpdateTargetType.CHANGE_BUY_PRICE, mNft);
		}

		private void OnRemoveBuyNowClicked()
		{
			CanvasPlayerPCManager.Instance.OpenUpdateNftPanel(eNFTUpdateTargetType.REMOVE_BUY_PRICE, mNft);
		}

		#endregion
	}
}
