//------------------------------------------------------------------------------
// Simple base class for Singletons
// Use it as "myclass : ASimpleSingleton<myclass>"
// When first creating the instance, the Init() method is called
//------------------------------------------------------------------------------
using frame8.Logic.Core.Interfaces;
using System;

namespace frame8.Logic.Misc.Other
{
	public abstract class Singleton8<T>
	: IInitializable8
	where T : IInitializable8, new()

	{
		static T _Instance;

		public static T Instance
		{
			get
			{
				if (_Instance == null)
				{
					_Instance = new T();
					_Instance.Init();
				}

				return _Instance;
			}
		}

		public abstract void Init();

		/*private void InternalInit()
		{

		}

		abstract */
	}
}


