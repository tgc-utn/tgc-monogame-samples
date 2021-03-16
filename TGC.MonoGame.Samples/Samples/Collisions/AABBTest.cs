using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Collisions
{
    /// <summary>
    ///     Shows how to test collision for two AABBs
    /// </summary>
    public class AABBTest : TGCSample
    {
        /// <inheritdoc />
        public AABBTest(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Collisions;
            Name = "AABB Test";
            Description = "Shows how to test collision for two AABBs.";
        }

        private Camera Camera { get; set; }

        private Model Robot { get; set; }

        private Matrix RobotOneWorld { get; set; }
        private Matrix RobotTwoWorld { get; set; }

        private BoundingBox RobotOneBox { get; set; }
        private BoundingBox RobotTwoBox { get; set; }

        private Vector3 RobotOnePosition { get; set; }
        private Vector3 RobotTwoPosition { get; set; }

        private bool AreAABBsTouching { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.CornflowerBlue;
            Camera = new StaticCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 250f, -Vector3.Normalize(Vector3.One), Vector3.Up);

            RobotTwoPosition = new Vector3(-60f, 0f, 0f);
            RobotOnePosition = new Vector3(60f, 0f, 0f);

            RobotOneWorld = Matrix.CreateTranslation(RobotOnePosition);           
            RobotTwoWorld = Matrix.CreateTranslation(RobotTwoPosition);

            AreAABBsTouching = false;

            base.Initialize();
        }


        /// <inheritdoc />
        protected override void LoadContent()
        {
            Robot = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            ((BasicEffect)Robot.Meshes.FirstOrDefault()?.Effects.FirstOrDefault())?.EnableDefaultLighting();


            // Create AABBs
            // This gets an AABB with the bounds of the robot model
            RobotOneBox = GetBounds(Robot);
            
            // This moves the min and max points to the world position of each robot (one and two)
            RobotTwoBox = new BoundingBox(RobotOneBox.Min + RobotTwoPosition, RobotOneBox.Max + RobotTwoPosition);
            RobotOneBox = new BoundingBox(RobotOneBox.Min + RobotOnePosition, RobotOneBox.Max + RobotOnePosition);



            base.LoadContent();
        }

        
        public BoundingBox GetBounds(Model model)
        {
            var minPoint = Vector3.One * float.MaxValue;
            var maxPoint = Vector3.One * float.MinValue;

            var meshes = model.Meshes;
            for (int index = 0; index < meshes.Count; index++)
            {
                var meshParts = meshes[index].MeshParts;
                for (int subIndex = 0; subIndex < meshParts.Count; subIndex++)
                {
                    var vertexBuffer = meshParts[subIndex].VertexBuffer;
                    var declaration = vertexBuffer.VertexDeclaration;
                    int vertexSize = declaration.VertexStride / sizeof(float);

                    float[] rawVertexBuffer = new float[vertexBuffer.VertexCount * vertexSize];
                    vertexBuffer.GetData(rawVertexBuffer);

                    for (int vertexIndex = 0; vertexIndex < rawVertexBuffer.Length; vertexIndex += vertexSize)
                    {
                        Vector3 vertex = new Vector3(rawVertexBuffer[vertexIndex], rawVertexBuffer[vertexIndex + 1], rawVertexBuffer[vertexIndex + 2]);
                        minPoint = Vector3.Min(minPoint, vertex);
                        maxPoint = Vector3.Max(maxPoint, vertex);
                    }
                }
            }
            return new BoundingBox(minPoint, maxPoint);
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Right))
                MoveRobot(Vector3.Right);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Left))
                MoveRobot(Vector3.Left);

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            AreAABBsTouching = RobotOneBox.Intersects(RobotTwoBox);

            base.Update(gameTime);
        }

        private void MoveRobot(Vector3 increment)
        {
            RobotTwoPosition += increment;
            RobotTwoBox = new BoundingBox(RobotTwoBox.Min + increment, RobotTwoBox.Max + increment);
            RobotTwoWorld = Matrix.CreateTranslation(RobotTwoPosition);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Robot.Draw(RobotOneWorld, Camera.View, Camera.Projection);
            Robot.Draw(RobotTwoWorld, Camera.View, Camera.Projection);

            // Draw bounding boxes
            // Center is half the average between min and max
            // Size is the difference between max and min
            // Also draw red if they touch

            var colorOne = AreAABBsTouching ? Color.Red : Color.Yellow;
            var colorTwo = AreAABBsTouching ? Color.Red : Color.Green;

            Game.Gizmos.DrawCube((RobotOneBox.Max + RobotOneBox.Min) / 2f, RobotOneBox.Max - RobotOneBox.Min, colorOne);
            Game.Gizmos.DrawCube((RobotTwoBox.Max + RobotTwoBox.Min) / 2f, RobotTwoBox.Max - RobotTwoBox.Min, colorTwo);

            base.Draw(gameTime);
        }


    }
}
