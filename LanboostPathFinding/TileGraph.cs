using Lanboost.PathFinding.GraphBuilders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lanboost.PathFinding.Graph
{
	public class Position
	{
		public int x;
		public int y;

		public Position(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public override bool Equals(object obj)
		{
			var position = obj as Position;
			return position != null &&
				   x == position.x &&
				   y == position.y;
		}

		public override int GetHashCode()
		{
			var hashCode = 1502939027;
			hashCode = hashCode * -1521134295 + x.GetHashCode();
			hashCode = hashCode * -1521134295 + y.GetHashCode();
			return hashCode;
		}
	}

	public class Edge
	{
		public Position first;
		public Position second;

		public Edge(Position first, Position second)
		{
			this.first = first;
			this.second = second;
		}

		public override bool Equals(object obj)
		{
			var edge = obj as Edge;
			return edge != null &&
				   EqualityComparer<Position>.Default.Equals(first, edge.first) &&
				   EqualityComparer<Position>.Default.Equals(second, edge.second);
		}

		public override int GetHashCode()
		{
			var hashCode = 405212230;
			hashCode = hashCode * -1521134295 + EqualityComparer<Position>.Default.GetHashCode(first);
			hashCode = hashCode * -1521134295 + EqualityComparer<Position>.Default.GetHashCode(second);
			return hashCode;
		}
	}
	
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

		public int GetCost(Edge link)
		{
			return 1;
		}

		public IEnumerable<Edge> GetEdges(Position node)
		{
			var dirs = new int[][] {
				new int[] { 0, -1 },
				new int[] { 1, 0 },
				new int[] { 0, 1 },
				new int[] { -1, 0 }
			};
			foreach(var d in dirs)
			{
				var x = node.x + d[0];
				var y = node.y + d[1];
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
			return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y);
		}

		public Position GetOtherNode(Position From, Edge link)
		{
			if (link.first.Equals(From))
			{
				return link.second;
			}
			return link.first;
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
			new int[] {0,0},
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

		public Edge CreateEdge(Position from, Position to)
		{
			return new Edge(from, to);
		}

		public int GetCost(Edge link)
		{
			var from = link.first;
			var to = link.second;
			return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y);
		}

		public int GetEstimation(Position from, Position to)
		{
			return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y);
		}

		public Position GetTile(Position p, TileDirection d)
		{
			var index = (int)d;
			var x = offset[index][0] + p.x;
			var y = offset[index][1] + p.y;
			return new Position(x, y);
		}

		public bool IsBlocked(Position p, TileDirection d)
		{
			var index = (int)d;
			var x = offset[index][0] + p.x;
			var y = offset[index][1] + p.y;

			if (x >= 0 && y >= 0 && y < grid.Length && x < grid[y].Length)
			{
				return !grid[y][x];
			}
			return true;
		}
	}
}
