using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Shaders;

/// <summary>
///     Sphere Basic Shader:
///     Units Involved:
///     # Unit 8 - Video Adapters - Shaders.
///     A basic vertex shader on a sphere.
///     Author: Ronán Vinitzca
/// </summary>
public class SphereVertexShader : TGCSample
{
    private Camera _camera;

    private Effect _effect;
    
    private SpherePrimitive _sphere;

    public SphereVertexShader(TGCViewer game) : base(game)
    {
        Category = TGCSampleCategory.Shaders;
        Name = "SphereVertexShader";
        Description = "Basic Sample for demonstrating Vertex Shaders.";
    }

    public override void Initialize()
    {
        _camera = new StaticCamera(GraphicsDevice.Viewport.AspectRatio,  Vector3.Forward * 4f, Vector3.Backward, Vector3.Up);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sphere = new SpherePrimitive(GraphicsDevice, 2f, 32);

        // Load a shader using Content pipeline.
        _effect = Game.Content.Load<Effect>(ContentFolderEffects + "SphereVertexShader");

        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        _camera.Update(gameTime);

        Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        Game.Background = Color.CornflowerBlue;

        // Draw using wireframe and store old rasterizer state
        var oldRasterizerState = GraphicsDevice.RasterizerState;
        var rasterizerState = new RasterizerState();
        rasterizerState.CullMode = CullMode.None;
        rasterizerState.FillMode = FillMode.WireFrame;
        GraphicsDevice.RasterizerState = rasterizerState;
        
        var time = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);

        _effect.Parameters["Time"]?.SetValue(time);
        _effect.Parameters["World"].SetValue(Matrix.Identity);
        _effect.Parameters["View"].SetValue(_camera.View);
        _effect.Parameters["Projection"].SetValue(_camera.Projection);

        _sphere.Draw(_effect);
        
        // Restore old rasterizer state
        GraphicsDevice.RasterizerState = oldRasterizerState;
        
        base.Draw(gameTime);
    }
}
