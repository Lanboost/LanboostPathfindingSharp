using Godot;
using System;
using Lanboost.PathFinding.Graph;
using System.Collections;
using System.Collections.Generic;
using Lanboost.PathFinding.Astar;
using System.Diagnostics;
using System.Linq;

public class TestTileMap : TileMap
{

	bool drawing = false;
	bool clearing = false;
    int SIZE_X = 64;
    int SIZE_Y = 35;
    Random random = new Random();
    TileGraph tileGraph = null;
    GridWorld gridWorld = null;

    IEnumerator<int> tilingProcess = null;

    PortalExtender pe = new PortalExtender();
    GlobalExtender ge = new GlobalExtender();

    SubgoalGraph2D<int> directedGraph;
    AStar<Position, SimpleEdge<int>> directedAStar;

    Position start;
    Position end;


    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Generate();
            watch.Stop();
            GD.Print($"Map Generated in: {watch.ElapsedMilliseconds} ms");
        }

        var pos = FindFreePosition();
        var player = GetNode<Node2D>("../Player");
        player.Position = MapToWorld(new Vector2(pos.x, pos.y));
        start = pos;
        end = FindFreePosition();
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            createSubgoalGraph();
            watch.Stop();
            GD.Print($"createSubgoalGraph in: {watch.ElapsedMilliseconds} ms");
        }
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            GeneratePath();
            watch.Stop();
            GD.Print($"GeneratePath in: {watch.ElapsedMilliseconds} ms");
        }

        var pathNodes = directedAStar.GetPath().Select(tempPos =>
        {
            return MapToWorld(new Vector2(tempPos.x, tempPos.y));
        }).ToArray();

        var pathRenderer = GetNode<Line2D>("Line2D");
        pathRenderer.DrawMultiline(pathNodes, Colors.Red);

    }
    public override void _Process(float delta)
    {
        //GD.Print("Hello, world!");

        if(tilingProcess != null)
        {
            if(!tilingProcess.MoveNext())
            {
                tilingProcess = null;
            }
        }
    }

    public IEnumerator<int> GetTileEnumerator()
    {
        int size = 5;

        int stepsX = SIZE_X / size + 1;
        int stepsY = SIZE_Y / size + 1;

        int stepsMax = stepsX * stepsY;

        
        for(int y = 0; y < stepsY;y++)
        {
            for (int x = 0; x < stepsX; x++)
            {
                var ix = x * size;
                var iy = y * size;
                this.UpdateBitmaskRegion(new Vector2(ix, iy), new Vector2(ix+size, iy+size));
                yield return (y * stepsX + x)*100 / stepsMax;
            }
        }
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
        catch (System.Exception e) { 
            //Debug.Log(e); 
        }

        directedAStar = new AStar<Position, SimpleEdge<int>>(directedGraph, 10000);
        //RenderFloor(0);
    }

    public void GeneratePath()
    {
        //if (start != Vector3IntNotSet && end != Vector3IntNotSet)
        //{
        GD.Print($"Generating from {start} {gridWorld.GetBlocked(start)} to {end} {gridWorld.GetBlocked(end)}");
            GD.Print("Astar directedGraph result: " + directedAStar.FindPath(new Position((int)start.x, (int)start.y), new Position((int)end.x, (int)end.y)));
            GD.Print(directedAStar.GetCost(new Position((int)end.x, (int)end.y)));
        //}
    }

    public void Generate()
    {
        var grid = new bool[SIZE_Y][];
        for (int y = 0; y < grid.Length; y++)
        {
            grid[y] = new bool[SIZE_X];
            for (int x = 0; x < grid[y].Length; x++)
            {
                grid[y][x] = random.Next(5) != 4;
                
            }
        }

        //border
        for (int x = 0; x < grid[0].Length; x++)
        {
            grid[0][x] = false;
            grid[SIZE_Y - 1][x] = false;
        }

        for (int y = 0; y < grid.Length; y++)
        {
            grid[y][0] = false;
            grid[y][SIZE_X - 1] = false;
        }

        for (int y = 0; y < grid.Length; y++)
        {
            for (int x = 0; x < grid[y].Length; x++)
            {
                this.SetCell(x, y, grid[y][x] ? 0 : -1);
            }
        }
        tilingProcess = GetTileEnumerator();


        tileGraph = new TileGraph(grid);
        gridWorld = new GridWorld(grid);

        
    }

    Position FindFreePosition()
    {
        for (int i = 0; i < 1000; i++)
        {
            var r = new Position(random.Next(gridWorld.GetChunkSize()), random.Next(gridWorld.GetChunkSize()), 0);
            if (!gridWorld.GetBlocked(r))
            {
                return r;
            }
        }
        return gridWorld.GetFirstFreePosition();
    }

    public override void _UnhandledInput(InputEvent eevent)
	{
        //if event is InputEventMouseButton:
        //if event.button_index == BUTTON_LEFT and event.pressed:
        //    var clicked_cell = world_to_map(event.position)

        //GetNode("../path_to_the_node")

        //GD.Print(eevent);

        Nullable<Vector2> position = null;
        {
            if (eevent is InputEventMouseButton eeevent)
            {
                if (eeevent.ButtonIndex == (int)ButtonList.Left && eeevent.Pressed)
                {
                    drawing = true;
                }
                if (eeevent.ButtonIndex == (int)ButtonList.Left && !eeevent.Pressed)
                {
                    drawing = false;
                }

                if (eeevent.ButtonIndex == (int)ButtonList.Right && eeevent.Pressed)
                {
                    clearing = true;
                }
                if (eeevent.ButtonIndex == (int)ButtonList.Right && !eeevent.Pressed)
                {
                    clearing = false;
                }

                position = WorldToMap(eeevent.Position);
            }
        }
        {
            if (eevent is InputEventMouseMotion eeevent)
            {
                position = WorldToMap(eeevent.Position);
            }
        }

        if (position != null)
        {
            if (drawing)
            {
                if (this.GetCellv(position.Value) != 0)
                {
                    this.SetCellv(position.Value, 0);
                    this.UpdateBitmaskArea(position.Value);
                }
            }
            if (clearing)
            {
                if (this.GetCellv(position.Value) != -1)
                {
                    this.SetCellv(position.Value, -1);
                    this.UpdateBitmaskArea(position.Value);
                }
            }
        }

        //if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Escape)
        //		GetTree().Quit();
    }

}
