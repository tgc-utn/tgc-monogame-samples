using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.Samples.Cameras
{
    internal class FreeCamera : Camera
    {
        private readonly bool _lockMouse;

        private readonly Point _screenCenter;
        private bool _changed;

        private Vector2 _pastMousePosition;
        private float _pitch;

        // Angles
        private float _yaw = -90f;

        public FreeCamera(float aspectRatio, Vector3 position, Point screenCenter) : this(aspectRatio, position)
        {
            _lockMouse = true;
            this._screenCenter = screenCenter;
        }

        public FreeCamera(float aspectRatio, Vector3 position) : base(aspectRatio)
        {
            Position = position;
            _pastMousePosition = Mouse.GetState().Position.ToVector2();
            UpdateCameraVectors();
            CalculateView();
        }

        public float MovementSpeed { get; set; } = 100f;
        public float MouseSensitivity { get; set; } = 5f;

        private void CalculateView()
        {
            View = Matrix.CreateLookAt(Position, Position + FrontDirection, UpDirection);
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            var elapsedTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            _changed = false;
            ProcessKeyboard(elapsedTime);
            ProcessMouseMovement(elapsedTime);

            if (_changed)
                CalculateView();
        }

        private void ProcessKeyboard(float elapsedTime)
        {
            var keyboardState = Keyboard.GetState();

            var currentMovementSpeed = MovementSpeed;
            if (keyboardState.IsKeyDown(Keys.LeftShift))
                currentMovementSpeed *= 5f;

            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
            {
                Position += -RightDirection * currentMovementSpeed * elapsedTime;
                _changed = true;
            }

            if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
            {
                Position += RightDirection * currentMovementSpeed * elapsedTime;
                _changed = true;
            }

            if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
            {
                Position += FrontDirection * currentMovementSpeed * elapsedTime;
                _changed = true;
            }

            if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
            {
                Position += -FrontDirection * currentMovementSpeed * elapsedTime;
                _changed = true;
            }

            if (keyboardState.IsKeyDown(Keys.Space))
            {
                Position += Vector3.Up * currentMovementSpeed * elapsedTime;
                _changed = true;
            }

            if (keyboardState.IsKeyDown(Keys.LeftControl))
            {
                Position -= Vector3.Up * currentMovementSpeed * elapsedTime;
                _changed = true;
            }

        }

        private void ProcessMouseMovement(float elapsedTime)
        {
            var mouseState = Mouse.GetState();

            if (mouseState.RightButton.Equals(ButtonState.Pressed))
            {
                var mouseDelta = mouseState.Position.ToVector2() - _pastMousePosition;
                mouseDelta *= MouseSensitivity * elapsedTime;

                _yaw -= mouseDelta.X;
                _pitch += mouseDelta.Y;

                if (_pitch > 89.0f)
                    _pitch = 89.0f;
                if (_pitch < -89.0f)
                    _pitch = -89.0f;

                _changed = true;
                UpdateCameraVectors();

                if (_lockMouse)
                {
                    Mouse.SetPosition(_screenCenter.X, _screenCenter.Y);
                    Mouse.SetCursor(MouseCursor.Crosshair);
                }
                else
                {
                    Mouse.SetCursor(MouseCursor.Arrow);
                }
            }

            _pastMousePosition = Mouse.GetState().Position.ToVector2();
        }

        private void UpdateCameraVectors()
        {
            // Calculate the new Front vector
            Vector3 tempFront;
            tempFront.X = MathF.Cos(MathHelper.ToRadians(_yaw)) * MathF.Cos(MathHelper.ToRadians(_pitch));
            tempFront.Y = MathF.Sin(MathHelper.ToRadians(_pitch));
            tempFront.Z = MathF.Sin(MathHelper.ToRadians(_yaw)) * MathF.Cos(MathHelper.ToRadians(_pitch));

            FrontDirection = Vector3.Normalize(tempFront);

            // Also re-calculate the Right and Up vector
            // Normalize the vectors, because their length gets closer to 0 the more you look up or down which results in slower movement.
            RightDirection = Vector3.Normalize(Vector3.Cross(FrontDirection, Vector3.Up));
            UpDirection = Vector3.Normalize(Vector3.Cross(RightDirection, FrontDirection));
        }
    }
}