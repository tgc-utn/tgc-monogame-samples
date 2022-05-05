using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.Samples.Viewer.Gizmos.Geometries
{
    /// <summary>
    ///     Gizmo for drawing the Axis Lines of an application.
    /// </summary>
    class AxisLines
    {
        private const float AxisScreenOffset = 20f;
        private const float AxisScreenDistance = 40f;

        private GraphicsDevice GraphicsDevice;

        private Model Model;

        private BasicEffect Effect;

        private Matrix BaseScaleTranslation { get; set; }

        /// <summary>
        ///     Constructs an AxisLines drawable object.
        /// </summary>
        /// <param name="device">The GraphicsDevice to bind the resources.</param>
        /// <param name="model">The Model of the AxisLines, loaded from content.</param>
        public AxisLines(GraphicsDevice device, Model model)
        {
            GraphicsDevice = device;
            Model = model;

            foreach (var mesh in Model.Meshes)
                foreach (var part in mesh.MeshParts)
                    Effect = (BasicEffect)part.Effect;

            Effect.Projection = Matrix.Identity;
            Effect.View = Matrix.Identity;
            Effect.World = Matrix.Identity;

            BaseScaleTranslation = 
                // Scale the arrows
                Matrix.CreateScale(0.02f) *
                // Translate them to the bottom left of the screen, and add a Z value to prevent clipping
                Matrix.CreateTranslation(0.87f, -0.9f, 0.2f);

            Effect.EnableDefaultLighting();
        }

        /// <summary>
        ///     Sets the view matrix needed to draw the Axis Lines in the screen.
        /// </summary>
        /// <param name="view">The view matrix, generally from a camera.</param>
        public void SetView(Matrix view)
        {
            view.Translation = Vector3.Zero;
            Effect.World =
                // Use the View matrix, with no translation, to make the arrows face where the camera is pointing at
                // Then multiply by the base Scale and Translation
                view * BaseScaleTranslation;            
        }
        

        /// <summary>
        ///     Draws the AxisLines
        /// </summary>
        public virtual void Draw()
        {
            foreach (var mesh in Model.Meshes)
                mesh.Draw();
        }
    }
}
