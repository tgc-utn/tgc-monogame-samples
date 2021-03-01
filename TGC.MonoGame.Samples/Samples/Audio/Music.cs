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

        private SpriteFont Font { get; set; }
        private string Instructions { get; set; }
        private Vector2 InstructionsSize { get; set; }
        private Song Song { get; set; }
        private string SongName { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.CornflowerBlue;
            Instructions = "Y = Play, U = Pause, I = Resume, O = Stop.";
            SongName = "No music";
            Game.Gizmos.Enabled = false;
            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            Font = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "Arial");
            InstructionsSize = Font.MeasureString(Instructions);
            SongName = "RetroPlatforming";
            Song = Game.Content.Load<Song>(ContentFolderMusic + SongName);

            MediaPlayer.IsRepeating = true;

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Y) && MediaPlayer.State == MediaState.Stopped)
                //Parar y reproducir MP3
                MediaPlayer.Play(Song);
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.U) && MediaPlayer.State == MediaState.Playing)
                //Pausar el MP3
                MediaPlayer.Pause();
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.I) && MediaPlayer.State == MediaState.Paused)
                //Resumir la ejecución del MP3
                MediaPlayer.Resume();
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.O) && MediaPlayer.State == MediaState.Playing)
                //Parar el MP3
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
                                   Font.MeasureString(SongName) / 2;
            Game.SpriteBatch.DrawString(Font, "Playing: " + SongName, songNamePosition, Color.DarkMagenta);
            var instructionsPosition = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2f, 60) -
                                       InstructionsSize / 2;
            Game.SpriteBatch.DrawString(Font, Instructions, instructionsPosition, Color.DarkGreen);

            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}