using System;
using System.Linq;
using Lanboost.PathFinding.Astar;
using Lanboost.PathFinding.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LanboostPathFindingTest
{

	

	[TestClass]
	public class TileGraphTest
	{
		[TestMethod]
		public void PositionShouldBeEqual()
		{
			Assert.AreEqual(new Tuple<int, int>(1,1), new Tuple<int, int>(1, 1));
		}

		[TestMethod]
		public void ShouldHandleGridEdges()
		{
			var tileGraph = new TileGraph(new bool[][]
			{
				new bool[]{true }
			});

			var ret = tileGraph.GetEdges(new Tuple<int, int>(0,0)).ToList();
			Assert.AreEqual(0, ret.Count);
		}

		[TestMethod]
		public void ShouldFindAllNeighbors()
		{
			var tileGraph = new TileGraph(new bool[][]
			{
				new bool[]{ true, true , true},
				new bool[]{ true, true , true},
				new bool[]{ true, true , true}
			});

			var ret = tileGraph.GetEdges(new Tuple<int, int>(1, 1)).ToList();
			Assert.AreEqual(4, ret.Count);
		}

		[TestMethod]
		public void ShouldRespectBlock()
		{
			var tileGraph = new TileGraph(new bool[][]
			{
				new bool[]{ true, false, true},
				new bool[]{ false, true , false},
				new bool[]{ true, false, true}
			});

			var ret = tileGraph.GetEdges(new Tuple<int, int>(1, 1)).ToList();
			Assert.AreEqual(0, ret.Count);
		}


	}
}
