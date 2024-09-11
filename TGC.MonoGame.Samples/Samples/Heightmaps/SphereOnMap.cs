using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Geometries;

namespace TGC.MonoGame.Samples.Samples.Heightmaps
{
    public class SphereOnMap : TGCSample
    {
        private const float CameraFollowRadius = 100f;
        private const float CameraUpDistance = 80f;
        private const float RobotSideSpeed = 100f;
        private const float RobotJumpSpeed = 150f;
        private const float Gravity = 350f;
        private const float RobotRotatingVelocity = 0.06f;
        private const float EPSILON = 0.00001f;

        private readonly Vector3 _sphereScale = new Vector3(2, 2, 2);
        private const float ShereAxisRotationSpeed = 2.5f;
        private const float AxisRotationSpeed = 0.125f;

        // Camera to draw the scene
        private TargetCamera Camera { get; set; }


        // Geometries
        private Model Robot { get; set; }
        private BoxPrimitive BoxPrimitive { get; set; }
        private QuadPrimitive Quad { get; set; }



        // Robot internal matrices and vectors
        private Matrix RobotScale { get; set; }
        private Matrix RobotRotation { get; set; }
        private Matrix SphereRotation { get; set; }
        private Vector3 RobotPosition { get; set; }
        private Vector3 RobotVelocity { get; set; }
        private Vector3 RobotAcceleration { get; set; }
        private Vector3 RobotFrontDirection { get; set; }

        // A boolean indicating if the Robot is on the ground
        private bool OnGround { get; set; }


        // World matrices
        private Matrix BoxWorld { get; set; }
        private Matrix[] StairsWorld { get; set; }
        private Matrix FloorWorld { get; set; }
        private Matrix RobotWorld { get; set; }



        // Textures
        private Texture2D StonesTexture { get; set; }
        private Texture2D WoodenTexture { get; set; }
        private Texture2D CobbleTexture { get; set; }


        // Effects

        // Tiling Effect for the floor
        private Effect TilingEffect { get; set; }

        // Effect for the stairs and boxes
        private BasicEffect BoxesEffect { get; set; }



        // Colliders

        // Bounding Boxes representing our colliders (floor, stairs, boxes)
        private BoundingBox[] Colliders { get; set; }

        private BoundingCylinder RobotCylinder { get; set; }


        private bool ShowGizmos { get; set; } = true;


        /// <inheritdoc />
        public SphereOnMap(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Heightmaps;
            Name = "Sphere On Map";
            Description = "Sphere On Map";
        }

        private SpherePrimitive Sphere { get; set; }

        private float AxisRotation { get; set; }
        private Matrix SphereTranslation { get; set; }
        private float ShereAxisRotation { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.Black;

            // Set the ground flag to false, as the Robot starts in the air
            OnGround = false;

            // Robot position and matrix initialization
            RobotPosition = Vector3.UnitX * 30f;
            RobotScale = Matrix.CreateScale(0.3f);

            RobotCylinder = new BoundingCylinder(RobotPosition, 10f, 20f);
            RobotRotation = Matrix.Identity;
            RobotFrontDirection = Vector3.Backward;

            SphereRotation = Matrix.Identity;

            // Create the Camera
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 100f, Vector3.Zero);

            // Create World matrices for our stairs
            StairsWorld = new Matrix[]
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
            FloorWorld = Matrix.CreateScale(200f, 0.001f, 200f);
            BoxWorld = Matrix.CreateScale(30f) * Matrix.CreateTranslation(85f, 15f, -15f);

            // Create Bounding Boxes for the static geometries
            // Stairs + Floor + Box
            Colliders = new BoundingBox[StairsWorld.Length + 2];

            // Instantiate Bounding Boxes for the stairs
            int index = 0;
            for (; index < StairsWorld.Length; index++)
                Colliders[index] = BoundingVolumesExtensions.FromMatrix(StairsWorld[index]);

