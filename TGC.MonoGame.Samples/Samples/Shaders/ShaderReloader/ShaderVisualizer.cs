
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Reflection;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Shaders
{
    /// <summary>
    ///     Shader Visualizer
    ///     Reloads and compiles a shader file in runtime
    /// </summary>
    public class ShaderVisualizer : TGCSample
    {
        private static readonly string ShaderName = "ShaderVisualizer";

        private static readonly string ContentFolderName = "Content/";


        private FullScreenQuad Quad { get; set; }

        private Effect Effect { get; set; }

        private ShaderReloader ShaderReloader { get; set; }


        /// <inheritdoc />
        public ShaderVisualizer(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Shaders;
            Name = "Shader Visualizer";
            Description = "Shader Visualizer. Reloads and compiles a shader file in runtime.";
        }

        public override void Initialize()
        {
            Game.Gizmos.Enabled = false;
            base.Initialize();
        }


        /// <inheritdoc />
        protected override void LoadContent()
        {
            Quad = new FullScreenQuad(GraphicsDevice);

            Effect = Game.Content.Load<Effect>(ContentFolderEffects + ShaderName);

            string projectDirectory = FindProjectDirectory() + "/";
            ShaderReloader = new ShaderReloader(projectDirectory + ContentFolderName + ContentFolderEffects + ShaderName + ".fx", GraphicsDevice);
            ShaderReloader.OnCompile += OnShaderCompile;

            // Make the window squared
            Game.Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Game.Graphics.ApplyChanges();

            base.LoadContent();
        }

        private void OnShaderCompile(Effect effect)
        {
            Effect.Dispose();
            Effect = effect;
        }

        private string FindProjectDirectory()
        {
            string rootDirectory;
            rootDirectory = Assembly.GetEntryAssembly()?.GetName().Name ?? Assembly.GetExecutingAssembly().GetName().Name;

            var environment = Environment.CurrentDirectory;

            var actual = new DirectoryInfo(environment);
            while (actual.Name.CompareTo(rootDirectory) != 0)
            {
                actual = actual.Parent;
                if (actual == null)
                    throw new DirectoryNotFoundException("Cannot find the project root");
            }
            return actual.FullName;
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.BlendState = BlendState.Opaque;

            Effect.Parameters["Time"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            Quad.Draw(Effect);
            base.Draw(gameTime);
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            base.UnloadContent();
            Quad.Dispose();
            ShaderReloader.Dispose();
            Effect.Dispose();
            
            // Restore window width
            Game.Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Game.Graphics.ApplyChanges();
        }


    }
}
