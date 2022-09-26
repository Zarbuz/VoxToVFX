using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI.Atomic
{
	public abstract class AbstractComplexDetailsPanel : MonoBehaviour
	{
		private VerticalLayoutGroup[] mVerticalLayoutGroups;

		protected virtual void OnEnable()
		{
			mVerticalLayoutGroups = GetComponentsInChildren<VerticalLayoutGroup>();
		}

		protected void RebuildAllVerticalRect()
		{
			foreach (VerticalLayoutGroup verticalLayoutGroup in mVerticalLayoutGroups.Reverse())
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(verticalLayoutGroup.GetComponent<RectTransform>());
			}
		}
	}
}
