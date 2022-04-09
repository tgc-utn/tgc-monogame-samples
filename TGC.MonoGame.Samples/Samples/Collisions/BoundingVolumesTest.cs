using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Models.Drawers;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Collisions
{
    /// <summary>
    ///     Shows how to test collision for different volumes
    /// </summary>
    public class BoundingVolumesTest : TGCSample
    {
        /// <inheritdoc />
        public BoundingVolumesTest(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Collisions;
            Name = "Bounding Volumes Test";
            Description = "Shows how to test collision for different Bounding Volumes.";
        }


        // Camera to draw the scene
        private Camera Camera { get; set; }

        // The Model of the Robot to draw
        private ModelDrawer Robot { get; set; }

        // BasicEffect to draw the Robot
        private Effect Effect { get; set; }

        // The World Matrix for the Robot
        private Matrix RobotWorld { get; set; }

        // The Robot's position
        private Vector3 RobotPosition { get; set; }

        // The <see cref="BoundingBox">BoundingBox</see> for the Robot
        private BoundingBox RobotBox { get; set; }




        // The Model of the Chair to draw
        private ModelDrawer Chair { get; set; }

        // The World Matrix for the Chair
        private Matrix ChairWorld { get; set; }

        // The World Matrix for the Chair Oriented Bounding Box
        private Matrix ChairOBBWorld { get; set; }

        // The OrientedBoundingBox of the Chair
        private OrientedBoundingBox ChairBox { get; set; }
        
        // The Y Angle for the Chair
        private float ChairAngle { get; set; }

        // The Scale Matrix of the Chair
        private Matrix ChairScale { get; set; }
        
        // The Translation Matrix of the Chair
        private Matrix ChairTranslation { get; set; }



        // The Model of the Tank to draw
        private ModelDrawer Tank { get; set; }

        // The World Matrix for the Tank
        private Matrix TankWorld { get; set; }

        // The BoundingSphere of the Tank
        private BoundingSphere _tankSphere;

        // The position of the Tank
        private Vector3 TankPosition { get; set; }




        // The World Matrix for the second Robot
        private Matrix RobotTwoWorld { get; set; }
        
        // The BoundingCylinder for the second Robot
        private BoundingCylinder RobotTwoCylinder { get; set; }



        // Indicates if the Robot is touching the Chair
        private bool TouchingChair { get; set; } = false;

        // Indicates if the Robot is touching the Chair
        private bool TouchingTank { get; set; } = false;

        // Indicates if the Robot is touching the other Robot
        private bool TouchingOtherRobot { get; set; } = false;

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.CornflowerBlue;

            // Creates a Static Camera pointing to the center
            Camera = new StaticCamera(GraphicsDevice.Viewport.AspectRatio, 
                Vector3.Forward * 100f + Vector3.Up * 100f, 
                -Vector3.Normalize(Vector3.Forward + Vector3.Up), 
                Vector3.Up);

            // Robot position and matrix initialization
            RobotPosition = Vector3.Zero;
            RobotWorld = Matrix.CreateScale(0.3f);


            // Chair position and matrix initialization
            ChairAngle = MathHelper.PiOver4;
            ChairScale = Matrix.CreateScale(0.5f);
            ChairTranslation = Matrix.CreateTranslation(Vector3.UnitX * 50f);
            ChairWorld = ChairScale * Matrix.CreateRotationY(ChairAngle) * ChairTranslation;

            // Tank position and matrix initialization
            TankPosition = -Vector3.UnitX * 10f + Vector3.UnitZ * 10f;
            TankWorld = Matrix.CreateScale(3f) * Matrix.CreateTranslation(TankPosition);

            // Robot two position and matrix initialization
            var robotTwoPosition = Vector3.UnitX * -50f + Vector3.UnitZ * 10f;
            RobotTwoWorld = Matrix.CreateScale(0.25f) * Matrix.CreateRotationY(MathHelper.PiOver4 + MathHelper.PiOver2) * Matrix.CreateTranslation(robotTwoPosition);
            RobotTwoCylinder = new BoundingCylinder(robotTwoPosition, 5f, 10f);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load the models
            var robotModel = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            var chairModel = Game.Content.Load<Model>(ContentFolder3D + "chair/chair");
            var tankModel = Game.Content.Load<Model>(ContentFolder3D + "tank/tank");


            // Load our Effect
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "DiffuseTexture");

            Robot = ModelInspector.CreateDrawerFrom(robotModel, Effect, EffectInspectionType.ALL);
            Chair = ModelInspector.CreateDrawerFrom(chairModel, Effect, EffectInspectionType.ALL);
            Tank = ModelInspector.CreateDrawerFrom(tankModel, Effect, EffectInspectionType.ALL);


            // Create an AABB
            // This gets an AABB with the bounds of the robot model
            RobotBox = BoundingVolumesExtensions.CreateAABBFrom(Robot);
            // Scale it down half-size as the model is scaled down as well
            RobotBox = BoundingVolumesExtensions.Scale(RobotBox, 0.3f);




            // Create an OBB for a model
            // First, get an AABB from the model
            var temporaryCubeAABB = BoundingVolumesExtensions.CreateAABBFrom(Chair);
            // Scale it to match the model's transform
            temporaryCubeAABB = BoundingVolumesExtensions.Scale(temporaryCubeAABB, 0.5f);
            // Create an Oriented Bounding Box from the AABB
            ChairBox = OrientedBoundingBox.FromAABB(temporaryCubeAABB);
            // Move the center
            ChairBox.Center = Vector3.UnitX * 50f;
            // Then set its orientation!
            ChairBox.Orientation = Matrix.CreateRotationY(ChairAngle);


            // Create a Bounding Sphere for a model
            _tankSphere = BoundingVolumesExtensions.CreateSphereFrom(Tank);
            _tankSphere.Center = TankPosition;
            _tankSphere.Radius *= 3f;


            // Set depth to default
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.LoadContent();
        }



        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Move the robot depending on key presses

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Right))
                MoveRobot(Vector3.Left);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Left))
                MoveRobot(Vector3.Right);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Up))
                MoveRobot(Vector3.Backward);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Down))
                MoveRobot(Vector3.Forward);


            // Update Gizmos with the View Projection matrices
            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);



            // Rotate the box
            ChairAngle += 0.01f;
            var rotation = Matrix.CreateRotationY(ChairAngle);
            ChairWorld = ChairScale * rotation * ChairTranslation;
            ChairBox.Orientation = rotation;

            // Create an OBB World-matrix so we can draw a cube representing it
            ChairOBBWorld = Matrix.CreateScale(ChairBox.Extents * 2f) *
                 ChairBox.Orientation *
                 ChairTranslation;



            // Update the boolean values depending on the intersection of the Bounding Volumes
            TouchingChair = ChairBox.Intersects(RobotBox);
            TouchingTank = _tankSphere.Intersects(RobotBox);
            TouchingOtherRobot = !RobotTwoCylinder.Intersects(RobotBox).Equals(BoxCylinderIntersection.None);

            base.Update(gameTime);
        }

        private void MoveRobot(Vector3 increment)
        {
            // Update the position of the Robot
            RobotPosition += increment;
            // Update its Bounding Box, moving both min and max positions
            RobotBox = new BoundingBox(RobotBox.Min + increment, RobotBox.Max + increment);
            RobotWorld = Matrix.CreateScale(0.3f) * Matrix.CreateTranslation(RobotPosition);
        }


        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            // Set the DiffuseColor to white to draw this Robot
            var viewProjection = Camera.View * Camera.Projection;
            Robot.ViewProjection = viewProjection;
            Robot.World = RobotWorld;
            Robot.Draw();

            Robot.World = RobotTwoWorld;
            Robot.Draw();

            // Draw the Chair
            Chair.ViewProjection = viewProjection;
            Chair.World = ChairWorld;
            Chair.Draw();

            // Draw the Tank
            Tank.ViewProjection = viewProjection;
            Tank.World = TankWorld;
            Tank.Draw();

            



            // Draw bounding volumes

            Game.Gizmos.DrawCube(BoundingVolumesExtensions.GetCenter(RobotBox), BoundingVolumesExtensions.GetExtents(RobotBox) * 2f, Color.Yellow);

            Game.Gizmos.DrawCube(ChairOBBWorld, TouchingChair ? Color.Orange : Color.Green);

            Game.Gizmos.DrawSphere(_tankSphere.Center, _tankSphere.Radius * Vector3.One, TouchingTank ? Color.Orange : Color.Purple);

            Game.Gizmos.DrawCylinder(RobotTwoCylinder.Transform, TouchingOtherRobot ? Color.Orange : Color.White);


            base.Draw(gameTime);
        }

    }
}
