using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Com.TheFallenGames.OSA.Core;

namespace Com.TheFallenGames.OSA.CustomAdapters.GridView
{
	/// <summary>Base class for params to be used with a <see cref="GridAdapter{TParams, TCellVH}"/></summary>
	[Serializable] // serializable, so it can be shown in inspector
	public class GridParams : BaseParams
	{
		#region Configuration
		[FormerlySerializedAs("grid")]
		[SerializeField]
		GridConfig _Grid = new GridConfig();
		[Obsolete("Use Grid instead", true)]
		public GridConfig grid { get { return Grid; } set { Grid = value; } }
		public GridConfig Grid { get { return _Grid; } set { _Grid = value; } }
		#endregion

		public int CurrentUsedNumCellsPerGroup { get { return _CurrentUsedNumCellsPerGroup; } }
		public LayoutElement CellPrefabLayoutElement { get { return _CellPrefabLayoutElement; } }

		//// Both of these should be at least 1
		//int NumCellsPerGroupHorizontally { get { return IsHorizontal ? 1 : numCellsPerGroup; } }
		//int NumCellsPerGroupVertically { get { return IsHorizontal ? numCellsPerGroup : 1; } }

		/// <summary>Cached prefab, auto-generated at runtime, first time <see cref="GetGroupPrefab(int)"/> is called</summary>
		LayoutGroup _TheOnlyGroupPrefab;
		HorizontalOrVerticalLayoutGroup _TheOnlyGroupPrefabAsHorizontalOrVertical;
		int _CurrentUsedNumCellsPerGroup;
		LayoutElement _CellPrefabLayoutElement;


		/// <inheritdoc/>
		public override void InitIfNeeded(IOSA iAdapter)
		{
			base.InitIfNeeded(iAdapter);

			if (!Grid.CellPrefab)
				throw new OSAException(typeof(GridParams) + ": the prefab was not set. Please set it through inspector or in code");

			_CellPrefabLayoutElement = Grid.CellPrefab.GetComponent<LayoutElement>();
			if (!_CellPrefabLayoutElement)
				throw new OSAException(typeof(GridParams) + ": no LayoutElement found on the cellPrefab: you should add one to configure how the cell's parent LayoutGroup should position/size it");

			AssertValidWidthHeight(Grid.CellPrefab);

			if (Grid.SpacingInGroup == -1f)
				Grid.SpacingInGroup = ContentSpacing;


			if (!Grid.UseDefaultItemSizeForCellGroupSize)
			{
				// DefaultItemSize refers to the group's size here, so we're also adding the groupPadding to it
				if (IsHorizontal)
				{
					DefaultItemSize = _CellPrefabLayoutElement.preferredWidth;
					if (DefaultItemSize < 0)
					{
						DefaultItemSize = _CellPrefabLayoutElement.minWidth;
						if (DefaultItemSize < 0)
						{
							if (_CellPrefabLayoutElement.flexibleWidth == -1)
								throw new OSAException(
									typeof(GridParams) + ".cellPrefab.LayoutElement: Could not determine the cell group's width(UseDefaulfItemSizeForCellGroupSize=false). " +
									"Please specify at least preferredWidth, minWidth or flexibleWidth(case in which the current width of the cell will be used as the group's width)"
								);
							DefaultItemSize = Grid.CellPrefab.rect.width;
						}
					}

					DefaultItemSize += Grid.GroupPadding.horizontal;
				}
				else
				{
					DefaultItemSize = _CellPrefabLayoutElement.preferredHeight;
					if (DefaultItemSize < 0)
					{
						DefaultItemSize = _CellPrefabLayoutElement.minHeight;
						if (DefaultItemSize < 0)
						{
							if (_CellPrefabLayoutElement.flexibleHeight == -1)
								throw new OSAException(
									typeof(GridParams) + ".cellPrefab.LayoutElement: Could not determine the cell group's height(UseDefaulfItemSizeForCellGroupSize=false). " +
									"Please specify at least preferredHeight, minHeight or flexibleHeight(case in which the current height of the cell will be used as the group's height)"
								);
							DefaultItemSize = Grid.CellPrefab.rect.height;
						}
					}

					DefaultItemSize += Grid.GroupPadding.vertical;
				}
			}

			if (Grid.MaxCellsPerGroup == 0)
				Grid.MaxCellsPerGroup = -1;

			_CurrentUsedNumCellsPerGroup = CalculateCurrentNumCellsPerGroup();

			// Hotfix 12.10.2017 14:45: There's a bug in Unity on some versions: creating a new GameObject at runtime and adding it a RectTransform cannot be done in Awake() or OnEnabled().
			// See: https://issuetracker.unity3d.com/issues/error-when-creating-a-recttransform-component-in-an-awake-call-of-an-instantiated-monobehaviour
			// The bug was initially found in a project where the initial count is 0 (when Start() is called), then the scrollview is disabled, set a non-zero count, then enabled back,
			// and in OnEnable() the user called ResetItems(), which triggered the lazy-instantiation of the group prefab - since it's created in the first GetGroupPrefab() call.
			// Solved it by creating the prefab here, because InitIfNeeded(IOSA) is called at Init time (in MonoBehaviour.Start())
			CreateOrReinitCellGroupPrefab();
		}

