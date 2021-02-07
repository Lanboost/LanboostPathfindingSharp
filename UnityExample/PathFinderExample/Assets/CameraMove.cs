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

	Vector3Int start;
	Vector3Int end;

	AStar<Position, Edge> aStar;
	AStar<Position, Edge> directedAStar;
	DynamicDirectedGraph<Position, Edge> directedGraph;

	int mode = 0;
	
	public void Start()
	{
		Application.targetFrameRate = 20;

		modebutton.onClick.AddListener(delegate ()
		{
			this.mode += 1;
			this.mode %= 2;
			RenderFloor(0);
			
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
		aStar = new AStar<Position, Edge>(tileGraph, 10000);

		directedGraph = new DynamicDirectedGraph<Position, Edge>(new SubGoalGraphBuilder2D<Position, Edge>(gridWorld));
		directedGraph.Load();

		directedAStar = new AStar<Position, Edge>(directedGraph, 10000);
		RenderFloor(0);
	}

	public void GeneratePath()
	{
		Debug.Log("Astar result: "+aStar.FindPath(new Position(start.x, start.y), new Position(end.x, end.y)));
		Debug.Log("Astar directedGraph result: " + directedAStar.FindPath(new Position(start.x, start.y), new Position(end.x, end.y)));
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

		if (mode == 1)
		{
			gameObjectWorldLine.SetActive(false);
			var path = aStar.GetPath();

			foreach (var p in path)
			{
				tileMap[TILEMAP_GAMEOBJECT].SetTile(new Vector3Int(p.first.x, p.first.y, 0), tiles[4]);
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
					lineRenderer.line.Push(new Vector3(p.first.x + 0.5f, p.first.y + 0.5f, -1), Vector3.zero, Vector3.zero, 0.5f);
				}
				if (path.Count > 0)
				{
					var last = path[path.Count - 1];
					lineRenderer.line.Push(new Vector3(last.second.x + 0.5f, last.second.y + 0.5f, -1), Vector3.zero, Vector3.zero, 0.5f);
				}
		}

		tileMap[TILEMAP_MOUSE].ClearAllTiles();
		tileMap[TILEMAP_MOUSE].SetTile(new Vector3Int(start.x, start.y, 0), tiles[1]);	
		tileMap[TILEMAP_MOUSE].SetTile(new Vector3Int(end.x, end.y, 0), tiles[2]);


		if (mode == 1)
		{
			positionText.text = "Standard Astar";
		}
		else
		{
			positionText.text = "SubGoal mode";
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
		else
		{
			end = position;
		}
		Debug.Log("---");
		Debug.Log(start);
		Debug.Log(end);
		GeneratePath();
		RenderFloor(0);
	}
}

