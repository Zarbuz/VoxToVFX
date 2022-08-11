using System;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.DataHelpers;

namespace Com.TheFallenGames.OSA.CustomParams
{
	/// <summary>
	/// This will soon become a legacy feature. Using a DataHelper is preferred instead. 
	/// See <see cref="BaseParamsWithPrefabAndData{TData}"/> and <see cref="LazyList{T}"/></summary>
	/// <typeparam name="TData">The model type to be used</typeparam>
	[System.Serializable]
	public abstract class BaseParamsWithPrefabAndLazyData<TData> : BaseParamsWithPrefab
	{
		public LazyList<TData> Data { get; set; }
		public Func<int, TData> NewModelCreator { get; set; }


		/// <inheritdoc/>
		public override void InitIfNeeded(IOSA iAdapter)
		{
			base.InitIfNeeded(iAdapter);

			if (Data == null) // this will only be null at init. When scrollview's size changes, the data should remain the same
				Data = new LazyList<TData>(NewModelCreator, 0);
		}
	}
}
