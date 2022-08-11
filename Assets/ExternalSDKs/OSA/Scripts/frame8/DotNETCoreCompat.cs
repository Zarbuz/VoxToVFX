using frame8.Logic.Misc.Other.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace frame8.Logic.Misc.Other
{
    public static class DotNETCoreCompat
	{
		public static List<TOut> ConvertAll<TIn, TOut>(IEnumerable<TIn> objects, Func<TIn, TOut> converter)
		{
			var list = new List<TOut>();
			foreach (var o in objects)
				list.Add(converter(o));

			return list;
		}

		public static TOut[] ConvertAllToArray<TIn, TOut>(IEnumerable<TIn> objects, Func<TIn, TOut> converter) { return ConvertAll(objects, converter).ToArray(); }

	}
}
