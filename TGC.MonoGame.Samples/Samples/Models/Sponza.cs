using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Models;

namespace TGC.MonoGame.Samples.Samples.Models;

/// <summary>
/// A sample showing how to draw the sponza cathedral.
/// </summary>
public class Sponza : TGCSample
{
    /// <summary>
    /// A camera to draw geometry
    /// </summary>
    private Camera _camera;

    /// <summary>
    /// The model to draw using both quaternions and euler rotations
    /// </summary>
    private Model _model;

    /// <summary>
    /// An effect to draw the meshes.
    /// </summary>
    private Effect _effect;

    /// <summary>
    /// Model info that facilitates rendering.
    /// </summary>
    private ModelInfo _info; 
    
    public Sponza(TGCViewer game) : base(game)
    {
        Category = TGCSampleCategory.Models;
        Name = "Sponza Model";
        Description =
            "Shows how to draw a complex model using the new ModelExtensions interface";
    }

    public override void Initialize()
    {
        Game.Background = Color.Black;

        var size = GraphicsDevice.Viewport.Bounds.Size;
        size.X /= 2;
        size.Y /= 2;
        _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 5, 0), size);
        _camera.BuildProjection(
            GraphicsDevice.Viewport.AspectRatio, 0.1f, 100000f,
            MathF.PI / 3f);

        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        // Load the chair model
        _model = Game.Content.Load<Model>(ContentFolder3D + "sponza/Sponza");

        _effect = Game.Content.Load<Effect>(ContentFolderEffects + "BasicTexture");

        // This copies the Vertex/Index Buffers into new Geometries.
        // They must be disposed later
        _info = ModelExtensions.GetCenteredXZ(_model);
        
        // Set the depth state to default
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        
        base.LoadContent();
    }
    
    public override void Update(GameTime gameTime)
    {
        // Update Camera and Gizmos
        _camera.Update(gameTime);

        Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        Game.Background = Color.CornflowerBlue;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.BlendState = BlendState.Opaque;
        
        var scaleMatrix = Matrix.CreateScale(0.01f);
        _effect.Parameters["View"].SetValue(_camera.View);
        _effect.Parameters["Projection"].SetValue(_camera.Projection);
        
        for (int index = 0; index < _info.GeometryData.Length; index++)
        {
            ref var geometryData = ref _info.GeometryData[index];

            if (geometryData.Textures.Length > 0)
            {
                _effect.Parameters["ModelTexture"].SetValue(geometryData.Textures[0]);
            }
            
            _effect.Parameters["World"].SetValue(geometryData.RelativeMatrix * scaleMatrix);
            
            geometryData.Geometry.Draw(_effect);
        }
        
        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        // Need to dispose the model info
        _info.Dispose();
    }
}
    
