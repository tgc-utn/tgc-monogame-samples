﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Models;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Tutorials
{
    /// <summary>
    ///     Tutorial 6:
    ///     Units Involved:
    ///     # Unit 5 - Animation
    ///     Sample showing how to apply simple animation to a rigid body tank model.
    ///     Author: Leandro Barbagallo, Matías Leone.
    /// </summary>
    public class Tutorial6 : TGCSample
    {
        public Tutorial6(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Tutorials;
            Name = "Tutorial 6";
            Description = "This sample shows how to apply simple program controlled rigid body animation to a model.";
        }

        private Camera Camera { get; set; }
        private Tank TankModel { get; set; }
        private Matrix TankWorld { get; set; }
        private Model TgcitoModel { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(1000, 600, 0),
                Vector3.UnitY * 150);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load the tank model from the ContentManager.
            TankModel = new Tank();
            var model = Game.Content.Load<Model>(ContentFolder3D + "tank/tank");
            TankModel.Load(model);

            TankWorld = Matrix.Identity;

            // TODO tgcito animation import from de tgcito model.
            TgcitoModel = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            var time = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);

            // Update the animation properties on the tank object. In a real game you would probably take this data from user inputs
            // or the physics system, rather than just making everything rotate like this!
            TankModel.WheelRotation = time * 5;
            TankModel.SteerRotation = (float) Math.Sin(time * 0.75f) * 0.5f;
            TankModel.TurretRotation = (float) Math.Sin(time * 0.333f) * 1.25f;
            TankModel.CannonRotation = (float) Math.Sin(time * 0.25f) * 0.333f - 0.333f;
            TankModel.HatchRotation = MathHelper.Clamp((float) Math.Sin(time * 2) * 2, -1, 0);


            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            

            // Calculate the camera matrices.
            var time = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);

            // Draw the tank model.
            TankModel.Draw(TankWorld * Matrix.CreateRotationY(time * 0.1f), Camera.View, Camera.Projection);

            base.Draw(gameTime);
        }
    }
}