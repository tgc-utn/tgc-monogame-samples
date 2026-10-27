using Microsoft.Xna.Framework;

namespace TGC.MonoGame.Samples.Samples.RenderPipeline.DeferredLighting
{
    internal class PointLight
    {
        internal PointLight(Vector3 position, Vector3 diffuseColor, Vector3 specularColor, float radius)
        {
            Position = position;
            DiffuseColor = diffuseColor;
            SpecularColor = specularColor;
            Radius = radius;
        }

        internal Vector3 Position { get; }

        internal Vector3 DiffuseColor { get; }

        internal Vector3 SpecularColor { get; }

        internal float Radius { get; }
    }
}
