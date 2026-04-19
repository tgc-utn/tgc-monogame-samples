using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Samples.UI;

public class ControlsSystem : IDisposable
{
    internal SpriteBatch SpriteBatch { get; private set; }

    private Texture2D _dummyTexture;
    

    public ControlsSystem(GraphicsDevice device)
    {
        _dummyTexture = new Texture2D(device, 1, 1);
        _dummyTexture.SetData(new[] { Color.White });

        SpriteBatch = new SpriteBatch(device);
    }

    public void Begin()
    {
        SpriteBatch.Begin();
    }

    public void End()
    {
        SpriteBatch.End();
    }

    internal void DrawRectangle(in Rectangle rectangle, Color color)
    {
        SpriteBatch.Draw(_dummyTexture, rectangle, color);
    }

    internal void DrawTexture(Texture2D texture, in Rectangle rectangle, Color color)
    {
        SpriteBatch.Draw(texture, rectangle, color);
    }
    
    public void Dispose()
    {
        _dummyTexture.Dispose();
        SpriteBatch.Dispose();  
    }
}