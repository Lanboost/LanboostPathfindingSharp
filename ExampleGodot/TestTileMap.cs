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
    int SIZE_X = 51;
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

    int currentStep = -1;
    List<Line2D> highlightLines = new List<Line2D>();


    public static String NODE_PLAYER = "../Player";
    public static String NODE_STEP_LABEL = "../RightPanel/VBox/StepLabel";

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
        var player = GetNode<Node2D>(NODE_PLAYER);
        player.Position = MapToWorld(new Vector2(pos.x, pos.y));
        start = pos;
        end = FindFreePosition();
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            createSubgoalGraph();
            watch.Stop();
            GD.Print($"createSubgoalGraph in: {watch.ElapsedMilliseconds} ms");
        }
        //GenerateAndShowPath();
    }

    public void RenderNextStep()
    {
        currentStep++;
        RenderStep();
    }

    public void RenderPreviousStep()
    {
        UndoStep();
        currentStep--;
        RenderStep();
    }

    public void RenderNextMainStep()
    {
        var currentStepStr = (currentStep >= 0 && currentStep < directedGraph.generationSteps.Count) ? directedGraph.generationSteps[currentStep].Step : null;
        
        while(true)
        {

            currentStep++;
            var newStepStr = (currentStep >= 0 && currentStep < directedGraph.generationSteps.Count) ? directedGraph.generationSteps[currentStep].Step : null;

            if(currentStepStr != newStepStr || newStepStr == null)
            {

                RenderStep();
                break;
            }

            RenderStep(true, true);
        }
    }

    public void UndoStep()
    {
        if (currentStep >= 0 && currentStep < directedGraph.generationSteps.Count)
        {
            var step = directedGraph.generationSteps[currentStep];
            foreach (var tileData in step.tileDatas)
            {
                if (tileData.prevHighLightForm == HighLightForm.NONE)
                {
                    GetNode<TileMap>("GraphTileMap").SetCell(tileData.position.x, tileData.position.y, -1);
                }
                else
                {
                    GetNode<TileMap>("GraphTileMap").SetCell(
                        tileData.position.x, 
                        tileData.position.y, 
                        0, 
                        autotileCoord: new Vector2((int)tileData.prevHighLightForm, (int)tileData.prevHighLightType)
                    );
                }
            }
        }
    }

    public void RenderStep(bool skipText = false, bool skipLines = false)
    {
        foreach(var line in highlightLines )
        {
            line.QueueFree();
        }
        highlightLines.Clear();

        if (currentStep >= 0 && currentStep < directedGraph.generationSteps.Count) {
            var step = directedGraph.generationSteps[currentStep];
            if (!skipText)
            {
                GetNode<Label>(NODE_STEP_LABEL).Text = $"Step: {currentStep + 1} / {directedGraph.generationSteps.Count}\n{step.Step}\n{step.SubStep}";
            }
            if (!skipLines)
            {
                foreach (var highlight in step.highLights)
                {
                    var line = new Line2D();
                    var start = highlight.start;
                    var end = start + new Position(1, 1, 0);
                    if (highlight.end != null)
                    {
                        end = highlight.end;
                    }
                    line.Points = new Vector2[]
                    {
                        MapToWorld(new Vector2(start.x, start.y)),
                        MapToWorld(new Vector2(end.x, start.y)),
                        MapToWorld(new Vector2(end.x, end.y)),
                        MapToWorld(new Vector2(start.x, end.y)),
                        MapToWorld(new Vector2(start.x, start.y)),
                        MapToWorld(new Vector2(end.x, start.y))
                    };
                    line.ZIndex = 100;
                    if (highlight.highLightColor == HighLightColor.RED)
                    {
                        line.DefaultColor = Colors.Red;
                    }
                    if (highlight.highLightColor == HighLightColor.GREEN)
                    {
                        line.DefaultColor = Colors.Green;
                    }
                    if (highlight.highLightColor == HighLightColor.YELLOW)
                    {
                        line.DefaultColor = Colors.Yellow;
                    }
                    if (highlight.highLightColor == HighLightColor.BLUE)
                    {
                        line.DefaultColor = Colors.Blue;
                    }

                    line.Width = 2;
                    GetNode<TileMap>("OverlayTileMap").AddChild(line);

                    /*var pathRenderer = GetNode<Line2D>("Line2D");
                    pathRenderer.Points = new Vector2[]
                    {
                        MapToWorld(new Vector2(start.x, start.y)),
                        MapToWorld(new Vector2(end.x, start.y)),
                        MapToWorld(new Vector2(end.x, end.y)),
                        MapToWorld(new Vector2(start.x, end.y)),
                        MapToWorld(new Vector2(start.x, start.y))
                    };*/

                    highlightLines.Add(line);
                }
            }

            foreach (var tileData in step.tileDatas)
            {
                if (!tileData.prev) {
                    var cell = GetNode<TileMap>("GraphTileMap").GetCell(tileData.position.x, tileData.position.y);
                    if (cell == -1)
                    {
                        tileData.prevHighLightForm = HighLightForm.NONE;
                    }
                    else
                    {
                        var autoTileCoord = GetNode<TileMap>("GraphTileMap").GetCellAutotileCoord(tileData.position.x, tileData.position.y);
                        // TODO
                        tileData.prevHighLightForm = (HighLightForm)autoTileCoord.x;
                        tileData.prevHighLightType = (HighLightColor)autoTileCoord.y;
                    }
                    tileData.prev = true;
                }
                

                if (tileData.highLightForm == HighLightForm.NONE) {
                    GetNode<TileMap>("GraphTileMap").SetCell(tileData.position.x, tileData.position.y, -1);
                }
                else
                {
                    GetNode<TileMap>("GraphTileMap").SetCell(tileData.position.x, tileData.position.y, 0, autotileCoord:new Vector2((int)tileData.highLightForm, (int)tileData.highLightColor));
                }
            }

        }
        else
        {
            
            GetNode<Label>(NODE_STEP_LABEL).Text = $"Step: {currentStep+1} / {directedGraph.generationSteps.Count}\nDone";
        }
    }

    public void GenerateAndShowPath()
    {
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            GeneratePath();
            watch.Stop();
            GD.Print($"GeneratePath in: {watch.ElapsedMilliseconds} ms");
        }
        var pathNodes = directedAStar.GetPath().Select(tempPos =>
        {
            return MapToWorld(new Vector2(tempPos.x, tempPos.y)) + new Vector2(8, 8);
        }).ToArray();

        var pathRenderer = GetNode<Line2D>("Line2D");
        pathRenderer.Points = pathNodes;
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

        
        directedGraph.Create();
        

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
                if (Input.IsKeyPressed((int)KeyList.Shift) && eeevent.ButtonIndex == (int)ButtonList.Left && eeevent.Pressed)
                {
                    //drawing = true;
                }
                else if (eeevent.ButtonIndex == (int)ButtonList.Left && eeevent.Pressed)
                {
                    var pos = WorldToMap(eeevent.Position);
                    end = new Position((int)pos.x, (int)pos.y);
                    GenerateAndShowPath();
                }
                if (eeevent.ButtonIndex == (int)ButtonList.Left && !eeevent.Pressed)
                {
                    drawing = false;
                }

                if (Input.IsKeyPressed((int)KeyList.Shift) && eeevent.ButtonIndex == (int)ButtonList.Right && eeevent.Pressed)
                {
                    //clearing = true;
                }
                else if (eeevent.ButtonIndex == (int)ButtonList.Right && eeevent.Pressed)
                {
                    var pos = WorldToMap(eeevent.Position);
                    var player = GetNode<Node2D>(NODE_PLAYER);
                    player.Position = MapToWorld(new Vector2(pos.x, pos.y));
                    start = new Position((int)pos.x, (int)pos.y);
                    GenerateAndShowPath();
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
        {
            if (eevent is InputEventKey eeevent && eeevent.Pressed)
            {
                if(eeevent.Scancode == (int)KeyList.R)
                {
                    RenderNextStep();
                }
                if (eeevent.Scancode == (int)KeyList.E)
                {
                    RenderNextMainStep();
                }
                if (eeevent.Scancode == (int)KeyList.W)
                {
                    RenderPreviousStep();
                }

                if (eeevent.Scancode == (int)KeyList.T && !eeevent.Echo)
                {
                    var n = GetNode<TileMap>("GraphTileMap");
                    if(n.Visible)
                    {
                        n.Hide();
                        GetNode<TileMap>("OverlayTileMap").Hide();
                    }
                    else
                    {
                        n.Show();
                        GetNode<TileMap>("OverlayTileMap").Show();
                    }
                }
            }
        }

            //if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Escape)
            //		GetTree().Quit();
        }

}
