using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;
using VoxToVFXFramework.Scripts.Managers;

public class TriggerCloudFunction : MonoBehaviour
{
#if UNITY_EDITOR
	[Button]
	public async void WatchMintedEventContract()
	{
		await NFTManager.Instance.WatchMintedEventContract();
	}

	[Button]
	public async void WatchTransferEventContract()
	{
		await NFTManager.Instance.WatchTransferEventContract();
	}

	[Button]
	public async void UnWatchTransferEventContract()
	{
		await NFTManager.Instance.UnWatchTransferEventContract();
	}
#endif
}
