using System;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class EventDatabaseManager : SimpleSingleton<EventDatabaseManager>
	{
		public event Action<AbstractContractEvent> OnDatabaseEventReceived;

		public async void OnEventReceived(AbstractContractEvent contractEvent)
		{
			await UnityMainThreadManager.Instance.EnqueueAsync(() =>
			{
				OnDatabaseEventReceived?.Invoke(contractEvent);
			});
		}
	}
}
