using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.Samples.Samples.UI.SceneTransitions;

public class MenuScene : IScene
{
    private const string Text = "Start Game";
    
    private readonly Action _transitionToGame;
    private Button _buttonStart;
    private ControlsSystem _system;
    private SpriteFont _spriteFont;
    private Vector2 _textPosition;
    private string _contentFolderSpriteFonts;

    public MenuScene(Action transitionToGameAction, string contentFolderSpriteFonts)
    {
        _transitionToGame = transitionToGameAction;
        _contentFolderSpriteFonts = contentFolderSpriteFonts;
    }

    public void LoadContent(ContentManager content, GraphicsDeviceManager graphics,
        ControlsSystem controls)
    {
        var baseTexture = content.Load<Texture2D>("Textures/ui/Robin/Default");
        var hoverTexture = content.Load<Texture2D>("Textures/ui/Robin/Hover");
        
        _spriteFont = content.Load<SpriteFont>(_contentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
        
        Point size = new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

        _system = controls;
        _buttonStart = new Button(controls, new Point(size.X / 2, size.Y / 2),
            _transitionToGame, baseTexture, hoverTexture);

        _textPosition = _buttonStart.Center - _spriteFont.MeasureString(Text) / 2f - new Vector2(0f, 20f);
    }

    public void Update(in MouseState mouseState, in KeyboardState keyboardState, GameTime gameTime)
    {
        _buttonStart.Update(mouseState);
    }

    public void Draw(GameTime gameTime)
    {
        _system.Begin();
        _buttonStart.Draw();
        _system.SpriteBatch.DrawString(_spriteFont, Text, _textPosition, Color.Black);
        _system.End();
    }
}
