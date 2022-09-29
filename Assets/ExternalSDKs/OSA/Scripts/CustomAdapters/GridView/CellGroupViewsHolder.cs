using UnityEngine;
using UnityEngine.UI;
using Com.TheFallenGames.OSA.Core;

namespace Com.TheFallenGames.OSA.CustomAdapters.GridView
{
    /// <summary>
    /// <para>A views holder representing a group of cells (row or column). It instantiates the maximum number of cells it can contain,</para>
    /// <para>but only those of them that should be displayed will have their <see cref="CellViewsHolder.views"/> enabled</para>
    /// </summary>
    /// <typeparam name="TCellVH">The views holder type used for the cells in this group</typeparam>
    public class CellGroupViewsHolder<TCellVH> : BaseItemViewsHolder where TCellVH : CellViewsHolder, new()
    {
        /// <summary>Uses base's implementation, but also updates the indices of all containing cells each time the setter is called</summary>
        public override int ItemIndex
        {
            get { return base.ItemIndex; }
            set
            {
                base.ItemIndex = value;
                if (_Capacity > 0)
                    OnGroupIndexChanged();
            }
        }

        /// <summary>The number of visible cells, i.e. that are used to display real data. The other ones are disabled and are either empty or hold obsolete data</summary>
        public int NumActiveCells
        {
            get { return _NumActiveCells; }
            set
            {
                if (_NumActiveCells != value)
                {
                    _NumActiveCells = value;
					TCellVH cellVH;
					GameObject viewsGO;
					bool active;
					// TODO (low-priority) also integrate the new SetViewsHolderEnabled functionality here, for grids
                    for (int i = 0; i < _Capacity; ++i)
					{
						cellVH = ContainingCellViewsHolders[i];
						viewsGO = cellVH.views.gameObject;
						active = i < _NumActiveCells;
						if (viewsGO.activeSelf != active)
							viewsGO.SetActive(active);
						if (cellVH.rootLayoutElement.ignoreLayout == active)
							cellVH.rootLayoutElement.ignoreLayout = !active;
					}
                }
            }
        }

        /// <summary>The views holders of all containing cells, active or not</summary>
        public TCellVH[] ContainingCellViewsHolders { get; private set; }


		//protected HorizontalOrVerticalLayoutGroup _LayoutGroup;
		protected int _Capacity = -1;
        protected int _NumActiveCells = 0;

		RectTransform[] _ContainingCellInstances;

        /// <summary>
        /// <para>Called by <see cref="Init(GameObject, int, RectTransform, int)"/>, after the GameObjects for the group and all containing cells are instantiated</para>
        /// <para>Creates the cells' views holders and initializes them, also setting their itemIndex based on this group's <see cref="ItemIndex"/></para>
        /// </summary>
        public override void CollectViews()
        {
            base.CollectViews();

            //if (capacity == -1) // not initialized
            //    throw new InvalidOperationException("ItemAsLayoutGroupViewsHolder.CollectViews(): call InitGroup(...) before!");

            //_LayoutGroup = root.GetComponent<HorizontalOrVerticalLayoutGroup>();

            ContainingCellViewsHolders = new TCellVH[_Capacity];
            for (int i = 0; i < _Capacity; ++i)
            {
                ContainingCellViewsHolders[i] = new TCellVH();
				//ContainingCellViewsHolders[i].InitWithExistingRootPrefab(root.GetChild(i) as RectTransform);
				var cellRT = _ContainingCellInstances[i];
				ContainingCellViewsHolders[i].InitWithExistingRootPrefab(cellRT);
				// TODO also integrate the new SetViewsHolderEnabled functionality here, for grids
				ContainingCellViewsHolders[i].views.gameObject.SetActive(false); // not visible, initially
            }

            if (ItemIndex != -1 && _Capacity > 0)
                UpdateIndicesOfContainingCells();
        }

        /// <summary>The only way to instantiate the group views holder. It's used internally, since the group managing is not done by the API user</summary>
        /// <param name="itemIndex">the group's index</param>
        internal void Init(GameObject groupPrefab, RectTransform parent, int itemIndex, RectTransform cellPrefab, int numCellsPerGroup)
        {
            base.Init(
                groupPrefab,
				parent,
				itemIndex, 
                true,
                false // not calling CollectViews, because we'll call it below
            );

            _Capacity = numCellsPerGroup;

			// Instantiate all the cells in the group
			var groupTR = groupPrefab.transform;

			_ContainingCellInstances = new RectTransform[_Capacity];
			for (int i = 0; i < _Capacity; ++i)
            {
                var cellInstance = (GameObject.Instantiate(cellPrefab.gameObject, groupTR, false) as GameObject).transform as RectTransform;
				// TODO also integrate the new SetViewsHolderEnabled functionality here, for grids
				cellInstance.gameObject.SetActive(true); // just in case the prefab was disabled
                cellInstance.SetParent(root);
				_ContainingCellInstances[i] = cellInstance;
			}
            CollectViews();
        }

		/// <inheritdoc/>
		public override void OnBeforeDestroy()
		{
			// Calling OnBeforeDestroy for all the child cells
			if (ContainingCellViewsHolders != null)
			{
				for (int i = 0; i < ContainingCellViewsHolders.Length; i++)
				{
					var c = ContainingCellViewsHolders[i];
					if (c != null)
						c.OnBeforeDestroy();
				}
			}

			base.OnBeforeDestroy();
		}

		/// <summary>This happens when the views holder is recycled or first created</summary>
		protected virtual void OnGroupIndexChanged()
        {
            if (_Capacity > 0)
                UpdateIndicesOfContainingCells();
        }

        protected virtual void UpdateIndicesOfContainingCells()
        {
            for (int i = 0; i < _Capacity; ++i)
                ContainingCellViewsHolders[i].ItemIndex = ItemIndex * _Capacity + i; 
        }
    }

}
