using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;

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
            Name = "MRT";
            Description = "Multiple Render Targets Sample. Color, Inverse Color, Normal, and Filter target at the same time.";
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
        private RenderTarget2D FilterTarget;
        private SpriteBatch SpriteBatch;
        private SpriteFont SpriteFont;
        /// <inheritdoc />
        public override void Initialize()
        {
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0f, 50f, 400f));
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
            FilterTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);

            // To easily draw render targets and text
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            SpriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "Arial");

            base.LoadContent();


        }

        private int ShowTarget;
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);

            //Keyboard input
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.D1))
                ShowTarget = 1;
            else if (keyboardState.IsKeyDown(Keys.D2))
                ShowTarget = 2;
            else if (keyboardState.IsKeyDown(Keys.D3))
                ShowTarget = 3;
            else if (keyboardState.IsKeyDown(Keys.D4))
                ShowTarget = 4;
            else
                ShowTarget = 0;
        }
        public override void Draw(GameTime gameTime)
        {
            // Set Time value in effect
            time += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            EffectTime.SetValue(time);
            
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Set the render targets we are going to be drawing to, in the correct order
            GraphicsDevice.SetRenderTargets(ColorTarget, InverseColorTarget, NormalTarget, FilterTarget);
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
            GraphicsDevice.Clear(Color.Black);
            
            SpriteBatch.Begin();

            String targetSelected = "";
            var width = GraphicsDevice.Viewport.Width;
            var height = GraphicsDevice.Viewport.Height;

            // Draw selected target
            if (ShowTarget == 0) 
            {
                SpriteBatch.Draw(ColorTarget,
                    new Vector2(0, 0), null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                SpriteBatch.Draw(InverseColorTarget,
                    new Vector2(width / 2, 0), null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                SpriteBatch.Draw(NormalTarget,
                    new Vector2(0, height / 2), null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                SpriteBatch.Draw(FilterTarget,
                    new Vector2(width / 2, height / 2), null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0);

                targetSelected = "Todos";
            }
            else if (ShowTarget == 1)
            {
                SpriteBatch.Draw(ColorTarget, Vector2.Zero, Color.White);
                targetSelected = "Color de textura";
            }
            else if (ShowTarget == 2)
            {
                SpriteBatch.Draw(InverseColorTarget, Vector2.Zero, Color.White);
                targetSelected = "Color invertido de textura";
            }
            else if (ShowTarget == 3)
            {
                SpriteBatch.Draw(NormalTarget, Vector2.Zero, Color.White);
                targetSelected = "Normales";
            }
            if (ShowTarget == 4)
            {
                SpriteBatch.Draw(FilterTarget, Vector2.Zero, Color.White);
                targetSelected = "Filtro de colores";
            }
            // Draw text
            SpriteBatch.DrawString(SpriteFont, 
                "Multiple Render Targets: Se utiliza para dibujar hasta 4 render targets al mismo tiempo", 
                new Vector2(GraphicsDevice.Viewport.Width / 3, 0), Color.White);

            SpriteBatch.DrawString(SpriteFont, 
                "Mostrando "+targetSelected+ ", para cambiar usar numeros 1,2,3,4", 
                new Vector2(GraphicsDevice.Viewport.Width / 3, 40), Color.White);

            SpriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}