using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.Samples.Samples.UI.SceneTransitions;

public class PauseScene : IScene
{
    private const string Text = "Game paused. Press 'P' to unpause.";

    private readonly Action _unpause;
    private ControlsSystem _system;
    private SpriteFont _font;
    private Vector2 _textPosition;
    private bool _wasPressed;

    public PauseScene(Action unpause)
    {
        _unpause = unpause;
        _wasPressed = true;
    }

    public void LoadContent(ContentManager content, GraphicsDeviceManager graphics, ControlsSystem controls)
    {
        _system = controls;
        _font = content.Load<SpriteFont>("SpriteFonts/CascadiaCode/CascadiaCodePL");

        Vector2 size = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
        Vector2 textSize = _font.MeasureString(Text);
        _textPosition = new Vector2((size.X - textSize.X), size.Y) / 2f;
    }

    public void Update(in MouseState mouseState, in KeyboardState keyboardState, GameTime gameTime)
    {
        bool isPressed = keyboardState.IsKeyDown(Keys.P);
        
        if (isPressed && !_wasPressed)
        {
            _unpause.Invoke();
        }
        
        _wasPressed = isPressed;
    }

    public void Draw(GameTime gameTime)
    {
        var device = _system.SpriteBatch.GraphicsDevice;
        device.Clear(Color.DarkSlateGray);
        device.DepthStencilState = DepthStencilState.Default;

        _system.Begin();
        _system.SpriteBatch.DrawString(_font, Text, _textPosition, Color.White);
        _system.End();
    }

}
