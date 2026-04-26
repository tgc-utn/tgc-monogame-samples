using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.Samples.Samples.UI.SceneTransitions;

public interface IScene
{
    void LoadContent(ContentManager content, GraphicsDeviceManager graphics, ControlsSystem controls);

    void Update(in MouseState mouseState, in KeyboardState keyboardState, GameTime gameTime);

    void Draw(GameTime gameTime);
}