		public virtual int CalculateCurrentNumCellsPerGroup()
		{
			if (Grid.MaxCellsPerGroup > 0)
			{
				if (IsHorizontal)
				{
					if (_CellPrefabLayoutElement.preferredHeight == -1f 
						&& _CellPrefabLayoutElement.minHeight == -1f 
						&& _CellPrefabLayoutElement.flexibleHeight == -1f
						&& !Grid.CellHeightForceExpandInGroup)
						Debug.Log(
							"OSA: " + typeof(GridParams) +
							".cellPrefab.LayoutElement: Using a fixed number of rows (Grid.MaxCellsPerGroup is " + Grid.MaxCellsPerGroup +
							"), but none of the prefab's minHeight/preferredHeight/flexibleHeight is set, nor cellHeightForceExpandInGroup is true. " +
							"The cells will have 0 height initially. This is rarely the intended behavior" +
							"), but none of the prefab's minWidth/preferredWidth/flexibleWidth is set. " +
							"Could not determine the cell group's width. Using the current prefab's width (" + DefaultItemSize + ")"
						);
				}
				else
				{
					if (_CellPrefabLayoutElement.minWidth == -1f
						&& _CellPrefabLayoutElement.preferredWidth == -1f
						&& _CellPrefabLayoutElement.flexibleWidth == -1f
						&& !Grid.CellWidthForceExpandInGroup)
						Debug.Log(
							"OSA: " + typeof(GridParams) +
							".cellPrefab.LayoutElement: Using a fixed number of columns (Grid.MaxCellsPerGroup is " + Grid.MaxCellsPerGroup +
							"), but none of the prefab's minWidth/preferredWidth/flexibleWidth is set, nor cellWidthForceExpandInGroup is true. " +
							"The cells will have 0 width initially. This is rarely the intended behavior"
						);
				}

				return Grid.MaxCellsPerGroup;
			}

			int minMaxCellsPerGroup = -OSAConst.MAX_CELLS_PER_GROUP_FACTOR_WHEN_INFERRING;
			if (Grid.MaxCellsPerGroup < minMaxCellsPerGroup)
			{
				Debug.Log(
					"OSA: " + typeof(GridParams) +
					".Grid.MaxCellsPerGroup: can't be less than " + minMaxCellsPerGroup + ". Clamping it! Because you're multiplying the already 'recommended' number of cells, velues beyond that could lead to significant framedrops."+
					" If you really know you need this, increase the value in OSAConst.MAX_CELLS_PER_GROUP_FACTOR_WHEN_INFERRING"
				);

				Grid.MaxCellsPerGroup = minMaxCellsPerGroup;
			}

			var scrollViewSize = ScrollViewRT.rect.size;
			float cellSize, availSize;
			if (IsHorizontal)
			{
				cellSize = _CellPrefabLayoutElement.preferredHeight;
				if (cellSize <= 0f)
				{
					cellSize = _CellPrefabLayoutElement.minHeight;
					if (cellSize <= 0f)
						throw new OSAException(
							typeof(GridParams) + ".cellPrefab.LayoutElement: Please specify at least preferredHeight or minHeight " +
							"when using a variable number of cells per group (Grid.MaxCellsPerGroup is " + Grid.MaxCellsPerGroup + ")"
						);
				}
				availSize = scrollViewSize.y - Grid.GroupPadding.vertical - ContentPadding.vertical;
			}
			else
			{
				cellSize = _CellPrefabLayoutElement.preferredWidth;
				if (cellSize <= 0f)
				{
					cellSize = _CellPrefabLayoutElement.minWidth;
					if (cellSize <= 0f)
						throw new OSAException(
							typeof(GridParams) + ".cellPrefab.LayoutElement: Please specify at least preferredWidth or minWidth " +
							"when using a variable number of cells per group (Grid.MaxCellsPerGroup is " + Grid.MaxCellsPerGroup + ")"
						);
				}
				availSize = scrollViewSize.x - Grid.GroupPadding.horizontal - ContentPadding.horizontal;
			}

			int numCellsPerGroupToUse = Mathf.FloorToInt((availSize + Grid.SpacingInGroup) / (cellSize + Grid.SpacingInGroup));
			if (numCellsPerGroupToUse < 1)
				numCellsPerGroupToUse = 1;

			return numCellsPerGroupToUse * -Grid.MaxCellsPerGroup;
		}

