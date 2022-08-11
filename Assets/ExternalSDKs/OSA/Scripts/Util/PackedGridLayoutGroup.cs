using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.TheFallenGames.OSA.Util
{
	[AddComponentMenu("Layout/Packed Grid Layout Group", 153)]
	/// <summary>
	///   Layout class to arrange children elements in a circular grid format. Circular in the . Items will try to occupy as much space as possible.
	/// </summary>
	public class PackedGridLayoutGroup : LayoutGroup
	{
		[SerializeField] protected float m_ForcedSpacing = 0f;

		[Tooltip("The specified axis will have the 'Preferred' size set based on children")]
		[SerializeField] protected AxisOrNone m_childrenControlSize = AxisOrNone.Vertical;

		[Tooltip("If true, the layout will start with bigger children. The starting position is defined by the 'Child Alignment' property")]
		[SerializeField] protected bool m_biggerChildrenFirst = true;

		[Tooltip("Set to as many as possible, if the FPS allows")]
		[Range(1, (int)Packer2DBox.NodeChoosingStrategy.COUNT_)]
		[SerializeField] protected int m_numPasses = (int)Packer2DBox.NodeChoosingStrategy.COUNT_;

		/// <summary>
		/// The spacing to use between layout elements in the grid on both axes. 
		/// The spacing is created by shrinking the childrens' sizes rather than actually adding spaces.
		/// If you want true spacing, consider modifying the children themselves to also include some padding inside them
		/// </summary>
		public float ForcedSpacing { get { return m_ForcedSpacing; } set { SetProperty(ref m_ForcedSpacing, value); } }

		/// <summary>
		/// The specified axis will have the 'Preferred' size set based on children
		/// </summary>
		public AxisOrNone ChildrenControlSize { get { return m_childrenControlSize; } set { SetProperty(ref m_childrenControlSize, value); } }

		/// <summary>
		/// If true, the layout will start with bigger children. The starting position is defined by the 'Child Alignment' property
		/// </summary>
		public bool BiggerChildrenFirst { get { return m_biggerChildrenFirst; } set { SetProperty(ref m_biggerChildrenFirst, value); } }

		/// <summary>
		/// <para>This refers to the number of different strategies to use when packing children. Set to as many as possible, if the FPS allows.
		/// See <see cref="Packer2DBox.NodeChoosingStrategy"/>.</para>
		/// <para> At the moment (15 Mar 2019), more than 1 pass is executing only if the boxes don't all fit in the available 
		/// space, as the first strategy (<see cref="Packer2DBox.NodeChoosingStrategy.MAX_VOLUME"/>) seems to always perform the best</para>
		/// </summary>
		public int NumPasses { get { return m_numPasses; } set { SetProperty(ref m_numPasses, value); } }


		protected PackedGridLayoutGroup()
		{ }


#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
		}
#endif
		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();

			float minWidthToSet;
			float preferredWidthToSet;
			if (m_childrenControlSize == AxisOrNone.Horizontal)
			{
				float width, _;
				GetChildSetups(out width, out _);

				minWidthToSet = preferredWidthToSet = width + padding.horizontal;
			}
			else
			{
				minWidthToSet = minWidth;
				preferredWidthToSet = preferredWidth;
			}

			SetLayoutInputForAxis(minWidthToSet, preferredWidthToSet, -1, 0);
		}

		public override void CalculateLayoutInputVertical()
		{
			float minHeightToSet;
			float preferredHeightToSet;
			if (m_childrenControlSize == AxisOrNone.Vertical)
			{
				float _, height;
				GetChildSetups(out _, out height);

				minHeightToSet = preferredHeightToSet = height + padding.vertical;
			}
			else
			{
				minHeightToSet = minHeight;
				preferredHeightToSet = preferredHeight;
			}

			SetLayoutInputForAxis(minHeightToSet, preferredHeightToSet, -1, 1);
		}

		public override void SetLayoutHorizontal()
		{
			LayoutChildren(true, false);
		}

		public override void SetLayoutVertical()
		{
			LayoutChildren(false, true);
		}
		
		void GetInsetAndEdges(float chWidth, float chHeight, out RectTransform.Edge xInsetEdge, out RectTransform.Edge yInsetEdge, out float xAddInset, out float yAddInset)
		{
			var gridRect = rectTransform.rect;

			xInsetEdge = RectTransform.Edge.Left;
			yInsetEdge = RectTransform.Edge.Top;
			xAddInset = 0f;
			yAddInset = 0f;
			if (m_childrenControlSize != AxisOrNone.Horizontal)
			{
				switch (childAlignment)
				{
					case TextAnchor.UpperLeft:
					case TextAnchor.LowerLeft:
					case TextAnchor.MiddleLeft:
						xInsetEdge = RectTransform.Edge.Left;
						xAddInset = padding.left;
						break;

					case TextAnchor.UpperRight:
					case TextAnchor.LowerRight:
					case TextAnchor.MiddleRight:
						xInsetEdge = RectTransform.Edge.Right;
						xAddInset = padding.right;
						break;

					case TextAnchor.UpperCenter:
					case TextAnchor.MiddleCenter:
					case TextAnchor.LowerCenter:
						xAddInset = Mathf.Max((gridRect.width - chWidth), padding.horizontal) / 2f;
						break;
				}
			}

			if (m_childrenControlSize != AxisOrNone.Vertical)
			{
				switch (childAlignment)
				{
					case TextAnchor.UpperLeft:
					case TextAnchor.UpperCenter:
					case TextAnchor.UpperRight:
						yInsetEdge = RectTransform.Edge.Top;
						yAddInset = padding.top;
						break;

					case TextAnchor.LowerLeft:
					case TextAnchor.LowerCenter:
					case TextAnchor.LowerRight:
						yInsetEdge = RectTransform.Edge.Bottom;
						yAddInset = padding.bottom;
						break;

					case TextAnchor.MiddleLeft:
					case TextAnchor.MiddleCenter:
					case TextAnchor.MiddleRight:
						yAddInset = Mathf.Max((gridRect.height - chHeight), padding.vertical) / 2f;
						break;
				}
			}
		}

		void LayoutChildren(bool hor, bool vert)
		{
			float chWidth, chHeight;
			var setups = GetChildSetups(out chWidth, out chHeight);

			RectTransform.Edge xInsetEdge, yInsetEdge;
			float xAddInset, yAddInset;
			GetInsetAndEdges(chWidth, chHeight, out xInsetEdge, out yInsetEdge, out xAddInset, out yAddInset);

			foreach (var child in setups)
			{
				var c = child as ChildSetup;
				if (c.box.position == null)
					continue;

				if (hor)
					c.child.SetInsetAndSizeFromParentEdge(xInsetEdge, (float)c.box.position.x + xAddInset, (float)c.box.width - ForcedSpacing);

				if (vert)
					c.child.SetInsetAndSizeFromParentEdge(yInsetEdge, (float)c.box.position.y + yAddInset, (float)c.box.height - ForcedSpacing);
			}
		}

		List<ChildSetup> GetChildSetups(out float width, out float height)
		{
			var list = new List<ChildSetup>(rectChildren.Count);
			foreach (var child in rectChildren)
			{
				float chWidth = LayoutUtility.GetPreferredSize(child, 0);
				float chHeight = LayoutUtility.GetPreferredSize(child, 1);
				list.Add(new ChildSetup(chWidth, chHeight, child));
			}

			if (BiggerChildrenFirst)
			{
				// Biggest boxes first with maxside, then secondarily by volume 
				// More info: https://codeincomplete.com/posts/bin-packing/
				list.Sort((a, b) =>
				{
					var aMax = System.Math.Max(a.box.width, a.box.height);
					var bMax = System.Math.Max(b.box.width, b.box.height);

					if (aMax != bMax)
						return (int)(bMax - aMax);

					return (int)(b.box.volume - a.box.volume);
				});
			}

			float availableWidth, availableHeight;
			if (m_childrenControlSize == AxisOrNone.Horizontal)
			{
				availableWidth = float.MaxValue;
				availableHeight = rectTransform.rect.height - padding.vertical;
			}
			else if (m_childrenControlSize == AxisOrNone.Vertical)
			{
				availableWidth = rectTransform.rect.width - padding.horizontal;
				availableHeight = float.MaxValue;
			}
			else
			{
				availableHeight = rectTransform.rect.height - padding.vertical;
				availableWidth = rectTransform.rect.width - padding.horizontal;
			}


			// Spacing usually creates mode big empty spaces where no item can fit
			float spacingToUse = 0f;
			var packer = new Packer2DBox(availableWidth, availableHeight, spacingToUse);

			int maxStrategiesToUse = m_numPasses;
			List<Packer2DBox.Box>[] boxesPerStrategy = new List<Packer2DBox.Box>[maxStrategiesToUse];
			int[] nullPositionsPerStrategy = new int[maxStrategiesToUse];
			double[] totalWidthsPerStrategy = new double[maxStrategiesToUse];
			double[] totalHeightsPerStrategy = new double[maxStrategiesToUse];

			int iBest = -1;
			bool copyBoxesForFinalResult = maxStrategiesToUse > 1;
			for (int i = 0; i < maxStrategiesToUse; i++)
			{
				var boxesThisPass = i == 0 ? list.ConvertAll(c => c.box) : list.ConvertAll(c => { c.ReinitBox(); return c.box; });
				double totalWidthThisPass;
				double totalHeightThisPass;
				packer.Pack(boxesThisPass, false, (Packer2DBox.NodeChoosingStrategy)i, out totalWidthThisPass, out totalHeightThisPass);
				int thisPassNullPositions = 0;
				for (int j = 0; j < boxesThisPass.Count; j++)
				{
					var b = boxesThisPass[j];
					if (b.position == null)
						++thisPassNullPositions;
				}

				boxesPerStrategy[i] = boxesThisPass;
				nullPositionsPerStrategy[i] = thisPassNullPositions;
				totalWidthsPerStrategy[i] = totalWidthThisPass;
				totalHeightsPerStrategy[i] = totalHeightThisPass;

				if (iBest == -1)
				{
					iBest = i;

					if (thisPassNullPositions == 0)
					{
						// Boxes won't be overridden by next strategies, so no need to copy them
						copyBoxesForFinalResult = false;

						// First pass is Packer2DBox.NodeChoosingStrategy.MAX_VOLUME and all boxes were fit => there's no better strategy
						break;
					}

					continue;
				}

				if (thisPassNullPositions < nullPositionsPerStrategy[iBest])
				{
					iBest = i;
					continue;
				}

				if (thisPassNullPositions > nullPositionsPerStrategy[iBest])
					continue;

				if (totalWidthThisPass * totalHeightThisPass < totalWidthsPerStrategy[i] * totalHeightsPerStrategy[i])
				{
					iBest = i;
					continue;
				}
			}

			if (copyBoxesForFinalResult)
			{
				var bestBoxes = boxesPerStrategy[iBest];
				for (int i = 0; i < list.Count; i++)
					list[i].box = bestBoxes[i];
			}

			////Testing whether first strategy is always better when no nulls are found
			//if (nullPositionsPerStrategy[0] == 0 && iBest != 0)
			//	throw new System.Exception(nullPositionsPerStrategy[0] + ", " + (Packer2DBox.NodeChoosingStrategy)iBest + ", " + nullPositionsPerStrategy[iBest]);

			//Debug.Log("Strategy used: " +  (Packer2DBox.NodeChoosingStrategy)iBest);

			width = (float)totalWidthsPerStrategy[iBest];
			height = (float)totalHeightsPerStrategy[iBest];

			return list;
		}


		public enum AxisOrNone
		{
			Horizontal = RectTransform.Axis.Horizontal,
			Vertical = RectTransform.Axis.Vertical,
			None
		}


		class ChildSetup
		{
			public RectTransform child;
			public Packer2DBox.Box box;


			public ChildSetup(double width, double height, RectTransform child)
			{
				this.child = child;

				box = new Packer2DBox.Box(width, height);
			}


			public void ReinitBox() { box = new Packer2DBox.Box(box.width, box.height); }
		}
	}
}