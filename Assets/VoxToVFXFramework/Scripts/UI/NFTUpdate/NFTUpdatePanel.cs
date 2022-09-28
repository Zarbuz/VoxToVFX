using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using Nethereum.Util;
using System;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Popups;
using VoxToVFXFramework.Scripts.UI.Profile;

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public enum eNFTUpdateTargetType
	{
		SET_BUY_PRICE,
		CHANGE_BUY_PRICE,
		REMOVE_BUY_PRICE,
		TRANSFER_NFT,
		BURN_NFT,
		BUY_NOW,
		MAKE_OFFER,
		CONGRATULATIONS
	}

	public class NFTUpdatePanel : MonoBehaviour
	{
		#region ScriptParameters

		[Header("Panels")]
		[SerializeField] private SetBuyPricePanel SetBuyNowPanel;
		[SerializeField] private CongratulationsPanel CongratulationsPanel;
		[SerializeField] private RemoveBuyPricePanel RemoveBuyPricePanel;
		[SerializeField] private TransferPanel TransferPanel;
		[SerializeField] private BurnPanel BurnPanel;
		[SerializeField] private BuyNowPanel BuyNowPanel;
		[SerializeField] private MakeOfferPanel MakeOfferPanel;
		[SerializeField] private ProfileListNFTItem ProfileListNftItem;

		#endregion

		#region Enum

		
		#endregion

		#region Fields

		public NftOwner Nft { get; private set; }

		private eNFTUpdateTargetType mPanelState;
		public eNFTUpdateTargetType NftUpdatePanelState
		{
			get => mPanelState;
			set
			{
				mPanelState = value;
				SetBuyNowPanel.gameObject.SetActive(mPanelState == eNFTUpdateTargetType.SET_BUY_PRICE);
				CongratulationsPanel.gameObject.SetActive(mPanelState == eNFTUpdateTargetType.CONGRATULATIONS);
				RemoveBuyPricePanel.gameObject.SetActive(mPanelState == eNFTUpdateTargetType.REMOVE_BUY_PRICE);
				TransferPanel.gameObject.SetActive(mPanelState == eNFTUpdateTargetType.TRANSFER_NFT);
				BurnPanel.gameObject.SetActive(mPanelState == eNFTUpdateTargetType.BURN_NFT);
				BuyNowPanel.gameObject.SetActive(mPanelState == eNFTUpdateTargetType.BUY_NOW);
				MakeOfferPanel.gameObject.SetActive(mPanelState == eNFTUpdateTargetType.MAKE_OFFER);
				ProfileListNftItem.transform.parent.gameObject.SetActive(mPanelState != eNFTUpdateTargetType.CONGRATULATIONS);
			}
		}

		#endregion

		#region PublicMethods

		public async void Initialize(eNFTUpdateTargetType nftUpdateTargetType, NftOwner nft)
		{
			Nft = nft;
			NftUpdatePanelState = nftUpdateTargetType;
			RemoveBuyPricePanel.Initialize(this);
			TransferPanel.Initialize(this);
			SetBuyNowPanel.Initialize(this);
			BuyNowPanel.Initialize(this);
			MakeOfferPanel.Initialize(this);
			ProfileListNftItem.IsReadyOnly = true;
			await ProfileListNftItem.Initialize(nft);
		}

		public void SetCongratulations(string title, string description, bool viewNFTButton = true, bool viewCollectionButton = true)
		{
			NftUpdatePanelState = eNFTUpdateTargetType.CONGRATULATIONS;
			CongratulationsPanel.Initialize(this, title, description, viewNFTButton, viewCollectionButton);
		}

		#endregion
	}
}