		/// <summary>Returns the prefab to use as LayoutGroup for the group with index <paramref name="forGroupAtThisIndex"/></summary>
		public virtual LayoutGroup GetGroupPrefab(int forGroupAtThisIndex)
		{
			if (_TheOnlyGroupPrefab == null)
				throw new OSAException("GridParams.InitIfNeeded() was not called by OSA. Did you forget to call base.Start() in <YourAdapter>.Start()?");

			return _TheOnlyGroupPrefab;
		}

		public virtual int GetGroupIndex(int cellIndex) { return cellIndex / _CurrentUsedNumCellsPerGroup; }

		public virtual int GetNumberOfRequiredGroups(int numberOfCells) { return numberOfCells == 0 ? 0 : GetGroupIndex(numberOfCells - 1) + 1; }

		protected void CreateOrReinitCellGroupPrefab()
		{
			if (!_TheOnlyGroupPrefab)
			{
				var go = CreateCellGroupPrefabGameObject();

				// Additional reminder of the "add recttransform in awake" bug explained in InitIfNeeded()
				if (!(go.transform is RectTransform))
					Debug.LogException(new OSAException("Don't call OSA.Init() outside MonoBehaviour.Start()!"));

				// TODO also integrate the new SetViewsHolderEnabled functionality here, for grids
				go.SetActive(false);
				go.transform.SetParent(ScrollViewRT, false);
				_TheOnlyGroupPrefab = AddLayoutGroupToCellGroupPrefab(go);
				_TheOnlyGroupPrefabAsHorizontalOrVertical = _TheOnlyGroupPrefab as HorizontalOrVerticalLayoutGroup;
			}
			InitOrReinitCellGroupPrefabLayoutGroup(_TheOnlyGroupPrefab);
		}

		protected virtual GameObject CreateCellGroupPrefabGameObject()
		{
			var go = new GameObject(ScrollViewRT.name + "_CellGroupPrefab", typeof(RectTransform));
			return go;
		}

		protected virtual LayoutGroup AddLayoutGroupToCellGroupPrefab(GameObject cellGroupGameObject)
		{
			if (IsHorizontal)
				return cellGroupGameObject.AddComponent<VerticalLayoutGroup>(); // groups are columns in a horizontal scrollview
			else
				return cellGroupGameObject.AddComponent<HorizontalLayoutGroup>(); // groups are rows in a vertical scrollview
		}

		protected virtual void InitOrReinitCellGroupPrefabLayoutGroup(LayoutGroup cellGroupGameObject)
		{
			if (_TheOnlyGroupPrefabAsHorizontalOrVertical)
			{
				_TheOnlyGroupPrefabAsHorizontalOrVertical.spacing = Grid.SpacingInGroup;
				_TheOnlyGroupPrefabAsHorizontalOrVertical.childForceExpandWidth = Grid.CellWidthForceExpandInGroup;
				_TheOnlyGroupPrefabAsHorizontalOrVertical.childForceExpandHeight = Grid.CellHeightForceExpandInGroup;
			}

			_TheOnlyGroupPrefab.childAlignment = Grid.AlignmentOfCellsInGroup;
			_TheOnlyGroupPrefab.padding = Grid.GroupPadding;
		}


		[Serializable]
		public class GridConfig
		{
			/// <summary>
			/// The max. number of cells in a row group (for vertical ScrollView) or column group (for horizontal ScrollView). 
			/// Set to -1 to fill with cells when there's space - note that it only works when cell's flexibleWidth is not used (flexibleHeight for horizontal scroll views)
			/// </summary>
			[SerializeField]
			[FormerlySerializedAs("numCellsPerGroup")] // in pre v4.0
			[Tooltip("The max. number of cells in a row group (for vertical ScrollView) or column group (for horizontal ScrollView).\n" +
					 "Set to -1 to fill with cells when there's space - note that it only works when cell's flexibleWidth is not used (flexibleHeight for horizontal scroll views).\n\n" +
					 "Set further to negative values to force more items than the space allows. For example, -2 sets 2x more items than the space allows. " +
					 "This may be useful for advanced scenarios where you use a custom LayoutGroup for the cell group")]
			int _MaxCellsPerGroup = -1;
			public int MaxCellsPerGroup { get { return _MaxCellsPerGroup; } set { _MaxCellsPerGroup = value; } }

