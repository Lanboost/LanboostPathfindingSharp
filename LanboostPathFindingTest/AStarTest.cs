using System;
using System.Collections.Generic;
using Lanboost.PathFinding.Astar;
using Lanboost.PathFinding.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LanboostPathFindingTest
{
	

	[TestClass]
	public class AStarTest
	{
		[TestMethod]
		public void ShouldFailIfStartAndEndIsSame()
		{
			var mock = new Mock<IGraph<int, int>>();

			var astar = new AStar<int, int>(mock.Object, 100);

			Assert.AreEqual("Start and end cannot be the same.", astar.FindPath(1, 1));
		}

		[TestMethod]
		public void ShouldFindPath()
		{
			var tileGraph = new TileGraph(new bool[][]
			{
				new bool[]{ true, true, true},
				new bool[]{ false, true , false},
				new bool[]{ true, true, true}
			});

			var astar = new AStar<Position, Edge>(tileGraph, 100);
			var start = new Position(0, 0);
			var end = new Position(0, 2);
			var expectedPath = new List<Edge>();
			expectedPath.Add(new Edge(new Position(0, 0), new Position(1, 0)));
			expectedPath.Add(new Edge(new Position(1, 0), new Position(1, 1)));
			expectedPath.Add(new Edge(new Position(1, 1), new Position(1, 2)));
			expectedPath.Add(new Edge(new Position(1, 2), new Position(0, 2)));

			Assert.AreEqual(null, astar.FindPath(start, end));
			var path = astar.GetPath();
			Assert.AreEqual(expectedPath.Count, path.Count);
			CollectionAssert.AreEqual(expectedPath, path);
		}

		[TestMethod]
		public void ShouldHitMaxExpansions()
		{
			var tileGraph = new TileGraph(new bool[][]
			{
				new bool[]{ true, true, true},
				new bool[]{ false, true , false},
				new bool[]{ true, true, true}
			});

			var astar = new AStar<Position, Edge>(tileGraph, 2);
			var start = new Position(0, 0);
			var end = new Position(2, 0);

			Assert.AreEqual("Hit max expansions.", astar.FindPath(start, end));
		}
	}
}