            // Instantiate a BoundingBox for the Box
            Colliders[index] = BoundingVolumesExtensions.FromMatrix(BoxWorld);
            index++;
            // Instantiate a BoundingBox for the Floor. Note that the height is almost zero
            Colliders[index] = new BoundingBox(new Vector3(-200f, -0.001f, -200f), new Vector3(200f, 0f, 200f));

            // Set the Acceleration (which in this case won't change) to the Gravity pointing down
            RobotAcceleration = Vector3.Down * Gravity;

            // Initialize the Velocity as zero
            RobotVelocity = Vector3.Zero;


            Sphere = new SpherePrimitive(GraphicsDevice, 20, 32, Color.DarkGreen);

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

            // Create a BasicEffect to draw the Box
            BoxesEffect = new BasicEffect(GraphicsDevice);
            BoxesEffect.TextureEnabled = true;

            // Load our Tiling Effect
            TilingEffect = Game.Content.Load<Effect>(ContentFolderEffects + "TextureTiling");
            TilingEffect.Parameters["Tiling"].SetValue(new Vector2(10f, 10f));

            // Load Textures
            StonesTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "stones");
            WoodenTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "wood/caja-madera-1");
            CobbleTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "floor/adoquin");

            // Create our Quad (to draw the Floor)
            Quad = new QuadPrimitive(GraphicsDevice);

            // Create our Box Model
            BoxPrimitive = new BoxPrimitive(GraphicsDevice, Vector3.One, WoodenTexture);


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
        ///     Updates the internal values of the Camera
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
            Camera.Position = RobotPosition + orbitalPosition + upDistance;

            // Set the Target as the Robot, the Camera needs to be always pointing to it
            Camera.TargetPosition = RobotPosition;

            // Build the View matrix from the Position and TargetPosition
            Camera.BuildView();
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
                RobotRotation *= Matrix.CreateRotationY(-RobotRotatingVelocity);
                RobotFrontDirection = Vector3.Transform(Vector3.Backward, RobotRotation);
            }
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                RobotRotation *= Matrix.CreateRotationY(RobotRotatingVelocity);
                RobotFrontDirection = Vector3.Transform(Vector3.Backward, RobotRotation);
            }

            // Check for the Jump key press, and add velocity in Y only if the Robot is on the ground
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Space) && OnGround)
                RobotVelocity += Vector3.Up * RobotJumpSpeed;

            // Check for key presses and add a velocity in the Robot's Front Direction
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Up))
            {
                SphereRotation *= Matrix.CreateRotationX(RobotFrontDirection.Z * RobotRotatingVelocity) * Matrix.CreateRotationZ(RobotFrontDirection.X * -RobotRotatingVelocity);
               // SphereRotation *= (Matrix.CreateRotationX(RobotFrontDirection.Z * RobotRotatingVelocity) * Matrix.CreateRotationZ(RobotFrontDirection.X * RobotRotatingVelocity));
                RobotVelocity += RobotFrontDirection * RobotSideSpeed;
            }
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.Down))
            {
                SphereRotation *= Matrix.CreateRotationX(RobotFrontDirection.Z * -RobotRotatingVelocity) * Matrix.CreateRotationZ(RobotFrontDirection.X * RobotRotatingVelocity);
                //SphereRotation *= (Matrix.CreateRotationX(RobotFrontDirection.Z * RobotRotatingVelocity) * Matrix.CreateRotationZ(RobotFrontDirection.X * RobotRotatingVelocity));
                RobotVelocity -= RobotFrontDirection * RobotSideSpeed;
            }

            // Add the Acceleration to our Velocity
            // Multiply by the deltaTime to have the Position affected by deltaTime * deltaTime
            // https://gafferongames.com/post/integration_basics/
            RobotVelocity += RobotAcceleration * deltaTime;

            // Scale the velocity by deltaTime
            var scaledVelocity = RobotVelocity * deltaTime;

            // Solve the Vertical Movement first (could be done in other order)
            SolveVerticalMovement(scaledVelocity);

            // Take only the horizontal components of the velocity
            scaledVelocity = new Vector3(scaledVelocity.X, 0f, scaledVelocity.Z);

            // Solve the Horizontal Movement
            SolveHorizontalMovementSliding(scaledVelocity);


            // Update the RobotPosition based on the updated Cylinder center
            RobotPosition = RobotCylinder.Center;

            // Reset the horizontal velocity, as accumulating this is not needed in this sample
            RobotVelocity = new Vector3(0f, RobotVelocity.Y, 0f);

            // Update the Robot World Matrix
            RobotWorld = RobotScale * RobotRotation * Matrix.CreateTranslation(RobotPosition);

            // Update the Camera accordingly, as it follows the Robot
            UpdateCamera();

            SphereTranslation = GetSpherTransform();

            var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            AxisRotation += AxisRotationSpeed * elapsedTime;
            ShereAxisRotation += ShereAxisRotationSpeed * elapsedTime;

            // Update Gizmos with the View Projection matrices
            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            //SphereEffect.Parameters["eyePosition"].SetValue(Camera.Position);

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
            RobotCylinder.Center += Vector3.Up * scaledVelocity.Y;
            // Set the OnGround flag on false, update it later if we find a collision
            OnGround = false;


            // Collision detection
            var collided = false;
            var foundIndex = -1;
            for (var index = 0; index < Colliders.Length; index++)
            {
                if (!RobotCylinder.Intersects(Colliders[index]).Equals(BoxCylinderIntersection.Intersecting))
                    continue;

                // If we collided with something, set our velocity in Y to zero to reset acceleration
                RobotVelocity = new Vector3(RobotVelocity.X, 0f, RobotVelocity.Z);

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
                var collider = Colliders[foundIndex];
                var colliderY = BoundingVolumesExtensions.GetCenter(collider).Y;
                var cylinderY = RobotCylinder.Center.Y;
                var extents = BoundingVolumesExtensions.GetExtents(collider);

                float penetration;
                // If we are on top of the collider, push up
                // Also, set the OnGround flag to true
                if (cylinderY > colliderY)
                {
                    penetration = colliderY + extents.Y - cylinderY + RobotCylinder.HalfHeight;
                    OnGround = true;
                }

                // If we are on bottom of the collider, push down
                else
                    penetration = -cylinderY - RobotCylinder.HalfHeight + colliderY - extents.Y;

                // Move our Cylinder so we are not colliding anymore
                RobotCylinder.Center += Vector3.Up * penetration;
                collided = false;

                // Check for collisions again
                for (var index = 0; index < Colliders.Length; index++)
                {
                    if (!RobotCylinder.Intersects(Colliders[index]).Equals(BoxCylinderIntersection.Intersecting))
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
            RobotCylinder.Center += new Vector3(scaledVelocity.X, 0f, scaledVelocity.Z);

            // Check intersection for every collider
            for (var index = 0; index < Colliders.Length; index++)
            {
                if (!RobotCylinder.Intersects(Colliders[index]).Equals(BoxCylinderIntersection.Intersecting))
                    continue;

                // Get the intersected collider and its center
                var collider = Colliders[index];
                var colliderCenter = BoundingVolumesExtensions.GetCenter(collider);

                // The Robot collided with this thing
                // Is it a step? Can the Robot climb it?
                bool stepClimbed = SolveStepCollision(collider, index);

                // If the Robot collided with a step and climbed it, stop here
                // Else go on
                if (stepClimbed)
                    return;

                // Get the cylinder center at the same Y-level as the box
                var sameLevelCenter = RobotCylinder.Center;
                sameLevelCenter.Y = colliderCenter.Y;

                // Find the closest horizontal point from the box
                var closestPoint = BoundingVolumesExtensions.ClosestPoint(collider, sameLevelCenter);

                // Calculate our normal vector from the "Same Level Center" of the cylinder to the closest point
                // This happens in a 2D fashion as we are on the same Y-Plane
                var normalVector = sameLevelCenter - closestPoint;
                var normalVectorLength = normalVector.Length();

                // Our penetration is the difference between the radius of the Cylinder and the Normal Vector
                // For precission problems, we push the cylinder with a small increment to prevent re-colliding into the geometry
                var penetration = RobotCylinder.Radius - normalVector.Length() + EPSILON;

                // Push the center out of the box
                // Normalize our Normal Vector using its length first
                RobotCylinder.Center += (normalVector / normalVectorLength * penetration);
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
            var distanceToTop = MathF.Abs((RobotCylinder.Center.Y - RobotCylinder.HalfHeight) - (colliderCenter.Y + extents.Y));
            if (distanceToTop >= 12f)
                return false;

            // We want to climb the step
            // It is climbable if we can reposition our cylinder in a way that
            // it doesn't collide with anything else
            var pastPosition = RobotCylinder.Center;
            RobotCylinder.Center += Vector3.Up * distanceToTop;
            for (int index = 0; index < Colliders.Length; index++)
                if (index != colliderIndex && RobotCylinder.Intersects(Colliders[index]).Equals(BoxCylinderIntersection.Intersecting))
                {
                    // We found a case in which the cylinder
                    // intersects with other colliders, so the climb is not possible
                    RobotCylinder.Center = pastPosition;
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
            var viewProjection = Camera.View * Camera.Projection;

            // Robot drawing
            Robot.Draw(RobotWorld, Camera.View, Camera.Projection);

            // Floor drawing

            // Set the Technique inside the TilingEffect to "BaseTiling", we want to control the tiling on the floor
            // Using its original Texture Coordinates
            TilingEffect.CurrentTechnique = TilingEffect.Techniques["BaseTiling"];
            // Set the Tiling value
            TilingEffect.Parameters["Tiling"].SetValue(new Vector2(10f, 10f));
            // Set the WorldViewProjection matrix
            TilingEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * viewProjection);
            // Set the Texture that the Floor will use
            TilingEffect.Parameters["Texture"].SetValue(StonesTexture);
            Quad.Draw(TilingEffect);


            // Steps drawing

            // Set the Technique inside the TilingEffect to "WorldTiling"
            // We want to use the world position of the steps to define how to sample the Texture
            TilingEffect.CurrentTechnique = TilingEffect.Techniques["WorldTiling"];
            // Set the Texture that the Steps will use
            TilingEffect.Parameters["Texture"].SetValue(CobbleTexture);
            // Set the Tiling value
            TilingEffect.Parameters["Tiling"].SetValue(Vector2.One * 0.05f);

            // Draw every Step
            for (int index = 0; index < StairsWorld.Length; index++)
            {
                // Get the World Matrix
                var matrix = StairsWorld[index];
                // Set the World Matrix
                TilingEffect.Parameters["World"].SetValue(matrix);
                // Set the WorldViewProjection Matrix
                TilingEffect.Parameters["WorldViewProjection"].SetValue(matrix * viewProjection);
                BoxPrimitive.Draw(TilingEffect);
            }


            // Draw the Box, setting every matrix and its Texture
            BoxesEffect.World = BoxWorld;
            BoxesEffect.View = Camera.View;
            BoxesEffect.Projection = Camera.Projection;

            BoxesEffect.Texture = WoodenTexture;
            BoxPrimitive.Draw(BoxesEffect);


            // Gizmos Drawing
            for (int index = 0; index < Colliders.Length; index++)
            {
                var box = Colliders[index];
                var center = BoundingVolumesExtensions.GetCenter(box);
                var extents = BoundingVolumesExtensions.GetExtents(box);
                Game.Gizmos.DrawCube(center, extents * 2f, Color.Red);
            }

            Game.Gizmos.DrawCylinder(RobotCylinder.Transform, Color.Yellow);

            DrawGeometry(Sphere, SphereTranslation);

            base.Draw(gameTime);
        }

        private Matrix GetSpherTransform()
        {
            var scale = Matrix.CreateScale(_sphereScale);

            return scale * SphereRotation * Matrix.CreateTranslation(RobotPosition);
        }

        private void DrawGeometry(GeometricPrimitive geometry, Matrix transform)
        {
            var effect = geometry.Effect;

            effect.World = transform;
            effect.View = Camera.View;
            effect.Projection = Camera.Projection;

            geometry.Draw(effect);
        }
    }
}
