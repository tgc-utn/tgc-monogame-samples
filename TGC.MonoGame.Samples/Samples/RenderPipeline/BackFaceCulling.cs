using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.RenderPipeline
{
    public class BackFaceCulling : TGCSample
    {
        private Camera Camera { get; set; }

        private GeometricPrimitive Primitive { get; set; }

        private Effect Effect { get; set; }

        private readonly float BaseScaleScalar = 10f;

        private readonly float Displacement = 15f;

        private Matrix BaseScale { get; set; }

        private List<Arrow> Arrows { get; set; }

        private SpriteFont SpriteFont { get; set; }

        private bool ShowWireframe { get; set; }

        private bool ShowArrows { get; set; }

        private bool BackFace { get; set; } = true;

        private bool PastWireframePressed { get; set; }

        private bool PastArrowsPressed { get; set; }

        private bool PastBackFacePressed { get; set; }

        private Effect DebugTextureEffect { get; set; }

        private Effect DrawDepthEffect { get; set; }

        private FullScreenQuad FullScreenQuad { get; set; }

        private RenderTarget2D DepthRenderTarget { get; set; }

        public BackFaceCulling(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.RenderPipeline;
            Name = "Back-Face Culling";
            Description = "Visualizing properties of the back-face culling command";
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            var screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0f, 0f, 10f), screenSize);
            
            BaseScale = Matrix.CreateScale(BaseScaleScalar);

            Arrows = new List<Arrow>();

            base.Initialize();
        }



        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the sphere meshe into a model
            Primitive = new SpherePrimitive(GraphicsDevice, 1f, 6);

            SpriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "Arial");

            LoadArrows(Primitive);

            // Load the effect
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "BackFace");

            // Load the debug texture effect to visualize the shadow map
            DebugTextureEffect = Game.Content.Load<Effect>(ContentFolderEffects + "DebugTexture");
            // Assign the near and far plane distances of the light camera to debug depth
            DebugTextureEffect.Parameters["nearPlaneDistance"].SetValue(Camera.DefaultNearPlaneDistance);
            DebugTextureEffect.Parameters["farPlaneDistance"].SetValue(Camera.DefaultFarPlaneDistance);
            DebugTextureEffect.CurrentTechnique = DebugTextureEffect.Techniques["DebugDepth"];

            DrawDepthEffect = Game.Content.Load<Effect>(ContentFolderEffects + "ShadowMap");


            FullScreenQuad = new FullScreenQuad(GraphicsDevice);


            // Create a depth render target. It stores depth from the camera
            DepthRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false,
                SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

            base.LoadContent();
        }

        private void LoadArrows(GeometricPrimitive primitive)
        {
            List<ushort> indices = primitive.Indices;
            List<VertexPositionColorNormal> vertices = primitive.Vertices;
            int indexCount = indices.Count;

            // Load arrows
            Vector3 vertexOne;
            Vector3 vertexTwo;
            Vector3 vertexThree;
            Vector3 normal;
            for (int index = 0; index < indexCount; index += 3)
            {
                vertexOne = vertices[indices[index]].Position;
                vertexTwo = vertices[indices[index + 1]].Position;
                vertexThree = vertices[indices[index + 2]].Position;

                Vector3 average = (vertexOne + vertexTwo + vertexThree) * BaseScaleScalar / 3f;
                normal = Vector3.Cross(vertexTwo - vertexOne, vertexThree - vertexOne);
                normal.Normalize();

                bool isForward = Vector3.Dot(normal, Vector3.UnitZ) >= 0f;
               
                bool inside = InsideCylinder(vertexOne);
                inside |= InsideCylinder(vertexTwo);
                inside |= InsideCylinder(vertexThree);

                if (!inside || isForward)
                {

                    // Center outer arrow
                    AddArrow(average, normal, Color.Magenta);

                    // Center inner arrow
                    AddArrow(average, -normal, Color.Yellow);

                    // Left outer arrow
                    Vector3 displacedAverage = average - Displacement * Vector3.UnitX;
                    AddArrow(displacedAverage, normal, Color.Magenta);

                    // Left inner arrow
                    AddArrow(displacedAverage, -normal, Color.Yellow);


                    // Right outer arrow
                    displacedAverage = average + Displacement * Vector3.UnitX;
                    AddArrow(displacedAverage, normal, Color.Magenta); 
                    
                    // Right outer arrow
                    AddArrow(displacedAverage, -normal, Color.Yellow);

                }

            }
        }

        private void AddArrow(Vector3 position, Vector3 normal, Color color)
        {
            Arrow outerArrow = new Arrow(GraphicsDevice);
            outerArrow.Thickness = 0.005f;
            outerArrow.HeadSize = new Vector2(0.03f, 0.1f);
            outerArrow.FromPosition = position;
            outerArrow.ToPosition = position + normal * 0.5f;
            outerArrow.BodyColor = color;
            outerArrow.HeadColor = Color.White;
            outerArrow.UpdateValues();
            Arrows.Add(outerArrow);
        }

        private bool InsideCylinder(Vector3 position)
        {
            return  (new Vector2(position.X, position.Y)).Length() <= 0.2f;
        }


        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            Camera.Update(gameTime);

            Effect.Parameters["cameraPosition"]?.SetValue(Camera.Position);

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.J))
            {
                if (!PastWireframePressed)
                    ShowWireframe = !ShowWireframe;
                PastWireframePressed = true;
            }
            else
                PastWireframePressed = false;

            if (keyboardState.IsKeyDown(Keys.K))
            {
                if (!PastArrowsPressed)
                    ShowArrows = !ShowArrows;
                PastArrowsPressed = true;
            }
            else
                PastArrowsPressed = false;


            if (keyboardState.IsKeyDown(Keys.L))
            {
                if (!PastBackFacePressed)
                    BackFace = !BackFace;
                PastBackFacePressed = true;
            }
            else
                PastBackFacePressed = false;

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }


        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;

            var viewProjection = Camera.View * Camera.Projection;

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            GraphicsDevice.SetRenderTarget(DepthRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            DrawSpheres(DrawDepthEffect, viewProjection);


            // Set the render target as null, we are drawing on the screen!
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            DrawSpheres(Effect, viewProjection);

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            if (ShowArrows)
                Arrows.ForEach(arrow => arrow.Draw(Matrix.Identity, Camera.View, Camera.Projection));


            // Debug our shadowmap!
            // Show a simple quad with the texture
            DebugTextureEffect.Parameters["World"].SetValue(Matrix.CreateScale(0.2f) * Matrix.CreateTranslation(new Vector3(-0.75f, -0.75f, 0f)));
            DebugTextureEffect.Parameters["baseTexture"].SetValue(DepthRenderTarget);
            FullScreenQuad.Draw(DebugTextureEffect);

            Game.SpriteBatch.Begin();
            Game.SpriteBatch.DrawString(SpriteFont, "Con la tecla 'J' se prende y apaga el modo Wireframe",
                new Vector2(50, 50), Color.Yellow);
            Game.SpriteBatch.DrawString(SpriteFont, "Con la tecla 'K' se prenden y apagan las flechas", new Vector2(50, 80),
                Color.LightYellow);
            Game.SpriteBatch.DrawString(SpriteFont, "Con la tecla 'L' se habilita y deshabilita el back-face culling", new Vector2(50, 110),
                Color.LightGreen);
            Game.SpriteBatch.End();

            
            base.Draw(gameTime);
        }

        private void DrawSpheres(Effect effect, Matrix viewProjection)
        {
            RasterizerState rasterizerState = new RasterizerState();
            if(!BackFace)
                rasterizerState.CullMode = CullMode.None;
            else
                rasterizerState.CullMode = CullMode.CullClockwiseFace;
            if (ShowWireframe)
                rasterizerState.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rasterizerState;

            var world = BaseScale * Matrix.CreateTranslation(Vector3.UnitX * Displacement);
            effect.Parameters["WorldViewProjection"].SetValue(world * viewProjection);
            Primitive.Draw(effect);

            rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            if (ShowWireframe)
                rasterizerState.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rasterizerState;

            world = BaseScale;
            effect.Parameters["WorldViewProjection"].SetValue(world * viewProjection);
            Primitive.Draw(effect);

            rasterizerState = new RasterizerState();
            if (!BackFace)
                rasterizerState.CullMode = CullMode.None;
            else
                rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            if (ShowWireframe)
                rasterizerState.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rasterizerState;

            world = BaseScale * Matrix.CreateTranslation(Vector3.UnitX * -Displacement);
            effect.Parameters["WorldViewProjection"].SetValue(world * viewProjection);
            Primitive.Draw(effect);
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            Primitive.Dispose();
            Arrows.ForEach(arrow => arrow.Dispose());
            Arrows.Clear();
            Primitive.Dispose();
            FullScreenQuad.Dispose();
            Effect.Dispose();
            DebugTextureEffect.Dispose();
            base.UnloadContent();
        }

    }
}
