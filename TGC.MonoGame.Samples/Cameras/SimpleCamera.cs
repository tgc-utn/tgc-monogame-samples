﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.Samples.Cameras
{
    /// <summary>
    /// Camera with simple movement.
    /// </summary>
    public class SimpleCamera : Camera
    {
        /// <summary>
        /// Forward direction of the camera.
        /// </summary>
        public readonly Vector3 DefaultWorldFrontVector = Vector3.Forward;

        /// <summary>
        /// The direction that is "up" from the camera's point of view.
        /// </summary>
        public readonly Vector3 DefaultWorldUpVector = Vector3.Up;

        /// <summary>
        /// Value with which the camera is going to move.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Camera with simple movement to be able to move in the 3D world, which has the up vector in (0,1,0) and the forward vector in (0,0,-1).
        /// </summary>
        /// <param name="aspectRatio">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="position">The position of the camera.</param>
        /// <param name="speed">The speed of movement.</param>
        public SimpleCamera(float aspectRatio, Vector3 position, float speed) : base(aspectRatio)
        {
            Position = position;
            FrontDirection = DefaultWorldFrontVector;
            Speed = speed;
            View = Matrix.CreateLookAt(Position, Position + FrontDirection, DefaultWorldUpVector);
        }

        ///<inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            var time = Convert.ToSingle(gameTime.ElapsedGameTime.TotalMilliseconds);

            // Check for input to rotate the camera.
            var pitch = 0f;
            var turn = 0f;

            if (keyboardState.IsKeyDown(Keys.Up))
                pitch += time * 0.001f;

            if (keyboardState.IsKeyDown(Keys.Down))
                pitch -= time * 0.001f;

            if (keyboardState.IsKeyDown(Keys.Left))
                turn += time * 0.001f;

            if (keyboardState.IsKeyDown(Keys.Right))
                turn -= time * 0.001f;

            RightDirection = Vector3.Cross(DefaultWorldUpVector, FrontDirection);
            var flatFront = Vector3.Cross(RightDirection, DefaultWorldUpVector);

            var pitchMatrix = Matrix.CreateFromAxisAngle(RightDirection, pitch);
            var turnMatrix = Matrix.CreateFromAxisAngle(DefaultWorldUpVector, turn);

            var tiltedFront = Vector3.TransformNormal(FrontDirection, pitchMatrix * turnMatrix);

            // Check angle so we can't flip over.
            if (Vector3.Dot(tiltedFront, flatFront) > 0.001f) FrontDirection = Vector3.Normalize(tiltedFront);

            // Check for input to move the camera around.
            if (keyboardState.IsKeyDown(Keys.W))
                Position += FrontDirection * time * Speed;

            if (keyboardState.IsKeyDown(Keys.S))
                Position -= FrontDirection * time * Speed;

            if (keyboardState.IsKeyDown(Keys.A))
                Position += RightDirection * time * Speed;

            if (keyboardState.IsKeyDown(Keys.D))
                Position -= RightDirection * time * Speed;

            View = Matrix.CreateLookAt(Position, Position + FrontDirection, DefaultWorldUpVector);
        }
    }
}
