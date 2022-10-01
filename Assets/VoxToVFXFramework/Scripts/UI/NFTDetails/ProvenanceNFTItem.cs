using System;
using System.Numerics;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using Nethereum.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.ContractTypes;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Managers.DataManager;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using WalletConnectSharp.Core.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

public class ProvenanceNFTItem : MonoBehaviour
{
	#region ScriptParameters

	[SerializeField] private TextMeshProUGUI ActionText;
	[SerializeField] private TextMeshProUGUI DateText;
	[SerializeField] private TextMeshProUGUI PriceText;
	[SerializeField] private AvatarImage FromAvatarImage;
	[SerializeField] private AvatarImage ToAvatarImage;
	[SerializeField] private Button OpenTransactionButton;

	#endregion

	#region Fields

	private AbstractContractEvent mAbstractContractEvent;

	#endregion

	#region PublicMethods

	public async void Initialize(AbstractContractEvent contractEvent)
	{
		OpenTransactionButton.onClick.AddListener(OnOpenTransactionClicked);
		mAbstractContractEvent = contractEvent;
		DateText.text = contractEvent.createdAt.HasValue ? contractEvent.createdAt.Value.ToString("F") : string.Empty;

		switch (contractEvent)
		{
			case CollectionMintedEvent collectionMintedEvent:
				{
					CustomUser user = await DataManager.Instance.GetUserWithCache(collectionMintedEvent.Creator);
					await FromAvatarImage.Initialize(user);
					FormatActionText(LocalizationKeys.MINTED_BY_LABEL.Translate(), user, collectionMintedEvent.Creator);
					PriceText.text = string.Empty;
					break;
				}
			case BuyPriceSetEvent buyPriceSetEvent:
				{
					CustomUser user = await DataManager.Instance.GetUserWithCache(buyPriceSetEvent.Seller);
					await FromAvatarImage.Initialize(user);
					FormatActionText(LocalizationKeys.BUY_NOW_SET_LABEL.Translate(), user, buyPriceSetEvent.Seller);
					BigInteger priceInWei = BigInteger.Parse(buyPriceSetEvent.Price);
					decimal price = UnitConversion.Convert.FromWei(priceInWei);
					PriceText.text = price.ToString("F2") + " " + Moralis.CurrentChain.Symbol;
					break;
				}
			case BuyPriceCanceledEvent buyPriceCanceledEvent:
				{
					FromAvatarImage.gameObject.SetActive(false);
					ActionText.text = LocalizationKeys.BUY_NOW_REMOVED_LABEL.Translate();
					PriceText.text = string.Empty;
					break;
				}
			case BuyPriceAcceptedEvent buyPriceAcceptedEvent:
				{
					CustomUser buyer = await DataManager.Instance.GetUserWithCache(buyPriceAcceptedEvent.Buyer);
					await FromAvatarImage.Initialize(buyer);
					FormatActionText(LocalizationKeys.BOUGHT_BY_LABEL.Translate(), buyer, buyPriceAcceptedEvent.Buyer);
					PriceText.text = buyPriceAcceptedEvent.TotalInEtherFixedPoint + " " + Moralis.CurrentChain.Symbol;
					break;
				}
			case OfferAcceptedEvent offerAcceptedEvent:
				{
					CustomUser buyer = await DataManager.Instance.GetUserWithCache(offerAcceptedEvent.Buyer);
					await FromAvatarImage.Initialize(buyer);
					FormatActionText(LocalizationKeys.OFFER_MADE_ACCEPTED_LABEL.Translate(), buyer, offerAcceptedEvent.Buyer);
					PriceText.text = offerAcceptedEvent.TotalInEtherFixedPoint + " " + Moralis.CurrentChain.Symbol;
					break;
				}
			case OfferMadeEvent offerMadeEvent:
				{
					CustomUser buyer = await DataManager.Instance.GetUserWithCache(offerMadeEvent.Buyer);
					await FromAvatarImage.Initialize(buyer);
					FormatActionText(LocalizationKeys.OFFER_MADE_BY_LABEL.Translate(), buyer, offerMadeEvent.Buyer);
					PriceText.text = offerMadeEvent.TotalInEtherFixedPoint + " " + Moralis.CurrentChain.Symbol;
					break;
				}
			case EthNFTTransfers ethNftTransfers:
				{
					CustomUser fromUser = await DataManager.Instance.GetUserWithCache(ethNftTransfers.FromAddress);
					CustomUser toUser = await DataManager.Instance.GetUserWithCache(ethNftTransfers.ToAddress);
					await FromAvatarImage.Initialize(fromUser);
					await ToAvatarImage.Initialize(toUser);
					ToAvatarImage.gameObject.SetActive(true);
					ActionText.text = string.Format(LocalizationKeys.TRANSFERRED_FROM_TO_LABEL.Translate(),
						fromUser != null ? fromUser.UserName : ethNftTransfers.FromAddress.FormatEthAddress(6),
						toUser != null ? toUser.UserName : ethNftTransfers.ToAddress.FormatEthAddress(6));
					PriceText.text = string.Empty;
					break;
				}

		}
	}

	#endregion

	#region PrivateMethods

	private void FormatActionText(string action, CustomUser user, string address)
	{
		ActionText.text = action + " " + (user != null ? user.UserName : address.FormatEthAddress(6));
	}

	private void OnOpenTransactionClicked()
	{
		string url = ConfigManager.Instance.EtherScanBaseUrl + "tx/" + mAbstractContractEvent.TransactionHash;
		Application.OpenURL(url);
	}

	#endregion
}
