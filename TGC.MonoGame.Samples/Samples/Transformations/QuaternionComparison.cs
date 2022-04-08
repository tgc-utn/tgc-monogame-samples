using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;
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
        /// <summary>
        /// A camera to draw geometry
        /// </summary>
        private Camera Camera { get; set; }

        /// <summary>
        /// The euler rotation matrix for the model
        /// </summary>
        private Matrix EulerRotation { get; set; }

        /// <summary>
        /// The resulting quaternion rotation matrix for the model
        /// </summary>
        private Matrix QuaternionRotation { get; set; }

        /// <summary>
        /// The model to draw using both quaternions and euler rotations
        /// </summary>
        private Model Model { get; set; }

        /// <summary>
        /// The first position in world space of the model
        /// </summary>
        private Vector3 FirstPosition { get; set; }

        /// <summary>
        /// The second position in world space of the model
        /// </summary>
        private Vector3 SecondPosition { get; set; }

        /// <summary>
        /// The position in screen space of the first banner
        /// </summary>
        private Vector2 FirstBannerScreenPosition { get; set; }

        /// <summary>
        /// The position in screen space of the second banner
        /// </summary>
        private Vector2 SecondTankBannerScreenPosition { get; set; }

        /// <summary>
        /// The first translation matrix that indicates where to place the model when drawing
        /// </summary>
        private Matrix FirstTranslationMatrix { get; set; }

        /// <summary>
        /// The second translation matrix that indicates where to place the model when drawing
        /// </summary>
        private Matrix SecondTranslationMatrix { get; set; }

        /// <summary>
        /// The base scale of the model
        /// </summary>
        private Matrix BaseScale { get; set; }

        /// <summary>
        /// A font to draw text into the screen
        /// </summary>
        private SpriteFont SpriteFont { get; set; }

        /// <summary>
        /// The rotation for the X axis in degrees
        /// </summary>
        private float Pitch { get; set; }

        /// <summary>
        /// The rotation for the Y axis in degrees
        /// </summary>
        private float Yaw { get; set; }

        /// <summary>
        /// The rotation for the Z axis in degrees
        /// </summary>
        private float Roll { get; set; }


        /// <inheritdoc />
        public QuaternionComparison(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Transformations;
            Name = "Quaternion Comparison";
            Description =
                "Shows the problem related to using Euler rotations vs. using Quaternions.";
        }


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

            FirstPosition = Vector3.Left * 15f;
            SecondPosition = Vector3.Right * 15f;

            FirstTranslationMatrix = Matrix.CreateTranslation(FirstPosition);
            SecondTranslationMatrix = Matrix.CreateTranslation(SecondPosition);

            var twoPI = 360f;

            ModifierController.AddFloat("X Axis (Pitch)", OnPitchChange, 0f, 0f, twoPI);
            ModifierController.AddFloat("Y Axis (Yaw)", OnYawChange, 0f, 0f, twoPI);
            ModifierController.AddFloat("Z Axis (Roll)", OnRollChange, 0f, 0f, twoPI);

            var firstTankBannerWorldPosition = FirstPosition + Vector3.Up * 10f;
            var secondTankBannerWorldPosition = SecondPosition + Vector3.Up * 10f;
            
            FirstBannerScreenPosition = ToVector2(GraphicsDevice.Viewport.Project(
                firstTankBannerWorldPosition, Camera.Projection, Camera.View, Matrix.Identity));

            SecondTankBannerScreenPosition = ToVector2(GraphicsDevice.Viewport.Project(
                secondTankBannerWorldPosition, Camera.Projection, Camera.View, Matrix.Identity));

            base.Initialize();
        }

        /// <summary>
        /// Returns a <see cref="Vector2"/> containing the XY components of a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3"/> to obtain its XY components</param>
        /// <returns>A <see cref="Vector2"/> containing the XY components of the given vector</returns>
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
        /// Converts an angle in degrees to radians.
        /// </summary>
        /// <param name="angleInDegrees">The angle to convert to degrees.</param>
        /// <returns>The converted angle in radians</returns>
        float ToRadians(float angleInDegrees)
        {
            return angleInDegrees * MathF.PI / 180f;
        }

        /// <summary>
        /// Update the matrices for each type of chair
        /// </summary>
        private void UpdateMatrices()
        {
            var yawRadians = ToRadians(Yaw);
            var pitchRadians = ToRadians(Pitch);
            var rollRadians = ToRadians(Roll);

            // Update matrices, setting the Euler version with yaw pitch roll
            // and Quaternion with Roll/Yaw/Pitch accordingly
            EulerRotation = Matrix.CreateFromYawPitchRoll(yawRadians, pitchRadians, rollRadians);

            QuaternionRotation = Matrix.CreateFromQuaternion(
                         Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rollRadians) *
                         Quaternion.CreateFromAxisAngle(Vector3.UnitY, yawRadians) *
                         Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitchRadians));
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
            Model.Draw(BaseScale * EulerRotation * FirstTranslationMatrix,
                Camera.View, Camera.Projection);
            Model.Draw(BaseScale * QuaternionRotation * SecondTranslationMatrix,
                Camera.View, Camera.Projection);

            // Draw Right vectors
            Game.Gizmos.DrawLine(FirstPosition, FirstPosition + Vector3.Right * 5f, Color.Red);
            Game.Gizmos.DrawLine(SecondPosition, SecondPosition + Vector3.Right * 5f, Color.Red);

            // Draw Up vectors
            Game.Gizmos.DrawLine(FirstPosition, FirstPosition + Vector3.Up * 5f, Color.Green);
            Game.Gizmos.DrawLine(SecondPosition, SecondPosition + Vector3.Up * 5f, Color.Green);

            // Draw Left vectors
            Game.Gizmos.DrawLine(FirstPosition, FirstPosition + Vector3.Backward * 5f, Color.Blue);
            Game.Gizmos.DrawLine(SecondPosition, SecondPosition + Vector3.Backward * 5f, Color.Blue);

            // Draw labels
            Game.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone);
            
            Game.SpriteBatch.DrawString(SpriteFont, "Euler", FirstBannerScreenPosition, Color.White);
            Game.SpriteBatch.DrawString(SpriteFont, "Quaternion", SecondTankBannerScreenPosition, Color.White);

            Game.SpriteBatch.End();


            base.Draw(gameTime);
        }
    }
}