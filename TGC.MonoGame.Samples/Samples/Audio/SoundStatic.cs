using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Audio
{
    /// <summary>
    ///     Static Sound:
    ///     Units Involved:
    ///     # Unit 3 - 3D Basics - Game Engine.
    ///     Shows how to play a static sound file.
    ///     Authors: Matias Leone, Leandro Barbagallo.
    /// </summary>
    public class SoundStatic : TGCSample
    {
        /// <inheritdoc />
        public SoundStatic(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Audio;
            Name = "Sound effect";
            Description = "Shows how to play a sound file. Audio from https://www.fesliyanstudios.com";
        }

        private SpriteFont Font { get; set; }
        private string Instructions { get; set; }
        private Vector2 InstructionsSize { get; set; }
        private SoundEffect Sound { get; set; }
        private SoundEffectInstance Instance { get; set; }
        private string SoundName { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Instructions = "Y = Play a new instance in loop, I = Play and forget.";
            SoundName = "No sound";

            Game.Gizmos.Enabled = false;

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            Font = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
            InstructionsSize = Font.MeasureString(Instructions);
            SoundName = "a2-8bit";
            Sound = Game.Content.Load<SoundEffect>(ContentFolderSounds + SoundName);

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Y))
            {
                // Play that can be manipulated after the fact
                Instance = Sound.CreateInstance();
                Instance.IsLooped = true;
                Instance.Play();
            }
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.I))
            {
                // Fire and forget play
                Sound.Play();
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;

            Game.SpriteBatch.Begin();

            var soundNamePosition = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2f, 20) -
                                    Font.MeasureString(SoundName) / 2;
            Game.SpriteBatch.DrawString(Font, "Playing: " + SoundName, soundNamePosition, Color.DarkMagenta);
            var instructionsPosition = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2f, 60) -
                                       InstructionsSize / 2;
            Game.SpriteBatch.DrawString(Font, Instructions, instructionsPosition, Color.DarkGreen);

            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}