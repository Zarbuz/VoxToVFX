using UnityEngine;
using UnityEngine.EventSystems;
using VoxToVFXFramework.Scripts.Utils.Extensions;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	public class AbstractCardItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		#region ScriptParameters

		[SerializeField] private CanvasGroup CanvasGroup;

		#endregion

		#region Fields

		private Coroutine mCoroutineAlphaEnter;
		private Coroutine mCoroutineAlphaExit;

		#endregion

		#region UnityMethods

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (mCoroutineAlphaExit != null)
			{
				StopCoroutine(mCoroutineAlphaExit);
				mCoroutineAlphaExit = null;
			}

			mCoroutineAlphaEnter = StartCoroutine(CanvasGroup.AlphaFade(1, 0.05f));
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (mCoroutineAlphaEnter != null)
			{
				StopCoroutine(mCoroutineAlphaEnter);
				mCoroutineAlphaEnter = null;
			}
			mCoroutineAlphaExit = StartCoroutine(CanvasGroup.AlphaFade(0, 0.05f));
		}

		#endregion
	}
}
