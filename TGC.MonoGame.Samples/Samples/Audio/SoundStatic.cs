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
            Name = "Sound Effect";
            Description = "Shows how to play a sound file. Audio from https://www.fesliyanstudios.com";
        }

        private SpriteFont _font;
        private string _instructions;
        private Vector2 _instructionsSize;
        private SoundEffect _sound;
        private SoundEffectInstance _instance;
        private string _soundName;

        /// <inheritdoc />
        public override void Initialize()
        {
            _instructions = "Y = Play a new instance in loop, I = Play and forget.";
            _soundName = "No sound";

            Game.Gizmos.Enabled = false;

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            _font = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
            _instructionsSize = _font.MeasureString(_instructions);
            _soundName = "a2-8bit";
            _sound = Game.Content.Load<SoundEffect>(ContentFolderSounds + _soundName);

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Y))
            {
                // Play that can be manipulated after the fact.
                _instance = _sound.CreateInstance();
                _instance.IsLooped = true;
                _instance.Play();
            }
            else if (Game.CurrentKeyboardState.IsKeyDown(Keys.I))
            {
                // Fire and forget play.
                _sound.Play();
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;

            Game.SpriteBatch.Begin();

            var soundNamePosition = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2f, 20) -
                                    _font.MeasureString(_soundName) / 2;
            Game.SpriteBatch.DrawString(_font, "Playing: " + _soundName, soundNamePosition, Color.DarkMagenta);
            var instructionsPosition = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2f, 60) -
                                       _instructionsSize / 2;
            Game.SpriteBatch.DrawString(_font, _instructions, instructionsPosition, Color.DarkGreen);

            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}