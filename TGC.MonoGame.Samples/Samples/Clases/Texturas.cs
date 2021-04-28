using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Tutorials
{
    /// <summary>
    ///     Tutorial 4:
    ///     Units Involved:
    ///     # Unit 4 - Textures and lighting - Textures
    ///     Shows how to create a Quad and a Box with a 2D image as a texture to give it color.
    ///     Author: Matías Leone.
    /// </summary>
    public class ClaseTexturas : TGCSample
    {
        /// <inheritdoc />
        public ClaseTexturas(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Clases;
            Name = "Texturas";
            Description = "Shows how to create a Quad and a Box with a 2D image as a texture to give it color.";
        }

        private Camera Camera { get; set; }
        private QuadPrimitive Quad { get; set; }
        private BoxPrimitive Box { get; set; }
        private Matrix BoxWorld { get; set; }
        private float BoxRotation { get; set; }

        private Effect Effect { get; set; }

        private float Tiling { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0,10,60));

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            var texture = Game.Content.Load<Texture2D>(ContentFolderTextures + "wood/caja-madera-3");
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "textures");
            Effect.Parameters["ModelTexture"].SetValue(texture);
            Quad = new QuadPrimitive(GraphicsDevice, Vector3.Zero, Vector3.Backward, Vector3.Up, 22, 22, texture,
                1);
            Box = new BoxPrimitive(GraphicsDevice, Vector3.One * 20, texture);
            BoxWorld = Matrix.CreateTranslation(Vector3.UnitX * -14);
            Tiling = 4f;

            base.LoadContent();
        }

        private KeyboardState PastState;

        private bool isOnKeyUp(KeyboardState state, Keys key) 
        {
            return PastState.IsKeyDown(key) && state.IsKeyUp(key);
        }
        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);
            BoxRotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            var keyboardState = Keyboard.GetState();
            float delta = 1f ;
            if(isOnKeyUp(keyboardState, Keys.Q)){
                Tiling -= delta;
            } else if(isOnKeyUp(keyboardState, Keys.E)){
                Tiling += delta;
            }

            PastState = keyboardState;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            Effect.Parameters["View"].SetValue(Camera.View);
            Effect.Parameters["Projection"].SetValue(Camera.Projection);

            Effect.Parameters["World"].SetValue(Matrix.CreateRotationY(BoxRotation) * BoxWorld);
            Effect.Parameters["tiling"].SetValue(Tiling);

            Effect.CurrentTechnique = Effect.Techniques["TextureWrap"];

            Box.Draw(Effect);
            
            // quad abajo izquierda
            Effect.CurrentTechnique = Effect.Techniques["TextureClamp"];
            Effect.Parameters["World"].SetValue(Matrix.CreateTranslation(Vector3.UnitX * 14));
            Quad.Draw(Effect);
            // quad abajo derecha
            Effect.CurrentTechnique = Effect.Techniques["TextureMirror"];
            Effect.Parameters["World"].SetValue(Matrix.CreateTranslation(Vector3.UnitX * 38));
            Quad.Draw(Effect);
            
            // quad arriba izquierda
            Effect.CurrentTechnique = Effect.Techniques["TextureWrap"];
            Effect.Parameters["World"].SetValue(Matrix.CreateTranslation(Vector3.UnitX * 14 + Vector3.UnitY * 24));
            Quad.Draw(Effect);
            // quad arriba derecha
            Effect.CurrentTechnique = Effect.Techniques["TextureBorder"];
            Effect.Parameters["World"].SetValue(Matrix.CreateTranslation(Vector3.UnitX * 38 + Vector3.UnitY * 24));
            Quad.Draw(Effect);

            Effect.CurrentTechnique = Effect.Techniques["TextureLod"];
            var bigQuadWorld = Matrix.CreateScale(2f) * Matrix.CreateTranslation(Vector3.UnitX * (14 + 22 + 2 +22+2+22+2) + Vector3.UnitY * 11);
            Effect.Parameters["World"].SetValue(bigQuadWorld);
            Quad.Draw(Effect);

            base.Draw(gameTime);
        }
    }
}