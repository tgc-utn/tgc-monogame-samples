using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Collisions
{
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
        private TargetCamera Camera { get; set; }


        // The Model of the Robot to draw
        private Model Robot { get; set; }


        // The World Matrix for the Robot
        private Matrix RobotWorld { get; set; }
        
        // The Scale Matrix of the Robot
        private Matrix RobotScale { get; set; }
        
        // The RotationMatrix of the Robot
        private Matrix RobotRotation { get; set; }

        // The Position of the Robot
        private Vector3 RobotPosition { get; set; }
        
        // The BoundingCylinder for the Robot
        private BoundingCylinder RobotCylinder { get; set; }

        // A Quad to draw the floor and walls
        private QuadPrimitive Quad { get; set; }
        
        // The World Matrix for the Floor
        private Matrix FloorWorld { get; set; }

        // The World Matrices for the Walls
        private Matrix[] WallWorldMatrices { get;set;}
        
        // The Bounding Boxes for the Walls
        private BoundingBox[] WallBoxes { get; set; }

        // A Tiling Effect to repeat the floor and wall textures
        private Effect TilingEffect { get; set; }

        // The Floor Texture
        private Texture2D FloorTexture { get; set; }
        
        // The Wall Texture
        private Texture2D WallTexture { get; set; }

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
            RobotPosition = Vector3.UnitX * 30f;
            RobotScale = Matrix.CreateScale(0.3f);
            RobotRotation = Matrix.Identity;

            // Create the BoundingCylinder enclosing the Robot
            RobotCylinder = new BoundingCylinder(RobotPosition, 10f, 20f);

            // Create the Camera
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 100f, Vector3.Zero);

            // Create the Matrices for drawing the Floor and Walls
            FloorWorld = Matrix.CreateScale(200f);
            WallWorldMatrices = new Matrix[5];
            var scale = new Vector3(200f, 1f, 200f);
            WallWorldMatrices[0] = Matrix.CreateScale(scale) * Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(-Vector3.UnitZ * 200f + Vector3.UnitY * 200f);
            WallWorldMatrices[1] = Matrix.CreateScale(scale) * Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(Vector3.UnitZ * 200f + Vector3.UnitY * 200f);
            WallWorldMatrices[2] = Matrix.CreateScale(scale) * Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateTranslation(Vector3.UnitX * 200f + Vector3.UnitY * 200f);
            WallWorldMatrices[3] = Matrix.CreateScale(scale) * Matrix.CreateRotationZ(-MathHelper.PiOver2) * Matrix.CreateTranslation(-Vector3.UnitX * 200f + Vector3.UnitY * 200f);
            WallWorldMatrices[4] = Matrix.CreateScale(new Vector3(100f, 1f, 100f)) * Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateTranslation(Vector3.UnitZ * 100f + Vector3.UnitY * 100f);


            // Create Bounding Boxes to enclose the Walls,
            // and to test the Robot and Camera against them
            WallBoxes = new BoundingBox[5];

            var minVector = Vector3.One * 0.25f;
            WallBoxes[0] = new BoundingBox(new Vector3(-200f, 0f, -200f) - minVector, new Vector3(200f, 200f, -200f) + minVector);
            WallBoxes[1] = new BoundingBox(new Vector3(-200f, 0f, 200f) - minVector, new Vector3(200f, 200f, 200f) + minVector);
            WallBoxes[2] = new BoundingBox(new Vector3(200f, 0f, -200f) - minVector, new Vector3(200f, 200f, 200f) + minVector);
            WallBoxes[3] = new BoundingBox(new Vector3(-200f, 0f, -200f) - minVector, new Vector3(-200f, 200f, 200f) + minVector);
            WallBoxes[4] = new BoundingBox(new Vector3(0f, 0f, 0f) - minVector, new Vector3(0f, 200f, 200f) + minVector);


            base.Initialize();
        }



        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load the models
            Robot = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");

            // Enable default lighting for the Robot
            foreach (var mesh in Robot.Meshes)
                ((BasicEffect)mesh.Effects.FirstOrDefault())?.EnableDefaultLighting();

            // Create the Quad
            Quad = new QuadPrimitive(GraphicsDevice);

            // Load the Floor and Wall textures
            FloorTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "stones");
            WallTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "pbr/metal/color");

            // Load our Tiling Effect and set its tiling value
            TilingEffect = Game.Content.Load<Effect>(ContentFolderEffects + "TextureTiling");
            TilingEffect.Parameters["Tiling"].SetValue(new Vector2(10f, 10f));

            
            // Calculate the height of the Model of the Robot
            // Create a Bounding Box from it, then subtract the max and min Y to get the height

            // Use the height to set the Position of the robot 
            // (it is half the height, multiplied by its scale in Y -RobotScale.M22-)

            var extents = BoundingVolumesExtensions.CreateAABBFrom(Robot);
            var height = extents.Max.Y - extents.Min.Y;

            RobotPosition += height * 0.5f * Vector3.Up * RobotScale.M22;

            // Assign the center of the Cylinder as the Robot Position
            RobotCylinder.Center = RobotPosition;

            // Update our World Matrix to draw the Robot
            RobotWorld = RobotScale * Matrix.CreateTranslation(RobotPosition);

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
            var robotBackDirection = Vector3.Transform(Vector3.Forward, RobotRotation);
            // Then scale the vector by a radius, to set an horizontal distance between the Camera and the Robot
            var orbitalPosition = robotBackDirection * CameraFollowRadius;


            // We will move the Camera in the Y axis by a given distance, relative to the Robot
            var upDistance = Vector3.Up * CameraUpDistance;

            // Calculate the new Camera Position by using the Robot Position, then adding the vector orbitalPosition that sends 
            // the camera further in the back of the Robot, and then we move it up by a given distance
            var newCameraPosition = RobotPosition + orbitalPosition + upDistance;

            // Check if the Camera collided with the scene
            var collisionDistance = CameraCollided(newCameraPosition);

            // If the Camera collided with the scene
            if(collisionDistance.HasValue)
            {
                // Limit our Horizontal Radius by subtracting the distance from the collision
                // Then clamp that value to be in between the near plane and the original Horizontal Radius
                var clampedDistance = MathHelper.Clamp(CameraFollowRadius - collisionDistance.Value, 0.1f, CameraFollowRadius);

                // Calculate our new Horizontal Position by using the Robot Back Direction multiplied by our new range
                // (a range we know doesn't collide with the scene)
                var recalculatedPosition = robotBackDirection * clampedDistance;
                
                // Set our new position. Up is unaffected
                Camera.Position = RobotPosition + recalculatedPosition + upDistance;
            }
            // If the Camera didn't collide with the scene
            else
                Camera.Position = newCameraPosition;

            // Set our Target as the Robot, the Camera needs to be always pointing to it
            Camera.TargetPosition = RobotPosition;

            // Build our View matrix from the Position and TargetPosition
            Camera.BuildView();
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
            var difference = RobotPosition - cameraPosition;
            var distanceToRobot = Vector3.Distance(RobotPosition, cameraPosition);
            var normalizedDifference = difference / distanceToRobot;

            Ray cameraToPlayerRay = new Ray(cameraPosition, normalizedDifference);

            // Test our ray against every wall Bounding Box
            for (int index = 0; index < WallBoxes.Length; index++)
            {
                // If there was an intersection 
                // And this intersection happened between the Robot and the Camera (and not further away)
                // Return the distance of collision
                var distance = cameraToPlayerRay.Intersects(WallBoxes[index]);
                if (distance.HasValue && distance < distanceToRobot)
                    return distance;
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
                RobotRotation *= Matrix.CreateRotationY(-RobotRotatingVelocity);
                rotated = true;
            }
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                RobotRotation *= Matrix.CreateRotationY(RobotRotatingVelocity);
                rotated = true;
            }


            // Check for key presses and set our advance variable accordingly
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Up))
            {
                advanceAmount = RobotVelocity;
                moved = true;
            }
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.Down))
            {
                advanceAmount = -RobotVelocity;
                moved = true;
            }

            // If there was any movement
            if (moved)
            {
                // Calculate the Robot new Position using the last Position, 
                // And adding an increment in the Robot Direction (calculated by rotating a vector by its rotation)
                var newPosition = RobotPosition + Vector3.Transform(Vector3.Backward, RobotRotation) * advanceAmount;

                // Set the Center of the Cylinder as our calculated position
                RobotCylinder.Center = newPosition;

                // Test against every wall. If there was a collision, move our Cylinder back to its original position
                for (int index = 0; index < WallBoxes.Length; index++)
                    if (!RobotCylinder.Intersects(WallBoxes[index]).Equals(BoxCylinderIntersection.None))
                    {
                        moved = false;
                        RobotCylinder.Center = RobotPosition;
                        break;
                    }

                // If there was no collision, update our Robot Position value
                if (moved)
                    RobotPosition = newPosition;
            }

            // If we effectively moved (with no collision) or rotated
            if(moved || rotated)
            {
                // Calculate our World Matrix again
                RobotWorld = RobotScale * RobotRotation * Matrix.CreateTranslation(RobotPosition);

                // Update the Camera accordingly, as it follows the Robot
                UpdateCamera();
            }


            // Update Gizmos with the View Projection matrices
            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);


            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            // Calculate the ViewProjection matrix
            var viewProjection = Camera.View * Camera.Projection;

            // Draw the Robot
            Robot.Draw(RobotWorld, Camera.View, Camera.Projection);

            // Set the WorldViewProjection and Texture for the Floor and draw it
            TilingEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * viewProjection);
            TilingEffect.Parameters["Texture"].SetValue(FloorTexture);
            Quad.Draw(TilingEffect);

            // Draw each Wall
            // First, set the Wall Texture
            TilingEffect.Parameters["Texture"].SetValue(WallTexture);
            for (int index = 0; index < WallWorldMatrices.Length - 1; index++)
            {
                // Set the WorldViewProjection matrix for each Wall
                TilingEffect.Parameters["WorldViewProjection"].SetValue(WallWorldMatrices[index] * viewProjection);
                // Draw the Wall
                Quad.Draw(TilingEffect);
            }

            // Draw the traversing Wall
            // For this, disable Back-Face Culling as we want to draw both sides of the Quad
            // Save the past RasterizerState
            var rasterizerState = GraphicsDevice.RasterizerState;

            // Use a RasterizerState which has Back-Face Culling disabled
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Set the WorldViewProjection matrix and draw the Wall
            TilingEffect.Parameters["WorldViewProjection"].SetValue(WallWorldMatrices[WallWorldMatrices.Length - 1] * viewProjection);
            Quad.Draw(TilingEffect);

            // Restore the old RasterizerState
            GraphicsDevice.RasterizerState = rasterizerState;


            // Draw Gizmos for Bounding Boxes and Robot Cylinder

            for (int index = 0; index < WallBoxes.Length; index++)
            {
                var box = WallBoxes[index];
                var center = box.GetCenter();
                var extents = box.GetExtents();
                Game.Gizmos.DrawCube(center, extents * 2f, Color.Red);
            }

            Game.Gizmos.DrawCylinder(RobotCylinder.Transform, Color.Yellow);

            base.Draw(gameTime);
        }
    }
}
