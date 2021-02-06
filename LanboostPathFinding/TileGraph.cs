using Lanboost.PathFinding.GraphBuilders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lanboost.PathFinding.Graph
{
	using Position = Tuple<int, int>;
	using Edge = Tuple<Tuple<int, int>, Tuple<int, int>>;
	public class TileGraph : IGraph<Position, Edge>
	{
		bool[][] grid;

		public TileGraph(bool[][] grid)
		{
			this.grid = grid;
		}

		public void AddTemporaryStartEndNodes(Position start, Position end)
		{

		}

		public int GetCost(Tuple<Position, Position> link)
		{
			return 1;
		}

		public IEnumerable<Tuple<Position, Position>> GetEdges(Position node)
		{
			var dirs = new int[][] {
				new int[] { 0, -1 },
				new int[] { 1, 0 },
				new int[] { 0, 1 },
				new int[] { -1, 0 }
			};
			foreach(var d in dirs)
			{
				var x = node.Item1 + d[0];
				var y = node.Item2 + d[1];
				if(x >= 0 && y >= 0 && y < grid.Length && x < grid[y].Length)
				{
					if (grid[y][x])
					{
						yield return new Edge(node, new Position(x, y));
					}
				}
			}
		}

		public int GetEstimation(Position from, Position to)
		{
			return Math.Abs(from.Item1 - to.Item1) + Math.Abs(from.Item2 - to.Item2);
		}

		public Position GetOtherNode(Position From, Tuple<Position, Position> link)
		{
			if (link.Item1.Equals(From))
			{
				return link.Item2;
			}
			return link.Item1;
		}
	}

	public class GridWorld : ITileWorld<Position, Edge>
	{
		bool[][] grid;
		int[][] offset = new int[][]
		{
			new int[] {0,-1 },
			new int[] {1,-1 },
			new int[] {1,0 },
			new int[] {1,1 },
			new int[] {0,1 },
			new int[] {-1,1 },
			new int[] {-1,0 },
			new int[] {-1,-1 },
		};

		public GridWorld(bool[][] grid)
		{
			this.grid = grid;
		}

		public IEnumerable<Position> BuilderGetTiles()
		{
			for(int y=0; y<grid.Length; y++)
			{
				for (int x = 0; x < grid[y].Length; x++)
				{
					yield return new Position(x, y);
				}
			}
		}

		public Tuple<Position, Position> CreateEdge(Position from, Position to)
		{
			return new Edge(from, to);
		}

		public int GetCost(Tuple<Position, Position> link)
		{
			var from = link.Item1;
			var to = link.Item2;
			return Math.Abs(from.Item1 - to.Item1) + Math.Abs(from.Item2 - to.Item2);
		}

		public int GetEstimation(Position from, Position to)
		{
			return Math.Abs(from.Item1 - to.Item1) + Math.Abs(from.Item2 - to.Item2);
		}

		public Position GetTile(Position p, TileDirection d)
		{
			var index = (int)d;
			var x = offset[index][0] + p.Item1;
			var y = offset[index][1] + p.Item2;
			return new Position(x, y);
		}

		public bool IsBlocked(Position p, TileDirection d)
		{
			var index = (int)d;
			var x = offset[index][0] + p.Item1;
			var y = offset[index][1] + p.Item2;

			if (x >= 0 && y >= 0 && y < grid.Length && x < grid[y].Length)
			{
				return !grid[y][x];
			}
			return true;
		}
	}
}
