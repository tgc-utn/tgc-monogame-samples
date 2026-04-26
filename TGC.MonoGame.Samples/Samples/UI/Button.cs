using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.Samples.Samples.UI;

/// <summary>
/// A simple button with a pressed event.
/// </summary>
public class Button
{
    private readonly ControlsSystem _controls;

    private readonly Texture2D _baseTexture;

    private readonly Texture2D _pressedTexture;

    private readonly Texture2D _hoverTexture;

    private readonly Action _onPressed;
    
    private Rectangle _bounds;

    /// <summary>
    /// Tells the current state the button is in on this tick.
    /// </summary>
    public State CurrentState { get; private set; }

    public Vector2 Center => _bounds.Center.ToVector2();
    
    public Button(ControlsSystem system, Point position, Action onPressed, Texture2D baseTexture,
        Texture2D pressedTexture = null, Texture2D hoverTexture = null)
        : this(system, new Rectangle(position.X - baseTexture.Width / 2,
               position.Y - baseTexture.Height / 2,
               baseTexture.Width, baseTexture.Height), onPressed,
           baseTexture, pressedTexture, hoverTexture)
    { }

    public Button(ControlsSystem system, Point position, Point size, Action onPressed, Texture2D baseTexture,
        Texture2D pressedTexture = null, Texture2D hoverTexture = null)
        : this(system, new Rectangle(position.X - size.X / 2,
               position.Y - size.Y / 2,
               size.X, size.Y), onPressed,
           baseTexture, pressedTexture, hoverTexture)
    { }

    public Button(ControlsSystem system, in Rectangle rectangle, Action onPressed, Texture2D baseTexture,
        Texture2D pressedTexture = null, Texture2D hoverTexture = null)
    {
        _controls = system;
        _bounds = rectangle;
        _onPressed += onPressed;
        _baseTexture = baseTexture;
        _pressedTexture = pressedTexture;
        _hoverTexture = hoverTexture;
        CurrentState = State.None;
    }

    public void Update(in MouseState mouseState)
    {
        bool intersects = _bounds.Contains(new Point(mouseState.X, mouseState.Y));
        bool wasPressed = CurrentState == State.Pressed;
        bool pressed = intersects && mouseState.LeftButton == ButtonState.Pressed;

        if (!wasPressed && pressed)
        {
            _onPressed.Invoke();
        }

        CurrentState = pressed ? State.Pressed
            : intersects ? State.Hovered
            : State.None;
    }

    public void Draw()
    {
        switch (CurrentState)
        {
            case State.None:
                _controls.DrawTexture(_baseTexture, _bounds, Color.White);
                break;

            case State.Pressed:
                if (_pressedTexture != null)
                {
                    _controls.DrawTexture(_pressedTexture, _bounds, Color.White);
                }
                else
                {
                    _controls.DrawTexture(_baseTexture, _bounds, Color.Gray);
                }
                break;

            case State.Hovered:
                if (_hoverTexture != null)
                {
                    _controls.DrawTexture(_hoverTexture, _bounds, Color.White);
                }
                else
                {
                    _controls.DrawTexture(_baseTexture, _bounds, Color.LightGray);
                }
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public enum State
    {
        None,
        Hovered,
        Pressed,
    }
}
