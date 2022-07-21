using System.Collections;
using UnityEngine;

namespace VoxToVFXFramework.Scripts.Utils.Extensions
{
	public static class ExtensionsCanvasGroup
	{
		public static IEnumerator AlphaFade(this CanvasGroup cg, float alpha, float alphaFadeSeconds)
		{
			float start = Time.time;
			while (cg.alpha != alpha)
			{
				float elapsed = Time.time - start;
				float normalisedTime = Mathf.Clamp((elapsed / alphaFadeSeconds) * Time.deltaTime, 0, 1);
				cg.alpha = Mathf.Lerp(cg.alpha, alpha, normalisedTime);
				yield return new WaitForEndOfFrame();
			}
			yield return true;
		}

	}
}