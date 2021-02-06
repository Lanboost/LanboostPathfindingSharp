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
	}

	public enum EdgeConstraint
	{
		Direction,
		Bidirectional
	}

	public interface ITileWorld<N, L>
	{
		N GetTile(N p, TileDirection d);

		bool IsBlocked(N p, TileDirection d);

		IEnumerable<N> BuilderGetTiles();

		int GetCost(L link);

		int GetEstimation(N from, N to);

		L CreateEdge(N from, N to);
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

	public class RemoteGraphBuilder<N, L> : IGraphBuilder<N, L>
	{
		Dictionary<N, List<RemoteLink<N, L>>> remotes = new Dictionary<N, List<RemoteLink<N, L>>>();
		Dictionary<N, List<RemoteLink<N, L>>> remotesTo = new Dictionary<N, List<RemoteLink<N, L>>>();
		Dictionary<L, int> cost = new Dictionary<L, int>();

		IGraphBuilder<N, L> parent;

		public RemoteGraphBuilder(IGraphBuilder<N, L> parent)
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
	}

	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// Does not support directed pathing!
	/// </remarks>
	/// <typeparam name="N"></typeparam>
	/// <typeparam name="L"></typeparam>
	public class SubGoalGraphBuilder2D<N, L> : IGraphBuilder<N, L>
	{
		ITileWorld<N, L> world;

		static TileDirection[][] tileDirections = new TileDirection[][]{
			new TileDirection[] { TileDirection.TopRight, TileDirection.Top, TileDirection.Right},
			new TileDirection[] { TileDirection.BottomRight, TileDirection.Right, TileDirection.Bottom},
			new TileDirection[] {TileDirection.BottomLeft, TileDirection.Bottom, TileDirection.Left},
			new TileDirection[] { TileDirection.TopLeft, TileDirection.Left, TileDirection.Top}
		};

		public SubGoalGraphBuilder2D(ITileWorld<N, L> world)
		{
			this.world = world;
		}

		Tuple<bool, N> Raycast(N n, TileDirection direction)
		{
			var now = n;
			while (true)
			{
				if (world.IsBlocked(now, direction))
				{
					return new Tuple<bool, N>(false, default(N));
				}
				else
				{
					now = world.GetTile(now, direction);
					if (IsSubGoal(now))
					{
						return new Tuple<bool, N>(true, now);
					}
				}
			}
		}

		List<N> FindSubGoalLinks(N n)
		{
			List<N> subGoals = new List<N>();

			foreach (var dir in tileDirections)
			{
				var now = n;
				while (true)
				{
					if (world.IsBlocked(now, dir[0]))
					{
						break;
					}
					else
					{
						now = world.GetTile(now, dir[0]);

						for (int i = 1; i < dir.Length; i++)
						{
							var r = Raycast(now, dir[i]);
							if (r.Item1)
							{
								subGoals.Add(r.Item2);
							}
						}
					}
				}
			}

			var tDirs = new TileDirection[]
			{
				TileDirection.Top, TileDirection.Right, TileDirection.Bottom, TileDirection.Left
			};

			for (int i = 0; i < tDirs.Length; i++)
			{
				var r = Raycast(n, tDirs[i]);
				if (r.Item1)
				{
					subGoals.Add(r.Item2);
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

		bool IsSubGoal(N tile)
		{

			foreach (var dir in tileDirections)
			{
				if (world.IsBlocked(tile, dir[0]) && !world.IsBlocked(tile, dir[1]) && !world.IsBlocked(tile, dir[2]))
				{
					return true;
				}
			}
			return false;
		}

		public IEnumerable<N> BuilderGetNodes()
		{
			foreach (var tile in world.BuilderGetTiles())
			{
				if (IsSubGoal(tile))
				{
					yield return tile;
				}
			}
		}

		public int GetCost(L link)
		{
			return world.GetCost(link);
		}

		public int GetEstimation(N from, N to)
		{
			return world.GetEstimation(from, to);
		}
	}
}
