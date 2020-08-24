using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Cameras
{
    public class UnrestrictedCamera : Camera
    {
        public UnrestrictedCamera(float aspectRatio) : base(aspectRatio)
        {

        }

        public void Update()
        {
            View = Matrix.CreateLookAt(Position, Position + FrontDirection, UpDirection);
        }


        public override void Update(GameTime gameTime)
        {
            //
        }
    }
}
