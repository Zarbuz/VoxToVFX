using UnityEngine;
using Com.TheFallenGames.OSA.Core;
using UnityEngine.UI;
using System;
using UnityEngine.Serialization;

namespace Com.TheFallenGames.OSA.CustomParams
{
	/// <summary>
	/// Custom params containing a single prefab. <see cref="ItemPrefabSize"/> is calculated on first accessing and invalidated each time <see cref="InitIfNeeded(IOSA)"/> is called.
	/// </summary>
	[System.Serializable]
	public class BaseParamsWithPrefab : BaseParams
	{
		[FormerlySerializedAs("itemPrefab")]
		[SerializeField]
		RectTransform _ItemPrefab = null;
#if OSA_PLAYMAKER
		// Avoid a compile error in playmaker package; this is a temporary change
		[Obsolete("Use ItemPrefab instead", false)]
#else
		[Obsolete("Use ItemPrefab instead", true)]
#endif
		public RectTransform itemPrefab { get { return ItemPrefab; } set { ItemPrefab = value; } }
		public RectTransform ItemPrefab { get { return _ItemPrefab; } set { _ItemPrefab = value; } }

		[Tooltip("Whether to set the BaseParam's ItemTransversalSize to the transversal size of the prefab, like it's done with DefaultItemSize.\n" +
			"Setting this to true naturally overrides any value you set to ItemTransversalSize.\n" +
			"Setting it to false, leaves ItemTransversalSize unchanged.\n")]
		[SerializeField]
		bool _AlsoControlItemTransversalSize = false;
		/// <summary>
		/// Whether to set the <see cref="BaseParams.ItemTransversalSize"/> to the transversal size of the prefab, like it's done with <see cref="BaseParams.DefaultItemSize"/>
		/// <para>Setting this to true naturally overrides any value you set to ItemTransversalSize</para>
		/// <para>Setting it to false, leaves <see cref="BaseParams.ItemTransversalSize"/> unchanged</para>
		/// </summary>
		public bool AlsoControlItemTransversalSize { get { return _AlsoControlItemTransversalSize; } set { _AlsoControlItemTransversalSize = value; } }

		public float ItemPrefabSize
		{
			get
			{
				if (!ItemPrefab)
					throw new OSAException(typeof(BaseParamsWithPrefab) + ": the prefab was not set. Please set it through inspector or in code");

				if (_ItemPrefabSize == -1f)
				{
					var rect = ItemPrefab.rect;
					if (IsHorizontal)
					{
						_ItemPrefabSize = rect.width;
						if (_AlsoControlItemTransversalSize)
						{
							ItemTransversalSize = rect.height;
							// Center it transversally
							ContentPadding.top = ContentPadding.bottom = -1;
						}
					}
					else
					{
						_ItemPrefabSize = ItemPrefab.rect.height;
						if (_AlsoControlItemTransversalSize)
						{
							ItemTransversalSize = rect.width;
							// Center it transversally
							ContentPadding.left = ContentPadding.right = -1;
						}
					}
				}

				return _ItemPrefabSize;
			}
		}

		float _ItemPrefabSize = -1f;


		/// <inheritdoc/>
		public override void InitIfNeeded(IOSA iAdapter)
		{
			base.InitIfNeeded(iAdapter);
			InitItemPrefab();
		}

		protected void InitItemPrefab()
		{
			if (ItemPrefab.parent != ScrollViewRT)
				LayoutRebuilder.ForceRebuildLayoutImmediate(ItemPrefab.parent as RectTransform);
			else
				LayoutRebuilder.ForceRebuildLayoutImmediate(ItemPrefab);

			AssertValidWidthHeight(ItemPrefab);
			_ItemPrefabSize = -1f; // so the prefab's size will be recalculated next time is accessed
			DefaultItemSize = ItemPrefabSize;
		}
	}
}