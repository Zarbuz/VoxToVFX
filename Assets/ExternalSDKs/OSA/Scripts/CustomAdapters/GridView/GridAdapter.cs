using System;
using UnityEngine;
using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;

namespace Com.TheFallenGames.OSA.CustomAdapters.GridView
{
	/// <summary>
	/// <para>An optimized adapter for a GridView </para>
	/// <para>Implements <see cref="OSA{TParams, TItemViewsHolder}"/> to simulate a grid by using</para>
	/// <para>a runtime-generated "row" prefab (or "colum" prefab, if horizontal ScrollView), having a Horizontal (or Vertical, respectively) LayoutGroup component, inside which its corresponding cells will lie.</para>
	/// <para>This prefab is represented by a <see cref="CellGroupViewsHolder{TCellVH}"/>, which nicely abstractizes the mechanism to using cell prefabs. This views holder is managed internally and is no concern for most users.</para> 
	/// <para>The cell prefab is used the same way as the "item prefab", for those already familiarized with the ListView examples. It is represented</para>
	/// <para>by a <see cref="CellViewsHolder"/>, which are the actual views holders you need to create/update and nothing else. </para>
	/// </summary>
	/// <typeparam name="TParams">Must inherit from GridParams. See also <see cref="OSA{TParams, TItemViewsHolder}.Parameters"/></typeparam>
	/// <typeparam name="TCellVH">The views holder type to use for the cell. Must inherit from CellViewsHolder</typeparam>
	public abstract class GridAdapter<TParams, TCellVH> : OSA<TParams, CellGroupViewsHolder<TCellVH>> 
        where TParams : GridParams
        where TCellVH : CellViewsHolder, new()
	{
		Action<int, int> _CellsRefreshed;

		/// <summary>
		/// This override the base's implementation to return the cells count, instead of the groups(rows) count.
		/// Params are: 1=prevCellCount, 2=newCellCount
		/// </summary>
		public override event Action<int, int> ItemsRefreshed
		{
			add { _CellsRefreshed += value; }
			remove { _CellsRefreshed -= value; }
		}

		public override bool InsertAtIndexSupported { get { return false; } }
		public override bool RemoveFromIndexSupported { get { return false; } }


		/// <summary>The "items count". Same value is returned in <see cref="GetItemsCount"/></summary>
		public int CellsCount { get { return _CellsCount; } }

		protected int _CellsCount;
		int _PrevCellsCount; // used for firing the ItemsRefreshed with the proper value. it's assigned in ChangeItemsCount

		// Used because the one in Parameters can change in multiple places
		int _LastKnownUsedNumCellsPerGroup;


		/// <summary>Not currently implemented for GridAdapters</summary>
		public sealed override void InsertItems(int index, int itemsCount, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{ throw new OSAException("Cannot use InsertItems() with a GridAdapter yet. Use ResetItems() instead."); }

		/// <summary>Not currently implemented fir GridAdapters</summary>
		public sealed override void RemoveItems(int index, int itemsCount, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{ throw new OSAException("Cannot use RemoveItems() with a GridAdapter yet. Use ResetItems() instead."); }

		/// <summary> Overridden in order to convert the cellsCount to groupsCount before passing it to the base's implementation</summary>
		/// <seealso cref="OSA{TParams, TItemViewsHolder}.ChangeItemsCount(ItemCountChangeMode, int, int, bool, bool)"/>
		public override void ChangeItemsCount(
			ItemCountChangeMode changeMode, 
			int cellsCount /*param name changed from itemsCount*/, 
			int indexIfAppendingOrRemoving = -1, 
			bool contentPanelEndEdgeStationary = false, 
			bool keepVelocity = false
		)
		{
			if (changeMode != ItemCountChangeMode.RESET)
				throw new OSAException("Only ItemCountChangeMode.RESET is supported with a GridAdapter for now");

			_PrevCellsCount = _CellsCount;
			_CellsCount = cellsCount;

			// The number of groups is passed to the base's implementation
			int groupsCount = _Params.GetNumberOfRequiredGroups(_CellsCount);

			base.ChangeItemsCount(changeMode, groupsCount, indexIfAppendingOrRemoving, contentPanelEndEdgeStationary, keepVelocity);
		}

		/// <summary>
		/// Tha base implementation finds the group. Here, we're narrowing the search in the group iself in order to return the CellViewsHolder
		/// </summary>
		public sealed override AbstractViewsHolder GetViewsHolderClosestToViewportLongitudinalNormalizedAbstractPoint(Canvas c, RectTransform canvasRectTransform, float viewportPoint01, float itemPoint01, out float distance)
		{
			var groupVH = base.GetViewsHolderClosestToViewportLongitudinalNormalizedAbstractPoint(c, canvasRectTransform, viewportPoint01, itemPoint01, out distance) as CellGroupViewsHolder<TCellVH>;

			if (groupVH == null 
				|| groupVH.NumActiveCells == 0) // 0 active cells is highly unlikely, but it's worth taking it into account
				return null;

			// Returning the cell closest to the middle
			return groupVH.ContainingCellViewsHolders[groupVH.NumActiveCells / 2];
		}

		/// <summary> Scrolls to the specified cell. Use <see cref="ScrollToGroup(int, float, float)"/> if that was intended instead</summary>
		public sealed override void ScrollTo(int cellIndex, float normalizedOffsetFromViewportStart = 0, float normalizedPositionOfItemPivotToUse = 0)
		{
			float originalItemPivot = normalizedPositionOfItemPivotToUse;
			int groupIndex = _Params.GetGroupIndex(cellIndex);
			bool groupVisible = ScrollTo_ConvertItemPivotToUseIfPossible(groupIndex, cellIndex, ref normalizedPositionOfItemPivotToUse);

			ScrollToGroup(groupIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);

			// Do subsequent ScrollTos if the group wasn't initially visible or a twin pass might've shifted some positions
			if (!groupVisible || _InternalState.lastComputeVisibilityHadATwinPass)
			{
				// Prevent double-conversion
				if (groupVisible)
					normalizedPositionOfItemPivotToUse = originalItemPivot;

				groupVisible = ScrollTo_ConvertItemPivotToUseIfPossible(groupIndex, cellIndex, ref normalizedPositionOfItemPivotToUse);
				ScrollToGroup(groupIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);
				if (!groupVisible || _InternalState.lastComputeVisibilityHadATwinPass)
				{
					// Prevent double-conversion
					if (groupVisible)
						normalizedPositionOfItemPivotToUse = originalItemPivot;

					ScrollTo_ConvertItemPivotToUseIfPossible(groupIndex, cellIndex, ref normalizedPositionOfItemPivotToUse);
					ScrollToGroup(groupIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);
				}
			}
		}

		/// <summary> Scrolls to the specified cell. Use <see cref="SmoothScrollToGroup(int, float, float, float, Func{float, bool}, Action, bool)"/> if that was intended instead</summary>
		public sealed override bool SmoothScrollTo(
			int cellIndex, 
			float duration, 
			float normalizedOffsetFromViewportStart = 0f, 
			float normalizedPositionOfItemPivotToUse = 0f, 
			Func<float, bool> onProgress = null,
			Action onDone = null,
			bool overrideAnyCurrentScrollingAnimation = false)
		{
			int groupIndex = _Params.GetGroupIndex(cellIndex);
			bool groupVisible = ScrollTo_ConvertItemPivotToUseIfPossible(groupIndex, cellIndex, ref normalizedPositionOfItemPivotToUse);

			Func<float, bool> newOnProgress = onProgress;

			var newOnDone = onDone;
			if (!groupVisible)
			{
				duration = duration / 2;

				if (onProgress != null)
					newOnProgress = p =>
					{
						if (onProgress != null)
							return onProgress(p / 2);

						return true;
					};

				if (onDone != null)
					newOnDone = () =>
					{
						if (newOnProgress != null)
							newOnProgress = p =>
							{
								if (onProgress != null)
									return onProgress(.5f + p / 2);

								return true;
							};

						groupVisible = ScrollTo_ConvertItemPivotToUseIfPossible(groupIndex, cellIndex, ref normalizedPositionOfItemPivotToUse);
						SmoothScrollToGroup(groupIndex, duration, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse, newOnProgress, onDone, overrideAnyCurrentScrollingAnimation);
					};
			}
			return SmoothScrollToGroup(groupIndex, duration, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse, newOnProgress, newOnDone, overrideAnyCurrentScrollingAnimation);
		}

		public override bool SmoothBringToView(int cellIndex, float duration, float? spacingFromViewportEdge = null, Func<float, bool> onProgress = null, Action onDone = null, bool overrideCurrentScrollingAnimation = false)
		{

			Debug.Log("OSA: SmoothBringToView doesn't yet work with grids. Falling back to SmoothScrollTo() instead");

			return SmoothScrollTo(cellIndex, duration, .5f, .5f, onProgress, onDone, overrideCurrentScrollingAnimation);

			//float spacingFromViewportEdgeValid;
			//if (spacingFromViewportEdge == null)
			//	spacingFromViewportEdgeValid = _Params.ContentSpacing + _Params.Grid.SpacingInGroup;
			//else
			//	spacingFromViewportEdgeValid = spacingFromViewportEdge.Value;
			//float vpPivot = spacingFromViewportEdgeValid / _InternalState.layoutInfo.vpSize;

			//int groupIndex = _Params.GetGroupIndex(cellIndex);
			//bool groupVisible = BringToView_ConvertItemInsetFromParentEdgeIfPossible(groupIndex, cellIndex, ref spacingFromViewportEdgeValid);

			//return base.SmoothBringToView(groupIndex, duration, spacingFromViewportEdge, onProgress, onDone, overrideCurrentScrollingAnimation);
		}

		/// <summary>
		/// Overriding base's implementation so that we pass the cells count to our own implementation which converts them to group count before further passing it to the base impl.
		/// </summary>
		/// <summary>See <see cref="OSA{TParams, TItemViewsHolder}.Refresh(bool, bool)"/></summary>
		public override void Refresh(bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{ ChangeItemsCount(ItemCountChangeMode.RESET, _CellsCount, -1, contentPanelEndEdgeStationary, keepVelocity); }

		/// <summary>Overriding base's implementation to return the cells count, instead of the groups count</summary>
		/// <seealso cref="OSA{TParams, TItemViewsHolder}.GetItemsCount"/>
		public sealed override int GetItemsCount() { return _CellsCount; }

		#region Cell views holders helpers
		public virtual int GetCellGroupsCount() { return base.GetItemsCount(); }

		/// <summary>The number of visible cells</summary>
		public virtual int GetNumVisibleCells()
		{
			if (_VisibleItemsCount == 0)
				return 0;
			return (_VisibleItemsCount - 1) * _Params.CurrentUsedNumCellsPerGroup + _VisibleItems[_VisibleItemsCount - 1].NumActiveCells;
		}

		/// <summary>
		/// <para>Retrieve the views holder of a cell with speciffic index in view. For example, one can iterate from 0 to <see cref="GetNumVisibleCells"/> </para>
		/// <para>in order to do something with each visible cell. Not to be mistaken for <see cref="GetCellViewsHolderIfVisible(int)"/>,</para>
		/// <para>which retrieves a cell by the index of its corresponding model in your data list (<see cref="AbstractViewsHolder.ItemIndex"/>)</para>
		/// </summary>
		public virtual TCellVH GetCellViewsHolder(int cellViewsHolderIndex)
		{
			if (_VisibleItemsCount == 0)
				return null;

			if (cellViewsHolderIndex > GetNumVisibleCells() - 1)
				return null;

			return _VisibleItems[_Params.GetGroupIndex(cellViewsHolderIndex)]
					.ContainingCellViewsHolders[cellViewsHolderIndex % _Params.CurrentUsedNumCellsPerGroup];
		}

		/// <summary>
		/// <para>Retrieve the views holder of a cell whose associated model's index in your data list is <paramref name="withCellItemIndex"/>.</para>
		/// <para>Not to be mistaken for <see cref="GetCellViewsHolder(int)"/> which retrieves a cell by its index in the "all visible cells" list</para>
		/// </summary>
		/// <returns>null, if the item is outside the viewport (and thus no view is associated with it)</returns>
		public virtual TCellVH GetCellViewsHolderIfVisible(int withCellItemIndex)
		{
			var groupVH = GetItemViewsHolderIfVisible(_Params.GetGroupIndex(withCellItemIndex));
			if (groupVH == null)
				return null;

			return GetCellViewsHolderIfVisible(groupVH, withCellItemIndex);
		}

		public virtual TCellVH GetCellViewsHolderIfVisible(CellGroupViewsHolder<TCellVH> groupVH, int withCellItemIndex)
		{
			int indexOfFirstCellInGroup = groupVH.ItemIndex * _Params.CurrentUsedNumCellsPerGroup;

			if (withCellItemIndex < indexOfFirstCellInGroup + groupVH.NumActiveCells)
				return groupVH.ContainingCellViewsHolders[withCellItemIndex - indexOfFirstCellInGroup];

			return null;
		}

		/// <summary>Scroll to the specified GROUP. Use <see cref="ScrollTo(int, float, float)"/> if scrolling to a CELL was intended instead</summary>
		/// <seealso cref="OSA{TParams, TItemViewsHolder}.ScrollTo(int, float, float)"/>
		public virtual void ScrollToGroup(int groupIndex, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f)
		{ base.ScrollTo(groupIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse); }

		/// <summary>See <see cref="ScrollToGroup(int, float, float)"/></summary>
		public virtual bool SmoothScrollToGroup(
			int groupIndex, 
			float duration, 
			float normalizedOffsetFromViewportStart = 0f, 
			float normalizedPositionOfItemPivotToUse = 0f, 
			Func<float, bool> onProgress = null, 
			Action onDone = null, 
			bool overrideAnyCurrentScrollingAnimation = false)
		{
			return base.SmoothScrollTo(groupIndex, duration, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse, onProgress, onDone, overrideAnyCurrentScrollingAnimation);
		}
		#endregion

		protected override void OnInitialized()
		{
			base.OnInitialized();

			_LastKnownUsedNumCellsPerGroup = _Params.CurrentUsedNumCellsPerGroup;
		}

		protected sealed override void OnItemsRefreshed(int prevCount, int newCount)
		{
			base.OnItemsRefreshed(prevCount, newCount);

			OnCellGroupsRefreshed(prevCount, newCount);
		}

		/// <summary> Creates the Group viewsholder which instantiates the group prefab using the provided params in <see cref="OSA{TParams, TItemViewsHolder}.Init"/>. Only override it if you have a custom cell group prefab</summary>
		/// <seealso cref="OSA{TParams, TItemViewsHolder}.CreateViewsHolder(int)"/>
		/// <param name="itemIndex">the index of the GROUP (attention, not the CELL) that needs creation</param>
		/// <returns>The created group views holder </returns>
		protected override CellGroupViewsHolder<TCellVH> CreateViewsHolder(int itemIndex)
        {
            var instance = GetNewCellGroupViewsHolder();
            instance.Init(_Params.GetGroupPrefab(itemIndex).gameObject, _Params.Content, itemIndex, _Params.Grid.CellPrefab, _Params.CurrentUsedNumCellsPerGroup);

			for (int i = 0; i < instance.ContainingCellViewsHolders.Length; i++)
			{
				var cellVH = instance.ContainingCellViewsHolders[i];
				OnCellViewsHolderCreated(cellVH, instance);
			}

			return instance;
        }

		/// <summary>
		/// Here the grid adapter checks if new groups need to be created or if old ones need to be disabled or destroyed, after which it calls <see cref="UpdateCellViewsHolder(TCellVH)"/> for each remaining cells.
		/// <para>Override it (and call the base implementation!) only if you know what you're doing. If you just want to update your cells' views, do it in <see cref="UpdateCellViewsHolder(TCellVH)"/></para>
		/// </summary>
		/// <seealso cref="OSA{TParams, TItemViewsHolder}.UpdateViewsHolder(TItemViewsHolder)"/>
		/// <param name="newOrRecycled">The viewsholder of the group that needs updated</param>
		protected override void UpdateViewsHolder(CellGroupViewsHolder<TCellVH> newOrRecycled)
        {
			// At this point there are enough groups for sure, but there may not be enough enabled cells, or there may be too many enabled cells

			int activeCellsForThisGroup;
            // If it's the last one
            if (newOrRecycled.ItemIndex + 1 == GetCellGroupsCount())
            {
                int totalCellsBeforeThisGroup = 0;
                if (newOrRecycled.ItemIndex > 0)
                {
                    totalCellsBeforeThisGroup = newOrRecycled.ItemIndex * _Params.CurrentUsedNumCellsPerGroup;
                }
                activeCellsForThisGroup = _CellsCount - totalCellsBeforeThisGroup;
            }
            else
            {
                activeCellsForThisGroup = _Params.CurrentUsedNumCellsPerGroup;
            }
            newOrRecycled.NumActiveCells = activeCellsForThisGroup;

            for (int i = 0; i < activeCellsForThisGroup; ++i)
                UpdateCellViewsHolder(newOrRecycled.ContainingCellViewsHolders[i]);
        }

		/// <summary>Provide your own implementation of the group prefab, if you have a custom one. Most often than not, you won't use this</summary>
		protected virtual CellGroupViewsHolder<TCellVH> GetNewCellGroupViewsHolder()
		{
			return new CellGroupViewsHolder<TCellVH>();
		}

		/// <summary>Called for each cell in a cell group at the moment the group is first created</summary>
		/// <param name="cellVH"></param>
		/// <param name="cellGroup">The cell's group</param>
		protected virtual void OnCellViewsHolderCreated(TCellVH cellVH, CellGroupViewsHolder<TCellVH> cellGroup)
		{

		}

		/// <summary>The only important callback for inheritors. It provides cell's views holder which has just become visible and whose views should be updated from its corresponding data model. viewsHolder.ItemIndex(<see cref="AbstractViewsHolder.ItemIndex"/>) can be used to know what data model is associated with. </summary>
		/// <param name="viewsHolder">The cell's views holder</param>
		protected abstract void UpdateCellViewsHolder(TCellVH viewsHolder);

		/// <summary>
		/// Overridden in order to call <see cref="OnBeforeRecycleOrDisableCellViewsHolder(TCellVH, int)"/> for each active cell in the group
		/// </summary>
		/// <seealso cref="OSA{TParams, TItemViewsHolder}.OnBeforeRecycleOrDisableViewsHolder(TItemViewsHolder, int)"/>
		protected sealed override void OnBeforeRecycleOrDisableViewsHolder(CellGroupViewsHolder<TCellVH> inRecycleBinOrVisible, int newItemIndex)
		{
			base.OnBeforeRecycleOrDisableViewsHolder(inRecycleBinOrVisible, newItemIndex);

			// 2 fors are more efficient
			if (newItemIndex == -1)
				for (int i = 0; i < inRecycleBinOrVisible.NumActiveCells; ++i)
					OnBeforeRecycleOrDisableCellViewsHolder(inRecycleBinOrVisible.ContainingCellViewsHolders[i], -1);
			else
				for (int i = 0; i < inRecycleBinOrVisible.NumActiveCells; ++i)
					OnBeforeRecycleOrDisableCellViewsHolder(inRecycleBinOrVisible.ContainingCellViewsHolders[i], newItemIndex * _Params.CurrentUsedNumCellsPerGroup + i);
		}

		/// <summary> This is not needed yet in case of grid adapters </summary>
		protected override void OnItemIndexChangedDueInsertOrRemove(CellGroupViewsHolder<TCellVH> shiftedViewsHolder, int oldIndex, bool wasInsert, int removeOrInsertIndex)
		{
			base.OnItemIndexChangedDueInsertOrRemove(shiftedViewsHolder, oldIndex, wasInsert, removeOrInsertIndex);

		}

		protected override void RebuildLayoutDueToScrollViewSizeChange()
		{
			base.RebuildLayoutDueToScrollViewSizeChange();

			//int prevCellsPerGroup = _Params.CurrentUsedNumCellsPerGroup;
			//int newCellsPerGroup = _Params.CalculateCurrentNumCellsPerGroup();
			int prevCellsPerGroup = _LastKnownUsedNumCellsPerGroup;
			int newCellsPerGroup = _Params.CurrentUsedNumCellsPerGroup;
			_LastKnownUsedNumCellsPerGroup = newCellsPerGroup;
			// The cell groups need to be rebuilt completely, since the new number of cells per group is different
			if (prevCellsPerGroup != newCellsPerGroup)
				ClearVisibleItems();
		}

		/// <summary>The only important callback for inheritors. It provides cell's views holder which has just become visible and whose views should be updated from its corresponding data model. viewsHolder.ItemIndex(<see cref="AbstractViewsHolder.ItemIndex"/>) can be used to know what data model is associated with. </summary>
		/// <param name="viewsHolder">The cell's views holder</param>
		protected virtual void OnBeforeRecycleOrDisableCellViewsHolder(TCellVH viewsHolder, int newItemIndex) { }

		protected virtual void OnCellGroupsRefreshed(int prevGroupsCount, int curGroupsCount)
		{
			if (_CellsRefreshed != null)
				_CellsRefreshed(_PrevCellsCount, _CellsCount);
		}

		protected bool ScrollTo_ConvertItemPivotToUseIfPossible(int groupIndex, int cellIndex, ref float normalizedPositionOfItemPivotToUse)
		{
			var groupVH = GetItemViewsHolderIfVisible(groupIndex);

			if (groupVH == null)
				return false;

			// the group is visible => the search can be more granular
			var cellVH = GetCellViewsHolderIfVisible(groupVH, cellIndex);
			if (cellVH == null)
				throw new OSAException("GetItemViewsHolderIfVisible " + groupIndex + " got group vh, but GetCellViewsHolderIfVisible " + cellIndex + " got null ?. Please report this bug");

			float groupSize = groupVH.root.rect.size[_InternalState.layoutInfo.hor0_vert1];
			float cellSize = cellVH.root.rect.size[_InternalState.layoutInfo.hor0_vert1];
			float cellInsetStart = cellVH.root.GetInsetFromParentEdge(groupVH.root, _InternalState.layoutInfo.startEdge);

			normalizedPositionOfItemPivotToUse = (cellInsetStart + normalizedPositionOfItemPivotToUse * cellSize) / groupSize;

			return true;
		}

		// WIP
		//protected bool BringToView_ConvertItemInsetFromParentEdgeIfPossible(int groupIndex, int cellIndex, ref float spacingFromViewportEdge)
		//{
		//	var groupVH = GetItemViewsHolderIfVisible(groupIndex);
		//	if (groupVH == null)
		//		return false;

		//	// the group is visible => the search can be more granular
		//	var cellVH = GetCellViewsHolderIfVisible(groupVH, cellIndex);
		//	if (cellVH == null)
		//		throw new OSAException("GetItemViewsHolderIfVisible " + groupIndex + " got group vh, but GetCellViewsHolderIfVisible " + cellIndex + " got null ?. Please report this bug");

		//	float groupSize = groupVH.root.rect.size[_InternalState.layoutInfo.hor0_vert1];
		//	float cellSize = cellVH.root.rect.size[_InternalState.layoutInfo.hor0_vert1];
		//	float cellInsetStart = cellVH.root.GetInsetFromParentEdge(groupVH.root, _InternalState.layoutInfo.startEdge);

		//	spacingFromViewportEdge = (cellInsetStart + spacingFromViewportEdge * cellSize) / groupSize;

		//	return true;
		//}
	}
}
