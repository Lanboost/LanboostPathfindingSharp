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
	
	using RealEdge = Lanboost.PathFinding.Graph.Edge<Position, Edge>;

	[TestClass]
	public class SubgoalGraph2DTest
	{
		[TestMethod]
		public void BuilderGetNodesShouldContainSubGoals()
		{
			var gridWorld = new GridWorld(new bool[][]
			{
				new bool[]{ false, true},
				new bool[]{ true, true},
			});
			var underTest = new SubgoalGraph2D<int>(gridWorld);

			underTest.Create();
			
			var expectedList = new List<Position>();
			expectedList.Add(new Position(1, 1));

			CollectionAssert.AreEqual(expectedList, underTest.GetNodes().ToList());
		}

		[TestMethod]
		public void BuilderGetNodesShouldOnlyContainSubGoals()
		{
			var gridWorld = new GridWorld(new bool[][]
			{
				new bool[]{ false, true},
				new bool[]{ false, true},
			});
			var underTest = new SubgoalGraph2D<int>(gridWorld);
			underTest.Create();
			var expectedList = new List<Position>();

			CollectionAssert.AreEqual(expectedList, underTest.GetNodes().ToList());
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
			var underTest = new SubgoalGraph2D<int>(gridWorld);
			underTest.Create();
			var expectedList = new List<Position>();
			expectedList.Add(new Position(2, 2));
			expectedList.Add(new Position(0, 1));
			expectedList.Add(new Position(2, 1));
			var ret = underTest.GetNodes().ToList();
			CollectionAssert.AreEquivalent(expectedList, ret);
		}

		[TestMethod]
		public void FindSubgoalsShouldFindAllSubGoals()
		{
			var gridWorld = new GridWorld(new bool[][]
			{
				new bool[]{ true, false, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, false},
			});
			var underTest = new SubgoalGraph2D<int>(gridWorld);
			underTest.Create();

			var ret = underTest.findSubgoalsFrom(new Position(0, 3)).ToList();
			var expectedList = new List<Position>();
			expectedList.Add(new Position(2, 2));
			expectedList.Add(new Position(0, 1));
			expectedList.Add(new Position(2, 1));
			CollectionAssert.AreEquivalent(expectedList, ret);
		}

		[TestMethod]
		public void AddTemporaryStartEndNodesShouldAddEdgeToEnd()
		{
			var gridWorld = new GridWorld(new bool[][]
			{
				new bool[]{ true, false, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, false},
			});
			var underTest = new SubgoalGraph2D<int>(gridWorld);
			underTest.Create();

			underTest.AddTemporaryStartEndNodes(new Position(3, 0), new Position(0, 3));
			var ret = underTest.GetEdges(new Position(0, 1)).Select(x => x.to).ToList();

			var expectedList = new List<Position>();
			expectedList.Add(new Position(0, 3));
			expectedList.Add(new Position(2, 2));
			expectedList.Add(new Position(2, 1));
			
			CollectionAssert.AreEquivalent(expectedList, ret);
		}

		[TestMethod]
		public void ShouldFindPath()
		{
			var gridWorld = new GridWorld(new bool[][]
			{
				new bool[]{ true, false, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, false},
			});
			var underTest = new SubgoalGraph2D<int>(gridWorld);
			underTest.Create();

			var astar = new AStar<Position, SimpleEdge<int>>(underTest, 100);
			var ret = astar.FindPath(new Position(0, 0), new Position(0, 3));

			Assert.AreEqual(null, ret);

			var path = astar.GetPath();
		}

		[TestMethod]
		public void ShouldNotGoDiagonalyIfBlocked()
		{
			var gridWorld = new GridWorld(new bool[][]
			{
				new bool[]{ true, false, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, false},
				new bool[]{ true, true, false, true},
			});
			var underTest = new SubgoalGraph2D<int>(gridWorld);
			underTest.Create();

			var astar = new AStar<Position, SimpleEdge<int>>(underTest, 100);
			var ret = astar.FindPath(new Position(0, 0), new Position(3, 3));

			Assert.AreEqual("No path exists.", ret);
		}


	}
	/*
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
					new Edge(new Position(2, 2), new Position(2, 1))));
			expectedList.Add(
				new RealEdge(
					new Position(0, 1),
					new Edge(new Position(2, 2), new Position(0, 1))));

			var ret = underTest.BuilderGetEdges(new Position(2, 2));

			CollectionAssert.AreEquivalent(expectedList, ret.Item1);
		}

		[TestMethod]
		public void BuilderGetEdgesShouldSkipDiagonalIfBlockedOnSides()
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
					new Position(0, 1),
					new Edge(new Position(0, 0), new Position(0, 1))));

			var ret = underTest.BuilderGetEdges(new Position(0, 0));
			var item1 = ret.Item1;
			CollectionAssert.AreEquivalent(expectedList, item1);

			var expectedDict = new Dictionary<Position, RealEdge>();
			expectedDict.Add(new Position(0, 1),
				new RealEdge(
					new Position(0, 0),
					new Edge(new Position(0, 1), new Position(0, 0))
				)
			);

			var item2 = ret.Item2;
			CollectionAssert.AreEquivalent(expectedDict, item2);
		}

		[TestMethod]
		public void BuilderGetNodesShouldNotGenerateGoalOnBlockedTile()
		{
			var gridWorld = new GridWorld(new bool[][]
			{
				new bool[]{ false, true},
				new bool[]{ true, false},
			});
			var underTest = new SubGoalGraphBuilder2D<Position, Edge>(gridWorld);

			var expectedList = new List<Position>();
			var ret = underTest.BuilderGetNodes().ToList();
			CollectionAssert.AreEqual(expectedList, ret);
		}


		[TestMethod]
		public void TestDirectionalPathFinding()
		{
			var gridWorld = new GridWorld(new bool[][]
			{
				new bool[]{ true, false, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, false},
			});
			var underTest = new SubGoalGraphBuilder2D<Position, Edge>(gridWorld);

			var directeGraph = new DynamicDirectedGraph<Position, Edge>(underTest);
			directeGraph.Load();
			var astar = new AStar<Position, Edge>(directeGraph, 100);

			Assert.AreEqual(null, astar.FindPath(new Position(0, 0), new Position(2, 3)));
		}

		[TestMethod]
		public void TestDirectionalPathFindingAndPath()
		{
			var gridWorld = new GridWorld(new bool[][]
			{
				new bool[]{ true, false, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, true},
				new bool[]{ true, true, true, false},
			});
			var underTest = new SubGoalGraphBuilder2D<Position, Edge>(gridWorld);

			var directeGraph = new DynamicDirectedGraph<Position, Edge>(underTest);
			directeGraph.Load();
			var astar = new AStar<Position, Edge>(directeGraph, 100);

			Assert.AreEqual(null, astar.FindPath(new Position(0, 0), new Position(2, 3)));

			var path = astar.GetPath();
		}

		[TestMethod]
		public void DirecitonalPathFindingShouldNotHaveMultipleLinksToSameNode()
		{
			//This currently fails...test and fix
			Assert.Fail();
		}
	}*/
}
