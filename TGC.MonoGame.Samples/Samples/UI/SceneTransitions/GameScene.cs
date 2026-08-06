using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Geometries;

namespace TGC.MonoGame.Samples.Samples.UI.SceneTransitions;

public class GameScene : IScene, IDisposable
{
    private readonly Action _pause;
    private readonly Action _exit;
    private GraphicsDevice _graphicsDevice;
    private CubePrimitive _plane;
    private CubePrimitive _cube;
    private CubePrimitive _oscillatingCube;
    private Matrix _view;
    private Matrix _projection;
    private float _time;
    private bool _wasPausePressed;
    private bool _wasTabPressed;

    public GameScene(Action pause, Action exit)
    {
        _pause = pause;
        _exit = exit;
    }

    public void LoadContent(ContentManager content, GraphicsDeviceManager graphics, ControlsSystem controls)
    {
        _graphicsDevice = graphics.GraphicsDevice;

        _plane = new CubePrimitive(_graphicsDevice, 1f, Color.ForestGreen);
        _cube = new CubePrimitive(_graphicsDevice, 1f, Color.CornflowerBlue);
        _oscillatingCube = new CubePrimitive(_graphicsDevice, 1f, Color.OrangeRed);

        _view = Matrix.CreateLookAt(new Vector3(0f, 5f, 10f), Vector3.Zero, Vector3.Up);
        _projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            _graphicsDevice.Viewport.AspectRatio,
            0.1f,
            100f);
    }

    public void Restart()
    {
        _time = 0f;
        _view = Matrix.CreateLookAt(new Vector3(0f, 5f, 10f), Vector3.Zero, Vector3.Up);
        _projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            _graphicsDevice.Viewport.AspectRatio,
            0.1f,
            100f);
    }

    public void Update(in MouseState mouseState, in KeyboardState keyboardState, GameTime gameTime)
    {
        _time += (float)gameTime.ElapsedGameTime.TotalSeconds;

        bool isPausePressed = keyboardState.IsKeyDown(Keys.P);

        if (isPausePressed && !_wasPausePressed)
        {
            _pause.Invoke();
        }
        
        _wasPausePressed = isPausePressed;

        bool isTabPressed = keyboardState.IsKeyDown(Keys.Tab);
        
        if (isTabPressed && !_wasTabPressed)
        {
            _exit.Invoke();
        }
        
        _wasTabPressed = isTabPressed;
    }

    public void Draw(GameTime gameTime)
    {
        _graphicsDevice.Clear(Color.DarkSlateGray);
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;

        Matrix planeWorld = Matrix.CreateScale(10f, 0.1f, 10f) * Matrix.CreateTranslation(0f, -0.55f, 0f);
        _plane.Draw(planeWorld, _view, _projection);

        Matrix cubeWorld = Matrix.CreateTranslation(-2f, 0.5f, 0f);
        _cube.Draw(cubeWorld, _view, _projection);

        float oscillation = MathF.Sin(_time * 2f) * 1.5f;
        Matrix oscillatingCubeWorld = Matrix.CreateTranslation(2f, 0.5f + oscillation, 0f);
        _oscillatingCube.Draw(oscillatingCubeWorld, _view, _projection);
    }

    public void Dispose()
    {
        _plane?.Dispose();
        _cube?.Dispose();
        _oscillatingCube?.Dispose();
    }
}
