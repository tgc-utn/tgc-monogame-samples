using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.RenderPipeline
{
    struct Arrowz
    {
        public Vector3 Position;
        public Vector3 Target;
        public Color Color;
    }

    public class BackFaceCulling : TGCSample
    {
        private Camera _camera;

        private GeometricPrimitive _primitive;

        private Effect _effect;

        private const float BaseScaleScalar = 10f;

        private const float Displacement = 15f;

        private Matrix _baseScale;

        private List<Arrowz> _arrows;

        private bool _showWireframe;

        private bool _showArrows;

        private bool _backFace = true;

        private Effect _drawDepthEffect;

        private RenderTarget2D _depthRenderTarget;

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
            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0f, 0f, 50f), screenSize);
            
            _baseScale = Matrix.CreateScale(BaseScaleScalar);

            _arrows = new List<Arrowz>();
            
            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the sphere mesh into a model
            _primitive = new SpherePrimitive(GraphicsDevice, 1f, 6);

            LoadArrows(_primitive);

            // Load the effect
            _effect = Game.Content.Load<Effect>(ContentFolderEffects + "BackFace");

            _drawDepthEffect = Game.Content.Load<Effect>(ContentFolderEffects + "ShadowMap");
            
            // Create a depth render target. It stores depth from the camera
            _depthRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false,
                SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);
                        
            ModifierController.AddToggle("Show Wireframe", (enabled) => _showWireframe = enabled, false);
            ModifierController.AddToggle("Show Triangle Normals", (enabled) => _showArrows = enabled, false);
            ModifierController.AddToggle("Enable Back-Face Culling", (enabled) => _backFace = enabled, true);
            ModifierController.AddTexture("Depth", _depthRenderTarget);

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
            _arrows.Add(new Arrowz()
            {
                Position = position,
                Target = position + normal * 0.75f,
                Color = color
            });
        }

        private bool InsideCylinder(Vector3 position)
        {
            return  (new Vector2(position.X, position.Y)).Length() <= 0.2f;
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            _camera.Update(gameTime);

            _effect.Parameters["cameraPosition"]?.SetValue(_camera.Position);

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;

            var viewProjection = _camera.View * _camera.Projection;

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            GraphicsDevice.SetRenderTarget(_depthRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            DrawSpheres(_drawDepthEffect, viewProjection);

            // Set the render target as null, we are drawing on the screen!
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            DrawSpheres(_effect, viewProjection);

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            if (_showArrows)
            {
                Arrowz arr;
                for (int index = 0; index < _arrows.Count; index++)
                {
                    arr = _arrows[index];
                    Game.Gizmos.DrawLine(arr.Position, arr.Target, arr.Color);
                }
            } 
            
            base.Draw(gameTime);
        }

        private void DrawSpheres(Effect effect, Matrix viewProjection)
        {
            RasterizerState rasterizerState = new RasterizerState();
            if(!_backFace)
                rasterizerState.CullMode = CullMode.None;
            else
                rasterizerState.CullMode = CullMode.CullClockwiseFace;
            if (_showWireframe)
                rasterizerState.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rasterizerState;

            var world = _baseScale * Matrix.CreateTranslation(Vector3.UnitX * Displacement);
            effect.Parameters["WorldViewProjection"].SetValue(world * viewProjection);
            _primitive.Draw(effect);

            rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            if (_showWireframe)
                rasterizerState.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rasterizerState;

            world = _baseScale;
            effect.Parameters["WorldViewProjection"].SetValue(world * viewProjection);
            _primitive.Draw(effect);

            rasterizerState = new RasterizerState();
            if (!_backFace)
                rasterizerState.CullMode = CullMode.None;
            else
                rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            if (_showWireframe)
                rasterizerState.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rasterizerState;

            world = _baseScale * Matrix.CreateTranslation(Vector3.UnitX * -Displacement);
            effect.Parameters["WorldViewProjection"].SetValue(world * viewProjection);
            _primitive.Draw(effect);
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            _primitive.Dispose();
            _arrows.Clear();
            _primitive.Dispose();
            _effect.Dispose();
            base.UnloadContent();
        }
    }
}
