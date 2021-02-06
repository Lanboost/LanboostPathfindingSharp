using System;
using System.Collections.Generic;
using System.Linq;
using Lanboost.PathFinding.Astar;
using Lanboost.PathFinding.Graph;
using Lanboost.PathFinding.GraphBuilders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LanboostPathFindingTest
{

	using Position = Tuple<int, int>;
	using Edge = Tuple<Tuple<int, int>, Tuple<int, int>>;
	using RealEdge = Lanboost.PathFinding.Graph.Edge<Tuple<int, int>, Tuple<Tuple<int, int>, Tuple<int, int>>>;

	[TestClass]
	public class SubGoalGraphBuilder2DTest
	{
		[TestMethod]
		public void BuilderGetNodesShouldContainSubGoals()
		{
			var gridWorld = new GridWorld(new bool[][]
			{
				new bool[]{ false, true},
				new bool[]{ true, true},
			});
			var underTest = new SubGoalGraphBuilder2D<Position, Edge>(gridWorld);

			var expectedList = new List<Position>();
			expectedList.Add(new Position(1,1));

			CollectionAssert.AreEqual(expectedList, underTest.BuilderGetNodes().ToList());
		}

		[TestMethod]
		public void BuilderGetNodesShouldOnlyContainSubGoals()
		{
			var gridWorld = new GridWorld(new bool[][]
			{
				new bool[]{ false, true},
				new bool[]{ false, true},
			});
			var underTest = new SubGoalGraphBuilder2D<Position, Edge>(gridWorld);

			var expectedList = new List<Position>();

			CollectionAssert.AreEqual(expectedList, underTest.BuilderGetNodes().ToList());
		}

		[TestMethod]
		public void BuilderGetNodesShouldContainAllSubGoals()
		{
			var gridWorld = new GridWorld(new bool[][]
			{
				new bool[]{ true, false, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, false},
			});
			var underTest = new SubGoalGraphBuilder2D<Position, Edge>(gridWorld);

			var expectedList = new List<Position>();
			expectedList.Add(new Position(2, 2));
			expectedList.Add(new Position(0, 1));
			expectedList.Add(new Position(2, 1));
			var ret = underTest.BuilderGetNodes().ToList();
			CollectionAssert.AreEquivalent(expectedList, ret);
		}

		[TestMethod]
		public void BuilderGetEdgesShouldBeCorrect()
		{
			var gridWorld = new GridWorld(new bool[][]
			{
				new bool[]{ true, false, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, false},
			});
			var underTest = new SubGoalGraphBuilder2D<Position, Edge>(gridWorld);

			var expectedList = new List<RealEdge>();
			expectedList.Add(
				new RealEdge(
					new Position(2, 1),
					new Edge(new Position(2,2), new Position(2, 1))));
			expectedList.Add(
				new RealEdge(
					new Position(0,1),
					new Edge(new Position(2, 2), new Position(0,1))));

			var ret = underTest.BuilderGetEdges(new Position(2,2));

			CollectionAssert.AreEquivalent(expectedList, ret.Item1);
		}
		
	}
}
