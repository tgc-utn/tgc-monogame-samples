using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Transformations
{
    public class QuaternionTrackball : TGCSample
    {
        /// <inheritdoc />
        public QuaternionTrackball(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Transformations;
            Name = "Quaternion Trackball";
            Description =
                "Shows how the Cross Product and the Dot Product can be used to rotate a mesh with Quaternions.";
        }

        private Camera Camera { get; set; }
        private Quaternion Quaternion { get; set; }
        private Model Model { get; set; }
        private Matrix BaseScale { get; set; }
        private BoundingSphere BoundingSphere { get; set; }
        private Ray CameraRay;

        private bool Casted { get; set; }

        private Vector3 CastedPoint;

        private Vector3 ClosingPoint;

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.Black;

            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio,
                new Vector3(5, 5, 5), Vector3.Zero, 1,
                3000);

            Quaternion = Quaternion.Identity;

            BaseScale = Matrix.CreateScale(0.1f);

            BoundingSphere = new BoundingSphere(Vector3.Zero, 2f);
            
            CameraRay = new Ray(Camera.Position, Camera.FrontDirection);

            Casted = false;

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load the chair model
            Model = Game.Content.Load<Model>(ContentFolder3D + "chair/chair");

            // Set the depth state to default
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.LoadContent();
        }

        private Vector3 Result;

        private Vector3 Temp;

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update Camera and Gizmos
            Camera.Update(gameTime);

            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton.Equals(ButtonState.Pressed))
            {
                if(!Casted)
                {
                    if (CastRay(mouseState, out CastedPoint))
                        Casted = true;
                }
                else
                    CastRay(mouseState, out ClosingPoint);
            }
            else if (mouseState.LeftButton.Equals(ButtonState.Released))
            {
                Quaternion *= Quaternion.CreateFromAxisAngle(
                    Vector3.Cross(Vector3.Normalize(CastedPoint), Vector3.Normalize(ClosingPoint)),
                    Vector3.Dot(Vector3.Normalize(CastedPoint), Vector3.Normalize(ClosingPoint)));
                Casted = false;
            }
            else
                Casted = false;

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            //Game.Gizmos.DrawSphere(Vector3.Zero, Vector3.One * 2f, Color.Red);

            base.Update(gameTime);
        }

        private bool CastRay(MouseState mouseState, out Vector3 resultPosition)
        {
            var mousePos = mouseState.Position;

            var ray = GetScreenVector2AsRayInto3dWorld(new Vector2(mousePos.X, mousePos.Y), Camera.Projection, Camera.View, Camera.View, 1f, GraphicsDevice);

            var result = ray.Intersects(BoundingSphere);

            if (result.HasValue)
                resultPosition = Camera.Position + ray.Direction * result.Value;
            else
                resultPosition = Vector3.Zero;

            return result.HasValue;
        }

        /// <summary>
        /// Near plane is typically just 0 in this function or some extremely small value. Far plane cant be more then one and so i just folded it automatically. In truth this isnt the near plane its the min clip but whatever.
        /// </summary>
        public Ray GetScreenVector2AsRayInto3dWorld(Vector2 screenPosition, Matrix projectionMatrix, Matrix viewMatrix, Matrix cameraWorld, float near, GraphicsDevice device)
        {
            //if (far > 1.0f) // this is actually a misnomer which caused me a headache this is supposed to be the max clip value not the far plane.
            //    throw new ArgumentException("Far Plane can't be more then 1f or this function will fail to work in many cases");
            Vector3 nearScreenPoint = new Vector3(screenPosition.X, screenPosition.Y, near); // must be more then zero.
            Vector3 nearWorldPoint = Unproject(nearScreenPoint, projectionMatrix, viewMatrix, Matrix.Identity, device);

            Vector3 farScreenPoint = new Vector3(screenPosition.X, screenPosition.Y, 1f); // the projection matrice's far plane value.
            Vector3 farWorldPoint = Unproject(farScreenPoint, projectionMatrix, viewMatrix, Matrix.Identity, device);

            Vector3 worldRaysNormal = Vector3.Normalize((farWorldPoint + nearWorldPoint) - nearWorldPoint);
            return new Ray(Camera.Position, worldRaysNormal);
        }

        /// <summary>
        /// Note the source position internally expects a Vector3 with a z value.
        /// That Z value can Not excced 1.0f or the function will error. I leave it as is for future advanced depth selection functionality which should be apparent.
        /// </summary>
        public Vector3 Unproject(Vector3 position, Matrix projection, Matrix view, Matrix world, GraphicsDevice gd)
        {
            if (position.Z > gd.Viewport.MaxDepth)
                throw new Exception("Source Z must be less than MaxDepth ");
            Matrix wvp = Matrix.Multiply(view, projection);
            Matrix inv = Matrix.Invert(wvp);
            Vector3 clipSpace = position;
            clipSpace.X = (((position.X - gd.Viewport.X) / ((float)gd.Viewport.Width)) * 2f) - 1f;
            clipSpace.Y = -((((position.Y - gd.Viewport.Y) / ((float)gd.Viewport.Height)) * 2f) - 1f);
            clipSpace.Z = (position.Z - gd.Viewport.MinDepth) / (gd.Viewport.MaxDepth - gd.Viewport.MinDepth); // >> Oo <<
            Vector3 invsrc = Vector3.Transform(clipSpace, inv);
            float a = (((clipSpace.X * inv.M14) + (clipSpace.Y * inv.M24)) + (clipSpace.Z * inv.M34)) + inv.M44;
            return invsrc / a;
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Model.Draw( Matrix.CreateScale(0.1f) *  Matrix.CreateFromQuaternion(Quaternion), Camera.View, Camera.Projection);

            Game.Gizmos.DrawLine(Vector3.Zero, CastedPoint, Color.Blue);
            Game.Gizmos.DrawLine(Vector3.Zero, ClosingPoint, Color.Green);
            Game.Gizmos.DrawLine(Vector3.Zero, Vector3.Cross(Vector3.Normalize(CastedPoint), Vector3.Normalize(ClosingPoint)), Color.Yellow);

            Game.Gizmos.DrawSphere(Vector3.Zero, Vector3.One * 2f, Color.Red);

            base.Draw(gameTime);
        }
    }
}
