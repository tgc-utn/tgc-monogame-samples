using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Samples.PBR
{
    public struct Light
    {
        public Vector3 Position;
        public Vector3 Color;

        public void SetLight(int index, Effect effect)
        {
            effect.Parameters["lights[" + index + "].Position"].SetValue(new[] {Position.X, Position.Y, Position.Z});
            effect.Parameters["lights[" + index + "].Color"].SetValue(new[] {Color.X, Color.Y, Color.Z});
        }
    }
}