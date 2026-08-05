using System;
using System.IO;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Shaders.ShaderReloader;

/// <summary>
///     Shader Visualizer:
///     Reloads and compiles a shader file in runtime.
///     Author: Ronan Vinitzca.
/// </summary>
public class ShaderVisualizer : TGCSample
{
    private static readonly string ShaderName = "ShaderVisualizer";

    private IConfigurationRoot _configuration;
    private Effect _effect;
    private FullScreenQuad _quad;
    private ShaderReloader _shaderReloader;

    /// <inheritdoc />
    public ShaderVisualizer(TGCViewer game)
        : base(game)
    {
        Category = TGCSampleCategory.Shaders;
        Name = "Shader Visualizer";
        Description = "Shader Visualizer. Reloads and compiles a shader file in runtime.";
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        Game.Gizmos.Enabled = false;
        base.Initialize();
    }

    /// <inheritdoc />
    protected override void LoadContent()
    {
        // Configuration file.
        var configurationFileName = "app-settings.json";
        _configuration = new ConfigurationBuilder().AddJsonFile(configurationFileName, true, true).Build();

        _quad = new FullScreenQuad(GraphicsDevice);
        _effect = Game.Content.Load<Effect>(ContentFolderEffects + ShaderName);

        var effectName = ShaderName + _configuration["FbxExtension"];
        var effectPath = Path.Combine(FindProjectDirectory(), Game.Content.RootDirectory, ContentFolderEffects,
            effectName);
        _shaderReloader = new ShaderReloader(effectPath, _configuration["ContentExtension"], GraphicsDevice);
        _shaderReloader.OnCompile += OnShaderCompile;

        // Make the window squared.
        Game.Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
        Game.Graphics.ApplyChanges();

        base.LoadContent();
    }

    private void OnShaderCompile(Effect newEffect)
    {
        var actualEffect = _effect;
        _effect = newEffect;
        actualEffect.Dispose();
    }

    private string FindProjectDirectory()
    {
        var rootDirectory = Assembly.GetEntryAssembly()?.GetName().Name ?? GetType().Assembly.GetName().Name;
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var actual = new DirectoryInfo(baseDirectory);

        while (actual.Name.CompareTo(rootDirectory) != 0)
        {
            actual = actual.Parent;
            if (actual == null)
            {
                throw new DirectoryNotFoundException("Cannot find the project root.");
            }
        }

        return actual.FullName;
    }

    /// <inheritdoc />
    public override void Draw(GameTime gameTime)
    {
        Game.Background = Color.Black;
        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        GraphicsDevice.BlendState = BlendState.Opaque;

        _effect.Parameters["Time"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
        _quad.Draw(_effect);
        base.Draw(gameTime);
    }

    /// <inheritdoc />
    protected override void UnloadContent()
    {
        _quad?.Dispose();
        _shaderReloader?.Dispose();
        _effect?.Dispose();

        // Restore window width.
        Game.Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
        Game.Graphics.ApplyChanges();

        base.UnloadContent();
    }
}
