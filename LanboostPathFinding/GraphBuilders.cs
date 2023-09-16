using Lanboost.PathFinding.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lanboost.PathFinding.GraphBuilders
{

	public enum TileDirection
	{
		Top,
		TopRight,
		Right,
		BottomRight,
		Bottom,
		BottomLeft,
		Left,
		TopLeft,
		None
	}

	public class TileDirectionUtils
	{
		/* Directions that are not diagonal */
		public static TileDirection[] OrthogonalDirections = new TileDirection[]
		{
			TileDirection.Top,
			TileDirection.Right,
			TileDirection.Bottom,
			TileDirection.Left
		};

        public static TileDirection[] DiagonalDirections = new TileDirection[]
        {
            TileDirection.TopRight,
            TileDirection.BottomRight,
            TileDirection.BottomLeft,
            TileDirection.TopLeft
        };


        public static int Opposite(TileDirection dir)
		{
			var t = new int[] {
				4,
				5,
				6,
				7,
				1,
				2,
				3
			};
			return t[(int)dir];
		}

		private static int[][] _TileDirectionOffset = new int[][] {
                new int[] { 0, 1 },
                new int[] { 1, 1 },
                new int[] { 1, 0 },
                new int[] { 1, -1 },
                new int[] { 0, -1 },
                new int[] { -1, -1 },
                new int[] { -1, 0 },
                new int[] { -1, 1 },
                new int[] { 0, 0 }
           };

        public static int[] TileDirectionOffset(TileDirection dir)
		{
			return _TileDirectionOffset[(int)dir];
		}

		public static TileDirection[] SplitDiagonal(TileDirection dir)
		{
			var t = new TileDirection[][] {
				null,
				new TileDirection[] { TileDirection.Top, TileDirection.Right },
				null,
				new TileDirection[] { TileDirection.Right, TileDirection.Bottom },
				null,
				new TileDirection[] { TileDirection.Bottom, TileDirection.Left },
				null,
				new TileDirection[] { TileDirection.Left, TileDirection.Top },
				null
			};
			return t[(int)dir];
		}
	}

	public enum EdgeConstraint
	{
		Direction,
		Bidirectional
	}

	public interface ITileWorld<N, L>
	{
		N GetTile(N p, TileDirection d);

		//bool IsBlocked(Position area, int offsetX, int offsetY);

		IEnumerable<N> BuilderGetTiles();

		int GetCost(N from, N to, L link);

		int GetEstimation(N from, N to);

		L CreateEdge(N from, N to);
		//N CreateNode(Position area, int offsetX, int offsetY);
	}

	public class RemoteLink<N, L>
	{
		public N from;
		public N to;
		public L link;

		public RemoteLink(N from, N to, L link)
		{
			this.from = from;
			this.to = to;
			this.link = link;
		}
	}
	/*
	public class RemoteGraphBuilder<A, N, L> : IGraphBuilder<A, N, L>
	{
		Dictionary<N, List<RemoteLink<N, L>>> remotes = new Dictionary<N, List<RemoteLink<N, L>>>();
		Dictionary<N, List<RemoteLink<N, L>>> remotesTo = new Dictionary<N, List<RemoteLink<N, L>>>();
		Dictionary<L, int> cost = new Dictionary<L, int>();

		IGraphBuilder<A, N, L> parent;

		public RemoteGraphBuilder(IGraphBuilder<A, N, L> parent)
		{
			this.parent = parent;
		}

		public void AddRemote(RemoteLink<N, L> remote)
		{
			if (!remotes.ContainsKey(remote.from))
			{
				remotes.Add(remote.from, new List<RemoteLink<N, L>>());
			}
			if (!remotesTo.ContainsKey(remote.to))
			{
				remotesTo.Add(remote.from, new List<RemoteLink<N, L>>());
			}
			remotes[remote.from].Add(remote);
			remotesTo[remote.to].Add(remote);
		}

		public Tuple<List<Edge<N, L>>, Dictionary<N, Edge<N, L>>> BuilderGetEdges(N p1)
		{
			var tuple = parent.BuilderGetEdges(p1);
			if (remotes.ContainsKey(p1))
			{
				foreach (var link in remotes[p1])
				{
					tuple.Item1.Add(new Edge<N, L>(link.to, link.link));
				}
			}

			if (remotesTo.ContainsKey(p1))
			{
				foreach (var link in remotesTo[p1])
				{
					tuple.Item2.Add(link.from, new Edge<N, L>(link.to, link.link));
				}
			}
			return tuple;
		}

		public IEnumerable<N> BuilderGetNodes()
		{
			return parent.BuilderGetNodes();
		}

		public int GetCost(L link)
		{
			return parent.GetCost(link);
		}

		public int GetEstimation(N from, N to)
		{
			return parent.GetEstimation(from, to);
		}
	}*/

	/*public class CacheArea {
		byte[][] blockType;
		short[][][] distance;
		byte[][][] distanceType;

		int dirty = 0;

		public static byte TYPE_BLOCKED = 1;
		public static byte TYPE_OPEN = 0;
		public static byte TYPE_SUBGOAL = 2;

		public static byte DIRTY_TOP = 1;
		public static byte DIRTY_RIGHT = 2;
		public static byte DIRTY_BOTTOM = 4;
		public static byte DIRTY_LEFT = 8;
		public static byte DIRTY_ALL = 15;

		public CacheArea(int width, int height)
		{
			this.blockType = new byte[height][];
			for(int y=0; y<height; y++)
			{
				this.blockType[y] = new byte[width];
			}

			this.distance = new short[4][][];
			this.distanceType = new byte[4][][];
			for (int i = 0; i < 4; i++)
			{
				this.distance[i] = new short[height][];
				this.distanceType[i] = new byte[height][];
				for (int y = 0; y < height; y++)
				{
					this.distance[i][y] = new short[width];
					this.distanceType[i][y] = new byte[width];
				}
			}
		}

		public byte Get(int x, int y)
		{
			return blockType[y][x];
		}

		public void Set(int x, int y, byte v)
		{
			blockType[y][x] = v;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// Does not support directed pathing!
	/// </remarks>
	/// <typeparam name="N"></typeparam>
	/// <typeparam name="L"></typeparam>
	public class SubGoalGraphBuilder2D<N, L> : IGraphBuilder<Position, N, L>
	{
		int[][][] subGoalOffsets = new int[][][]
		{
			new int[][]
			{
				new int[] {1,1 },
				new int[] {0,1 },
				new int[] {1,0 },
			},
			new int[][]
			{
				new int[] {1,-1 },
				new int[] {0,-1 },
				new int[] {1,0 },
			},
			new int[][]
			{
				new int[] {-1,-1 },
				new int[] {0,-1 },
				new int[] {-1,0 },
			},
			new int[][]
			{
				new int[] {-1,1 },
				new int[] {0,1 },
				new int[] {-1,0 },
			}
		};

		Dictionary<Position, CacheArea> areas = new Dictionary<Position, CacheArea>();
		ITileWorld<N, L> world;
		int width = 0;
		int height = 0;

		public SubGoalGraphBuilder2D(ITileWorld<N, L> world)
		{
			this.world = world;
			this.width = world.GetAreaWidth();
			this.height = world.GetAreaHeight();
		}

		

		struct AreaOffset{
			int dirX;
			int dirY;
			public int offsetX;
			public int offsetY;
			public CacheArea cache;
			public Position area;
			TileDirection dir;
			int height;
			int width;
			bool error;

			Dictionary<Position, CacheArea> areas;
			ITileWorld<N, L> world;

			public AreaOffset(Dictionary<Position, CacheArea> areas, ITileWorld<N, L> world, Position area, int x, int y, TileDirection dir)
			{
				this.areas = areas;
				this.world = world;
				var off = TileDirectionUtils.TileDirectionOffset(dir);
				this.dir = dir;
				dirX = off[0];
				dirY = off[1];
				offsetX = x;
				offsetY = y;
				cache = areas[area];
				this.area = area;
				this.width = world.GetAreaWidth();
				this.height = world.GetAreaHeight();
				error = false;
			}

			public bool Step()
			{
				offsetX += dirX;
				offsetY += dirY;
				bool moveArea = false;
				if (offsetX < 0)
				{
					offsetX = width - 1;
					moveArea = true;
				}
				else if (offsetY < 0)
				{
					offsetY = height - 1;
					moveArea = true;
				}
				else if (offsetX >= 0)
				{
					offsetX = 0;
					moveArea = true;
				}
				else if (offsetY >= height)
				{
					offsetY = 0;
					moveArea = true;
				}



				if (moveArea)
				{
					var tupleArea = world.GetArea(area, dir);
					if (!tupleArea.Item1)
					{
						error = true;
						return false;
					}
					cache = areas[tupleArea.Item2];
					area = tupleArea.Item2;
				}

				return true;
			}

			public int Get()
			{
				return cache.Get(offsetX, offsetY);
			}

			public N CreateNode()
			{
				return world.CreateNode(area, offsetX, offsetY);
			}

			public AreaOffset ChangeDirection(TileDirection dir)
			{
				return new AreaOffset(areas, world, area, offsetX, offsetY, dir);
			}

			public TileDirection GetDirection()
			{
				return this.dir;
			}

			public bool StepFailed()
			{
				return error;
			}

			public AreaOffset Copy()
			{
				return new AreaOffset(areas, world, area, offsetX, offsetY, dir);
			}
		}

		Tuple<bool, N> LineRaycast(AreaOffset areaOffset)
		{
			
			while (true)
			{
				var v = areaOffset.Get();
				if (v == CacheArea.SUBGOAL)
				{
					return new Tuple<bool, N>(true, areaOffset.CreateNode());
				}
				if (v == CacheArea.BLOCKED)
				{
					break;
				}
				if(!areaOffset.Step())
				{
					break;
				}
			}
			return new Tuple<bool,N>(false, default(N));
		}

		IEnumerable<N> DiagonalRaycast(AreaOffset areaOffset)
		{
			while (true)
			{
				var v = areaOffset.Get();
				if (v == CacheArea.BLOCKED)
				{
					break;
				}
				else if (v == CacheArea.SUBGOAL)
				{
					yield return areaOffset.CreateNode();
					break;
				}
				else
				{
					var diags = TileDirectionUtils.SplitDiagonal(areaOffset.GetDirection());

					// If the LineRaycasts hit a blocked on first, we should break as we cannot step diagonally then
					bool shouldBreak = false;
					foreach(var dia in diags)
					{
						var nao = areaOffset.ChangeDirection(dia);
						if (nao.Step())
						{
							if (nao.Get() == CacheArea.BLOCKED)
							{
								shouldBreak = true;
							}
							var ret = LineRaycast(nao);
							if (ret.Item1)
							{
								yield return ret.Item2;
							}
						}
					}
					if(shouldBreak)
					{
						break;
					}
					if(!areaOffset.Step())
					{
						break;
					}
				}
			}
		}

		List<N> FindSubGoalLinks(N n)
		{

			AreaOffset areaOffset;
			// This is needed because bad programming...
			if (areaOffset.Get() == CacheArea.BLOCKED)
			{
				return new List<N>();
			}

			List<N> subGoals = new List<N>();

			AreaOffset[] lineAreaOffsets = new AreaOffset[]
			{
				areaOffset.ChangeDirection(TileDirection.Top),
				areaOffset.ChangeDirection(TileDirection.Right),
				areaOffset.ChangeDirection(TileDirection.Bottom),
				areaOffset.ChangeDirection(TileDirection.Left)
			};

			foreach(var v in lineAreaOffsets)
			{
				v.Step();
			}

			int[][] lineAreaOffsetIndex = new int[][]{
				new int[] { 0,1},
				new int[] { 1,2},
				new int[] { 2,3},
				new int[] { 3,0}
			};

			TileDirection[] lineAreaOffsetIndexDirection = new TileDirection[]
			{
				TileDirection.TopRight,
				TileDirection.BottomRight,
				TileDirection.BottomLeft,
				TileDirection.TopLeft
			};


			for(int i=0; i< lineAreaOffsetIndexDirection.Length; i++)
			{
				var failed = false;
				foreach(var v in lineAreaOffsetIndex[i])
				{
					if(lineAreaOffsets[v].StepFailed() || lineAreaOffsets[v].Get() == CacheArea.BLOCKED)
					{
						failed = true;
						break;
					}
				}
				if(!failed)
				{
					foreach(var v in DiagonalRaycast(areaOffset.ChangeDirection(lineAreaOffsetIndexDirection[i])))
					{
						subGoals.Add(v);
					}
				}
			}

			//This have to be done last, as otherwise we will change the underlying datastructure

			foreach (var v in lineAreaOffsets)
			{
				if(!v.StepFailed())
				{
					var vv = LineRaycast(v);
					if(vv.Item1)
					{
						subGoals.Add(vv.Item2);
					}
				}
			}

			return subGoals;
		}

		public Tuple<List<Edge<N, L>>, Dictionary<N, Edge<N, L>>> BuilderGetEdges(N p1)
		{
			var subGoals = FindSubGoalLinks(p1);
			var item1List = new List<Edge<N, L>>();
			var item2Dict = new Dictionary<N, Edge<N, L>>();

			foreach (var subGoal in subGoals)
			{
				item1List.Add(new Edge<N, L>(subGoal, world.CreateEdge(p1, subGoal)));
				item2Dict.Add(subGoal, new Edge<N, L>(p1, world.CreateEdge(subGoal, p1)));
			}
			return new Tuple<List<Edge<N, L>>, Dictionary<N, Edge<N, L>>>(item1List, item2Dict);
		}

		bool IsSubGoalNoCache(CacheArea area, int x, int y)
		{
			if(area.Get(x,y) == CacheArea.BLOCKED)
			{
				return false;
			}

			foreach (var dir in subGoalOffsets)
			{
				if (area.Get(x+ dir[0][0], y + dir[0][1]) == CacheArea.BLOCKED &&
					area.Get(x + dir[1][0], y + dir[1][1]) != CacheArea.BLOCKED &&
					area.Get(x + dir[2][0], y + dir[2][1]) != CacheArea.BLOCKED)
				{
					return true;
				}
			}
			return false;
		}

		void dirtySurroundingAreas(Position area)
		{
			TileDirection[] dirs = new TileDirection[]
			{
				TileDirection.Top,
				TileDirection.TopRight,
				TileDirection.Right,
				TileDirection.BottomRight,
				TileDirection.Bottom,
				TileDirection.BottomLeft,
				TileDirection.Left,
				TileDirection.TopLeft
			};

			int[] dirtyFlags = new int[]
			{
				CacheArea.DIRTY_BOTTOM,
				CacheArea.DIRTY_BOTTOM, // The diagional does not matter
				CacheArea.DIRTY_LEFT,
				CacheArea.DIRTY_TOP,
				CacheArea.DIRTY_TOP,
				CacheArea.DIRTY_TOP,
				CacheArea.DIRTY_RIGHT,
				CacheArea.DIRTY_BOTTOM,
			};

			for (int i = 0; i < dirs.Length; i++)
			{
				var a = world.GetArea(area, dirs[i]);
				if (a != null)
				{
					SetDirty(a, dirtyFlags[i]);
				}
			}
		}

		public void CleanDirtyAreas()
		{
			while(true)
			{

			}
		}

		public void UnloadArea(Position area)
		{
			areas.Remove(area);

			dirtySurroundingAreas(area);
		}

		public void UpdateCacheEdges(CacheArea area)
		{
			//world.GetArea(area, TileDirection.Top);
		}

		public List<N> LoadArea(Position area)
		{
			var cache = new CacheArea(width, height);
			areas.Add(area, cache);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					var blocked = world.IsBlocked(area, x, y);
					if (blocked)
					{
						cache.Set(x, y, 1);
					}
					else
					{
						cache.Set(x, y, 0);
					}
				}
			}


			dirtySurroundingAreas(area);
			SetDirty(area, CacheArea.DIRTY_ALL);

			return nodes;
		}

		public int GetCost(N from, N to, L link)
		{
			return world.GetCost(from, to, link);
		}

		public int GetEstimation(N from, N to)
		{
			return world.GetEstimation(from, to);
		}
	}*/
}
