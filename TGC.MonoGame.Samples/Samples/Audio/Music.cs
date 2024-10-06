using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Audio
{
    /// <summary>
    ///     Music:
    ///     Units Involved:
    ///     # Unit 3 - 3D Basics - Game Engine.
    ///     Shows how to play a song file.
    ///     Authors: Matias Leone, Leandro Barbagallo.
    /// </summary>
    public class Music : TGCSample
    {
        /// <inheritdoc />
        public Music(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Audio;
            Name = "Music";
            Description =
                "Shows how to play a song file in MP3 format for example. Audio from https://www.fesliyanstudios.com";
        }

        private SpriteFont _font;
        private string _instructions;
        private Vector2 _instructionsSize;
        private Song _song;
        private string _songName;

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.CornflowerBlue;
            _instructions = "Y = Play, U = Pause, I = Resume, O = Stop.";
            _songName = "No music";
            Game.Gizmos.Enabled = false;
            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            _font = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
            _instructionsSize = _font.MeasureString(_instructions);
            _songName = "retro-platforming";
            _song = Game.Content.Load<Song>(ContentFolderMusic + _songName);

            MediaPlayer.IsRepeating = true;

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Y) && MediaPlayer.State == MediaState.Stopped)
                //Start and Stop the MP3
                MediaPlayer.Play(_song);
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.U) && MediaPlayer.State == MediaState.Playing)
                //Pause the MP3
                MediaPlayer.Pause();
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.I) && MediaPlayer.State == MediaState.Paused)
                //Resume the MP3
                MediaPlayer.Resume();
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.O) && MediaPlayer.State == MediaState.Playing)
                //Stop the MP3
                MediaPlayer.Stop();

            Game.Gizmos.UpdateViewProjection(Matrix.Identity, Matrix.Identity);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;

            //TODO add magic of Song.FromUri when we have controller to do file explorer.
            Game.SpriteBatch.Begin();

            var songNamePosition = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2f, 20) -
                                   _font.MeasureString(_songName) / 2;
            Game.SpriteBatch.DrawString(_font, "Playing: " + _songName, songNamePosition, Color.DarkMagenta);
            var instructionsPosition = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2f, 60) -
                                       _instructionsSize / 2;
            Game.SpriteBatch.DrawString(_font, _instructions, instructionsPosition, Color.DarkGreen);

            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}