using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.TheFallenGames.OSA.Util
{
	/*
		License type: MIT
		Quoted from github: "A short and simple permissive license with conditions 
		only requiring preservation of copyright and license notices. Licensed works, modifications, 
		and larger works may be distributed under different terms and without source code"
	*/

	/*
		Copyright (c) 2011, 2012, 2013, 2014, 2015, 2016 Jake Gordon and contributors
		Permission is hereby granted, free of charge, to any person obtaining a copy
		of this software and associated documentation files (the "Software"), to deal
		in the Software without restriction, including without limitation the rights
		to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
		copies of the Software, and to permit persons to whom the Software is
		furnished to do so, subject to the following conditions:

		The above copyright notice and this permission notice shall be included in all
		copies or substantial portions of the Software.

		THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
		IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
		FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
		AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
		LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
		OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
		SOFTWARE.
	*/

	/// <summary>
	/// Heavily modified version of https://github.com/cariquitanmac/2D-Bin-Pack-Binary-Search,
	/// which is a C# implementation of Jakes Gordon Binary Tree Algorithm for 2D Bin Packing https://github.com/jakesgordon/bin-packing/
	/// <para>All rights go to the original author.</para>
	/// </summary>
	public class Packer2DBox
	{
		List<Box> _Boxes;
		Node _RootNode;
		double _Spacing;
		bool _AlternatingStrategyBiggerRightNode;
		bool _AlternatingOtherStrategyBiggerRightNode;
		double _ContainerWidth;
		double _ContainerHeight;
		NodeChoosingStrategy _ChoosingStrategy;


		public Packer2DBox(double containerWidth, double containerHeight, double spacing)
		{
			this._ContainerWidth = containerWidth;
			this._ContainerHeight = containerHeight;
			this._Spacing = spacing;
		}


		public void Pack(List<Box> boxes, bool sort, NodeChoosingStrategy choosingStrategy, out double totalWidth, out double totalHeight)
		{
			_ChoosingStrategy = choosingStrategy;

			_AlternatingStrategyBiggerRightNode = _ChoosingStrategy == NodeChoosingStrategy.ALTERNATING_START_WITH_RIGHT;

			_RootNode = new Node(0d, 0d) { height = _ContainerHeight, width = _ContainerWidth };
			_Boxes = boxes;
			if (sort)
			{
				// Biggest boxes first with maxside, then secondarily by volume 
				// More info: https://codeincomplete.com/posts/bin-packing/
				_Boxes.Sort((a, b) =>
				{
					var aMax = System.Math.Max(a.width, a.height);
					var bMax = System.Math.Max(b.width, b.height);

					if (aMax != bMax)
						return (int)(bMax - aMax);

					return (int)(b.volume - a.volume);
				});
				//_Boxes = _Boxes.Sort((a, b) =>
				//{
				//	var aMax = Math.Max(a.width, a.height);
				//	var bMax = Math.Max(b.width, b.height);

				//	if (aMax != bMax)
				//		return (int)(bMax - aMax);

				//	return (int)(b.volume - a.volume);
				//});
				//_Boxes = _Boxes.OrderByDescending(x => Math.Max(x.width, x.height)).ToList();
				////_Boxes = _Boxes.OrderByDescending(x => x.volume).ToList();
			}

			totalWidth = 0f;
			totalHeight = 0f;
			foreach (var box in _Boxes)
			{
				Node node = null;
				FindNode(_RootNode, box.width, box.height, ref node);

				if (node != null)
				{
					// Split rectangles
					box.position = SplitNode(node, box.width, box.height);

					double width = box.position.x + box.width;
					if (width > totalWidth)
						totalWidth = width;

					double height = box.position.y + box.height;
					if (height > totalHeight)
						totalHeight = height;
				}
			}
		}

		void FindNode(Node rootNode, double boxWidth, double boxHeight, ref Node node)
		{
			if (rootNode.isOccupied)
			{
				FindNode(rootNode.rightNode, boxWidth, boxHeight, ref node);
				FindNode(rootNode.bottomNode, boxWidth, boxHeight, ref node);
			}
			else if (boxWidth <= rootNode.width && boxHeight <= rootNode.height)
			{
				 if (node == null || rootNode.distFromOrigin < node.distFromOrigin)
					node = rootNode;
			}
		}

		Node SplitNode(Node node, double boxWidth, double boxHeight)
		{
			node.isOccupied = true;

			double rightNodeFullWidth = node.width - (boxWidth + _Spacing);
			double rightNodeFullHeight = node.height;

			double bottomNodeFullWidth = node.width;
			double bottomNodeFullHeight = node.height - (boxHeight + _Spacing);

			bool biggerRightNode;

			var localStrategy = _ChoosingStrategy;
			if (localStrategy == NodeChoosingStrategy.MAX_VOLUME)
			{
				double rightVolume = rightNodeFullWidth * rightNodeFullHeight;
				double bottomVolume = bottomNodeFullWidth * bottomNodeFullHeight;

				if (rightVolume == bottomVolume)
				{
					// In case of equality, alternate between what we chose the last time
					biggerRightNode = _AlternatingOtherStrategyBiggerRightNode;
					_AlternatingOtherStrategyBiggerRightNode = !_AlternatingOtherStrategyBiggerRightNode;
				}
				else
				{
					biggerRightNode = rightVolume > bottomVolume;
				}
			}
			else if (localStrategy == NodeChoosingStrategy.MAX_SIDE)
			{
				double rightMaxSide = Math.Max(rightNodeFullWidth, rightNodeFullHeight);
				double bottomMaxSide = Math.Max(bottomNodeFullWidth, bottomNodeFullHeight);

				if (rightMaxSide == bottomMaxSide)
				{
					// In case of equality, alternate between what we chose the last time
					biggerRightNode = _AlternatingOtherStrategyBiggerRightNode;
					_AlternatingOtherStrategyBiggerRightNode = !_AlternatingOtherStrategyBiggerRightNode;
				}
				else
				{
					biggerRightNode = rightMaxSide > bottomMaxSide;
				}
			}
			else if (localStrategy == NodeChoosingStrategy.MAX_SIDE)
			{
				double rightMaxSide = Math.Max(rightNodeFullWidth, rightNodeFullHeight);
				double bottomMaxSide = Math.Max(bottomNodeFullWidth, bottomNodeFullHeight);

				if (rightMaxSide == bottomMaxSide)
				{
					// In case of equality, alternate between what we chose the last time
					biggerRightNode = _AlternatingOtherStrategyBiggerRightNode;
					_AlternatingOtherStrategyBiggerRightNode = !_AlternatingOtherStrategyBiggerRightNode;
				}
				else
				{
					biggerRightNode = rightMaxSide > bottomMaxSide;
				}
			}
			else if (localStrategy == NodeChoosingStrategy.MAX_SUM)
			{
				double rightSum = rightNodeFullWidth + rightNodeFullHeight;
				double bottomSum = bottomNodeFullWidth + bottomNodeFullHeight;

				if (rightSum == bottomSum)
				{
					// In case of equality, alternate between what we chose the last time
					biggerRightNode = _AlternatingOtherStrategyBiggerRightNode;
					_AlternatingOtherStrategyBiggerRightNode = !_AlternatingOtherStrategyBiggerRightNode;
				}
				else
				{
					biggerRightNode = rightSum > bottomSum;
				}
			}
			else
			{
				if (_ChoosingStrategy == NodeChoosingStrategy.RIGHT)
					biggerRightNode = true;
				else if (_ChoosingStrategy == NodeChoosingStrategy.BOTTOM)
					biggerRightNode = false;
				else
				{
					// Alternating
					biggerRightNode = _AlternatingStrategyBiggerRightNode;
					_AlternatingStrategyBiggerRightNode = !_AlternatingStrategyBiggerRightNode;
				}
			}

			node.rightNode = new Node(node.x + (boxWidth + _Spacing), node.y)
			{
				depth = node.depth + 1,
				width = rightNodeFullWidth,
				height = biggerRightNode ? node.height : boxHeight
			};
			node.bottomNode = new Node(node.x, node.y + (boxHeight + _Spacing))
			{
				depth = node.depth + 1,
				width = biggerRightNode ? boxWidth : node.width,
				height = bottomNodeFullHeight
			};

			return node;
		}


		public class Node
		{
			public int depth;
			public Node rightNode;
			public Node bottomNode;
			public double x;
			public double y;
			public double width;
			public double height;
			readonly public double distFromOrigin;
			public bool isOccupied;


			public Node(double x, double y)
			{
				this.x = x;
				this.y = y;
				distFromOrigin = Math.Sqrt(x * x + y * y);
			}
		}


		public class Box
		{
			public double height;
			public double width;
			public double volume;
			public Node position;


			public Box(double width, double height)
			{
				this.width = width;
				this.height = height;
				volume = width * height;
			}
		}


		/// <summary>Note expanding choices, in order of success rate</summary>
		public enum NodeChoosingStrategy
		{
			MAX_VOLUME,
			MAX_SUM,
			MAX_SIDE,
			ALTERNATING_START_WITH_BOTTOM,
			RIGHT,
			BOTTOM,
			ALTERNATING_START_WITH_RIGHT, // same as BOTTOM, in 99% of cases
			COUNT_
		}
	}
}