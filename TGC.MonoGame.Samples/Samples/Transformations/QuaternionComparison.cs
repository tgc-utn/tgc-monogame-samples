using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Viewer.Gizmos;
using TGC.MonoGame.Samples.Viewer.GUI.Modifiers;

namespace TGC.MonoGame.Samples.Samples.Transformations
{
    /// <summary>
    ///     Quaterion Comparison.
    ///     Units Involved:
    ///     # Unit 2 - 3D Basics - Transformations
    ///     Shows the problem related to using Euler rotations vs. using Quaternions.
    ///     Authors: Ronan Vinitzca.
    /// </summary>
    public class QuaternionComparison : TGCSample
    {
        /// <inheritdoc />
        public QuaternionComparison(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Transformations;
            Name = "Quaternion Comparison";
            Description =
                "Shows the problem related to using Euler rotations vs. using Quaternions.";
        }

        private Camera Camera { get; set; }
        private Matrix EulerRotation { get; set; }
        private Matrix QuaternionRotation { get; set; }
        private Model Model { get; set; }

        private Vector3 FirstTankPosition { get; set; }
        private Vector3 SecondTankPosition { get; set; }

        private Vector2 FirstTankBannerScreenPosition { get; set; }
        private Vector2 SecondTankBannerScreenPosition { get; set; }

        private Matrix FirstTankTranslationMatrix { get; set; }
        private Matrix SecondTankTranslationMatrix { get; set; }

        private Matrix BaseScale { get; set; }

        private SpriteFont SpriteFont { get; set; }

        private float Pitch { get; set; }
        private float Roll { get; set; }
        private float Yaw { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.Black;

            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, 
                new Vector3(0, 30, 30), Vector3.Zero, 1,
                3000);

            EulerRotation = Matrix.Identity;
            QuaternionRotation = Matrix.Identity;

            Pitch = 0f;
            Roll = 0f;
            Yaw = 0f;

            BaseScale = Matrix.CreateScale(0.1f);

            FirstTankPosition = Vector3.Left * 15f;
            SecondTankPosition = Vector3.Right * 15f;

            FirstTankTranslationMatrix = Matrix.CreateTranslation(FirstTankPosition);
            SecondTankTranslationMatrix = Matrix.CreateTranslation(SecondTankPosition);

            var twoPI = MathF.PI * 2f;

            ModifierController.AddFloat("X Axis (Pitch)", OnPitchChange, 0f, 0f, twoPI);
            ModifierController.AddFloat("Y Axis (Yaw)", OnYawChange, 0f, 0f, twoPI);
            ModifierController.AddFloat("Z Axis (Roll)", OnRollChange, 0f, 0f, twoPI);

            /*Modifiers = new[]
            {
                new FloatModifier("X Axis (Pitch)", OnPitchChange, 0f, 0f, twoPI),
                new FloatModifier("Y Axis (Yaw)", OnYawChange, 0f, 0f, twoPI),
                new FloatModifier("Z Axis (Roll)", OnRollChange, 0f, 0f, twoPI),
            };*/

            var firstTankBannerWorldPosition = FirstTankPosition + Vector3.Up * 10f;
            var secondTankBannerWorldPosition = SecondTankPosition + Vector3.Up * 10f;
            
            FirstTankBannerScreenPosition = ToVector2(GraphicsDevice.Viewport.Project(
                firstTankBannerWorldPosition, Camera.Projection, Camera.View, Matrix.Identity));

            SecondTankBannerScreenPosition = ToVector2(GraphicsDevice.Viewport.Project(
                secondTankBannerWorldPosition, Camera.Projection, Camera.View, Matrix.Identity));



            base.Initialize();
        }

        private Vector2 ToVector2(Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load the chair model
            Model = Game.Content.Load<Model>(ContentFolder3D + "chair/chair");

            // Set the depth state to default
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            // Load the Sprite Font to draw text
            SpriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");

            base.LoadContent();
        }
        
        /// <summary>
        /// Processes a change in the Pitch
        /// </summary>
        /// <param name="newPitch">The new value of the Pitch in radians</param>
        private void OnPitchChange(float newPitch)
        {
            Pitch = newPitch;
            UpdateMatrices();
        }

        /// <summary>
        /// Processes a change in the Yaw
        /// </summary>
        /// <param name="newYaw">The new value of the Yaw in radians</param>
        private void OnYawChange(float newYaw)
        {
            Yaw = newYaw;
            UpdateMatrices();
        }

        /// <summary>
        /// Processes a change in the Roll
        /// </summary>
        /// <param name="newRoll">The new value of the Roll in radians</param>
        private void OnRollChange(float newRoll)
        {
            Roll = newRoll;
            UpdateMatrices();
        }

        /// <summary>
        /// Update the matrices for each type of chair
        /// </summary>
        private void UpdateMatrices()
        {

            // Update matrices, setting the Euler version with yaw pitch roll
            // and Quaternion with Roll/Yaw/Pitch accordingly
            EulerRotation = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll);

            QuaternionRotation = Matrix.CreateFromQuaternion(                        
                         Quaternion.CreateFromAxisAngle(Vector3.Backward, Roll) *
                         Quaternion.CreateFromAxisAngle(Vector3.Up, Yaw) *
                         Quaternion.CreateFromAxisAngle(Vector3.Right, Pitch));
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update Camera and Gizmos
            Camera.Update(gameTime);

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            // Draw the models
            // Note that they contain their own world transforms before applying the passed
            Model.Draw(BaseScale * EulerRotation * FirstTankTranslationMatrix,
                Camera.View, Camera.Projection);
            Model.Draw(BaseScale * QuaternionRotation * SecondTankTranslationMatrix,
                Camera.View, Camera.Projection);

            // Draw Right vectors
            Game.Gizmos.DrawLine(FirstTankPosition, FirstTankPosition + Vector3.Right * 5f, Color.Red);
            Game.Gizmos.DrawLine(SecondTankPosition, SecondTankPosition + Vector3.Right * 5f, Color.Red);

            // Draw Up vectors
            Game.Gizmos.DrawLine(FirstTankPosition, FirstTankPosition + Vector3.Up * 5f, Color.Green);
            Game.Gizmos.DrawLine(SecondTankPosition, SecondTankPosition + Vector3.Up * 5f, Color.Green);

            // Draw Left vectors
            Game.Gizmos.DrawLine(FirstTankPosition, FirstTankPosition + Vector3.Backward * 5f, Color.Blue);
            Game.Gizmos.DrawLine(SecondTankPosition, SecondTankPosition + Vector3.Backward * 5f, Color.Blue);

            // Draw labels
            Game.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone);
            
            Game.SpriteBatch.DrawString(SpriteFont, "Euler", FirstTankBannerScreenPosition, Color.White);
            Game.SpriteBatch.DrawString(SpriteFont, "Quaternion", SecondTankBannerScreenPosition, Color.White);

            Game.SpriteBatch.End();


            base.Draw(gameTime);
        }
    }
}