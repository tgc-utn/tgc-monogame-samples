using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Viewer.Gizmos.Geometries;

/// <summary>
///     Gizmo for drawing the Axis Lines of an application.
/// </summary>
internal class AxisLines
{
    private const float AxisScreenOffset = 20f;
    private const float AxisScreenDistance = 40f;

    private readonly BasicEffect _effect;
    private readonly Model _model;
    private GraphicsDevice _graphicsDevice;

    /// <summary>
    ///     Constructs an AxisLines drawable object.
    /// </summary>
    /// <param name="device">The GraphicsDevice to bind the resources.</param>
    /// <param name="model">The Model of the AxisLines, loaded from content.</param>
    public AxisLines(GraphicsDevice device, Model model)
    {
        _graphicsDevice = device;
        _model = model;

        foreach (var mesh in _model.Meshes)
        foreach (var part in mesh.MeshParts)
        {
            _effect = (BasicEffect)part.Effect;
        }

        _effect.Projection = Matrix.Identity;
        _effect.View = Matrix.Identity;
        _effect.World = Matrix.Identity;

        BaseScaleTranslation =
            // Scale the arrows
            Matrix.CreateScale(0.02f) *
            // Translate them to the bottom left of the screen, and add a Z value to prevent clipping
            Matrix.CreateTranslation(0.87f, -0.9f, 0.2f);

        _effect.EnableDefaultLighting();
    }


    private Matrix BaseScaleTranslation { get; }

    /// <summary>
    ///     Sets the view matrix needed to draw the Axis Lines in the screen.
    /// </summary>
    /// <param name="view">The view matrix, generally from a camera.</param>
    public void SetView(Matrix view)
    {
        view.Translation = Vector3.Zero;
        _effect.World =
            // Use the View matrix, with no translation, to make the arrows face where the camera is pointing at
            // Then multiply by the base Scale and Translation
            view * BaseScaleTranslation;
    }


    /// <summary>
    ///     Draws the AxisLines
    /// </summary>
    public virtual void Draw()
    {
        foreach (var mesh in _model.Meshes)
        {
            mesh.Draw();
        }
    }
}
