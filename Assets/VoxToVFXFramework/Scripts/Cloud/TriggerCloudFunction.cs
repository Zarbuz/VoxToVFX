using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.UI;

public class TriggerCloudFunction : MonoBehaviour
{
#if UNITY_EDITOR
	[Button]
	public async void WatchMintedEventContract()
	{
		await NFTManager.Instance.WatchMintedEventContract();
	}

	[Button]
	public async void WatchSefDestructEventContract()
	{
		await NFTManager.Instance.WatchSelfDestructEventContract();
	}


	[Button]
	public async void OpenManuallyProfile()
	{
		CustomUser user = await UserManager.Instance.LoadUserFromEthAddress("0x1632f51558fd5cf54109d61c57e06bd664742438");
		CanvasPlayerPCManager.Instance.OpenProfilePanel(user);
	}

#endif
}
