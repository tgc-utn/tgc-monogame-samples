using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.Samples.Samples.UI;

public class Slider
{
    private const int ThumbWidth = 20;
    private const int ThumbHeight = 40;
    private const int TrackHeight = 8;
    
    private readonly ControlsSystem _controls;

    private readonly Rectangle _trackBounds;
    private Rectangle _thumbBounds;
    
    private readonly float _min;
    private readonly float _max;

    private readonly Action<float> _onValueChanged;

    private bool _isDragging;
    private bool _isHovered;

    public float Value { get; private set; }

    /// <param name="controls">A system that allows for drawing rectangles and manages the controls</param>
    /// <param name="trackBounds">The full area of the slider track</param>
    /// <param name="min">Minimum value the slider can slide to</param>
    /// <param name="max">Maximum value the slider can slide to</param>
    /// <param name="initialValue">Starting value (clamped to [min, max])</param>
    /// <param name="onValueChanged">Called whenever the value changes while dragging</param>
    public Slider(ControlsSystem controls, Rectangle trackBounds, float min, float max, float initialValue,
        Action<float> onValueChanged)
    {
        _controls = controls;
        _trackBounds = trackBounds;
        _min = min;
        _max = max;
        _onValueChanged = onValueChanged;
        
        Value = Math.Clamp(initialValue, min, max);

        _thumbBounds = ComputeThumbBounds();
    }

    private Rectangle ComputeThumbBounds()
    {
        float t = (Value - _min) / (_max - _min);
        int thumbX = _trackBounds.X + (int)(t * (_trackBounds.Width - ThumbWidth));
        int thumbY = _trackBounds.Center.Y - ThumbHeight / 2;
        return new Rectangle(thumbX, thumbY, ThumbWidth, ThumbHeight);
    }

    public void Update(in MouseState mouseState)
    {
        var mousePosition = new Point(mouseState.X, mouseState.Y);
        bool mousePressed = mouseState.LeftButton == ButtonState.Pressed;
        
        _isHovered = _thumbBounds.Contains(mousePosition);
        
        // Start dragging, but could be dragging from past ticks
        if (_isHovered && mousePressed)
        {
            _isDragging = true;
        }

        if (!_isDragging)
        {
            return;
        }
        
        if (!mousePressed)
        {
            _isDragging = false;
        }
        else
        {
            float relativeX = mouseState.X - _trackBounds.X - ThumbWidth / 2;
            float totalWidth = _trackBounds.Width - ThumbWidth;
            
            float newValue = MathUtils.MathUtils.RemapClamped(0f, totalWidth, _min, _max, relativeX);
                
            if (Math.Abs(newValue - Value) > 0.0001f)
            {
                Value = newValue;
                _onValueChanged?.Invoke(Value);
            }
                
            _thumbBounds = ComputeThumbBounds();
        }
    }

    public void Draw()
    {
        var trackRect = new Rectangle(
            _trackBounds.X,
            _trackBounds.Center.Y - TrackHeight / 2,
            _trackBounds.Width,
            TrackHeight);
        
        _controls.DrawRectangle(trackRect, Color.DimGray);

        // Filled portion (left of thumb)
        var filledRect = new Rectangle(trackRect.X, trackRect.Y, _thumbBounds.Center.X - trackRect.X, TrackHeight);
        _controls.DrawRectangle(filledRect, Color.CornflowerBlue);
        
        // Thumb
        Color thumbColor = _isDragging ? Color.LightSkyBlue : _isHovered ? Color.LightBlue : Color.White;
        _controls.DrawRectangle(_thumbBounds, thumbColor);
    }
}
