#define ENABLE_PATH_STEPS

using Lanboost.PathFinding.GraphBuilders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lanboost.PathFinding.Graph
{

#if ENABLE_PATH_STEPS
	public class GenerationStep
	{
		public string Step;
		public string SubStep;

		public List<HighLight> highLights = new List<HighLight>();
		public List<TileData> tileDatas = new List<TileData>();

        public GenerationStep(string step, string subStep, params HighLight[] highLights)
        {
            Step = step;
            SubStep = subStep;
            this.highLights.AddRange(highLights);
        }

		public GenerationStep AddTileData(params TileData[] tileDatas)
		{
            this.tileDatas.AddRange(tileDatas);
			return this;
        }
    }

	public enum HighLightColor
	{
		WHITE=0,
		RED = 1,
		GREEN = 2,
		BLUE = 3,
		PURPLE = 4,
		CYAN = 5,
		YELLOW = 6
	}

    public enum HighLightForm
    {
        BORDERBOX = 0,
		BOX = 1,
		CIRCLE = 2,
		DIAMOND = 3,
		BLOCKED = 4,
		NONE = 5,
    }


    public class HighLight
	{
        
        public HighLightColor highLightColor;
        public Position start = null;
		public Position end = null;

        public HighLight(HighLightColor highlightColor, Position start, Position end = null)
        {
            this.highLightColor = highlightColor;
            this.start = start;
            this.end = end;
        }
    }

    public class TileData
    {
        public Position position = null;

        public HighLightForm highLightForm;
        public HighLightColor highLightColor;

		// Must be set by the one that steps, will not be set by graph code
		public bool prev = false;
        public HighLightForm prevHighLightForm;
        public HighLightColor prevHighLightType;

        public TileData(Position position, HighLightForm highLightForm, HighLightColor highLightType)
        {
            this.position = position;
            this.highLightForm = highLightForm;
            this.highLightColor = highLightType;
        }
    }

#endif


    /// <summary>
    /// Interface to implement for any graph to be able to run pathfinding on it.
    /// </summary>
    public interface IGraph<N, L>
	{
		int GetEstimation(N from, N to);
		IEnumerable<Edge<N, L>> GetEdges(N node);
		int GetCost(N from, N to, L link);
		void AddTemporaryStartEndNodes(N start, N end);
	}

	public interface Link
	{
		int getCost();
	}
	public class SimpleEdge<I>: Link
	{
		public Func<I, Boolean> requirement;
		Position start;
		Position end;
		int cost = -1;

		public SimpleEdge(Position start, Position end, int cost = -1)
		{
			this.start = start;
			this.end = end;
			this.cost = cost;
		}

		public int getCost()
		{
			if (cost != -1)
			{
				return cost;
			}
			return Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y) + Math.Abs(start.plane - end.plane)*10;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(SimpleEdge<I>))
			{
				var i = (SimpleEdge<I>)obj;
				return i.start.Equals(start) && i.end.Equals(end) && i.cost.Equals(cost) && i.requirement.Equals(requirement);
			}
			return false;
		}

		public override int GetHashCode()
		{
			var hashCode = -740587475;
			hashCode = hashCode * -1521134295 + EqualityComparer<Position>.Default.GetHashCode(start);
			hashCode = hashCode * -1521134295 + EqualityComparer<Position>.Default.GetHashCode(end);
			hashCode = hashCode * -1521134295 + EqualityComparer<int>.Default.GetHashCode(cost);
			hashCode = hashCode * -1521134295 + EqualityComparer<Func<I, Boolean>>.Default.GetHashCode(requirement);
			return hashCode;
		}
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
			if (obj.GetType() == typeof(Edge<P, L>))
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

	public interface World2D {
		int GetChunkSize();
		IEnumerable<Position> GetChunks();
		IEnumerable<Boolean> GetChunkBlocedPositions(Position chunk);
	}

	public class SubGoal2DChunk
	{
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

		public SubGoal2DChunk(int width, int height)
		{
			this.blockType = new byte[height][];
			for (int y = 0; y < height; y++)
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

		public void SetDistance(int x, int y, int dir, int distance, byte distType)
		{
            this.distance[dir/2][y][x] = (short)distance;
			distanceType[dir/2][y][x] = distType;
		}

		public int GetDistance(int x, int y, int dir)
		{
			return distance[dir / 2][y][x];
		}

		public int GetDistanceType(int x, int y, int dir)
		{
			return distanceType[dir / 2][y][x];
		}
	}

	public interface Graph2DExtender<I>
	{
		IEnumerable<Position> GetExtraNodes();

		IEnumerable<Edge<Position, SimpleEdge<I>>> GetEdges(Position node);

		IEnumerable<Func<Position, Edge<Position, SimpleEdge<I>>>> GetGlobalEdgeFunc();

		int GetEstimation(Position from, Position to);
	}

	/** Cursor to a position in the world
	 * 
	 * Used to easily step between chunks
	 */
	struct PositionCursor
	{
		int dirX;
		int dirY;
		public int offsetX;
		public int offsetY;
		public SubGoal2DChunk cache;
		public Position area;
		TileDirection dir;
		int height;
		int width;
		bool error;

		public Dictionary<Position, SubGoal2DChunk> areas;

		public PositionCursor(int chunkSize, Dictionary<Position, SubGoal2DChunk> areas, Position area, int x, int y, TileDirection dir)
		{
			this.areas = areas;
			var off = TileDirectionUtils.TileDirectionOffset(dir);
			this.dir = dir;
			dirX = off[0];
			dirY = off[1];
			offsetX = x;
			offsetY = y;
			cache = areas[area];
			this.area = area;
			this.width = chunkSize;
			this.height = chunkSize;
			error = false;
		}

		public bool Step()
		{
			var pox = offsetX;
			var poy = offsetY;
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
			else if (offsetX >= width)
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
				var o = TileDirectionUtils.TileDirectionOffset(dir);
				var k = new Position(o[0]+area.x, o[1]+area.y, area.plane);
				if(!areas.ContainsKey(k)) {
					//SubgoalGraph2D.logs.Add("" + pox+","+poy+","+dirX+","+dirY+","+k.x+","+k.y);
					error = true;
					return false;
				}
				cache = areas[k];
				area = k;
			}

			return true;
		}

		public bool StepWithDiagonalBlockCheck()
		{
			var ndirs = TileDirectionUtils.SplitDiagonal(this.dir);
			for(int i=0; i<ndirs.Length; i++)
			{
				var ao1 = this.ChangeDirection(ndirs[i]);
				if(!ao1.Step() || ao1.Get() == SubGoal2DChunk.TYPE_BLOCKED)
				{
					return false;
				}
			}
			return this.Step();
		}

		public int Get()
		{
			return cache.Get(offsetX, offsetY);
		}

		public void SetDistance(int dir, int distance, int distType)
		{
            cache.SetDistance(offsetX, offsetY, dir, distance, (byte) distType);
		}

		public int getDistance(int dir)
		{
			return cache.GetDistance(offsetX, offsetY, dir);
		}

		public int GetDistanceType(int dir)
		{
			return cache.GetDistanceType(offsetX, offsetY, dir);
		}

		public PositionCursor ChangeDirection(TileDirection dir)
		{
			return new PositionCursor(this.width, areas, area, offsetX, offsetY, dir);
		}

		public Position GetPosition()
		{
			return new Position(this.area.x * this.width+ offsetX, this.area.y * this.width+ offsetY, this.area.plane);
		}

		public TileDirection GetDirection()
		{
			return this.dir;
		}

		public bool StepFailed()
		{
			return error;
		}

		public PositionCursor Copy()
		{
			return new PositionCursor(this.width, areas, area, offsetX, offsetY, dir);
		}
	}

	public class SubgoalGraph2D<I>: IGraph<Position, SimpleEdge<I>>
	{
#if ENABLE_PATH_STEPS
		public List<GenerationStep> generationSteps = new List<GenerationStep>();
#endif
		public Dictionary<Position, SubGoal2DChunk> chunks = new Dictionary<Position, SubGoal2DChunk>();
		World2D world;

		Position start;
		List<Edge<Position, SimpleEdge<I>>> startEdges;
		List<Func<Position, Edge<Position, SimpleEdge<I>>>> globalStartEdges = new List<Func<Position, Edge<Position, SimpleEdge<I>>>>();
		Position end;
		Dictionary<Position, Edge<Position, SimpleEdge<I>>> endEdges;

		Dictionary<Position, List<Edge<Position, SimpleEdge<I>>>> subgoalEdges = new Dictionary<Position, List<Edge<Position, SimpleEdge<I>>>>();

		List<Graph2DExtender<I>> extenders = new List<Graph.Graph2DExtender<I>>();
		public static List<String> logs = new List<String>();

		public SubgoalGraph2D(World2D world, List<Graph.Graph2DExtender<I>> extenders = null)
		{
			this.world = world;
			if(extenders != null)
			{
				this.extenders = extenders;
			}
		}

		public IEnumerable<Position> GetNodes()
		{
			foreach(var v in subgoalEdges.Keys)
			{
				yield return v;
			}
		}

		public void Create()
		{
			var chunkSize = world.GetChunkSize();
			foreach (var c in world.GetChunks())
			{
				var sgc = new SubGoal2DChunk(chunkSize, chunkSize);
				var iter = world.GetChunkBlocedPositions(c);
				int y = 0;
				int x = 0;
				foreach(var blocked in world.GetChunkBlocedPositions(c))
				{
					if (blocked)
					{
#if ENABLE_PATH_STEPS
                        generationSteps.Add(new GenerationStep(
                            "Create Blocked Positions",
                            "Blocking Position"
                        ).AddTileData(new TileData(new Position(c.x*chunkSize+x, c.y*chunkSize+y, c.plane), HighLightForm.BLOCKED, HighLightColor.RED)));
#endif
                        sgc.Set(x, y, SubGoal2DChunk.TYPE_BLOCKED);
					}
					else
					{
						sgc.Set(x, y, SubGoal2DChunk.TYPE_OPEN);
					}

					x++;
					if(x >= chunkSize)
					{
						x = 0;
						y++;
					}
				}
				chunks.Add(c, sgc);
			}

			createSubgoals();
			createClearanceMap();
			createSubgoalEdges();


			foreach(var e in extenders)
			{
				foreach (var f in e.GetGlobalEdgeFunc())
				{
					globalStartEdges.Add(f);
				}
			}
		}

		private void createSubgoalEdges()
		{
			var chunkSize = world.GetChunkSize();
			foreach (var kv in chunks)
			{
				var area = kv.Value;
				for (int y = 0; y < chunkSize; y++)
				{
					for (int x = 0; x < chunkSize; x++)
					{
						if(area.Get(x, y) == SubGoal2DChunk.TYPE_SUBGOAL)
						{
							var p = new Position(kv.Key.x * chunkSize + x, kv.Key.y * chunkSize + y, kv.Key.plane);
							subgoalEdges.Add(p, createEdgesFor(p));
						}
					}
				}
			}
		}

		private List<Edge<Position, SimpleEdge<I>>> createEdgesFor(Position p)
		{
			var l = new List<Edge<Position, SimpleEdge<I>>>();
			foreach (var s in findSubgoalsFrom(p))
			{
				l.Add(new Edge<Position, SimpleEdge<I>>(s, new SimpleEdge<I>(p, s)));
			}
#if ENABLE_PATH_STEPS
			HighLight[] highLights = new HighLight[l.Count+1];
			for(int i = 0; i < highLights.Length-1; i++)
			{
				highLights[i+1] = new HighLight(HighLightColor.PURPLE, l[i].to);
            }
			highLights[0] = new HighLight(HighLightColor.YELLOW, p);

            generationSteps.Add(new GenerationStep(
                "Link Subgoals",
				"Links found for subgoal",
                highLights
            ));
#endif

            foreach (var e in extenders)
			{
				foreach (var f in e.GetEdges(p))
				{
					l.Add(f);
				}
			}
			return l;
		}

		private bool ShouldPositonBeSubgoal(Position key)
		{
			if (safeGet(key) == SubGoal2DChunk.TYPE_BLOCKED)
			{
#if ENABLE_PATH_STEPS
                generationSteps.Add(new GenerationStep(
                    "Creating Subgoals",
                    "Position Blocked",
                    new HighLight(HighLightColor.RED, key)
                ));
#endif
                return false;
			}

			// Requirements for subgoal
			// 1. Diagonal is blocked
			// 2. Both non diagonals are free
			// 3. Check in all diagonal directions

			foreach(var diagonal in TileDirectionUtils.DiagonalDirections)
			{
				var offset = TileDirectionUtils.TileDirectionOffset(diagonal);
#if ENABLE_PATH_STEPS
                generationSteps.Add(new GenerationStep(
                    "Creating Subgoals",
                    "Check that diagonal is blocked",
                    new HighLight(HighLightColor.YELLOW, key),
                    new HighLight(HighLightColor.BLUE, new Position(key.x + offset[0], key.y + offset[1], key.plane))
                ));
#endif


                if (safeGet(new Position(key.x + offset[0], key.y + offset[1], key.plane)) == SubGoal2DChunk.TYPE_BLOCKED)
				{
					var ortogonalDirections = TileDirectionUtils.SplitDiagonal(diagonal);
					bool failedCheck = false;
                    foreach (var ortogonalDirection in ortogonalDirections)
					{
                        var offsetOrtogonal = TileDirectionUtils.TileDirectionOffset(ortogonalDirection);
#if ENABLE_PATH_STEPS
                        generationSteps.Add(new GenerationStep(
							"Creating Subgoals",
							"Check that ortogonal position is free",
							new HighLight(HighLightColor.YELLOW, key),
							new HighLight(HighLightColor.BLUE, new Position(key.x + offsetOrtogonal[0], key.y + offsetOrtogonal[1], key.plane))
						));
#endif

                        if (safeGet(new Position(key.x + offsetOrtogonal[0], key.y + offsetOrtogonal[1], key.plane)) == SubGoal2DChunk.TYPE_BLOCKED)
						{
#if ENABLE_PATH_STEPS
							generationSteps.Add(new GenerationStep(
								"Creating Subgoals",
                                "Failed check: ortogonal position is blocked",
								new HighLight(HighLightColor.YELLOW, key),
								new HighLight(HighLightColor.RED, new Position(key.x + offsetOrtogonal[0], key.y + offsetOrtogonal[1], key.plane))
							));
#endif
                            failedCheck = true;
							break;
                        }
                    }
					if (!failedCheck)
					{
						return true;
					}
                }
				else
				{
#if ENABLE_PATH_STEPS
					generationSteps.Add(new GenerationStep(
						"Creating Subgoals",
						"Diagonal is not blocked",
						new HighLight(HighLightColor.YELLOW, key),
						new HighLight(HighLightColor.RED, new Position(key.x + offset[0], key.y + offset[1], key.plane))
					));
#endif
                }
            }
			return false;
		}

		private int safeGet(Position key)
		{
			if(key.x < 0 || key.y < 0)
			{
				return SubGoal2DChunk.TYPE_BLOCKED;
			}

			var chunk = GetChunkFromGlobalPosition(key);
			if (chunk == null)
			{
				return SubGoal2DChunk.TYPE_BLOCKED;
			}
			
			var p = GetLocalChunkPositionFromGlobalPosition(key);
			
			return chunk.Get(p.x, p.y);
		}

		private void createSubgoals()
		{
#if ENABLE_PATH_STEPS
			generationSteps.Add(new GenerationStep(
				"Creating Subgoals",
				"Looking for position with potential for sub goal"
			));
#endif
            var chunkSize = world.GetChunkSize();
			foreach (var kv in chunks)
			{
				var chunk = kv.Value;
				for (int y = 0; y < chunkSize; y++) {
					for (int x = 0; x < chunkSize; x++)
					{
#if ENABLE_PATH_STEPS
						generationSteps.Add(new GenerationStep(
							"Creating Subgoals",
							"Check specific position for subgoal",
							new HighLight(HighLightColor.YELLOW, new Position(kv.Key.x * chunkSize + x, kv.Key.y * chunkSize + y, kv.Key.plane))
						));
#endif
						if (ShouldPositonBeSubgoal(new Position(kv.Key.x* chunkSize + x, kv.Key.y * chunkSize + y, kv.Key.plane)))
						{
#if ENABLE_PATH_STEPS
							generationSteps.Add(new GenerationStep(
								"Creating Subgoals",
								"Found subgoal",
								new HighLight(HighLightColor.GREEN, new Position(kv.Key.x * chunkSize + x, kv.Key.y * chunkSize + y, kv.Key.plane))
							).AddTileData(new TileData(new Position(kv.Key.x * chunkSize + x, kv.Key.y * chunkSize + y, kv.Key.plane), HighLightForm.CIRCLE, HighLightColor.GREEN)));
#endif
                            chunk.Set(x, y, SubGoal2DChunk.TYPE_SUBGOAL);
						}
					}
				}
			}

			foreach(var e in extenders)
			{
				foreach (var n in e.GetExtraNodes())
				{
					var chunk = GetChunkFromGlobalPosition(n);
					var p = GetLocalChunkPositionFromGlobalPosition(n);
					chunk.Set(p.x, p.y, SubGoal2DChunk.TYPE_SUBGOAL);
				}
			}
		}

		private SubGoal2DChunk GetChunkFromGlobalPosition(Position n)
		{
			var p= new Position(n.x / world.GetChunkSize(), n.y / world.GetChunkSize(), n.plane);
			if(chunks.ContainsKey(p))
			{
				return chunks[p];
			}
			return null;
		}

		private Position GetAreaKeyFromWorldPos(Position n)
		{
			return new Position(n.x / world.GetChunkSize(), n.y / world.GetChunkSize(), n.plane);
		}

		private Position GetLocalChunkPositionFromGlobalPosition(Position n)
		{
			return new Position(n.x % world.GetChunkSize(), n.y % world.GetChunkSize(), n.plane);
		}


        /** Create a distance / clearance map 
		 * 
		 * Used for fast lookup of clearance in findDirectHReachable
		 */
        private void createClearanceMap()
		{
			var directions = TileDirectionUtils.OrthogonalDirections;


			var chunkSize = world.GetChunkSize();
			foreach (var value in chunks)
			{
				var areaKey = value.Key;
				var area = value.Value;

				for (int y = 0; y < chunkSize; y++)
				{
					for (int x = 0; x < chunkSize; x++)
					{
						if(area.Get(x, y) != SubGoal2DChunk.TYPE_OPEN)
						{
#if ENABLE_PATH_STEPS
							generationSteps.Add(new GenerationStep(
                                "Creating Clearence Map",
                                "Position is blocked, investigate clearence",
                                new HighLight(
                                    HighLightColor.YELLOW, new Position(areaKey.x * chunkSize + x, areaKey.y * chunkSize + y, areaKey.plane)
                                )
                            ));
#endif
                            foreach (var tileDirection in directions)
							{
								var oppositeDirection = TileDirectionUtils.Opposite(tileDirection);
								int dist = 0;
								PositionCursor ao = new PositionCursor(world.GetChunkSize(), this.chunks, areaKey, x, y, tileDirection);
								while (ao.Step())
								{
									dist++;
									if (ao.Get() != SubGoal2DChunk.TYPE_OPEN)
									{
#if ENABLE_PATH_STEPS
										generationSteps.Add(new GenerationStep(
											"Creating Clearence Map",
											$"Position is blocked, set clearance to {dist}",
											new HighLight(
												HighLightColor.YELLOW, new Position(areaKey.x * chunkSize + x, areaKey.y * chunkSize + y, areaKey.plane)
											),
                                            new HighLight(
                                                HighLightColor.RED, new Position(ao.area.x * chunkSize + ao.offsetX, ao.area.y * chunkSize + ao.offsetY, areaKey.plane)
                                            )
                                        ));
#endif
                                        ao.SetDistance(oppositeDirection, dist, area.Get(x, y));
										break;
									}
									else
									{
#if ENABLE_PATH_STEPS
										generationSteps.Add(new GenerationStep(
                                            "Creating Clearence Map",
                                            "Position is blocked, investigate clearence",
                                            new HighLight(
                                                HighLightColor.YELLOW, new Position(areaKey.x * chunkSize + x, areaKey.y * chunkSize + y, areaKey.plane)
                                            ),
                                            new HighLight(
                                                HighLightColor.BLUE, new Position(ao.area.x * chunkSize + ao.offsetX, ao.area.y * chunkSize + ao.offsetY, areaKey.plane)
                                            )
                                        ));
#endif
                                        ao.SetDistance(oppositeDirection, dist, area.Get(x, y));
										
									}
								}
								if(dist == 0)
								{
#if ENABLE_PATH_STEPS
									generationSteps.Add(new GenerationStep(
                                        "Creating Clearence Map",
                                        $"Position is out of bound, set clearance to {dist}",
                                        new HighLight(
                                            HighLightColor.YELLOW, new Position(areaKey.x * chunkSize + x, areaKey.y * chunkSize + y, areaKey.plane)
                                        )
                                    ));
#endif
                                    //SubgoalGraph2D.logs.Add("dist=0:" + x + "," + y + "," + dir + "," + dist);
                                }
							}
						}
						else
						{
#if ENABLE_PATH_STEPS
							generationSteps.Add(new GenerationStep(
                                "Creating Clearence Map",
                                "Position is open: skip",
                                new HighLight(
									HighLightColor.RED, new Position(areaKey.x * chunkSize + x, areaKey.y * chunkSize + y, areaKey.plane)
								)
                            ));
#endif
                        }
                    }
				}
			}
		}

		public IEnumerable<Position> findSubgoalsFrom(Position pos, bool returnExplored = false)
		{
#if ENABLE_PATH_STEPS
			generationSteps.Add(new GenerationStep(
                "Link Subgoals",
                "Find links for subgoal",
                new HighLight(
                    HighLightColor.YELLOW, pos
                )
            ));
#endif

            foreach(var diagonalDirection in TileDirectionUtils.DiagonalDirections)
			{
				var v = GetLocalChunkPositionFromGlobalPosition(pos);
				PositionCursor ao = new PositionCursor(world.GetChunkSize(), this.chunks, GetAreaKeyFromWorldPos(pos), v.x, v.y, diagonalDirection);
				
				var ortogonalDirections = TileDirectionUtils.SplitDiagonal(diagonalDirection);
                var dmaxBoth = new int[] { ao.getDistance((int)ortogonalDirections[0]), ao.getDistance((int)ortogonalDirections[1]) };

				

				while (ao.StepWithDiagonalBlockCheck())
				{
#if ENABLE_PATH_STEPS
					generationSteps.Add(new GenerationStep(
						"Link Subgoals",
						"Find links for subgoal",
						new HighLight(
							HighLightColor.YELLOW, pos
						),
                        new HighLight(
                            HighLightColor.BLUE, ao.GetPosition()
                        )
                    ));
#endif
                    if (ao.Get() == SubGoal2DChunk.TYPE_SUBGOAL)
					{
						var p = ao.GetPosition();
						yield return new Position(p.x, p.y, p.plane);
						break;
					}
					else if (ao.Get() == SubGoal2DChunk.TYPE_OPEN)
					{
						for (int cdir = 0; cdir < 2; cdir++)
						{
							var d = ortogonalDirections[cdir];

							var offset = TileDirectionUtils.TileDirectionOffset(d);
							if (ao.GetDistanceType((int)d) == SubGoal2DChunk.TYPE_SUBGOAL && ao.getDistance((int)d) <= dmaxBoth[cdir])
							{
								var p = ao.GetPosition();
								yield return new Position(p.x + offset[0] * ao.getDistance((int)d), p.y + offset[1] * ao.getDistance((int)d), p.plane);
							}
							dmaxBoth[cdir] = Math.Min(dmaxBoth[cdir], ao.getDistance((int)d) - 1);
							if (returnExplored)
							{
								{
									var p = ao.GetPosition();
									yield return new Position(p.x, p.y, p.plane);
								}
								for (int i = 0; i <= dmaxBoth[cdir]; i++)
								{
									var p = ao.GetPosition();
									yield return new Position(p.x + offset[0] * i, p.y + offset[1] * i, p.plane);
								}
							}
						}
					}
					else
					{
						break;
					}
				}
			}

            foreach (var orthogonalDirection in TileDirectionUtils.OrthogonalDirections)
            {
                var v = GetLocalChunkPositionFromGlobalPosition(pos);
				PositionCursor ao = new PositionCursor(world.GetChunkSize(), this.chunks, GetAreaKeyFromWorldPos(pos), v.x, v.y, orthogonalDirection);
				var offset = TileDirectionUtils.TileDirectionOffset(orthogonalDirection);
				if (returnExplored)
				{
					for (int i = 0; i < ao.getDistance((int)orthogonalDirection); i++)
					{
						var p = ao.GetPosition();
						yield return new Position(p.x + offset[0] * i, p.y + offset[1] * i, p.plane);
					}
				}
				else
				{
					if (ao.GetDistanceType((int)orthogonalDirection) == SubGoal2DChunk.TYPE_SUBGOAL)
					{
						
						var p = ao.GetPosition();
						yield return new Position(p.x + offset[0] * ao.getDistance((int)orthogonalDirection), p.y + offset[1] * ao.getDistance((int)orthogonalDirection), p.plane);
					}
				}
			}
		}

		public int GetEstimation(Position start, Position end)
		{
			int estimation = ManhattanDistance(start, end);
			foreach (var e in extenders)
			{
				estimation = Math.Min(e.GetEstimation(start, end), estimation);
			}
			return estimation;
		}

		public static int ManhattanDistance(Position start, Position end)
		{
			return Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y) + Math.Abs(start.plane - end.plane) * 10;
		}

		public IEnumerable<Edge<Position, SimpleEdge<I>>> GetEdges(Position node)
		{
			if (node.Equals(start))
			{
				if (startEdges != null)
				{
					foreach (var e in startEdges)
					{
						yield return e;
					}
				}
				else
				{
					foreach (var v in subgoalEdges[node])
					{
						yield return v;
					}
				}
				foreach (var e in globalStartEdges)
				{
					yield return e(start);
				}
			}
			else
			{
				if (endEdges != null && endEdges.ContainsKey(node))
				{
					yield return endEdges[node];
				}
				foreach (var v in subgoalEdges[node])
				{
					yield return v;
				}
			}
		}

		public int GetCost(Position from, Position to, SimpleEdge<I> link)
		{
			return link.getCost();
		}

		public void AddTemporaryStartEndNodes(Position start, Position end)
		{
			this.start = start;
			if (subgoalEdges.ContainsKey(start))
			{
				this.startEdges = null;
			}
			else
			{
				var subgoals = findSubgoalsFrom(start);
				startEdges = new List<Edge<Position, SimpleEdge<I>>>();
				foreach (var v in subgoals)
				{
					startEdges.Add(new Edge<Position, SimpleEdge<I>>(v, new SimpleEdge<I>(start, v)));
				}
			}
			this.end = end;
			if(subgoalEdges.ContainsKey(end))
			{
				this.end = null;
				endEdges = null;
			}
			else
			{
				var subgoals = findSubgoalsFrom(end);
				endEdges = new Dictionary<Position, Edge<Position, SimpleEdge<I>>>();
				foreach(var v in subgoals)
				{
					endEdges.Add(v, new Edge<Position, SimpleEdge<I>>(end, new SimpleEdge<I>(v, end)));
				}
			}
		}
	}
}