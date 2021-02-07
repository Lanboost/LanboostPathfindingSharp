using System;
using System.Collections.Generic;
using System.Text;

namespace Lanboost.PathFinding.Graph
{
	/// <summary>
	/// Interface to implement for any graph to be able to run pathfinding on it.
	/// </summary>
	public interface IGraph<N, L>
	{
		int GetEstimation(N from, N to);
		IEnumerable<L> GetEdges(N node);
		int GetCost(L link);
		N GetOtherNode(N From, L link);
		void AddTemporaryStartEndNodes(N start, N end);
	}

	public interface IGraphBuilder<N, L>
	{
		IEnumerable<N> BuilderGetNodes();
		Tuple<List<Edge<N, L>>, Dictionary<N, Edge<N, L>>> BuilderGetEdges(N p1);

		int GetCost(L link);

		int GetEstimation(N from, N to);
	}

	public class Edge<P, L>
	{
		public P to;
		public L link;

		public Edge(P to, L link)
		{
			this.to = to;
			this.link = link;
		}

		public override bool Equals(object obj)
		{
			if(obj.GetType() == typeof(Edge<P,L>))
			{
				var i = (Edge<P, L>)obj;
				return i.to.Equals(to) && i.link.Equals(link);
			}
			return false;
		}

		public override int GetHashCode()
		{
			var hashCode = -740587475;
			hashCode = hashCode * -1521134295 + EqualityComparer<P>.Default.GetHashCode(to);
			hashCode = hashCode * -1521134295 + EqualityComparer<L>.Default.GetHashCode(link);
			return hashCode;
		}
	}

	public class Node<P, L>
	{
		public List<Edge<P, L>> edges = new List<Edge<P, L>>();
	}

	public class DynamicDirectedGraph<P, L> : IGraph<P, L>
	{
		IGraphBuilder<P, L> graphBuilder;
		Dictionary<P, Node<P, L>> nodes = new Dictionary<P, Node<P, L>>();
		Dictionary<P, List<P>> nodesToNode = new Dictionary<P, List<P>>();

		P startNode;
		P endNode;
		List<Edge<P, L>> startEdges;
		Dictionary<P, Edge<P, L>> endEdges;


		public IEnumerable<P> GetNodes()
		{
			foreach(var k in nodes.Keys)
			{
				yield return k;
			}
		}

		public DynamicDirectedGraph(IGraphBuilder<P, L> graphBuilder)
		{
			this.graphBuilder = graphBuilder;
		}

		public int GetCost(L link)
		{
			return graphBuilder.GetCost(link);
		}

		public int GetEstimation(P from, P to)
		{
			return graphBuilder.GetEstimation(from, to);
		}

		public IEnumerable<L> GetEdges(P node)
		{

			if (startNode.Equals(node))
			{
				foreach (var l in startEdges)
				{
					yield return l.link;
				}

			}
			else
			{

				if (endEdges.ContainsKey(node))
				{
					
					yield return endEdges[node].link;
					
				}

				if (nodes.ContainsKey(node))
				{
					var inner = nodes[node];
					foreach (var l in inner.edges)
					{
						yield return l.link;
					}
				}
			}
		}

		public P GetOtherNode(P From, L link)
		{
			if (startNode.Equals(From))
			{
				foreach (var l in startEdges)
				{
					if (l.link.Equals(link))
					{
						return l.to;
					}
				}
			}
			else if (endNode.Equals(From))
			{
				foreach(var e in endEdges)
				{
					if(e.Value.link.Equals(link))
					{
						return e.Key;
					}
				}
			}
			else
			{
				// Check if link is in endEdges
				if (endEdges.ContainsKey(From))
				{
					if (endEdges[From].link.Equals(link))
					{
						return endEdges[From].to;
					}
				}

				{
					var inner = nodes[From];
					foreach (var l in inner.edges)
					{
						if (l.link.Equals(link))
						{
							return l.to;
						}
					}
				}

				// check if we are backtracking
				{
					var innerNodes = nodesToNode[From];
					foreach (var innerNode in innerNodes)
					{
						var inner = nodes[innerNode];
						foreach (var l in inner.edges)
						{
							if (l.link.Equals(link))
							{
								return innerNode;
							}
						}
					}
				}

				//backtrack to start
				foreach (var l in startEdges)
				{
					if (l.link.Equals(link))
					{
						return startNode;
					}
				}
			}
			return default(P);
		}

		public void Load()
		{
			// to not have to implement multiple, use load nodes
			LoadNodes(this.graphBuilder.BuilderGetNodes());
		}

		public void UnloadNodes(IEnumerable<P> nodes)
		{
			foreach (var n in nodes)
			{
				// Remove from node list
				this.nodes.Remove(n);

				// Remove from all edges
				if (nodesToNode.ContainsKey(n))
				{
					var listOfNodes = nodesToNode[n];
					foreach (var nn in listOfNodes)
					{
						var nodeWithEdge = this.nodes[nn];
						foreach (var e in nodeWithEdge.edges)
						{
							if (e.to.Equals(n))
							{
								nodeWithEdge.edges.Remove(e);
								break;
							}
						}
					}
					nodesToNode.Remove(n);
				}
			}
		}

		public void LoadNodes(IEnumerable<P> nodes)
		{
			// On dynamic, we need to store all nodes, as we might have a connection to a node
			// later even if it is empty now.
			foreach (P p1 in nodes)
			{
				this.nodes.Add(p1, new Node<P, L>());
			}

			foreach (var n in nodes)
			{
				var edgeTuple = this.graphBuilder.BuilderGetEdges(n);

				//first item is from edges

				var node = this.nodes[n];
				foreach (var e in edgeTuple.Item1)
				{
					node.edges.Add(e);
					if (!nodesToNode.ContainsKey(e.to))
					{
						nodesToNode.Add(e.to, new List<P>());
					}
					nodesToNode[e.to].Add(n);
				}

				// second item is to edges
				foreach (var e in edgeTuple.Item2.Keys)
				{
					var fromNode = this.nodes[e];
					fromNode.edges.Add(edgeTuple.Item2[e]);
					if (!nodesToNode.ContainsKey(n))
					{
						nodesToNode.Add(n, new List<P>());
					}
					nodesToNode[n].Add(e);
				}
			}
		}

		public void AddTemporaryStartEndNodes(P start, P end)
		{
			

			if (!this.nodes.ContainsKey(start))
			{
				var edgeTuple = this.graphBuilder.BuilderGetEdges(start);
				startNode = start;
				startEdges = edgeTuple.Item1;
			}

			if (!this.nodes.ContainsKey(end))
			{
				endNode = end;
				var edgeTuple = this.graphBuilder.BuilderGetEdges(end);
				endEdges = edgeTuple.Item2;
			}
		}
	}
}