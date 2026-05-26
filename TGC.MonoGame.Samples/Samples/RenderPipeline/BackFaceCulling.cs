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

        private GeometricPrimitive _teapot;

        private GeometricPrimitive _cilinder;

        private GeometricPrimitive _currentPrimitive;

        private Effect _effect;

        private const float BaseScaleScalar = 10f;

        private const float Displacement = 15f;

        private Matrix _baseScale;

        private Matrix _baseRotation;


        private List<Arrowz> _cilinderArrows;
        private List<Arrowz> _teapotArrows;
        private List<Arrowz> _arrows;

        private bool _showWireframe;

        private bool _showArrows;

        private bool _backFace = true;

        private Effect _drawDepthEffect;

        private RenderTarget2D _depthRenderTarget;

        private List<string> _texts;
        private List<Vector2> _textScreenPositions;
        private List<Vector3> _textWorldPositions;

        private SpriteFont _spriteFont;

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

            _baseRotation = Matrix.Identity;

            _arrows = new List<Arrowz>();

            _texts = new List<string>
            {
                "Clockwise culling",
                "Cull none",
                "Counter-clockwise culling",
            };


            _textScreenPositions = new List<Vector2>
            {
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
            };
            var offset = Vector3.Up * 15f;

            _textWorldPositions = new List<Vector3>
            {
                -Displacement * Vector3.UnitX + offset,
                Vector3.Zero + offset,
                Displacement * Vector3.UnitX + offset,
            };


            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            _spriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
            // We load the primitive meshes into models
            _teapot = new TeapotPrimitive(GraphicsDevice);
            _cilinder = new CylinderPrimitive(GraphicsDevice);
            _currentPrimitive = _cilinder;
            _cilinderArrows = new List<Arrowz>();
            _teapotArrows = new List<Arrowz>();
            LoadArrows(_cilinder, _cilinderArrows);
            LoadArrows(_teapot, _teapotArrows);
            _arrows = _cilinderArrows;

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

        private void LoadArrows(GeometricPrimitive primitive, List<Arrowz> arrowzs)
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
                    AddArrow(arrowzs, average, normal, Color.Magenta);

                    // Center inner arrow
                    AddArrow(arrowzs, average, -normal, Color.Yellow);

                    // Left outer arrow
                    Vector3 displacedAverage = average - Displacement * Vector3.UnitX;
                    AddArrow(arrowzs, displacedAverage, normal, Color.Magenta);
                    // Left inner arrow
                    AddArrow(arrowzs, displacedAverage, -normal, Color.Yellow);


                    // Right outer arrow
                    displacedAverage = average + Displacement * Vector3.UnitX;
                    AddArrow(arrowzs, displacedAverage, normal, Color.Magenta);

                    // Right inner arrow
                    AddArrow(arrowzs, displacedAverage, -normal, Color.Yellow);
                }
            }
        }

        private void AddArrow(List<Arrowz> arrowzs, Vector3 position, Vector3 normal, Color color)
        {
            arrowzs.Add(new Arrowz
            {
                Position = position,
                Target = position + normal * 0.75f,
                Color = color
            });
        }

        private bool InsideCylinder(Vector3 position)
        {
            return (new Vector2(position.X, position.Y)).Length() <= 0.2f;
        }


        /// <summary>
        /// Returns a <see cref="Vector2"/> containing the XY components of a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3"/> to obtain its XY components</param>
        /// <returns>A <see cref="Vector2"/> containing the XY components of the given vector</returns>
        private Vector2 ToVector2(Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
        /// <summary>
        /// Updates text positions in screen space to face the camera, 
        /// based on the world space positions and the camera values.
        /// </summary>
        private void UpdateTextPositions()
        {
            for (var index = 0; index < _textScreenPositions.Count; index++)
            {
                var size = _spriteFont.MeasureString(_texts[index]) / 2f;
                _textScreenPositions[index] = ToVector2(GraphicsDevice.Viewport.Project(
                        _textWorldPositions[index], _camera.Projection, _camera.View, Matrix.Identity)) - size;
            }
        }
        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            _camera.Update(gameTime);

            _effect.Parameters["cameraPosition"]?.SetValue(_camera.Position);

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);
            UpdateTextPositions();

            _baseRotation *= Matrix.CreateFromAxisAngle(Vector3.UnitY, (float)gameTime.ElapsedGameTime.TotalSeconds);
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

            DrawCylinders(_drawDepthEffect, viewProjection);

            // Set the render target as null, we are drawing on the screen!
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            DrawCylinders(_effect, viewProjection);

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            if (_showArrows)
            {
                Arrowz arr;
                for (int index = 0; index < _arrows.Count; index++)
                {
                    arr = _arrows[index];
                    Game.Gizmos.DrawLine(Vector3.Transform(arr.Position, _baseRotation), arr.Target, arr.Color);
                }
            }

            // Draw labels
            Game.SpriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone);

            for (var index = 0; index < _textScreenPositions.Count; index++)
            {
                Game.SpriteBatch.DrawString(_spriteFont, _texts[index], _textScreenPositions[index], Color.White);
            }

            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }


        private void DrawPrimitive(Effect effect, Matrix viewProjection, CullMode cullMode, float displacement)
        {
            RasterizerState rasterizerState = new RasterizerState();
            if (_showWireframe)
            {
                rasterizerState.FillMode = FillMode.WireFrame;
            }
            if (!_backFace)
            {
                cullMode = CullMode.None;
            }
            rasterizerState.CullMode = cullMode;
            GraphicsDevice.RasterizerState = rasterizerState;

            var world = _baseScale * _baseRotation * Matrix.CreateTranslation(Vector3.UnitX * displacement);
            effect.Parameters["WorldViewProjection"].SetValue(world * viewProjection);
            _cilinder.Draw(effect);
        }
        private void DrawCylinders(Effect effect, Matrix viewProjection)
        {
            // Clockwise primitive
            DrawPrimitive(effect, viewProjection, CullMode.CullClockwiseFace, -Displacement);

            // Cull none primitive
            DrawPrimitive(effect, viewProjection, CullMode.None, 0f);

            // Counter-clockwise primitive
            DrawPrimitive(effect, viewProjection, CullMode.CullCounterClockwiseFace, Displacement);
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            _teapot.Dispose();
            _cilinder.Dispose();
            _arrows.Clear();
            _effect.Dispose();
            base.UnloadContent();
        }
    }
}
