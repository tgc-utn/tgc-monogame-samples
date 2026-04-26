using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.UI.SceneTransitions;

/// <summary>
/// Shows how to implement menu transitions within the same game.
/// </summary>
public class SceneTransitions : TGCSample
{
    private IScene _menuScene;
    private IScene _pauseScene;
    private IScene _gameScene;
    private IScene _currentScene;
    private ControlsSystem _system;

    public SceneTransitions(TGCViewer game) : base(game)
    {
        Category = TGCSampleCategory.UI;
        Name = GetType().Name;
        Description = "Shows how to implement menu transitions within the same game. " +
                      "Press P to pause and TAB to go back to the main menu.";
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        _system = new ControlsSystem(GraphicsDevice);

        _menuScene = new MenuScene(() => { _currentScene = _gameScene; }, ContentFolderSpriteFonts);
        _menuScene.LoadContent(Game.Content, Game.Graphics, _system);

        _gameScene = new GameScene(() => _currentScene = _pauseScene, 
            () =>
            {
                _currentScene = _menuScene;
                ((GameScene)_gameScene).Restart();
            });
        _gameScene.LoadContent(Game.Content, Game.Graphics, _system);

        _pauseScene = new PauseScene(() => _currentScene = _gameScene);
        _pauseScene.LoadContent(Game.Content, Game.Graphics, _system);

        _currentScene = _menuScene;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        var mouseState = Mouse.GetState();
        var keyboardState = Keyboard.GetState();

        _currentScene.Update(mouseState, keyboardState, gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        _currentScene.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();
        _system.Dispose();
        ((IDisposable)_gameScene).Dispose();
    }
}
