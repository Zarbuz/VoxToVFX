using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VoxToVFXFramework.Scripts.Singleton;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class UnityMainThreadManager : ModuleSingleton<UnityMainThreadManager>
	{
		private static readonly Queue<Action> _executionQueue = new Queue<Action>();
		private static readonly ConcurrentQueue<Action> _concurentQueue = new ConcurrentQueue<Action>();
		public void Update()
		{
			//TODO _concurentQueue
			lock (_executionQueue)
			{
				while (_executionQueue.Count > 0)
				{
					_executionQueue.Dequeue().Invoke();
				}
			}
		}

		/// <summary>
		/// Locks the queue and adds the IEnumerator to the queue
		/// </summary>
		/// <param name="action">IEnumerator function that will be executed from the main thread.</param>
		public void Enqueue(IEnumerator action)
		{
			lock (_executionQueue)
			{
				_executionQueue.Enqueue(() => {
					StartCoroutine(action);
				});
			}
		}

		/// <summary>
		/// Locks the queue and adds the Action to the queue
		/// </summary>
		/// <param name="action">function that will be executed from the main thread.</param>
		public void Enqueue(Action action)
		{
			Enqueue(ActionWrapper(action));
		}

		/// <summary>
		/// Locks the queue and adds the Action to the queue, returning a Task which is completed when the action completes
		/// </summary>
		/// <param name="action">function that will be executed from the main thread.</param>
		/// <returns>A Task that can be awaited until the action completes</returns>
		public System.Threading.Tasks.Task EnqueueAsync(Action action)
		{
			var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();

			void WrappedAction()
			{
				try
				{
					action();
					tcs.TrySetResult(true);
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			}

			Enqueue(ActionWrapper(WrappedAction));
			return tcs.Task;
		}


		private IEnumerator ActionWrapper(Action a)
		{
			a();
			yield return null;
		}
	}
}
