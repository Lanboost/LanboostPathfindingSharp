using Lanboost.PathFinding;
using Lanboost.PathFinding.Graph;
using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Library for running Astar on a graph.
/// </summary>
namespace Lanboost.PathFinding.Astar
{
	/// <summary>
	/// Runs Astar on a given graph.
	/// </summary>
	public class AStar<N, L> : IPathFinder<N, L>
	{
		SimplePriorityQueue<N> priorityQueue = new SimplePriorityQueue<N>();
		Dictionary<N, L> parent = new Dictionary<N, L>();
		Dictionary<N, int> cost = new Dictionary<N, int>();
		IGraph<N, L> graph;
		int maxExpansions;
		N End;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="graph">The underlying graph to run Astar on.</param>
		/// <param name="maxExpansions">The maximum nodes to expand when searching for the goal.</param>
		public AStar(IGraph<N, L> graph, int maxExpansions)
		{
			this.maxExpansions = maxExpansions;
			this.graph = graph;
		}

		/// <summary>
		/// Get the path found in last search.
		/// </summary>
		/// <returns>A list of <c>IEdge</c>'s traversed in the path.</returns>
		public List<L> GetPath()
		{
			List<L> temp = new List<L>();
			var keyNow = End;
			while (parent.ContainsKey(keyNow))
			{
				temp.Add(parent[keyNow]);
				keyNow = graph.GetOtherNode(keyNow, parent[keyNow]);
			}
			temp.Reverse();
			return temp;
		}

		/// <summary>
		/// Attempts to find a path between two nodes.
		/// </summary>
		/// <param name="Start">The start node.</param>
		/// <param name="End">The goal node.</param>
		/// <returns><c>null</c> if search was successful and a path exists, otherwise string with error.</returns>
		public String FindPath(N Start, N End)
		{
			if(Start.Equals(End))
			{
				return "Start and end cannot be the same.";
			}

			this.End = End;
			parent.Clear();
			cost.Clear();
			priorityQueue.Clear();
			this.graph.AddTemporaryStartEndNodes(Start, End);

			cost.Add(Start, 0);
			priorityQueue.Enqueue(Start, 0);

			int c = 0;

			while (priorityQueue.Count > 0)
			{

				c++;
				if (c > maxExpansions)
				{
					return "Hit max expansions.";
				}

				N current = priorityQueue.Dequeue();

				if (current.Equals(End))
				{
					return null;
				}

				var mycost = cost[current];
				var adjacencies = graph.GetEdges(current);

				foreach (var n in adjacencies)
				{
					var edgeNode = graph.GetOtherNode(current, n);
					var pathCost = graph.GetCost(n);
					var totalCost = mycost + pathCost;
					if (!edgeNode.Equals(Start) && !parent.ContainsKey(edgeNode) || totalCost < cost[edgeNode])
					{
						if (!parent.ContainsKey(edgeNode))
						{
							parent.Add(edgeNode, n);
							cost.Add(edgeNode, totalCost);
						}
						else
						{
							parent[edgeNode] = n;
							cost[edgeNode] = totalCost;
						}

						int heu = totalCost + graph.GetEstimation(edgeNode, End);

						priorityQueue.Enqueue(edgeNode, heu);
					}
				}
			}
			return "No path exists.";
		}
	}
}
