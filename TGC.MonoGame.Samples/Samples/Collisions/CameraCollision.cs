﻿using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Collisions;

/// <summary>
///     Shows how to prevent the camera from surpassing surfaces or areas.
/// </summary>
public class CameraCollision : TGCSample
{
    private const float CameraFollowRadius = 100f;
    private const float CameraUpDistance = 80f;
    private const float RobotVelocity = 2f;
    private const float RobotRotatingVelocity = 0.06f;

    // Camera to draw the scene
    private TargetCamera _camera;

    // The Floor Texture
    private Texture2D _floorTexture;

    // The World Matrix for the Floor
    private Matrix _floorWorld;

    // A Quad to draw the floor and walls
    private QuadPrimitive _quad;

    // The Model of the Robot to draw
    private Model _robot;

    // The BoundingCylinder for the Robot
    private BoundingCylinder _robotCylinder;

    // The Position of the Robot
    private Vector3 _robotPosition;

    // The RotationMatrix of the Robot
    private Matrix _robotRotation;

    // The Scale Matrix of the Robot
    private Matrix _robotScale;

    // The World Matrix for the Robot
    private Matrix _robotWorld;

    // A Tiling Effect to repeat the floor and wall textures
    private Effect _tilingEffect;

    // The Bounding Boxes for the Walls
    private BoundingBox[] _wallBoxes;

    // The Wall Texture
    private Texture2D _wallTexture;

    // The World Matrices for the Walls
    private Matrix[] _wallWorldMatrices;

    /// <inheritdoc />
    public CameraCollision(TGCViewer game) : base(game)
    {
        Category = TGCSampleCategory.Collisions;
        Name = "Camera Collision";
        Description = "Shows how to prevent the camera from surpassing surfaces or areas.";
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        Game.Background = Color.Black;

        // Robot position and matrix initialization
        _robotPosition = Vector3.UnitX * 30f;
        _robotScale = Matrix.CreateScale(0.3f);
        _robotRotation = Matrix.Identity;

        // Create the BoundingCylinder enclosing the Robot
        _robotCylinder = new BoundingCylinder(_robotPosition, 10f, 20f);

        // Create the Camera
        _camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 100f, Vector3.Zero);

        // Create the Matrices for drawing the Floor and Walls
        _floorWorld = Matrix.CreateScale(200f);
        _wallWorldMatrices = new Matrix[5];
        var scale = new Vector3(200f, 1f, 200f);
        _wallWorldMatrices[0] = Matrix.CreateScale(scale) * Matrix.CreateRotationX(MathHelper.PiOver2) *
                                Matrix.CreateTranslation(-Vector3.UnitZ * 200f + Vector3.UnitY * 200f);
        _wallWorldMatrices[1] = Matrix.CreateScale(scale) * Matrix.CreateRotationX(-MathHelper.PiOver2) *
                                Matrix.CreateTranslation(Vector3.UnitZ * 200f + Vector3.UnitY * 200f);
        _wallWorldMatrices[2] = Matrix.CreateScale(scale) * Matrix.CreateRotationZ(MathHelper.PiOver2) *
                                Matrix.CreateTranslation(Vector3.UnitX * 200f + Vector3.UnitY * 200f);
        _wallWorldMatrices[3] = Matrix.CreateScale(scale) * Matrix.CreateRotationZ(-MathHelper.PiOver2) *
                                Matrix.CreateTranslation(-Vector3.UnitX * 200f + Vector3.UnitY * 200f);
        _wallWorldMatrices[4] = Matrix.CreateScale(new Vector3(100f, 1f, 100f)) *
                                Matrix.CreateRotationZ(MathHelper.PiOver2) *
                                Matrix.CreateTranslation(Vector3.UnitZ * 100f + Vector3.UnitY * 100f);

        // Create Bounding Boxes to enclose the Walls,
        // and to test the Robot and Camera against them
        _wallBoxes = new BoundingBox[5];

        var minVector = Vector3.One * 0.25f;
        _wallBoxes[0] = new BoundingBox(new Vector3(-200f, 0f, -200f) - minVector,
            new Vector3(200f, 200f, -200f) + minVector);
        _wallBoxes[1] = new BoundingBox(new Vector3(-200f, 0f, 200f) - minVector,
            new Vector3(200f, 200f, 200f) + minVector);
        _wallBoxes[2] = new BoundingBox(new Vector3(200f, 0f, -200f) - minVector,
            new Vector3(200f, 200f, 200f) + minVector);
        _wallBoxes[3] = new BoundingBox(new Vector3(-200f, 0f, -200f) - minVector,
            new Vector3(-200f, 200f, 200f) + minVector);
        _wallBoxes[4] = new BoundingBox(new Vector3(0f, 0f, 0f) - minVector, new Vector3(0f, 200f, 200f) + minVector);

