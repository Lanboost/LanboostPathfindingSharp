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
		Dictionary<N, L> parentLink = new Dictionary<N, L>();
		Dictionary<N, N> parentNode = new Dictionary<N, N>();
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

		public int GetCost(N n)
		{
			return cost[n];
		}

		/// <summary>
		/// Get the path of links found in last search.
		/// </summary>
		/// <returns>A list of <c>IEdge</c>'s traversed in the path.</returns>
		public List<L> GetPathLinks()
		{
			List<L> temp = new List<L>();
			var keyNow = End;
			while (parentNode.ContainsKey(keyNow))
			{
				temp.Add(parentLink[keyNow]);
				keyNow = parentNode[keyNow];
			}
			temp.Reverse();
			return temp;
		}

		/// <summary>
		/// Get the path of nodes found in last search.
		/// </summary>
		/// <returns>A list of <c>IEdge</c>'s traversed in the path.</returns>
		public List<N> GetPath()
		{
			List<N> temp = new List<N>();
			var keyNow = End;
			temp.Add(keyNow);
			while (parentNode.ContainsKey(keyNow))
			{
				temp.Add(parentNode[keyNow]);
				keyNow = parentNode[keyNow];
			}
			temp.Add(keyNow);
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
			parentLink.Clear();
			parentNode.Clear();
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
					var link = n.link;
					var edgeNode = n.to;
					var pathCost = graph.GetCost(current, edgeNode, link);
					var totalCost = mycost + pathCost;
					if (!edgeNode.Equals(Start) && !parentNode.ContainsKey(edgeNode) || totalCost < cost[edgeNode])
					{
						if (parentNode.ContainsKey(edgeNode))
						{
							parentNode[edgeNode] = current;
							parentLink[edgeNode] = link;
							cost[edgeNode] = totalCost;
						}
						else
						{
							parentNode.Add(edgeNode, current);
							parentLink.Add(edgeNode, link);
							cost.Add(edgeNode, totalCost);
						}

						int heu = totalCost + graph.GetEstimation(edgeNode, End);

						priorityQueue.Enqueue(edgeNode, heu);
					}
				}
			}
			return "No path exists.";
		}

        /// <summary>
        /// Attempts to find a path between two nodes, but path ends when 'end' is within 'dist'.
		/// 
		/// Useful for pathfinding when end is "blocked" but want a path close to it.
		/// E.g. In 2D Door is closed (and counted as blocking), but want path to either tile close to door.
        /// </summary>
        /// <param name="Start">The start node.</param>
        /// <param name="End">The goal node.</param>
        /// <returns><c>null</c> if search was successful and a path exists, otherwise string with error.</returns>
        public String FindPathClose(N Start, N End, float dist)
        {
            if (Start.Equals(End))
            {
                return "Start and end cannot be the same.";
            }

            this.End = End;
            parentLink.Clear();
            parentNode.Clear();
            cost.Clear();
            priorityQueue.Clear();
            this.graph.AddTemporaryStartEndNodes(Start, End);

            cost.Add(Start, 0);
            priorityQueue.Enqueue(Start, 0);

            int c = 0;
			var d2 = dist * dist;
            while (priorityQueue.Count > 0)
            {

                c++;
                if (c > maxExpansions)
                {
                    return "Hit max expansions.";
                }

                N current = priorityQueue.Dequeue();

                if (current.Equals(End) || this.graph.DistanceSquared(current, End) <= d2)
                {
                    this.End = current;
                    return null;
                }

                var mycost = cost[current];
                var adjacencies = graph.GetEdges(current);

                foreach (var n in adjacencies)
                {
                    var link = n.link;
                    var edgeNode = n.to;
                    var pathCost = graph.GetCost(current, edgeNode, link);
                    var totalCost = mycost + pathCost;
                    if (!edgeNode.Equals(Start) && !parentNode.ContainsKey(edgeNode) || totalCost < cost[edgeNode])
                    {
                        if (parentNode.ContainsKey(edgeNode))
                        {
                            parentNode[edgeNode] = current;
                            parentLink[edgeNode] = link;
                            cost[edgeNode] = totalCost;
                        }
                        else
                        {
                            parentNode.Add(edgeNode, current);
                            parentLink.Add(edgeNode, link);
                            cost.Add(edgeNode, totalCost);
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
