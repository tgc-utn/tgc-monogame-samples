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
        private Camera _camera;

        /// <summary>
        /// The euler rotation matrix for the model
        /// </summary>
        private Matrix _eulerRotation;

        /// <summary>
        /// The resulting quaternion rotation matrix for the model
        /// </summary>
        private Matrix _quaternionRotation;

        /// <summary>
        /// The model to draw using both quaternions and euler rotations
        /// </summary>
        private Model _model;

        /// <summary>
        /// The first position in world space of the model
        /// </summary>
        private Vector3 _firstPosition;

        /// <summary>
        /// The second position in world space of the model
        /// </summary>
        private Vector3 _secondPosition;

        /// <summary>
        /// The position in screen space of the first banner
        /// </summary>
        private Vector2 _firstBannerScreenPosition;

        /// <summary>
        /// The position in screen space of the second banner
        /// </summary>
        private Vector2 _secondTankBannerScreenPosition;

        /// <summary>
        /// The first translation matrix that indicates where to place the model when drawing
        /// </summary>
        private Matrix _firstTranslationMatrix;

        /// <summary>
        /// The second translation matrix that indicates where to place the model when drawing
        /// </summary>
        private Matrix _secondTranslationMatrix;

        /// <summary>
        /// The base scale of the model
        /// </summary>
        private Matrix _baseScale;

        /// <summary>
        /// A font to draw text into the screen
        /// </summary>
        private SpriteFont _spriteFont;

        /// <summary>
        /// The rotation for the X axis in degrees
        /// </summary>
        private float _pitch;

        /// <summary>
        /// The rotation for the Y axis in degrees
        /// </summary>
        private float _yaw;

        /// <summary>
        /// The rotation for the Z axis in degrees
        /// </summary>
        private float _roll;

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

            _camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, 
                new Vector3(0, 30, 30), Vector3.Zero, 1,
                3000);

            _eulerRotation = Matrix.Identity;
            _quaternionRotation = Matrix.Identity;

            _pitch = 0f;
            _roll = 0f;
            _yaw = 0f;

            _baseScale = Matrix.CreateScale(0.1f);

            _firstPosition = Vector3.Left * 15f;
            _secondPosition = Vector3.Right * 15f;

            _firstTranslationMatrix = Matrix.CreateTranslation(_firstPosition);
            _secondTranslationMatrix = Matrix.CreateTranslation(_secondPosition);

            var twoPI = 360f;

            ModifierController.AddFloat("X Axis (Pitch)", OnPitchChange, 0f, 0f, twoPI);
            ModifierController.AddFloat("Y Axis (Yaw)", OnYawChange, 0f, 0f, twoPI);
            ModifierController.AddFloat("Z Axis (Roll)", OnRollChange, 0f, 0f, twoPI);

            var firstTankBannerWorldPosition = _firstPosition + Vector3.Up * 10f;
            var secondTankBannerWorldPosition = _secondPosition + Vector3.Up * 10f;
            
            _firstBannerScreenPosition = ToVector2(GraphicsDevice.Viewport.Project(
                firstTankBannerWorldPosition, _camera.Projection, _camera.View, Matrix.Identity));

            _secondTankBannerScreenPosition = ToVector2(GraphicsDevice.Viewport.Project(
                secondTankBannerWorldPosition, _camera.Projection, _camera.View, Matrix.Identity));

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
            _model = Game.Content.Load<Model>(ContentFolder3D + "chair/chair");

            // Set the depth state to default
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            // Load the Sprite Font to draw text
            _spriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");

            base.LoadContent();
        }
        
        /// <summary>
        /// Processes a change in the Pitch
        /// </summary>
        /// <param name="newPitch">The new value of the Pitch in radians</param>
        private void OnPitchChange(float newPitch)
        {
            _pitch = newPitch;
            UpdateMatrices();
        }

        /// <summary>
        /// Processes a change in the Yaw
        /// </summary>
        /// <param name="newYaw">The new value of the Yaw in radians</param>
        private void OnYawChange(float newYaw)
        {
            _yaw = newYaw;
            UpdateMatrices();
        }

        /// <summary>
        /// Processes a change in the Roll
        /// </summary>
        /// <param name="newRoll">The new value of the Roll in radians</param>
        private void OnRollChange(float newRoll)
        {
            _roll = newRoll;
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
            var yawRadians = ToRadians(_yaw);
            var pitchRadians = ToRadians(_pitch);
            var rollRadians = ToRadians(_roll);

            // Update matrices, setting the Euler version with yaw pitch roll
            // and Quaternion with Roll/Yaw/Pitch accordingly
            _eulerRotation = Matrix.CreateFromYawPitchRoll(yawRadians, pitchRadians, rollRadians);

            _quaternionRotation = Matrix.CreateFromQuaternion(
                         Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rollRadians) *
                         Quaternion.CreateFromAxisAngle(Vector3.UnitY, yawRadians) *
                         Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitchRadians));
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update Camera and Gizmos
            _camera.Update(gameTime);

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            // Draw the models
            // Note that they contain their own world transforms before applying the passed
            _model.Draw(_baseScale * _eulerRotation * _firstTranslationMatrix,
                _camera.View, _camera.Projection);
            _model.Draw(_baseScale * _quaternionRotation * _secondTranslationMatrix,
                _camera.View, _camera.Projection);

            // Draw Right vectors
            Game.Gizmos.DrawLine(_firstPosition, _firstPosition + Vector3.Right * 5f, Color.Red);
            Game.Gizmos.DrawLine(_secondPosition, _secondPosition + Vector3.Right * 5f, Color.Red);

            // Draw Up vectors
            Game.Gizmos.DrawLine(_firstPosition, _firstPosition + Vector3.Up * 5f, Color.Green);
            Game.Gizmos.DrawLine(_secondPosition, _secondPosition + Vector3.Up * 5f, Color.Green);

            // Draw Left vectors
            Game.Gizmos.DrawLine(_firstPosition, _firstPosition + Vector3.Backward * 5f, Color.Blue);
            Game.Gizmos.DrawLine(_secondPosition, _secondPosition + Vector3.Backward * 5f, Color.Blue);

            // Draw labels
            Game.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone);
            
            Game.SpriteBatch.DrawString(_spriteFont, "Euler", _firstBannerScreenPosition, Color.White);
            Game.SpriteBatch.DrawString(_spriteFont, "Quaternion", _secondTankBannerScreenPosition, Color.White);

            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}