        base.Initialize();
    }

    /// <inheritdoc />
    protected override void LoadContent()
    {
        // Load the models
        _robot = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");

        // Enable default lighting for the Robot
        foreach (var mesh in _robot.Meshes)
        {
            ((BasicEffect)mesh.Effects.FirstOrDefault())?.EnableDefaultLighting();
        }

        // Create the Quad
        _quad = new QuadPrimitive(GraphicsDevice);

        // Load the Floor and Wall textures
        _floorTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "stones");
        _wallTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "pbr/metal/color");

        // Load our Tiling Effect and set its tiling value
        _tilingEffect = Game.Content.Load<Effect>(ContentFolderEffects + "TextureTiling");
        _tilingEffect.Parameters["Tiling"].SetValue(new Vector2(10f, 10f));

        // Calculate the height of the Model of the Robot
        // Create a Bounding Box from it, then subtract the max and min Y to get the height

        // Use the height to set the Position of the robot 
        // (it is half the height, multiplied by its scale in Y -RobotScale.M22-)

        var extents = BoundingVolumesExtensions.CreateAABBFrom(_robot);
        var height = extents.Max.Y - extents.Min.Y;

        _robotPosition += height * 0.5f * Vector3.Up * _robotScale.M22;

        // Assign the center of the Cylinder as the Robot Position
        _robotCylinder.Center = _robotPosition;

        // Update our World Matrix to draw the Robot
        _robotWorld = _robotScale * Matrix.CreateTranslation(_robotPosition);

        // Update our camera to set its initial values
        UpdateCamera();

        base.LoadContent();
    }

    /// <summary>
    ///     Updates the internal values of the Camera, performing collision testing and correction with the scene
    /// </summary>
    private void UpdateCamera()
    {
        // Create a position that orbits the Robot by its direction (Rotation)

        // Create a normalized vector that points to the back of the Robot
        var robotBackDirection = Vector3.Transform(Vector3.Forward, _robotRotation);
        // Then scale the vector by a radius, to set an horizontal distance between the Camera and the Robot
        var orbitalPosition = robotBackDirection * CameraFollowRadius;

        // We will move the Camera in the Y axis by a given distance, relative to the Robot
        var upDistance = Vector3.Up * CameraUpDistance;

        // Calculate the new Camera Position by using the Robot Position, then adding the vector orbitalPosition that sends 
        // the camera further in the back of the Robot, and then we move it up by a given distance
        var newCameraPosition = _robotPosition + orbitalPosition + upDistance;

        // Check if the Camera collided with the scene
        var collisionDistance = CameraCollided(newCameraPosition);

        // If the Camera collided with the scene
        if (collisionDistance.HasValue)
        {
            // Limit our Horizontal Radius by subtracting the distance from the collision
            // Then clamp that value to be in between the near plane and the original Horizontal Radius
            var clampedDistance =
                MathHelper.Clamp(CameraFollowRadius - collisionDistance.Value, 0.1f, CameraFollowRadius);

            // Calculate our new Horizontal Position by using the Robot Back Direction multiplied by our new range
            // (a range we know doesn't collide with the scene)
            var recalculatedPosition = robotBackDirection * clampedDistance;

            // Set our new position. Up is unaffected
            _camera.Position = _robotPosition + recalculatedPosition + upDistance;
        }
        // If the Camera didn't collide with the scene
        else
        {
            _camera.Position = newCameraPosition;
        }

        // Set our Target as the Robot, the Camera needs to be always pointing to it
        _camera.TargetPosition = _robotPosition;

        // Build our View matrix from the Position and TargetPosition
        _camera.BuildView();
    }

    /// <summary>
    ///     Tests the Camera against the scene and checks if it collides with any wall
    /// </summary>
    /// <param name="cameraPosition">The Camera Position to test</param>
    /// <returns>No value if there was no collision, the scalar distance to a wall otherwise</returns>
    private float? CameraCollided(Vector3 cameraPosition)
    {
        // Create a Ray that goes from the Robot Position to the Camera
        // The direction must be normalized
        var difference = _robotPosition - cameraPosition;
        var distanceToRobot = Vector3.Distance(_robotPosition, cameraPosition);
        var normalizedDifference = difference / distanceToRobot;

        var cameraToPlayerRay = new Ray(cameraPosition, normalizedDifference);

        // Test our ray against every wall Bounding Box
        foreach (var t in _wallBoxes)
        {
            // If there was an intersection 
            // And this intersection happened between the Robot and the Camera (and not further away)
            // Return the distance of collision
            var distance = cameraToPlayerRay.Intersects(t);
            if (distance.HasValue && distance < distanceToRobot)
            {
                return distance;
            }
        }

        // Return null if there was no valid collision
        return null;
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        // Initialize values indicating if the Robot moved, if it rotated, and how much movement should be applied
        var advanceAmount = 0f;
        var rotated = false;
        var moved = false;

        // Check for key presses and rotate accordingly
        // We can stack rotations in a given axis by multiplying our past matrix
        // By a new matrix containing a new rotation to apply
        if (Game.CurrentKeyboardState.IsKeyDown(Keys.Right))
        {
            _robotRotation *= Matrix.CreateRotationY(-RobotRotatingVelocity);
            rotated = true;
        }
        
        if (Game.CurrentKeyboardState.IsKeyDown(Keys.Left))
        {
            _robotRotation *= Matrix.CreateRotationY(RobotRotatingVelocity);
            rotated = true;
        }

        // Check for key presses and set our advance variable accordingly
        if (Game.CurrentKeyboardState.IsKeyDown(Keys.Up))
        {
            advanceAmount = RobotVelocity;
            moved = true;
        }
        
        if (Game.CurrentKeyboardState.IsKeyDown(Keys.Down))
        {
            advanceAmount = -RobotVelocity;
            moved = true;
        }

        // If there was any movement
        if (moved)
        {
            // Calculate the Robot new Position using the last Position, 
            // And adding an increment in the Robot Direction (calculated by rotating a vector by its rotation)
            var newPosition = _robotPosition + Vector3.Transform(Vector3.Backward, _robotRotation) * advanceAmount;

            // Set the Center of the Cylinder as our calculated position
            _robotCylinder.Center = newPosition;

            // Test against every wall. If there was a collision, move our Cylinder back to its original position
            foreach (var t in _wallBoxes)
            {
                if (!_robotCylinder.Intersects(t).Equals(BoxCylinderIntersection.None))
                {
                    moved = false;
                    _robotCylinder.Center = _robotPosition;
                    break;
                }
            }

            // If there was no collision, update our Robot Position value
            if (moved)
            {
                _robotPosition = newPosition;
            }
        }

        // If we effectively moved (with no collision) or rotated
        if (moved || rotated)
        {
            // Calculate our World Matrix again
            _robotWorld = _robotScale * _robotRotation * Matrix.CreateTranslation(_robotPosition);

            // Update the Camera accordingly, as it follows the Robot
            UpdateCamera();
        }

        // Update Gizmos with the View Projection matrices
        Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

        base.Update(gameTime);
    }

    /// <inheritdoc />
    public override void Draw(GameTime gameTime)
    {
        // Calculate the ViewProjection matrix
        var viewProjection = _camera.View * _camera.Projection;

        // Draw the Robot
        _robot.Draw(_robotWorld, _camera.View, _camera.Projection);

        // Set the WorldViewProjection and Texture for the Floor and draw it
        _tilingEffect.Parameters["WorldViewProjection"].SetValue(_floorWorld * viewProjection);
        _tilingEffect.Parameters["Texture"].SetValue(_floorTexture);
        _quad.Draw(_tilingEffect);

        // Draw each Wall
        // First, set the Wall Texture
        _tilingEffect.Parameters["Texture"].SetValue(_wallTexture);
        for (var index = 0; index < _wallWorldMatrices.Length - 1; index++)
        {
            // Set the WorldViewProjection matrix for each Wall
            _tilingEffect.Parameters["WorldViewProjection"].SetValue(_wallWorldMatrices[index] * viewProjection);
            // Draw the Wall
            _quad.Draw(_tilingEffect);
        }

        // Draw the traversing Wall
        // For this, disable Back-Face Culling as we want to draw both sides of the Quad
        // Save the past RasterizerState
        var rasterizerState = GraphicsDevice.RasterizerState;

        // Use a RasterizerState which has Back-Face Culling disabled
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        // Set the WorldViewProjection matrix and draw the Wall
        _tilingEffect.Parameters["WorldViewProjection"]
            .SetValue(_wallWorldMatrices[_wallWorldMatrices.Length - 1] * viewProjection);
        _quad.Draw(_tilingEffect);

        // Restore the old RasterizerState
        GraphicsDevice.RasterizerState = rasterizerState;

        // Draw Gizmos for Bounding Boxes and Robot Cylinder

        foreach (var box in _wallBoxes)
        {
            var center = BoundingVolumesExtensions.GetCenter(box);
            var extents = BoundingVolumesExtensions.GetExtents(box);
            Game.Gizmos.DrawCube(center, extents * 2f, Color.Red);
        }

        Game.Gizmos.DrawCylinder(_robotCylinder.Transform, Color.Yellow);

        base.Draw(gameTime);
    }
}