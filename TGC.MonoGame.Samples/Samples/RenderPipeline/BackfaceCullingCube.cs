using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.RenderPipeline;

/// <summary>
///     Default example with TGC logo.
/// </summary>
public class BackfaceCullingCube : TGCSample
{
    private readonly List<CullMode> _cullModes =
    [
        CullMode.CullClockwiseFace,
        CullMode.None,
        CullMode.CullCounterClockwiseFace,
    ];

    private Quaternion _rotation = Quaternion.Identity;

    // The following fields are initialized in LoadContent, so they are marked as non-nullable with the null-forgiving operator (!).
    private Camera _camera = null!;

    private CubePrimitive _cube = null!;

    private Effect _effect = null!;

    private SpriteFont _spriteFont = null!;

    private bool _wireframeEnabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackfaceCullingCube"/> class.
    ///     Default constructor.
    /// </summary>
    /// <param name="game">The game.</param>
    public BackfaceCullingCube(TGCViewer game)
        : base(game)
    {
        Name = "Back-Face Culling Cube";
        Description = Description = "Visualize the back-face culling command";
        Category = TGCSampleCategory.RenderPipeline;
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        _camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.UnitZ * 150, Vector3.UnitZ);
        base.Initialize();
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        var totalTime = (float)gameTime.TotalGameTime.TotalSeconds;
        _rotation = Quaternion.CreateFromYawPitchRoll(MathF.Sin(totalTime), MathF.Cos(totalTime), MathF.Sin(MathF.Abs(totalTime)));
        Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

        base.Update(gameTime);
    }

    /// <inheritdoc />
    public override void Draw(GameTime gameTime)
    {
        Game.Background = Color.Black;
        _effect.Parameters["ViewProjection"].SetValue(_camera.View * _camera.Projection);

        var existingRasterizerState = GraphicsDevice.RasterizerState;
        var step = 35f;
        var offset = -step;
        foreach (var cullMode in _cullModes)
        {
            GraphicsDevice.RasterizerState = new RasterizerState { CullMode = cullMode, FillMode = _wireframeEnabled ? FillMode.WireFrame : FillMode.Solid };
            var world = Matrix.CreateFromQuaternion(_rotation) * Matrix.CreateTranslation(offset, 0f, 0f);
            _effect.Parameters["World"].SetValue(world);
            _cube.Draw(_effect);
            offset += step;
        }

        // Always restore the previous rasterizer state after drawing
        GraphicsDevice.RasterizerState = existingRasterizerState;

        DrawLabels();
        base.Draw(gameTime);
    }

    /// <inheritdoc />
    protected override void LoadContent()
    {
        // Load mesh.
        _cube = new CubePrimitive(GraphicsDevice, 10f, Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Cyan, Color.Magenta);
        _effect = Game.Content.Load<Effect>(ContentFolderEffects + "ExplodeColored");
        _spriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
        ModifierController.AddToggle("Show Wireframe", (enabled) => _wireframeEnabled = enabled, false);
        base.LoadContent();
    }

    /// <inheritdoc />
    protected override void UnloadContent()
    {
        _cube.Dispose();

        _effect.Dispose();
        _spriteFont.Texture.Dispose();
        base.UnloadContent();
    }

    private void DrawLabels()
    {
        // Draw labels
        Game.SpriteBatch.Begin(
            SpriteSortMode.Immediate,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.Default,
            RasterizerState.CullNone);

        var step = 35f;
        var offset = -step;
        foreach (var cm in _cullModes)
        {
            string label = cm switch
            {
                CullMode.CullClockwiseFace => "Cull Clockwise",
                CullMode.None => "No Culling",
                CullMode.CullCounterClockwiseFace => "Cull Counter-Clockwise",
                _ => "Unknown",
            };

            var size = _spriteFont.MeasureString(label) / 2f;
            var projectedPosition = GraphicsDevice.Viewport.Project(
                    new Vector3(offset, -20f, 0f), _camera.Projection, _camera.View, Matrix.Identity);

            var textPosition = new Vector2(projectedPosition.X, projectedPosition.Y) - size;

            Game.SpriteBatch.DrawString(_spriteFont, label, textPosition, Color.White);

            offset += step;
        }

        Game.SpriteBatch.End();
    }
}
