using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.UI;

public class BasicControls : TGCSample
{
    private ControlsSystem _controls;
    
    private Button _buttonA;
    private Button _buttonB;
    private Button _buttonC;

    private Slider _slider;

    private string _text = "";
    private float _sliderValue = 0.5f;

    private SpriteFont _spriteFont;

    public BasicControls(TGCViewer game) : base(game)
    {
        Category = TGCSampleCategory.UI;
        Name = GetType().Name;
        Description = "Shows basic controls that can be used on any graphics application";
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        _controls = new ControlsSystem(GraphicsDevice);
        
        var baseTexture = Game.Content.Load<Texture2D>("Textures/ui/Robin/Default");
        var hoverTexture = Game.Content.Load<Texture2D>("Textures/ui/Robin/Hover");

        _spriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");

        var size = new Point(Game.Graphics.PreferredBackBufferWidth, Game.Graphics.PreferredBackBufferHeight);
        var center = new Point(size.X / 2, size.Y / 2);
        var buttonSize = new Point(200, 100);

        _buttonA = new Button(_controls, center - new Point(400, 0), buttonSize,
            ButtonOnePressed, baseTexture, null, hoverTexture);

        _buttonB = new Button(_controls, center, buttonSize,
            ButtonTwoPressed, baseTexture, null, hoverTexture);

        _buttonC = new Button(_controls, center + new Point(400, 0), buttonSize,
            ButtonThreePressed, baseTexture, null, hoverTexture);

        var sliderTrack = new Rectangle(center.X - 300, center.Y + 150, 600, 60);
        _slider = new Slider(_controls, sliderTrack, 0f, 100f, 50f, 
            newValue => _sliderValue = newValue);
    }

    private void ButtonOnePressed() => _text = "Button One Pressed";
    private void ButtonTwoPressed() => _text = "Button Two Pressed";
    private void ButtonThreePressed() => _text = "Button Three Pressed";

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        MouseState mouseState = Mouse.GetState();
        
        _buttonA.Update(mouseState);
        _buttonB.Update(mouseState);
        _buttonC.Update(mouseState);
        _slider.Update(mouseState);

        if (_buttonA.CurrentState != Button.State.Pressed &&
            _buttonB.CurrentState != Button.State.Pressed &&
            _buttonC.CurrentState != Button.State.Pressed)
        {
            _text = "";
        }
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        var previousBlendState = GraphicsDevice.BlendState;
        var previousRasterizerState = GraphicsDevice.RasterizerState;

        // Buttons and Slider
        _controls.Begin();
        
        _buttonA.Draw();
        _buttonB.Draw();
        _buttonC.Draw();
        _slider.Draw();
        
        // Text for button press and slider values 
        Point screenSize = new Point(Game.Graphics.PreferredBackBufferWidth, Game.Graphics.PreferredBackBufferHeight);
        
        DrawControlTexts(screenSize);
        
        DrawAlignedTexts(screenSize);
        
        _controls.End();
        
        GraphicsDevice.BlendState = previousBlendState;
        GraphicsDevice.RasterizerState = previousRasterizerState;
    }

    private void DrawControlTexts(in Point screenSize)
    {
        Point center = new Point(screenSize.X / 2, screenSize.Y / 2);
        Vector2 textSize = _spriteFont.MeasureString(_text);
        
        _controls.SpriteBatch.DrawString(_spriteFont, _text, 
            center.ToVector2() + new Vector2(-textSize.X * 0.5f, -100f), Color.Red);

        string sliderLabel = $"Slider: {_sliderValue:F1}";
        _controls.SpriteBatch.DrawString(_spriteFont, sliderLabel, center.ToVector2() + new Vector2(-50, 220), Color.White);
    }

    private void DrawAlignedTexts(in Point screenSize)
    {
        _controls.SpriteBatch.DrawString(_spriteFont, "Left-Aligned Text", 
            new Vector2(40f, 100f), Color.Red);

        string centerText = "Centered Text";
        Vector2 centerAlignedTextSize = _spriteFont.MeasureString(centerText);
        _controls.SpriteBatch.DrawString(_spriteFont, centerText, 
            new Vector2((screenSize.X - centerAlignedTextSize.X) / 2f, 100f), Color.Red);
        
        string rightText = "Right-Aligned Text";
        Vector2 rightAlignedTextSize = _spriteFont.MeasureString(rightText);
        _controls.SpriteBatch.DrawString(_spriteFont, rightText, 
            new Vector2(screenSize.X - rightAlignedTextSize.X - 40f, 100f), Color.Red);
    }
}
