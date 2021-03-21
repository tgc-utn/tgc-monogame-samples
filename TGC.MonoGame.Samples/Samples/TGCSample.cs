using Microsoft.Xna.Framework;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Viewer.GUI;

namespace TGC.MonoGame.Samples.Samples
{
    /// <summary>
    ///     Component drawable with some helpers, those who inherit from this class will be loaded automatically in the sample
    ///     tree.
    /// </summary>
    public abstract class TGCSample : DrawableGameComponent
    {
        public const string ContentFolder2D = "2D/";
        public const string ContentFolder3D = "3D/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="game">The game.</param>
        public TGCSample(TGCViewer game) : base(game)
        {
            Game = game;
        }

        /// <summary>
        ///     Category where the example belongs, this value is used to build the example tree.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        ///     The viewer where the example is shown.
        /// </summary>
        protected new TGCViewer Game { get; }

        /// <summary>
        ///     The name of the example.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Description of the topics applied in the example.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Initialize the game settings here.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        ///     Reload the sample content.
        /// </summary>
        public void ReloadContent()
        {
            LoadContent();
        }

        /// <summary>
        ///     Load all content here.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
        }

        /// <summary>
        ///     Updates the game.
        /// </summary>
        /// <param name="gameTime">Holds the time state of a <see cref="Game" />.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        ///     Draws the game.
        /// </summary>
        /// <param name="gameTime">Holds the time state of a <see cref="Game" />.</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        /// <summary>
        ///     Unloads the content for this sample.
        ///     This sample can be shown again and is not yet disposed.
        /// </summary>
        public void UnloadSampleContent()
        {
            UnloadContent();
        }

        /// <summary>
        ///     Unload any content here.
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();
            Game.Content.Unload();
        }
    }
}