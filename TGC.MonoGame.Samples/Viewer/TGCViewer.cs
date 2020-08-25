using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Viewer.Models;

namespace TGC.MonoGame.Samples.Viewer
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TGCViewer : Game
    {
        /// <summary>
        /// The folder which the game will search for content.
        /// </summary>
        public const string ContentFolder = "Content";

        /// <summary>
        /// Initializes a new instance of the <see cref="TGCViewer" /> class.
        /// The main game constructor is used to initialize the starting variables.
        /// </summary>
        public TGCViewer()
        {
            Graphics = new GraphicsDeviceManager(this);
            //Graphics.IsFullScreen = true;
            //Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            //Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            //Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            //Graphics.PreferMultiSampling = true;
            Content.RootDirectory = ContentFolder;
            IsMouseVisible = true;

            Model = new TGCViewerModel(this);
            Model.LoadTreeSamples();
        }

        /// <summary>
        /// Sample background color.
        /// </summary>
        public Color Background { get; set; }

        /// <summary>
        /// Represents a state of keystrokes recorded by a keyboard input device.
        /// </summary>
        public KeyboardState CurrentKeyboardState { get; set; }
        
        /// <summary>
        /// Handles the configuration and management of the graphics device.
        /// </summary>
        public GraphicsDeviceManager Graphics { get; }

        /// <summary>
        /// The model has the logic for the creation of the sample explorer.
        /// </summary>
        private TGCViewerModel Model { get; set; }

        /// <summary>
        /// Enables a group of sprites to be drawn using the same settings.
        /// </summary>
        public SpriteBatch SpriteBatch { get; set; }

        /// <summary>
        /// This method is called after the constructor, but before the main game loop (Update/Draw).
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where you can query any required services and load any non-graphic related content.
        /// Calling base.Initialize will enumerate through any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            // Needs here because of https://github.com/MonoGame/MonoGame/pull/7299
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Graphics.ApplyChanges();

            Model.LoadImgGUI();
            Model.LoadWelcomeSample();

            Background = Color.CornflowerBlue;

            base.Initialize();
        }

        /// <summary>
        /// This method is used to load your game content.
        /// It is called only once per game, after Initialize method, but before the main game loop methods.
        /// </summary>
        protected override void LoadContent()
        {
            //TODO: use this.Content to load your game content here

            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// This method is called multiple times per second, and is used to update your game state (updating the world,
        /// checking for collisions, gathering input, playing audio, etc.).
        /// </summary>
        /// <param name="gameTime">Holds the time state of a <see cref="Game" />.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here

            HandleInput();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// Similar to the Update method, it is also called multiple times per second.
        /// </summary>
        /// <param name="gameTime">Holds the time state of a <see cref="Game" />.</param>
        protected override void Draw(GameTime gameTime)
        {
            //TODO: Add your drawing code here

            GraphicsDevice.Clear(Background);

            base.Draw(gameTime);

            Model.DrawSampleExplorer(gameTime);
        }

        /// <summary>
        /// Unload the resources loaded by the game.
        /// </summary>
        protected override void UnloadContent()
        {
            Model.Dispose();
            Content.Unload();
        }

        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        private void HandleInput()
        {
            CurrentKeyboardState = Keyboard.GetState();

            // Check for exit.
            if (CurrentKeyboardState.IsKeyDown(Keys.Escape)) Exit();
        }
    }
}