public abstract class SimpleSingleton<T> where T : class, new()
{
	#region ConstStatic

	protected static T mInstance;
	public static T Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new T();
			}

			return mInstance;
		}
	}

	protected SimpleSingleton()
	{
		Init();
	}

	#endregion

	#region PrivateMethods

	protected virtual void Init()
	{
	}

	#endregion
}