			/// <summary> 
			/// If true, the <see cref="BaseParams.DefaultItemSize"/> property will be used for the height of a row (the width of a column, 
			/// for horizotal orientation). Leave to false to automatically infer based on the cellPrefab's LayoutElement's values.
			/// </summary>
			[SerializeField]
			[Tooltip("If true, the DefaultItemSize property will be used for the height of a row, if using a vertical scroll view (the width of a column, otherwise). " +
					 "Leave to false to automatically infer based on the cellPrefab's LayoutElement's values")]
			[FormerlySerializedAs("_UseDefaulfItemSizeForCellGroupSize")] // correction
			bool _UseDefaultItemSizeForCellGroupSize = false;
			public bool UseDefaultItemSizeForCellGroupSize { get { return _UseDefaultItemSizeForCellGroupSize; } protected set { _UseDefaultItemSizeForCellGroupSize = value; } }

			[FormerlySerializedAs("cellPrefab")]
			[SerializeField]
			RectTransform _CellPrefab = null;
			[Obsolete("Use CellPrefab instead", true)]
			public RectTransform cellPrefab { get { return CellPrefab; } set { CellPrefab = value; } }
			/// <summary>The prefab to use for each cell</summary>
			public RectTransform CellPrefab { get { return _CellPrefab; } set { _CellPrefab = value; } }

			[Tooltip("The alignment of cells inside their parent LayoutGroup (Vertical or Horizontal, depending on ScrollView's orientation)")]
			[FormerlySerializedAs("alignmentOfCellsInGroup")]
			[SerializeField]
			TextAnchor _AlignmentOfCellsInGroup = TextAnchor.UpperLeft;
			[Obsolete("Use AlignmentOfCellsInGroup instead", true)]
			public TextAnchor alignmentOfCellsInGroup { get { return AlignmentOfCellsInGroup; } set { AlignmentOfCellsInGroup = value; } }
			/// <summary>The alignment of cells inside their parent LayoutGroup (Vertical or Horizontal, depending on ScrollView's orientation)</summary>
			public TextAnchor AlignmentOfCellsInGroup { get { return _AlignmentOfCellsInGroup; } set { _AlignmentOfCellsInGroup = value; } }

			[Tooltip("The spacing between the cells of a group. Leave it to -1 to use the same value as contentSpacing " +
					 "(i.e. the spacing between the groups), so vertical and horizontal spacing will be the same")]
			[FormerlySerializedAs("spacingInGroup")]
			[SerializeField]
			float _SpacingInGroup = -1f;
			[Obsolete("Use SpacingInGroup instead", true)]
			public float spacingInGroup { get { return SpacingInGroup; } set { SpacingInGroup = value; } }
			public float SpacingInGroup { get { return _SpacingInGroup; } set { _SpacingInGroup = value; } }

			[FormerlySerializedAs("groupPadding")]
			[SerializeField]
			RectOffset _GroupPadding = new RectOffset();
			[Obsolete("Use GroupPadding instead", true)]
			public RectOffset groupPadding { get { return GroupPadding; } set { GroupPadding = value; } }
			/// <summary>The padding of cells as a whole inside their parent LayoutGroup</summary>
			public RectOffset GroupPadding { get { return _GroupPadding; } set { _GroupPadding = value; } }

			[FormerlySerializedAs("cellWidthForceExpandInGroup")]
			[SerializeField]
			bool _CellWidthForceExpandInGroup = false;
			[Obsolete("Use CellWidthForceExpandInGroup instead", true)]
			public bool cellWidthForceExpandInGroup { get { return CellWidthForceExpandInGroup; } set { CellWidthForceExpandInGroup = value; } }
			/// <summary>Wether to force the cells to expand in width inside their parent LayoutGroup</summary>
			public bool CellWidthForceExpandInGroup { get { return _CellWidthForceExpandInGroup; } set { _CellWidthForceExpandInGroup = value; } }

			[FormerlySerializedAs("cellHeightForceExpandInGroup")]
			[SerializeField]
			bool _CellHeightForceExpandInGroup = false;
			[Obsolete("Use CellHeightForceExpandInGroup instead", true)]
			public bool cellHeightForceExpandInGroup { get { return CellHeightForceExpandInGroup; } set { CellHeightForceExpandInGroup = value; } }
			/// <summary>Wether to force the cells to expand in height inside their parent LayoutGroup</summary>
			public bool CellHeightForceExpandInGroup { get { return _CellHeightForceExpandInGroup; } set { _CellHeightForceExpandInGroup = value; } }
		}
	}
}