using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Mathematics;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Optimizations;

/// <summary>
/// Shows how to create and use a sparse grid that is updated in real-time.
/// Author: Ronan Vinitzca.
/// </summary>
public class SparseGrid : TGCSample
{
    /// <summary>
    /// Size of the grid cells.
    /// </summary>
    private const float CellSize = 1000f;
    
    /// <summary>
    /// Size of the world. Instances can spawn on a box that has this size
    /// on any axis.
    /// </summary>
    private const float WorldRadius = 50000f;
    
    /// <summary>
    /// Random number generator for instance movement and generating random positions.
    /// </summary>
    private readonly Random _random = new();
    
    /// <summary>
    /// A list of instances that represent objects we want to interact/draw
    /// in world space.
    /// </summary>
    private List<InstanceData> _instances;

    /// <summary>
    /// Represents a sparse grid. Indices are cells,
    /// values are lists of instance indices contained on each cell.
    /// <remarks>Could be int[][] if we wanted it to be a dense grid</remarks>
    /// </summary>
    private readonly Dictionary<Vector3I, List<int>> _grid = new();

    // Camera to draw the scene
    private Camera _camera;
    
    // The Model of the Robot to draw
    private Model _robot;
    
    /// <summary>
    /// The AABB of the robot in local space.
    /// </summary>
    private BoundingBox _robotAABB;
    
    /// <summary>
    /// A Camera to check against bounding volumes
    /// </summary>
    private Camera _testCamera;
    
    /// <summary>
    /// A Bounding Frustum to check visibility
    /// </summary>
    private BoundingFrustum _boundingFrustum;

    /// <summary>
    /// A list of instance indices to be drawn this frame.
    /// </summary>
    private List<int> _indicesToDraw = new();

    /// <summary>
    /// If the grid should be enabled or if regular frustum-culling should be used.
    /// </summary>
    private bool _enabled = true;
    
    public SparseGrid(TGCViewer game) : base(game)
    {
        Category = TGCSampleCategory.Optimizations;
        Name = "Sparse Grid";
        Description = "Shows how to create a Sparse Grid to optimize rendering";
    }

