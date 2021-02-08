using Lanboost.PathFinding.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PortalExtender : Graph2DExtender<int>
{
	public Dictionary<Position, Position> edges = new Dictionary<Position, Position>();

	public void Add(Position p1, Position p2)
	{
		if(!edges.ContainsKey(p1) && !edges.ContainsKey(p2))
		{
			edges.Add(p1, p2);
			edges.Add(p2, p1);
		}
	}

	public void Clear()
	{
		edges.Clear();
	}

	public IEnumerable<Edge<Position, SimpleEdge<int>>> GetEdges(Position node)
	{
		if(edges.ContainsKey(node))
		{
			yield return new Edge<Position, SimpleEdge<int>>(edges[node], new SimpleEdge<int>(node, edges[node], 1));
		}
		yield break;
	}

	public IEnumerable<Position> GetExtraNodes()
	{
		foreach(var v in edges.Keys)
		{
			yield return v;
		}
		yield break;
	}

	public IEnumerable<Func<Position, Edge<Position, SimpleEdge<int>>>> GetGlobalEdgeFunc()
	{
		yield break;
	}

	public int GetEstimation(Position from, Position to)
	{
		var m = int.MaxValue;
		foreach(var v in edges)
		{
			m = Math.Min(m, SubgoalGraph2D<int>.ManhattanDistance(from, v.Key) + SubgoalGraph2D<int>.ManhattanDistance(v.Value, to));
		}


		return m;
	}
}

class GlobalExtender : Graph2DExtender<int>
{
	public List<Position> positions = new List<Position>();
	public void Add(Position p1)
	{
		positions.Add(p1);
	}

	public void Clear()
	{
		positions.Clear();
	}

	public IEnumerable<Edge<Position, SimpleEdge<int>>> GetEdges(Position node)
	{
		yield break;
	}

	public int GetEstimation(Position from, Position to)
	{
		return int.MaxValue;
	}

	public IEnumerable<Position> GetExtraNodes()
	{
		foreach (var v in positions)
		{
			yield return v;
		}
		yield break;
	}

	public IEnumerable<Func<Position, Edge<Position, SimpleEdge<int>>>> GetGlobalEdgeFunc()
	{
		foreach(var v in positions)
		{
			yield return delegate (Position start)
			{
				return new Edge<Position, SimpleEdge<int>>(v, new SimpleEdge<int>(start, v, 10));
			};
		}

		yield break;
	}
}