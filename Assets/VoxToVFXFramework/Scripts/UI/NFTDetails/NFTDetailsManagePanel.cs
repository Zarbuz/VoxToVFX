using System;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.UI.NFTUpdate;

namespace VoxToVFXFramework.Scripts.UI.NFTDetails
{
	public class NFTDetailsManagePanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Button SetPriceButton;
		[SerializeField] private Button ListButton;
		[SerializeField] private Toggle OpenManagerToggle;
		[SerializeField] private GameObject SubPanelManager;
		[SerializeField] private Button TransferNFTButton;
		[SerializeField] private Button BurnNFTButton;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			SetPriceButton.onClick.AddListener(OnSetPriceClicked);
			ListButton.onClick.AddListener(OnSetListClicked);
			OpenManagerToggle.onValueChanged.AddListener(OnOpenManagerValueChanged);
			TransferNFTButton.onClick.AddListener(OnTransferNFTClicked);
			BurnNFTButton.onClick.AddListener(OnBurnNFTClicked);
		}

		private void OnDisable()
		{
			SetPriceButton.onClick.RemoveListener(OnSetPriceClicked);
			ListButton.onClick.RemoveListener(OnSetListClicked);
			OpenManagerToggle.onValueChanged.RemoveListener(OnOpenManagerValueChanged);
			TransferNFTButton.onClick.RemoveListener(OnTransferNFTClicked);
			BurnNFTButton.onClick.RemoveListener(OnBurnNFTClicked);
		}

		#endregion

		#region PrivateMethods

		private void OnSetPriceClicked()
		{
			CanvasPlayerPCManager.Instance.OpenNFTUpdatePanel(eUpdateTargetType.SET_BUY_PRICE);
		}

		private void OnSetListClicked()
		{

		}

		private void OnOpenManagerValueChanged(bool isOn)
		{
			SubPanelManager.SetActive(isOn);
		}

		private void OnTransferNFTClicked()
		{

		}

		private void OnBurnNFTClicked()
		{

		}

		#endregion
	}
}