    public override void Initialize()
    {
        Game.Background = Color.CornflowerBlue;
            
        // Creates a Static Camera looking at the origin
        _camera = new StaticCamera(GraphicsDevice.Viewport.AspectRatio, 
            Vector3.One * 5000f, -Vector3.Normalize(Vector3.One), Vector3.Up);
        
        _camera.BuildProjection(GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000000f,
            (MathF.PI / 180f) * 60f);

        var size = GraphicsDevice.Viewport.Bounds.Size;
        size.X /= 2;
        size.Y /= 2;

        // Create a camera not to render objects but to test them against the grid and/or frustum
        _testCamera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 50, 1000), size);
        _testCamera.BuildProjection(GraphicsDevice.Viewport.AspectRatio, 0.1f, 5000f,
            (MathF.PI / 180f) * 35f);

        _boundingFrustum = new BoundingFrustum(_testCamera.View * _testCamera.Projection);

        ModifierController.AddToggle("Use Sparse Grid", (toggle) => _enabled = toggle, true);
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Load the Robot Model and enable default lighting
        _robot = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
        ((BasicEffect)_robot.Meshes.FirstOrDefault()?.Effects.FirstOrDefault())?.EnableDefaultLighting();
        
        _robotAABB = BoundingVolumesExtensions.CreateAABBFrom(_robot);
        
        _instances = new List<InstanceData>();

        // Populate instances and put them on their cell
        for (int index = 0; index < 450_000; index++)
        {
            _instances.Add(new InstanceData()
            {
                Color = new Color(new Vector3(_random.NextSingle(), 
                    _random.NextSingle(),
                    _random.NextSingle())),
                Objective = GeneratePositionInRange(_random),
                Timer = _random.NextSingle(),
                From = GeneratePositionInRange(_random),
            });

            ref var instance = ref CollectionsMarshal.AsSpan(_instances)[index];
            instance.Position = Vector3.Lerp(instance.From, instance.Objective, instance.Timer);
            
            var cell = GetCell(instance.Position);
            instance.CurrentCell = cell;
            instance.BoundingBox = new BoundingBox(_robotAABB.Min + instance.Position, _robotAABB.Max + instance.Position);

            // If cell doesn't exist, create it
            if (!_grid.TryGetValue(cell, out var list))
            {
                list = new();
                _grid.Add(cell, list);
            }

            list.Add(index);
        }
        
        // Set depth to default
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        var instanceSpan = CollectionsMarshal.AsSpan(_instances);
        
        // Update N random instances
        MoveInstances(instanceSpan, elapsedTime);

        _indicesToDraw.Clear();

        _testCamera.Update(gameTime);
        
        _boundingFrustum.Matrix = _testCamera.View * _testCamera.Projection;

        if (_enabled)
        {
            TraverseGridAndFindInstances();
        }
        else
        {
            // Populate indices to draw by just doing frustum-culling
            for (int index = 0; index < _instances.Count; index++)
            {
                var box = _instances[index].BoundingBox;
                
                if (box.Intersects(_boundingFrustum))
                {
                    _indicesToDraw.Add(index);
                }
            }
        }
        
        foreach(var index in _indicesToDraw)
        {
            var box = _instances[index].BoundingBox;
            var center = (box.Max + box.Min) * 0.5f;
            var extents = (box.Max - box.Min) * 0.5f;
            
            Game.Gizmos.DrawCube(center, extents, Color.Lime);
        }
        
        // Draw a gizmo for the frustum
        Game.Gizmos.DrawFrustum(_testCamera.View * _testCamera.Projection, Color.Yellow);
        
        
        // Update Gizmos with the View Projection matrices
        Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);
        
        base.Update(gameTime);
    }

    private void TraverseGridAndFindInstances()
    {
        var corners = new Vector3[8];
        _boundingFrustum.GetCorners(corners);

        BoundingBox box = new BoundingBox();
        foreach (Vector3 corner in corners)
        {
            box.Min = Vector3.Min(box.Min, corner);
            box.Max = Vector3.Max(box.Max, corner);
        }

        var min = GetCell(box.Min);
        var max = GetCell(box.Max);
            
        for(int x = min.X; x <= max.X; x++)
        for(int y = min.Y; y <= max.Y; y++)
        for(int z = min.Z; z <= max.Z; z++)
        {
            var cellPositionInWorld = new Vector3(x, y, z) * CellSize;
            
            var bb = new BoundingBox(cellPositionInWorld, 
                cellPositionInWorld + new Vector3(CellSize));

            if (!bb.Intersects(_boundingFrustum))
                continue;

            var center = (bb.Max + bb.Min) * 0.5f;
            var extents = bb.Max - bb.Min;

            if (!_grid.TryGetValue(new Vector3I(x, y, z), out var list))
            {
                Game.Gizmos.DrawCube(center, extents, Color.Red);
                continue;
            }

            Game.Gizmos.DrawCube(center, extents, Color.Green);
                
            foreach (int item in list)
            {
                if (_boundingFrustum.Intersects(_instances[item].BoundingBox))
                {
                    _indicesToDraw.Add(item);
                }
            }
        }
    }

    private void MoveInstances(Span<InstanceData> instanceSpan, float elapsedTime)
    {
        for (int index = 0; index < 100; index++)
        {
            ref var element = ref instanceSpan[index];
            element.Timer += elapsedTime;

            if (element.Timer >= 1f)
            {
                element.Timer %= 1f;
                element.From = element.Objective;
                element.Objective = GeneratePositionInRange(_random);
            }
            
            element.Position = Vector3.Lerp(element.From, element.Objective, element.Timer);
            element.BoundingBox = new BoundingBox(_robotAABB.Min + element.Position, _robotAABB.Max + element.Position);

            var cell = GetCell(element.Position);

            // Instance is on the same cell
            if (cell.Equals(element.CurrentCell))
                continue;
            
            _grid[element.CurrentCell].Remove(index);

            if (_grid[element.CurrentCell].Count == 0)
            {
                _grid.Remove(element.CurrentCell);
            }

            if (!_grid.TryGetValue(cell, out var list))
            {
                list = new List<int>();
                _grid.Add(cell, list);
            }
                
            list.Add(index);
                
            element.CurrentCell = cell;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.CornflowerBlue);
    
        var instanceSpan = CollectionsMarshal.AsSpan(_instances);
    
        foreach (var index in _indicesToDraw)
        {
            ref var element = ref instanceSpan[index];

            if (_robot.Meshes.FirstOrDefault() != null &&
                _robot.Meshes.FirstOrDefault()!.Effects.FirstOrDefault() is BasicEffect basicEffect)
            {
                basicEffect.DiffuseColor = element.Color.ToVector3();
            }
            
            var world = Matrix.CreateTranslation(element.Position);
            
            _robot.Draw(world, _camera.View, _camera.Projection);
        }
    }

    private Vector3 GeneratePositionInRange(Random random)
    {
        return new Vector3(RandomRange(random, -WorldRadius, WorldRadius),
            RandomRange(random, -WorldRadius, WorldRadius),
            RandomRange(random, -WorldRadius, WorldRadius));
    }

    private float RandomRange(Random random, float min, float max)
    {
        return random.NextSingle() * (max - min) + min;
    }

    private Vector3I GetCell(in Vector3 position)
    {
        return (Vector3I)Vector3.Floor(position / CellSize);
    }
    
    private struct InstanceData
    {
        public Vector3 Position;
        public Vector3 From;
        public Vector3 Objective;
        public float Timer;
        public Color Color;
        public Vector3I CurrentCell;
        public BoundingBox BoundingBox;
    }
}