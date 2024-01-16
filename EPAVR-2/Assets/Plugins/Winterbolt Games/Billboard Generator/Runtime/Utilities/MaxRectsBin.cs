using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WinterboltGames.BillboardGenerator.Runtime.Utilities
{
	public sealed class MaxRectsBin
	{
		public enum FreeRectChoiceHeuristic
		{
			/// <summary>
			/// BSSF: Positions the rectangle against the short side of a free rectangle into which it fits the best.
			/// </summary>
			RectBestShortSideFit,

			/// <summary>
			/// BLSF: Positions the rectangle against the long side of a free rectangle into which it fits the best.
			/// </summary>
			RectBestLongSideFit,

			/// <summary>
			/// BAF: Positions the rectangle into the smallest free rect into which it fits.
			/// </summary>
			RectBestAreaFit,

			/// <summary>
			/// BL: Does the Tetris placement.
			/// </summary>
			RectBottomLeftRule,

			/// <summary>
			/// CP: Chooses the placement where the rectangle touches other rects as much as possible.
			/// </summary>
			RectContactPointRule,
		}

		private readonly int _width;
		private readonly int _height;

		private readonly bool _allowRotations;

		private readonly List<Rect> _usedRectangles = new List<Rect>();
		private readonly List<Rect> _freeRectangles = new List<Rect>();

		public Vector2Int Extents => new Vector2Int((int)_usedRectangles.Max(rect => rect.xMax), (int)_usedRectangles.Max(rect => rect.yMax));

		public MaxRectsBin(int width, int height, bool allowRotations = true)
		{
			_width = width;
			_height = height;

			_allowRotations = allowRotations;

			Rect n = new Rect
			{
				x = 0.0f,
				y = 0.0f,

				width = width,
				height = height
			};

			_usedRectangles.Clear();

			_freeRectangles.Clear();

			_freeRectangles.Add(n);
		}

		public Rect Insert(int width, int height, FreeRectChoiceHeuristic method)
		{
			int score1 = 0; // Unused in this function. We don't need to know the score after finding the position.
			int score2 = 0;

			Rect newNode = method switch
			{
				FreeRectChoiceHeuristic.RectBestShortSideFit => FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2),

				FreeRectChoiceHeuristic.RectBottomLeftRule => FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2),

				FreeRectChoiceHeuristic.RectContactPointRule => FindPositionForNewNodeContactPoint(width, height, ref score1),

				FreeRectChoiceHeuristic.RectBestLongSideFit => FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1),

				FreeRectChoiceHeuristic.RectBestAreaFit => FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2),

				_ => throw new ArgumentOutOfRangeException(nameof(method)),
			};

			if (newNode.height == 0.0f)
			{
				return newNode;
			}

			int numRectanglesToProcess = _freeRectangles.Count;

			for (int i = 0; i < numRectanglesToProcess; ++i)
			{
				if (!SplitFreeNode(_freeRectangles[i], ref newNode)) continue;

				_freeRectangles.RemoveAt(i);

				--i;

				--numRectanglesToProcess;
			}

			PruneFreeList();

			_usedRectangles.Add(newNode);

			return newNode;
		}

		public void Insert(List<Rect> rects, List<Rect> dst, FreeRectChoiceHeuristic method)
		{
			dst.Clear();

			while (rects.Count > 0)
			{
				int bestScore1 = int.MaxValue;
				int bestScore2 = int.MaxValue;

				int bestRectIndex = -1;

				Rect bestNode = new Rect();

				for (int i = 0; i < rects.Count; ++i)
				{
					int score1 = 0;
					int score2 = 0;

					Rect newNode = ScoreRect((int)rects[i].width, (int)rects[i].height, method, ref score1, ref score2);

					if (score1 >= bestScore1 && (score1 != bestScore1 || score2 >= bestScore2)) continue;

					bestScore1 = score1;
					bestScore2 = score2;

					bestNode = newNode;

					bestRectIndex = i;
				}

				if (bestRectIndex == -1) return;

				PlaceRect(bestNode);

				rects.RemoveAt(bestRectIndex);
			}
		}

		private void PlaceRect(Rect node)
		{
			int numRectanglesToProcess = _freeRectangles.Count;

			for (int i = 0; i < numRectanglesToProcess; ++i)
			{
				if (!SplitFreeNode(_freeRectangles[i], ref node)) continue;

				_freeRectangles.RemoveAt(i);

				--i;

				--numRectanglesToProcess;
			}

			PruneFreeList();

			_usedRectangles.Add(node);
		}

		private Rect ScoreRect(int width, int height, FreeRectChoiceHeuristic method, ref int score1, ref int score2)
		{
			Rect newNode;

			score1 = int.MaxValue;
			score2 = int.MaxValue;

			switch (method)
			{
				case FreeRectChoiceHeuristic.RectBestShortSideFit:

					newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2);

					break;

				case FreeRectChoiceHeuristic.RectBottomLeftRule:

					newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2);

					break;

				case FreeRectChoiceHeuristic.RectContactPointRule:

					newNode = FindPositionForNewNodeContactPoint(width, height, ref score1);

					// Reverse since we are minimizing, but for contact point score bigger is better.
					score1 = -score1;

					break;

				case FreeRectChoiceHeuristic.RectBestLongSideFit:

					newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1);

					break;

				case FreeRectChoiceHeuristic.RectBestAreaFit:

					newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2);

					break;

				default:

					throw new ArgumentOutOfRangeException(nameof(method), method, null);
			}

			// Cannot fit the current rectangle.
			if (newNode.height != 0.0f) return newNode;

			score1 = int.MaxValue;
			score2 = int.MaxValue;

			return newNode;
		}

		private Rect FindPositionForNewNodeBottomLeft(int width, int height, ref int bestY, ref int bestX)
		{
			Rect bestNode = new Rect();

			bestY = int.MaxValue;

			foreach (Rect freeRect in _freeRectangles)
			{
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRect.width >= width && freeRect.height >= height)
				{
					int topSideY = (int)freeRect.y + height;

					if (topSideY < bestY || (topSideY == bestY && freeRect.x < bestX))
					{
						bestNode.x = freeRect.x;
						bestNode.y = freeRect.y;

						bestNode.width = width;
						bestNode.height = height;

						bestY = topSideY;

						bestX = (int)freeRect.x;
					}
				}

				if (_allowRotations && freeRect.width >= height && freeRect.height >= width)
				{
					int topSideY = (int)freeRect.y + width;

					if (topSideY >= bestY && (topSideY != bestY || !(freeRect.x < bestX))) continue;

					bestNode.x = freeRect.x;
					bestNode.y = freeRect.y;

					bestNode.width = height;
					bestNode.height = width;

					bestY = topSideY;

					bestX = (int)freeRect.x;
				}
			}

			return bestNode;
		}

		private Rect FindPositionForNewNodeBestShortSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
		{
			Rect bestNode = new Rect();

			bestShortSideFit = int.MaxValue;

			foreach (Rect freeRect in _freeRectangles)
			{
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRect.width >= width && freeRect.height >= height)
				{
					int leftoverHoriz = Mathf.Abs((int)freeRect.width - width);
					int leftoverVert = Mathf.Abs((int)freeRect.height - height);

					int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
					int longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

					if (shortSideFit < bestShortSideFit || (shortSideFit == bestShortSideFit && longSideFit < bestLongSideFit))
					{
						bestNode.x = freeRect.x;
						bestNode.y = freeRect.y;

						bestNode.width = width;
						bestNode.height = height;

						bestShortSideFit = shortSideFit;
						bestLongSideFit = longSideFit;
					}
				}

				if (_allowRotations && freeRect.width >= height && freeRect.height >= width)
				{
					int flippedLeftoverHoriz = Mathf.Abs((int)freeRect.width - height);
					int flippedLeftoverVert = Mathf.Abs((int)freeRect.height - width);

					int flippedShortSideFit = Mathf.Min(flippedLeftoverHoriz, flippedLeftoverVert);
					int flippedLongSideFit = Mathf.Max(flippedLeftoverHoriz, flippedLeftoverVert);

					if (flippedShortSideFit >= bestShortSideFit && (flippedShortSideFit != bestShortSideFit || flippedLongSideFit >= bestLongSideFit)) continue;

					bestNode.x = freeRect.x;
					bestNode.y = freeRect.y;

					bestNode.width = height;
					bestNode.height = width;

					bestShortSideFit = flippedShortSideFit;
					bestLongSideFit = flippedLongSideFit;
				}
			}

			return bestNode;
		}

		private Rect FindPositionForNewNodeBestLongSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
		{
			Rect bestNode = new Rect();

			bestLongSideFit = int.MaxValue;

			foreach (Rect freeRect in _freeRectangles)
			{
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRect.width >= width && freeRect.height >= height)
				{
					int leftoverHoriz = Mathf.Abs((int)freeRect.width - width);
					int leftoverVert = Mathf.Abs((int)freeRect.height - height);

					int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
					int longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

					if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
					{
						bestNode.x = freeRect.x;
						bestNode.y = freeRect.y;

						bestNode.width = width;
						bestNode.height = height;

						bestShortSideFit = shortSideFit;
						bestLongSideFit = longSideFit;
					}
				}

				if (_allowRotations && freeRect.width >= height && freeRect.height >= width)
				{
					int leftoverHoriz = Mathf.Abs((int)freeRect.width - height);
					int leftoverVert = Mathf.Abs((int)freeRect.height - width);

					int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
					int longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

					if (longSideFit >= bestLongSideFit && (longSideFit != bestLongSideFit || shortSideFit >= bestShortSideFit)) continue;

					bestNode.x = freeRect.x;
					bestNode.y = freeRect.y;

					bestNode.width = height;
					bestNode.height = width;

					bestShortSideFit = shortSideFit;
					bestLongSideFit = longSideFit;
				}
			}

			return bestNode;
		}

		private Rect FindPositionForNewNodeBestAreaFit(int width, int height, ref int bestAreaFit, ref int bestShortSideFit)
		{
			Rect bestNode = new Rect();

			bestAreaFit = int.MaxValue;

			foreach (Rect freeRect in _freeRectangles)
			{
				int areaFit = (int)freeRect.width * (int)freeRect.height - width * height;

				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRect.width >= width && freeRect.height >= height)
				{
					int leftoverHoriz = Mathf.Abs((int)freeRect.width - width);
					int leftoverVert = Mathf.Abs((int)freeRect.height - height);

					int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);

					if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
					{
						bestNode.x = freeRect.x;
						bestNode.y = freeRect.y;

						bestNode.width = width;
						bestNode.height = height;

						bestShortSideFit = shortSideFit;

						bestAreaFit = areaFit;
					}
				}

				if (_allowRotations && freeRect.width >= height && freeRect.height >= width)
				{
					int leftoverHoriz = Mathf.Abs((int)freeRect.width - height);
					int leftoverVert = Mathf.Abs((int)freeRect.height - width);

					int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);

					if (areaFit >= bestAreaFit && (areaFit != bestAreaFit || shortSideFit >= bestShortSideFit)) continue;

					bestNode.x = freeRect.x;
					bestNode.y = freeRect.y;

					bestNode.width = height;
					bestNode.height = width;

					bestShortSideFit = shortSideFit;

					bestAreaFit = areaFit;
				}
			}

			return bestNode;
		}

		/// <summary>
		/// Returns 0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.
		/// </summary>
		private static int CommonIntervalLength(int i1Start, int i1End, int i2Start, int i2End) => i1End < i2Start || i2End < i1Start ? 0 : Mathf.Min(i1End, i2End) - Mathf.Max(i1Start, i2Start);

		private int ContactPointScoreNode(int x, int y, int width, int height)
		{
			int score = 0;

			if (x == 0 || x + width == _width) score += height;

			if (y == 0 || y + height == _height) score += width;

			foreach (Rect usedRect in _usedRectangles)
			{
				if (usedRect.x == x + width || usedRect.x + usedRect.width == x) score += CommonIntervalLength((int)usedRect.y, (int)usedRect.y + (int)usedRect.height, y, y + height);

				if (usedRect.y == y + height || usedRect.y + usedRect.height == y) score += CommonIntervalLength((int)usedRect.x, (int)usedRect.x + (int)usedRect.width, x, x + width);
			}

			return score;
		}

		private Rect FindPositionForNewNodeContactPoint(int width, int height, ref int bestContactScore)
		{
			Rect bestNode = new Rect();

			bestContactScore = -1;

			foreach (Rect freeRect in _freeRectangles)
			{
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRect.width >= width && freeRect.height >= height)
				{
					int score = ContactPointScoreNode((int)freeRect.x, (int)freeRect.y, width, height);

					if (score > bestContactScore)
					{
						bestNode.x = (int)freeRect.x;
						bestNode.y = (int)freeRect.y;

						bestNode.width = width;
						bestNode.height = height;

						bestContactScore = score;
					}
				}

				if (_allowRotations && freeRect.width >= height && freeRect.height >= width)
				{
					int score = ContactPointScoreNode((int)freeRect.x, (int)freeRect.y, height, width);

					if (score <= bestContactScore) continue;

					bestNode.x = (int)freeRect.x;
					bestNode.y = (int)freeRect.y;

					bestNode.width = height;
					bestNode.height = width;

					bestContactScore = score;
				}
			}

			return bestNode;
		}

		private bool SplitFreeNode(Rect freeNode, ref Rect usedNode)
		{
			// Test with SAT if the rectangles even intersect.
			if (usedNode.x >= freeNode.x + freeNode.width || usedNode.x + usedNode.width <= freeNode.x || usedNode.y >= freeNode.y + freeNode.height || usedNode.y + usedNode.height <= freeNode.y) return false;

			if (usedNode.x < freeNode.x + freeNode.width && usedNode.x + usedNode.width > freeNode.x)
			{
				// New node at the top side of the used node.
				if (usedNode.y > freeNode.y && usedNode.y < freeNode.y + freeNode.height)
				{
					Rect newNode = freeNode;

					newNode.height = usedNode.y - newNode.y;

					_freeRectangles.Add(newNode);
				}

				// New node at the bottom side of the used node.
				if (usedNode.y + usedNode.height < freeNode.y + freeNode.height)
				{
					Rect newNode = freeNode;

					newNode.y = usedNode.y + usedNode.height;

					newNode.height = freeNode.y + freeNode.height - (usedNode.y + usedNode.height);

					_freeRectangles.Add(newNode);
				}
			}

			if (usedNode.y < freeNode.y + freeNode.height && usedNode.y + usedNode.height > freeNode.y)
			{
				// New node at the left side of the used node.
				if (usedNode.x > freeNode.x && usedNode.x < freeNode.x + freeNode.width)
				{
					Rect newNode = freeNode;

					newNode.width = usedNode.x - newNode.x;

					_freeRectangles.Add(newNode);
				}

				// New node at the right side of the used node.
				if (usedNode.x + usedNode.width < freeNode.x + freeNode.width)
				{
					Rect newNode = freeNode;

					newNode.x = usedNode.x + usedNode.width;

					newNode.width = freeNode.x + freeNode.width - (usedNode.x + usedNode.width);

					_freeRectangles.Add(newNode);
				}
			}

			return true;
		}

		private void PruneFreeList()
		{
			for (int i = 0; i < _freeRectangles.Count; ++i)
			{
				for (int j = i + 1; j < _freeRectangles.Count; ++j)
				{
					if (IsContainedIn(_freeRectangles[i], _freeRectangles[j]))
					{
						_freeRectangles.RemoveAt(i);

						--i;

						break;
					}

					if (!IsContainedIn(_freeRectangles[j], _freeRectangles[i])) continue;

					_freeRectangles.RemoveAt(j);

					--j;
				}
			}
		}

		// TODO: Replace with Rect.Contains
		private static bool IsContainedIn(Rect a, Rect b)
		{
			return a.x >= b.x && a.y >= b.y && a.x + a.width <= b.x + b.width && a.y + a.height <= b.y + b.height;
		}
	}
}
