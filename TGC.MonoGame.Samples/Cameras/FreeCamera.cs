using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace TGC.MonoGame.Samples.Cameras
{
    class FreeCamera : Camera
    {
        // Angles
        private float yaw = -90f;
        private float pitch = 0f;

        private Vector2 pastMousePosition;

        public float MovementSpeed { get; set; } = 100f;
        public float MouseSensitivity { get; set; } = 5f;

        private bool changed = false, lockMouse = false;

        private Point screenCenter;

        public FreeCamera(Vector3 position)
        {
            Position = position;
            pastMousePosition = Mouse.GetState().Position.ToVector2();
            UpdateCameraVectors();
            CalculateView();
        }

        public FreeCamera(Vector3 position, Point screenCenter) : this(position)
        {
            lockMouse = true;
            this.screenCenter = screenCenter;
        }


        private void CalculateView()
        {
            ViewMatrix = Matrix.CreateLookAt(Position, Position + FrontDirection, UpDirection);
        }

        public void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            changed = false;
            ProcessKeyboard(elapsedTime);
            ProcessMouseMovement(elapsedTime);

            if (changed)
                CalculateView();
        }

        void ProcessKeyboard(float elapsedTime)
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
            {
                Position += -RightDirection * MovementSpeed * elapsedTime;
                changed = true;
            }

            if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
            {
                Position += RightDirection * MovementSpeed * elapsedTime;
                changed = true;
            }

            if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
            {
                Position += FrontDirection * MovementSpeed * elapsedTime;
                changed = true;
            }

            if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
            {
                Position += -FrontDirection * MovementSpeed * elapsedTime;
                changed = true;
            }

        }

        void ProcessMouseMovement(float elapsedTime)
        {
            var mouseState = Mouse.GetState();

            if(mouseState.RightButton.Equals(ButtonState.Pressed))
            {
                Vector2 mouseDelta = mouseState.Position.ToVector2() - pastMousePosition;
                mouseDelta *= MouseSensitivity * elapsedTime;

                yaw -= mouseDelta.X;
                pitch += mouseDelta.Y;

                if (pitch > 89.0f)
                    pitch = 89.0f;
                if (pitch < -89.0f)
                    pitch = -89.0f;

                changed = true;
                UpdateCameraVectors();

                if (lockMouse)
                {
                    Mouse.SetPosition(screenCenter.X, screenCenter.Y);
                    Mouse.SetCursor(MouseCursor.Crosshair);
                }
            }
            pastMousePosition = Mouse.GetState().Position.ToVector2();
        }

        private void UpdateCameraVectors()
        {
            // Calculate the new Front vector
            Vector3 tempFront;
            tempFront.X = MathF.Cos(MathHelper.ToRadians(yaw)) * MathF.Cos(MathHelper.ToRadians(pitch));
            tempFront.Y = MathF.Sin(MathHelper.ToRadians(pitch));
            tempFront.Z = MathF.Sin(MathHelper.ToRadians(yaw)) * MathF.Cos(MathHelper.ToRadians(pitch));

            FrontDirection = Vector3.Normalize(tempFront);

            // Also re-calculate the Right and Up vector
            // Normalize the vectors, because their length gets closer to 0 the more you look up or down which results in slower movement.
            RightDirection = Vector3.Normalize(Vector3.Cross(FrontDirection, Vector3.Up));  
            UpDirection = Vector3.Normalize(Vector3.Cross(RightDirection, FrontDirection));
        }
    };

}


