using UnityEngine;

namespace VoxToVFXFramework.Scripts.Singleton
{
	public abstract class ModuleSingletonBase : MonoBehaviour
	{
		#region UnityMethods

		internal abstract void InitSingleton();

		protected void Awake()
		{
			InitSingleton();
			OnAwake();
		}

		protected void Start()
		{
			OnStart();
		}

		protected virtual void OnAwake() { }

		protected virtual void OnStart() { }

		#endregion
	}

	public abstract class ModuleSingleton<T> : ModuleSingletonBase where T : ModuleSingletonBase
	{
		#region ConstStatic

		private static T mInstance;
		public static T Instance
		{
			get
			{
				if (mInstance == null)
				{
					mInstance = FindObjectOfType<T>();
				}

				return mInstance;
			}
		}

		#endregion

		#region PrivateMethods

		internal sealed override void InitSingleton()
		{
			// If the static Instance is called before the Awake, the instance is already set.
			if (mInstance == null)
			{
				mInstance = this as T;
			}
			else if (mInstance.gameObject != gameObject)
			{
				Destroy(gameObject);
				return;
			}

			if (transform.parent == null)
			{
				DontDestroyOnLoad(gameObject);
			}
		}

		#endregion
	}
}