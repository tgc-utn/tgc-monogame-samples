using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Viewer.GUI;
using TGC.MonoGame.Samples.Viewer.GUI.Modifiers;

namespace TGC.MonoGame.Samples.Samples.Shaders
{
    /// <summary>
    ///     Multiple render target technique:
    ///     Allows drawing to up to 4 render targets at the same time with a modified shader
    ///     
    ///     Author: Leandro Osuna
    /// </summary>
    public class MRT : TGCSample
    {
        public MRT(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Shaders;
            Name = "Multiple Render Target";
            Description = "Draw to up to 4 render targets at the same time";
        }
        private float _time;

        private Camera _camera;
        private Effect _effect;
        private Model _model;
        private Texture2D _texture;

        private EffectParameter _effectWorld;
        private EffectParameter _effectWorldViewProjection;
        private EffectParameter _effectTime;

        private RenderTarget2D _colorTarget;
        private RenderTarget2D _inverseColorTarget;
        private RenderTarget2D _normalTarget;
        private RenderTarget2D _animatedTarget;
        private SpriteBatch _spriteBatch;
        private RenderTargetType _renderTargetToShow;

        /// <inheritdoc />
        public override void Initialize()
        {
            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0f, 0f, 300f));
            _time = 0;
            
            base.Initialize();
        }
        
        /// <inheritdoc />
        protected override void LoadContent()
        {
            _model = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            // From the effect of the model I keep the texture.
            _texture = ((BasicEffect) _model.Meshes.FirstOrDefault()?.MeshParts.FirstOrDefault()?.Effect)?.Texture;

            // Load a shader using Content pipeline.
            _effect = Game.Content.Load<Effect>(ContentFolderEffects + "MRT");

            // Set the texture of the model in the shader
            _effect.Parameters["ModelTexture"].SetValue(_texture);
            
            // For faster access in draw
            _effectWorld = _effect.Parameters["World"];
            _effectWorldViewProjection = _effect.Parameters["WorldViewProjection"];
            _effectTime = _effect.Parameters["Time"];

            // Asign the effect to the meshes
            foreach (var mesh in _model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = _effect;
                }
            }

            // Create the targets we are going to use
            _colorTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
               GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
               RenderTargetUsage.DiscardContents);
            _inverseColorTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            _normalTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            _animatedTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);

            // To easily draw render targets
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            ModifierController.AddOptions("Choose Target", new []
            {
                "All targets",
                "Color",
                "Inverted Color",
                "Normals",
                "Animated Color",
            }, RenderTargetType.AllTargets, OnTargetSwitch);

            ModifierController.AddTexture("Base Color Target", _colorTarget);

            base.LoadContent();
        }

        private void OnTargetSwitch(RenderTargetType renderTargetToShow)
        {
            _renderTargetToShow = renderTargetToShow;
        }

        public override void Update(GameTime gameTime)
        {
            _camera.Update(gameTime);

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Set Time value in effect
            _time += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            _effectTime.SetValue(_time);
            
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Set the render targets we are going to be drawing to, in the correct order
            GraphicsDevice.SetRenderTargets(_colorTarget, _inverseColorTarget, _normalTarget, _animatedTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
           
            // Draw our model or models, keep in mind that all of them must have MRT effect assigned

            var mesh = _model.Meshes.FirstOrDefault();
            if (mesh != null)
            {
                var world = mesh.ParentBone.Transform;

                _effectWorld.SetValue(world);
                _effectWorldViewProjection.SetValue(world * _camera.View * _camera.Projection);
                mesh.Draw();
            }

            // Now we can draw any target, or send them as textures to another shader
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.White);
            
            var width = GraphicsDevice.Viewport.Width;
            var height = GraphicsDevice.Viewport.Height;
            var halfWidth = width / 2;
            var halfHeight = height / 2;

            var topLeft = Vector2.Zero;
            var topRight = Vector2.UnitX * halfWidth;
            var bottomLeft = Vector2.UnitY * halfHeight;
            var bottomRight = new Vector2(halfWidth, halfHeight);

            var scale = 0.5f;

            _spriteBatch.Begin();
            
            // Verify default begin options in your project (RasterizerState, DepthStencil...)
            // Draw selected target
            switch (_renderTargetToShow)
            {
                default:
                case RenderTargetType.AllTargets:
                    _spriteBatch.Draw(_colorTarget, topLeft,
                        null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                    _spriteBatch.Draw(_inverseColorTarget, topRight,
                        null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                    _spriteBatch.Draw(_normalTarget, bottomLeft,
                        null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                    _spriteBatch.Draw(_animatedTarget, bottomRight,
                        null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                    break;
                case RenderTargetType.Color:
                    _spriteBatch.Draw(_colorTarget, topLeft, Color.White);
                    break;
                case RenderTargetType.InvertedColor:
                    _spriteBatch.Draw(_inverseColorTarget, topLeft, Color.White);
                    break;
                case RenderTargetType.Normals:
                    _spriteBatch.Draw(_normalTarget, topLeft, Color.White);
                    break;
                case RenderTargetType.AnimatedColor:
                    _spriteBatch.Draw(_animatedTarget, topLeft, Color.White);
                    break;
            }

            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
        private enum RenderTargetType
        {
            AllTargets,
            Color,
            InvertedColor,
            Normals,
            AnimatedColor,
        }
    }
}