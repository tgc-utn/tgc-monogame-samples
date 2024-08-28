using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Collisions
{
    /// <summary>
    ///     Shows how to make a third person platformer with custom physics
    /// </summary>
    public class ThirdPersonPlatformer : TGCSample
    {
        private const float CameraFollowRadius = 100f;
        private const float CameraUpDistance = 80f;
        private const float RobotSideSpeed = 100f;
        private const float RobotJumpSpeed = 150f;
        private const float Gravity = 350f;
        private const float RobotRotatingVelocity = 0.06f;
        private const float Epsilon = 0.00001f;

        // Camera to draw the scene
        private TargetCamera _camera;

        // Geometries
        private Model _robot;
        private BoxPrimitive _boxPrimitive;
        private QuadPrimitive _quad;

        // Robot internal matrices and vectors
        private Matrix _robotScale;
        private Matrix _robotRotation;
        private Vector3 _robotPosition;
        private Vector3 _robotVelocity;
        private Vector3 _robotAcceleration;
        private Vector3 _robotFrontDirection;
        
        // A boolean indicating if the Robot is on the ground
        private bool _onGround;

        // World matrices
        private Matrix _boxWorld;
        private Matrix[] _stairsWorld;
        private Matrix _floorWorld;
        private Matrix _robotWorld;

        // Textures
        private Texture2D _stonesTexture;
        private Texture2D _woodenTexture;
        private Texture2D _cobbleTexture;
        
        // Effects

        // Tiling Effect for the floor
        private Effect _tilingEffect;

        // Effect for the stairs and boxes
        private BasicEffect _boxesEffect;
        
        // Colliders

        // Bounding Boxes representing our colliders (floor, stairs, boxes)
        private BoundingBox[] _colliders;

        private BoundingCylinder _robotCylinder;
        
        /// <inheritdoc />
        public ThirdPersonPlatformer(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Collisions;
            Name = "Third Person Platformer";
            Description = "Shows an example of a third person platformer game with different type of interactions.";
        }
        
        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.Black;

            // Set the ground flag to false, as the Robot starts in the air
            _onGround = false;

            // Robot position and matrix initialization
            _robotPosition = Vector3.UnitX * 30f;
            _robotScale = Matrix.CreateScale(0.3f);

            _robotCylinder = new BoundingCylinder(_robotPosition, 10f, 20f);
            _robotRotation = Matrix.Identity;
            _robotFrontDirection = Vector3.Backward;

            // Create the Camera
            _camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 100f, Vector3.Zero);

            // Create World matrices for our stairs
            _stairsWorld = new Matrix[]
            {
                Matrix.CreateScale(70f, 6f, 15f) * Matrix.CreateTranslation(0f, 3f, 125f),
                Matrix.CreateScale(70f, 6f, 15f) * Matrix.CreateTranslation(0f, 9f, 140f),
                Matrix.CreateScale(70f, 6f, 15f) * Matrix.CreateTranslation(0f, 15f, 155f),
                Matrix.CreateScale(70f, 6f, 40f) * Matrix.CreateTranslation(0f, 21f, 182.5f),
                Matrix.CreateScale(15f, 6f, 40f) * Matrix.CreateTranslation(-42.5f, 27f, 182.5f),
                Matrix.CreateScale(15f, 6f, 40f) * Matrix.CreateTranslation(-57.5f, 33f, 182.5f),
                Matrix.CreateScale(15f, 6f, 40f) * Matrix.CreateTranslation(-72.5f, 39f, 182.5f),
                Matrix.CreateScale(100f, 6f, 100f) * Matrix.CreateTranslation(-130f, 45f, 152.5f),
            };

            // Create World matrices for the Floor and Box
            _floorWorld = Matrix.CreateScale(200f, 0.001f, 200f);
            _boxWorld = Matrix.CreateScale(30f) * Matrix.CreateTranslation(85f, 15f, -15f);

            // Create Bounding Boxes for the static geometries
            // Stairs + Floor + Box
            _colliders = new BoundingBox[_stairsWorld.Length + 2];

            // Instantiate Bounding Boxes for the stairs
            int index = 0;
            for (; index < _stairsWorld.Length; index++)
                _colliders[index] = BoundingVolumesExtensions.FromMatrix(_stairsWorld[index]);

            // Instantiate a BoundingBox for the Box
            _colliders[index] = BoundingVolumesExtensions.FromMatrix(_boxWorld);
            index++;
            // Instantiate a BoundingBox for the Floor. Note that the height is almost zero
            _colliders[index] = new BoundingBox(new Vector3(-200f, -0.001f, -200f), new Vector3(200f, 0f, 200f));

            // Set the Acceleration (which in this case won't change) to the Gravity pointing down
            _robotAcceleration = Vector3.Down * Gravity;

            // Initialize the Velocity as zero
            _robotVelocity = Vector3.Zero;

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load the models
            _robot = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");

            // Enable default lighting for the Robot
            foreach (var mesh in _robot.Meshes)
                ((BasicEffect)mesh.Effects.FirstOrDefault())?.EnableDefaultLighting();
            
            // Create a BasicEffect to draw the Box
            _boxesEffect = new BasicEffect(GraphicsDevice);
            _boxesEffect.TextureEnabled = true;

            // Load our Tiling Effect
            _tilingEffect = Game.Content.Load<Effect>(ContentFolderEffects + "TextureTiling");
            _tilingEffect.Parameters["Tiling"].SetValue(new Vector2(10f, 10f));

            // Load Textures
            _stonesTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "stones");
            _woodenTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "wood/caja-madera-1");
            _cobbleTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "floor/adoquin");

            // Create our Quad (to draw the Floor)
            _quad = new QuadPrimitive(GraphicsDevice);
            
            // Create our Box Model
            _boxPrimitive = new BoxPrimitive(GraphicsDevice, Vector3.One, _woodenTexture);
            
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
        ///     Updates the internal values of the Camera
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
            _camera.Position = _robotPosition + orbitalPosition + upDistance;

            // Set the Target as the Robot, the Camera needs to be always pointing to it
            _camera.TargetPosition = _robotPosition;

            // Build the View matrix from the Position and TargetPosition
            _camera.BuildView();
        }
        
        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // The time that passed between the last loop
            var deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            
            // Check for key presses and rotate accordingly
            // We can stack rotations in a given axis by multiplying our past matrix
            // By a new matrix containing a new rotation to apply
            // Also, recalculate the Front Directoin
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                _robotRotation *= Matrix.CreateRotationY(-RobotRotatingVelocity);
                _robotFrontDirection = Vector3.Transform(Vector3.Backward, _robotRotation);
            }
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                _robotRotation *= Matrix.CreateRotationY(RobotRotatingVelocity);
                _robotFrontDirection = Vector3.Transform(Vector3.Backward, _robotRotation);
            }

            // Check for the Jump key press, and add velocity in Y only if the Robot is on the ground
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Space) && _onGround)
                _robotVelocity += Vector3.Up * RobotJumpSpeed;

            // Check for key presses and add a velocity in the Robot's Front Direction
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Up))
                _robotVelocity += _robotFrontDirection * RobotSideSpeed;
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.Down))
                _robotVelocity -= _robotFrontDirection * RobotSideSpeed;

            // Add the Acceleration to our Velocity
            // Multiply by the deltaTime to have the Position affected by deltaTime * deltaTime
            // https://gafferongames.com/post/integration_basics/
            _robotVelocity += _robotAcceleration * deltaTime;

            // Scale the velocity by deltaTime
            var scaledVelocity = _robotVelocity * deltaTime;

            // Solve the Vertical Movement first (could be done in other order)
            SolveVerticalMovement(scaledVelocity);
            
            // Take only the horizontal components of the velocity
            scaledVelocity = new Vector3(scaledVelocity.X, 0f, scaledVelocity.Z);

            // Solve the Horizontal Movement
            SolveHorizontalMovementSliding(scaledVelocity);
            
            // Update the RobotPosition based on the updated Cylinder center
            _robotPosition = _robotCylinder.Center;

            // Reset the horizontal velocity, as accumulating this is not needed in this sample
            _robotVelocity = new Vector3(0f, _robotVelocity.Y, 0f);

            // Update the Robot World Matrix
            _robotWorld = _robotScale * _robotRotation * Matrix.CreateTranslation(_robotPosition);

            // Update the Camera accordingly, as it follows the Robot
            UpdateCamera();
            
            // Update Gizmos with the View Projection matrices
            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Apply horizontal movement, detecting and solving collisions
        /// </summary>
        /// <param name="scaledVelocity">The current velocity scaled by deltaTime</param>
        private void SolveVerticalMovement(Vector3 scaledVelocity)
        {
            // If the Robot has vertical velocity
            if (scaledVelocity.Y == 0f)
                return;

            // Start by moving the Cylinder
            _robotCylinder.Center += Vector3.Up * scaledVelocity.Y;
            // Set the OnGround flag on false, update it later if we find a collision
            _onGround = false;
            
            // Collision detection
            var collided = false;
            var foundIndex = -1;
            for (var index = 0; index < _colliders.Length; index++)
            {
                if (!_robotCylinder.Intersects(_colliders[index]).Equals(BoxCylinderIntersection.Intersecting))
                    continue;
                
                // If we collided with something, set our velocity in Y to zero to reset acceleration
                _robotVelocity = new Vector3(_robotVelocity.X, 0f, _robotVelocity.Z);

                // Set our index and collision flag to true
                // The index is to tell which collider the Robot intersects with
                collided = true;
                foundIndex = index;
                break;
            }
            
            // We correct based on differences in Y until we don't collide anymore
            // Not usual to iterate here more than once, but could happen
            while (collided)
            {
                var collider = _colliders[foundIndex];
                var colliderY = BoundingVolumesExtensions.GetCenter(collider).Y;
                var cylinderY = _robotCylinder.Center.Y;
                var extents = BoundingVolumesExtensions.GetExtents(collider);

                float penetration;
                // If we are on top of the collider, push up
                // Also, set the OnGround flag to true
                if (cylinderY > colliderY)
                {
                    penetration = colliderY + extents.Y - cylinderY + _robotCylinder.HalfHeight;
                    _onGround = true;
                }

                // If we are on bottom of the collider, push down
                else
                    penetration = -cylinderY - _robotCylinder.HalfHeight + colliderY - extents.Y;

                // Move our Cylinder so we are not colliding anymore
                _robotCylinder.Center += Vector3.Up * penetration;
                collided = false;

                // Check for collisions again
                for (var index = 0; index < _colliders.Length; index++)
                {
                    if (!_robotCylinder.Intersects(_colliders[index]).Equals(BoxCylinderIntersection.Intersecting))
                        continue;

                    // Iterate until we don't collide with anything anymore
                    collided = true;
                    foundIndex = index;
                    break;
                }
            }
        }

        /// <summary>
        ///     Apply horizontal movement, detecting and solving collisions with sliding
        /// </summary>
        /// <param name="scaledVelocity">The current velocity scaled by deltaTime</param>
        private void SolveHorizontalMovementSliding(Vector3 scaledVelocity)
        {
            // Has horizontal movement?
            if (Vector3.Dot(scaledVelocity, new Vector3(1f, 0f, 1f)) == 0f)
                return;
            
            // Start by moving the Cylinder horizontally
            _robotCylinder.Center += new Vector3(scaledVelocity.X, 0f, scaledVelocity.Z);

            // Check intersection for every collider
            for (var index = 0; index < _colliders.Length; index++)
            {
                if (!_robotCylinder.Intersects(_colliders[index]).Equals(BoxCylinderIntersection.Intersecting))
                    continue;

                // Get the intersected collider and its center
                var collider = _colliders[index];
                var colliderCenter = BoundingVolumesExtensions.GetCenter(collider);

                // The Robot collided with this thing
                // Is it a step? Can the Robot climb it?
                bool stepClimbed = SolveStepCollision(collider, index);

                // If the Robot collided with a step and climbed it, stop here
                // Else go on
                if (stepClimbed)
                    return;

                // Get the cylinder center at the same Y-level as the box
                var sameLevelCenter = _robotCylinder.Center;
                sameLevelCenter.Y = colliderCenter.Y;

                // Find the closest horizontal point from the box
                var closestPoint = BoundingVolumesExtensions.ClosestPoint(collider, sameLevelCenter);

                // Calculate our normal vector from the "Same Level Center" of the cylinder to the closest point
                // This happens in a 2D fashion as we are on the same Y-Plane
                var normalVector = sameLevelCenter - closestPoint;
                var normalVectorLength = normalVector.Length();

                // Our penetration is the difference between the radius of the Cylinder and the Normal Vector
                // For precission problems, we push the cylinder with a small increment to prevent re-colliding into the geometry
                var penetration = _robotCylinder.Radius - normalVector.Length() + Epsilon;

                // Push the center out of the box
                // Normalize our Normal Vector using its length first
                _robotCylinder.Center += (normalVector / normalVectorLength * penetration);
            }
        }

        /// <summary>
        ///     Solves the intersection between the Robot and a collider.
        /// </summary>
        /// <param name="collider">The collider the Robot intersected with</param>
        /// <param name="colliderIndex">The index of the collider in the collider array the Robot intersected with</param>
        /// <returns>True if the collider was a step and it was climbed, False otherwise</returns>
        private bool SolveStepCollision(BoundingBox collider, int colliderIndex)
        {
            // Get the collider properties to check if it's a step
            // Also, to calculate penetration
            var extents = BoundingVolumesExtensions.GetExtents(collider);
            var colliderCenter = BoundingVolumesExtensions.GetCenter(collider);

            // Is this collider a step?
            // If not, exit
            if (extents.Y >= 6f)
                return false;

            // Is the base of the cylinder close to the step top?
            // If not, exit
            var distanceToTop = MathF.Abs((_robotCylinder.Center.Y - _robotCylinder.HalfHeight) - (colliderCenter.Y + extents.Y));
            if (distanceToTop >= 12f)
                return false;

            // We want to climb the step
            // It is climbable if we can reposition our cylinder in a way that
            // it doesn't collide with anything else
            var pastPosition = _robotCylinder.Center;
            _robotCylinder.Center += Vector3.Up * distanceToTop;
            for (int index = 0; index < _colliders.Length; index++)
                if (index != colliderIndex && _robotCylinder.Intersects(_colliders[index]).Equals(BoxCylinderIntersection.Intersecting))
                {
                    // We found a case in which the cylinder
                    // intersects with other colliders, so the climb is not possible
                    _robotCylinder.Center = pastPosition;
                    return false;
                }

            // If we got here the climb was possible
            // (And the Robot position was already updated)
            return true;
        }
        
        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            // Calculate the ViewProjection matrix
            var viewProjection = _camera.View * _camera.Projection;

            // Robot drawing
            _robot.Draw(_robotWorld, _camera.View, _camera.Projection);

            // Floor drawing
            
            // Set the Technique inside the TilingEffect to "BaseTiling", we want to control the tiling on the floor
            // Using its original Texture Coordinates
            _tilingEffect.CurrentTechnique = _tilingEffect.Techniques["BaseTiling"];
            // Set the Tiling value
            _tilingEffect.Parameters["Tiling"].SetValue(new Vector2(10f, 10f));
            // Set the WorldViewProjection matrix
            _tilingEffect.Parameters["WorldViewProjection"].SetValue(_floorWorld * viewProjection);
            // Set the Texture that the Floor will use
            _tilingEffect.Parameters["Texture"].SetValue(_stonesTexture);
            _quad.Draw(_tilingEffect);
            
            // Steps drawing

            // Set the Technique inside the TilingEffect to "WorldTiling"
            // We want to use the world position of the steps to define how to sample the Texture
            _tilingEffect.CurrentTechnique = _tilingEffect.Techniques["WorldTiling"];
            // Set the Texture that the Steps will use
            _tilingEffect.Parameters["Texture"].SetValue(_cobbleTexture);
            // Set the Tiling value
            _tilingEffect.Parameters["Tiling"].SetValue(Vector2.One * 0.05f);

            // Draw every Step
            for (int index = 0; index < _stairsWorld.Length; index++)
            {
                // Get the World Matrix
                var matrix = _stairsWorld[index];
                // Set the World Matrix
                _tilingEffect.Parameters["World"].SetValue(matrix);
                // Set the WorldViewProjection Matrix
                _tilingEffect.Parameters["WorldViewProjection"].SetValue(matrix * viewProjection);
                _boxPrimitive.Draw(_tilingEffect);
            }
            
            // Draw the Box, setting every matrix and its Texture
            _boxesEffect.World = _boxWorld;
            _boxesEffect.View = _camera.View;
            _boxesEffect.Projection = _camera.Projection;

            _boxesEffect.Texture = _woodenTexture;
            _boxPrimitive.Draw(_boxesEffect);
            
            // Gizmos Drawing
            for (int index = 0; index < _colliders.Length; index++)
            {
                var box = _colliders[index];
                var center = BoundingVolumesExtensions.GetCenter(box);
                var extents = BoundingVolumesExtensions.GetExtents(box);
                Game.Gizmos.DrawCube(center, extents * 2f, Color.Red);
            }

            Game.Gizmos.DrawCylinder(_robotCylinder.Transform, Color.Yellow);

            base.Draw(gameTime);
        }
    }
}
