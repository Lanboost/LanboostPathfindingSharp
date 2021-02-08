using geniikw.DataRenderer2D;
using Lanboost.PathFinding.Astar;
using Lanboost.PathFinding.Graph;
using Lanboost.PathFinding.GraphBuilders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class CameraMove : MonoBehaviour, IPointerClickHandler
{
	[System.NonSerialized]
	float zoom = 70;
	[System.NonSerialized]
	public Vector3 position = new Vector3(50, 50, 0);

	public Text positionText;
	public Tilemap tilemapPrefab;
	public GameObject tilemapParent;
	public GameObject gameObjectWorldLine;
	public Button modebutton;
	public Button addPortal;
	public Button clearPortal;
	public Button addGlobal;
	public Button clearGlobal;
	[System.NonSerialized]
	public Tilemap[] tileMap;

	public TileBase[] tiles;
	public TileBase[] tileBorders;

	Vector3 mousePosition;
	bool lastDown = false;

	public static int TILEMAP_GROUND = 0;
	public static int TILEMAP_GAMEOBJECT = 1;
	public static int TILEMAP_BORDER = 2;
	public static int TILEMAP_CHUNKBORDER = TILEMAP_BORDER + 8;
	public static int TILEMAP_PATH = TILEMAP_CHUNKBORDER + 1;
	public static int TILEMAP_MOUSE = TILEMAP_PATH + 1;
	public static int TILEMAP_MOUSEOVERLAY = TILEMAP_MOUSE + 1;

	bool[][] grid;
	TileGraph tileGraph;
	GridWorld gridWorld;
	public static Vector3Int Vector3IntNotSet = new Vector3Int(-1, -1, -1);
	Vector3Int start = Vector3IntNotSet;
	Vector3Int end  = Vector3IntNotSet;
	Vector3Int subgoalExplore = Vector3IntNotSet;

	AStar<Position, Edge> aStar;
	AStar<Position, SimpleEdge<int>> directedAStar;
	SubgoalGraph2D<int> directedGraph;

	PortalExtender pe = new PortalExtender();
	GlobalExtender ge = new GlobalExtender();

	int mode = 0;

	string[] modeNames = new string[]
	{
		"None [red = blocked, green = open]",
		"Subgoals [red = blocked, green = open, black = subgoals]",
		"distance map (North) [red = blocked, black = subgoals, blue = distanceType blocked, green = distanceType subgoal]",
		"distance map (East)",
		"distance map (South)",
		"distance map (West)",
		"distance map (West)",
	};
	
	public void Start()
	{
		Application.targetFrameRate = 20;

		modebutton.onClick.AddListener(delegate ()
		{
			this.mode += 1;
			this.mode %= (modeNames.Length-1);
			RenderFloor(0);
			
		});

		addPortal.onClick.AddListener(delegate ()
		{
			var p1 = findFreePosition();
			var p2 = findFreePosition();

			pe.Add(p1, p2);

			createSubgoalGraph();
		});

		clearPortal.onClick.AddListener(delegate ()
		{
			pe.Clear();

			createSubgoalGraph();
		});

		addGlobal.onClick.AddListener(delegate ()
		{
			var p1 = findFreePosition();

			ge.Add(p1);

			createSubgoalGraph();
		});

		clearGlobal.onClick.AddListener(delegate ()
		{
			ge.Clear();

			createSubgoalGraph();
		});

		tileMap = new Tilemap[14];
		for (int i = 0; i < tileMap.Length; i++)
		{
			tileMap[i] = Instantiate<Tilemap>(tilemapPrefab, tilemapParent.transform);
			tileMap[i].GetComponent<TilemapRenderer>().sortingOrder = i;

			if (i == 0)
			{
				tileMap[i].GetComponent<TilemapRenderer>().mode = TilemapRenderer.Mode.Chunk;
			}
		}
		Generate();
	}

	Position findFreePosition()
	{
		for (int i = 0; i < 1000; i++) {
			var r = new Position((int)Random.Range(0, gridWorld.GetChunkSize()), (int)Random.Range(0, gridWorld.GetChunkSize()), 0);
			if(!gridWorld.GetBlocked(r))
			{
				return r;
			}
		}
		return new Position(0, 0, 0);
	}

	public void Generate()
	{
		int size = 100;
		grid = new bool[size][];
		for (int y = 0; y < grid.Length; y++)
		{
			grid[y] = new bool[size];
			for (int x = 0; x < grid.Length; x++)
			{
				grid[y][x] = Random.Range(0,4) != 0;
			}
		}

		//border
		for (int x = 0; x < grid.Length; x++)
		{
			grid[0][x] = false;
			grid[size - 1][x] = false;
		}

		for (int y = 0; y < grid.Length; y++)
		{
			grid[y][0] = false;
			grid[y][size - 1] = false;
		}

		tileGraph = new TileGraph(grid);
		gridWorld = new GridWorld(grid);

		createSubgoalGraph();
	}

	public void createSubgoalGraph()
	{
		var ed = new List<Lanboost.PathFinding.Graph.Graph2DExtender<int>>();
		ed.Add(pe);
		ed.Add(ge);
		directedGraph = new SubgoalGraph2D<int>(gridWorld, ed);
		
		try
		{
			directedGraph.Create();
		}
		catch (System.Exception e) { Debug.Log(e); }

		directedAStar = new AStar<Position, SimpleEdge<int>>(directedGraph, 10000);
		RenderFloor(0);
	}

	public void GeneratePath()
	{
		//Debug.Log("Astar result: "+aStar.FindPath(new Position(start.x, start.y), new Position(end.x, end.y)));

		if (start != Vector3IntNotSet && end != Vector3IntNotSet)
		{
			Debug.Log("Astar directedGraph result: " + directedAStar.FindPath(new Position(start.x, start.y), new Position(end.x, end.y)));
			Debug.Log(directedAStar.GetCost(new Position(end.x, end.y)));
		}
	}

	public void RenderFloor(int floor)
	{
		tileMap[TILEMAP_GROUND].ClearAllTiles();
		tileMap[TILEMAP_GAMEOBJECT].ClearAllTiles();


		for (int y = 0; y < grid.Length; y++)
		{
			for (int x = 0; x < grid[y].Length; x++)
			{
				var tile = tiles[0];
				if (!grid[y][x])
				{
					tile = tiles[3];
				}
				tileMap[TILEMAP_GROUND].SetTile(new Vector3Int(x, y, 0), tile);
			}
		}

		foreach(var v in pe.edges)
		{
			{
				var p = v.Key;
				tileMap[TILEMAP_GAMEOBJECT].SetTile(new Vector3Int(p.x, p.y, 0), tiles[7]);
			}
			{
				var p = v.Value;
				tileMap[TILEMAP_GAMEOBJECT].SetTile(new Vector3Int(p.x, p.y, 0), tiles[7]);
			}
		}
		foreach (var p in ge.positions)
		{
			{
				tileMap[TILEMAP_GAMEOBJECT].SetTile(new Vector3Int(p.x, p.y, 0), tiles[6]);
			}
		}

		/*if (mode == 1)
		{
			gameObjectWorldLine.SetActive(false);
			var path = aStar.GetPath();

			foreach (var p in path)
			{
				tileMap[TILEMAP_GAMEOBJECT].SetTile(new Vector3Int(p.x, p.y, 0), tiles[4]);
			}
		}
		else
		{
			gameObjectWorldLine.SetActive(true);
			foreach (var p in directedGraph.GetNodes())
			{
				tileMap[TILEMAP_GAMEOBJECT].SetTile(new Vector3Int(p.x, p.y, 0), tiles[5]);
			}
			
				var path = directedAStar.GetPath();
				
				var lineRenderer = GameObject.FindObjectOfType<WorldLine>();
				lineRenderer.line.Clear();
				List<Vector3> pos = new List<Vector3>();
				foreach (var p in path)
				{
					lineRenderer.line.Push(new Vector3(p.x + 0.5f, p.y + 0.5f, -1), Vector3.zero, Vector3.zero, 0.5f);
				}
				if (path.Count > 0)
				{
					var last = path[path.Count - 1];
					lineRenderer.line.Push(new Vector3(last.x + 0.5f, last.y + 0.5f, -1), Vector3.zero, Vector3.zero, 0.5f);
				}
		}*/
		try
		{
			var path = directedAStar.GetPath();

			var lineRenderer = GameObject.FindObjectOfType<WorldLine>();
			lineRenderer.line.Clear();
			List<Vector3> pos = new List<Vector3>();

			lineRenderer.line.Push(new Vector3(start.x + 0.5f, start.y + 0.5f, -1), Vector3.zero, Vector3.zero, 0.5f);
			foreach (var p in path)
			{
				lineRenderer.line.Push(new Vector3(p.x + 0.5f, p.y + 0.5f, -1), Vector3.zero, Vector3.zero, 0.5f);
			}
			if (path.Count > 0)
			{
				//var last = path[path.Count - 1];
				//lineRenderer.line.Push(new Vector3(last.x + 0.5f, last.y + 0.5f, -1), Vector3.zero, Vector3.zero, 0.5f);
			}
		}
		catch
		{

		}

		if (mode == 1)
		{
			var v = directedGraph.areas[new Position(0, 0, 0)];
			for (int y = 0; y < grid.Length; y++)
			{
				for (int x = 0; x < grid[y].Length; x++)
				{
					var t = (TileBase)GameObject.FindObjectOfType<FontTileLoader>().tiles[GameObject.FindObjectOfType<FontTileLoader>().tiles.Length - 1];

					t = tiles[0];
					if (v.Get(x, y) == SubGoal2DChunk.TYPE_BLOCKED)
					{
						t = tiles[3];
					}
					else if (v.Get(x, y) == SubGoal2DChunk.TYPE_SUBGOAL)
					{
						t = tiles[1];
					}
					tileMap[TILEMAP_MOUSEOVERLAY].SetTile(new Vector3Int(x, y, 0), t);
				}
			}
		}
		else if (mode >= 2)
		{

			

			var v = directedGraph.areas[new Position(0, 0, 0)];
			for (int y = 0; y < grid.Length; y++)
			{
				for (int x = 0; x < grid[y].Length; x++)
				{
					TileBase t = null;
					var vv = v.GetDistance(x, y, mode-2);
					if (vv < GameObject.FindObjectOfType<FontTileLoader>().tiles.Length - 1)
					{
						t = GameObject.FindObjectOfType<FontTileLoader>().tiles[vv];
					}
					tileMap[TILEMAP_MOUSEOVERLAY].SetTile(new Vector3Int(x, y, 0), t);
				}
			}
		}



		positionText.text = modeNames[mode];

	}

	public void RenderMouse()
	{
		tileMap[TILEMAP_MOUSE].ClearAllTiles();
		if (start != Vector3IntNotSet)
		{
			tileMap[TILEMAP_MOUSE].SetTile(new Vector3Int(start.x, start.y, 0), tiles[1]);
		}
		if (end != Vector3IntNotSet)
		{
			tileMap[TILEMAP_MOUSE].SetTile(new Vector3Int(end.x, end.y, 0), tiles[2]);
			RenderFloor(0);
		}

		if (subgoalExplore != Vector3IntNotSet)
		{
			foreach (var v in directedGraph.findSubgoalsFrom(new Position(subgoalExplore.x, subgoalExplore.y, 0), true))
			{
				tileMap[TILEMAP_MOUSE].SetTile(new Vector3Int(v.x, v.y, 0), tiles[2]);
			}
			Debug.Log("Clicked on: " + subgoalExplore.x + "," + subgoalExplore.y);
			foreach (var v in directedGraph.findSubgoalsFrom(new Position(subgoalExplore.x, subgoalExplore.y, 0)))
			{
				Debug.Log("Subgoal at: " + v.x + "," + v.y);
				tileMap[TILEMAP_MOUSE].SetTile(new Vector3Int(v.x, v.y, 0), tiles[1]);
			}
		}
	}


	

	public void Drag(Vector3 start, Vector3 stop)
	{
		Vector3 pstart = Camera.main.ScreenToWorldPoint(start);
		Vector3 pstop = Camera.main.ScreenToWorldPoint(stop);

		var tstart = tileMap[0].WorldToLocal(pstart);
		var tstop = tileMap[0].WorldToLocal(pstop);

		var delta = tstop - tstart;

		position -= delta;

	}

	public void Update()
	{
		//positionText.text = $"Position: {position.x}, {position.y}, {position.z}";

		//tileMap.tileAnchor = new Vector3(-position.x + 0.5f, -position.y + 0.5f, -position.z);

		Camera.main.transform.position = new Vector3(position.x, position.y, -10);

		var delta = Input.mouseScrollDelta;

		if (zoom > 10)
		{
			zoom -= delta.y * 10;
		}
		else
		{
			zoom -= delta.y;
		}

		if (zoom < 1)
		{
			zoom = 1;
		}
		Camera.main.orthographicSize = zoom;

		if (Input.GetMouseButton(0))
		{
			if (lastDown)
			{
				Drag(mousePosition, Input.mousePosition);
			}

			mousePosition = Input.mousePosition;

			lastDown = true;
		}
		else
		{
			lastDown = false;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Vector3Int position = Vector3Int.zero;
		if (eventData.button == 0)
		{
			Vector3 pstop = Camera.main.ScreenToWorldPoint(eventData.position);



			position = this.tileMap[0].WorldToCell(pstop);

		}

		if (Input.GetKey(KeyCode.LeftShift))
		{
			start = position;
		}
		else if(Input.GetKey(KeyCode.LeftControl))
		{
			subgoalExplore = position;
		}
		else
		{
			end = position;
		}
		Debug.Log("---");
		Debug.Log(start);
		Debug.Log(end);
		GeneratePath();
		//RenderFloor(0);
		RenderMouse();
	}
}

