

namespace Com.TheFallenGames.OSA.Core
{
	public enum ItemCountChangeMode
	{
		/// <summary>
		/// The items count will be set to the specified count.
		/// The cached list of sizes will be cleared and all items will initially have the default size <see cref="BaseParams.DefaultItemSize"/>
		/// </summary>
		RESET = 0,

		///// <summary>
		///// The count param will be ignored. This just recalculates positions and sizes
		///// </summary>
		//REFRESH,

		/// <summary>The items count will be increased by the specified count</summary>
		INSERT = 1,

		/// <summary>The items count will be decreased by the specified count</summary>
		REMOVE = -1,
	}
}
