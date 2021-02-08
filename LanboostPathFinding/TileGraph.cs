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
		public int plane;

		public Position(int x, int y, int plane = 0)
		{
			this.x = x;
			this.y = y;
			this.plane = plane;
		}

		public override bool Equals(object obj)
		{
			var position = obj as Position;
			return position != null &&
				   x == position.x &&
				   y == position.y &&
				   plane == position.plane;
		}

		public override int GetHashCode()
		{
			var hashCode = 1502939027;
			hashCode = hashCode * -1521134295 + x.GetHashCode();
			hashCode = hashCode * -1521134295 + y.GetHashCode();
			hashCode = hashCode * -1521134295 + plane.GetHashCode();
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

	public class NoEdge
	{

	}
	
	public class TileGraph : IGraph<Position, NoEdge>
	{
		bool[][] grid;

		public TileGraph(bool[][] grid)
		{
			this.grid = grid;
		}

		public void AddTemporaryStartEndNodes(Position start, Position end)
		{

		}

		public int GetCost(Position start, Position end, NoEdge link)
		{
			return 1;
		}

		public IEnumerable<Edge<Position, NoEdge>> GetEdges(Position node)
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
						yield return new Edge<Position, NoEdge>(new Position(x, y), null);
					}
				}
			}
		}

		public int GetEstimation(Position from, Position to)
		{
			return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y);
		}
	}

	public class GridWorld : ITileWorld<Position, Edge>, World2D
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

		public IEnumerable<bool> GetChunkBlocedPositions(Position chunk)
		{
			for (int y = 0; y < grid.Length; y++)
			{
				for (int x = 0; x < grid[y].Length; x++)
				{
					yield return !grid[y][x];
				}
			}
		}

		public IEnumerable<Position> GetChunks()
		{
			yield return new Position(0, 0, 0);
		}

		public int GetChunkSize()
		{
			return this.grid.Length;
		}

		public int GetCost(Position p1, Position p2, Edge link)
		{
			var from = link.first;
			var to = link.second;
			return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y);
		}

		public int GetEstimation(Position from, Position to)
		{
			return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y);
		}

		public bool GetBlocked(Position p)
		{
			return !grid[p.y][p.x];
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
