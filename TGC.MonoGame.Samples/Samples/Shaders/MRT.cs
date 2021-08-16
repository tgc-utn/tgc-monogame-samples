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
        private float time;

        private Camera Camera { get; set; }
        private Effect Effect { get; set; }
        private Model Model { get; set; }
        private Texture2D Texture { get; set; }

        private EffectParameter EffectWorld;
        private EffectParameter EffectWorldViewProjection;
        private EffectParameter EffectTime;

        private RenderTarget2D ColorTarget;
        private RenderTarget2D InverseColorTarget;
        private RenderTarget2D NormalTarget;
        private RenderTarget2D AnimatedTarget;
        private SpriteBatch SpriteBatch;
        private int ShowTarget;

        /// <inheritdoc />
        public override void Initialize()
        {
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0f, 0f, 300f));
            time = 0;

            
            base.Initialize();
        }
        
        
        /// <inheritdoc />
        protected override void LoadContent()
        {
            Model = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            // From the effect of the model I keep the texture.
            Texture = ((BasicEffect) Model.Meshes.FirstOrDefault()?.MeshParts.FirstOrDefault()?.Effect)?.Texture;

            // Load a shader using Content pipeline.
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "MRT");

            // Set the texture of the model in the shader
            Effect.Parameters["ModelTexture"].SetValue(Texture);
            
            // For faster access in draw
            EffectWorld = Effect.Parameters["World"];
            EffectWorldViewProjection = Effect.Parameters["WorldViewProjection"];
            EffectTime = Effect.Parameters["Time"];

            // Asign the effect to the meshes
            foreach (var mesh in Model.Meshes)
                foreach (var part in mesh.MeshParts)
                    part.Effect = Effect;

            // Create the targets we are going to use
            ColorTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
               GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
               RenderTargetUsage.DiscardContents);
            InverseColorTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            NormalTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            AnimatedTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);

            // To easily draw render targets
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            
            Modifiers = new IModifier[]
            {
                new OptionsModifier("Choose Target", new string[]
                            {
                                "All targets",
                                "Color",
                                "Inverted Color",
                                "Normals",
                                "Animated Color",
                            }, 0, OnTargetSwitch)
                ,
                new TextureModifier("Base Color Target", ColorTarget)
            };


            base.LoadContent();


        }
        private void OnTargetSwitch(int index, string name)
        {
            ShowTarget = index;
        }
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Set Time value in effect
            time += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            EffectTime.SetValue(time);
            
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Set the render targets we are going to be drawing to, in the correct order
            GraphicsDevice.SetRenderTargets(ColorTarget, InverseColorTarget, NormalTarget, AnimatedTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
           
            // Draw our model or models, keep in mind that all of them must have MRT effect assigned

            var mesh = Model.Meshes.FirstOrDefault();
            if (mesh != null)
            {
                var world = mesh.ParentBone.Transform;

                EffectWorld.SetValue(world);
                EffectWorldViewProjection.SetValue(world * Camera.View * Camera.Projection);
                mesh.Draw();
            }

            // Now we can draw any target, or send them as textures to another shader
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.White);
            
            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            Vector2 topLeft = Vector2.Zero;
            Vector2 topRight = Vector2.UnitX * halfWidth;
            Vector2 bottomLeft = Vector2.UnitY * halfHeight;
            Vector2 bottomRight = new Vector2(halfWidth, halfHeight);

            float scale = 0.5f;

            SpriteBatch.Begin();
            // Verify default begin options in your project (RasterizerState, DepthStencil...)
            // Draw selected target
            if (ShowTarget == 0) 
            {
                SpriteBatch.Draw(ColorTarget, topLeft,
                    null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                SpriteBatch.Draw(InverseColorTarget, topRight,
                    null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                SpriteBatch.Draw(NormalTarget, bottomLeft,
                    null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                SpriteBatch.Draw(AnimatedTarget, bottomRight,
                    null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            }
            else if (ShowTarget == 1)
                SpriteBatch.Draw(ColorTarget, topLeft, Color.White);
            
            else if (ShowTarget == 2)
                SpriteBatch.Draw(InverseColorTarget, topLeft, Color.White);
            
            else if (ShowTarget == 3)
                SpriteBatch.Draw(NormalTarget, topLeft, Color.White);
            
            else if (ShowTarget == 4)
                SpriteBatch.Draw(AnimatedTarget, topLeft, Color.White);
                        
            SpriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}