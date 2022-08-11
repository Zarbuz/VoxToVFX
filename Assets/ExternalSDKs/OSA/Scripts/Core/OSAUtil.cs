

using frame8.Logic.Misc.Visual.UI.MonoBehaviours;
using UnityEngine;

namespace Com.TheFallenGames.OSA.Core
{
	public class OSAUtil
	{
		public static bool DotNETCoreCompat_IsAssignableFrom(System.Type to, System.Type from)
		{
#if NETFX_CORE
			if (!to.GetTypeInfo().IsAssignableFrom(from.Ge‌​tTypeInfo()))
#else
			if (!to.IsAssignableFrom(from))
#endif
				return false;

			return true;
		}

		public static ScrollbarFixer8 ConfigureDinamicallyCreatedScrollbar(UnityEngine.UI.Scrollbar scrollbar, IOSA iAdapter, RectTransform viewport, bool addFixerIfDoesntExist = true)
		{
			var instanceScollbarFixer = scrollbar.GetComponent<ScrollbarFixer8>();
			if (!instanceScollbarFixer)
			{
				if (!addFixerIfDoesntExist)
					return null;
				instanceScollbarFixer = scrollbar.gameObject.AddComponent<ScrollbarFixer8>();
			}
			//instanceScollbarFixer.scrollRect = _WindowParams.scrollRect;
			instanceScollbarFixer.externalScrollRectProxy = iAdapter;
			instanceScollbarFixer.viewport = viewport;

			return instanceScollbarFixer;
		}

	}
}
