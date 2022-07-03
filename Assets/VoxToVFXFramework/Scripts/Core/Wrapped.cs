using System;

namespace VoxToVFXFramework.Scripts.Core
{
	public class Wrapped<T>
	{
		#region Fields

		public Action OnValueChanged;
		private T mValue;

		public T Value
		{
			get => mValue;
			set
			{
				if (!mValue.Equals(value))
				{
					mValue = value;
					OnValueChanged?.Invoke();
				}
			}
		}

		#endregion

		#region Constructor

		public Wrapped(T value)
		{
			mValue = value;
		}

		#endregion
	}
}