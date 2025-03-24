#region File Description

//-----------------------------------------------------------------------------
// Tank.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion File Description

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion Using Statements

namespace TGC.MonoGame.Samples.Models
{
    /// <summary>
    ///     Helper class for drawing a tank model with animated wheels and turret.
    ///     This class was borrowed from the XNA Simple Animation Sample.
    /// </summary>
    public class Tank
    {
        /// <summary>
        ///     Loads the tank model.
        /// </summary>
        public void Load(Model model)
        {
            _tankModel = model;

            // Look up shortcut references to the bones we are going to animate.
            _leftBackWheelBone = _tankModel.Bones["l_back_wheel_geo"];
            _rightBackWheelBone = _tankModel.Bones["r_back_wheel_geo"];
            _leftFrontWheelBone = _tankModel.Bones["l_front_wheel_geo"];
            _rightFrontWheelBone = _tankModel.Bones["r_front_wheel_geo"];
            _leftSteerBone = _tankModel.Bones["l_steer_geo"];
            _rightSteerBone = _tankModel.Bones["r_steer_geo"];
            _turretBone = _tankModel.Bones["turret_geo"];
            _cannonBone = _tankModel.Bones["canon_geo"];
            _hatchBone = _tankModel.Bones["hatch_geo"];

            // Store the original transform matrix for each animating bone.
            _leftBackWheelTransform = _leftBackWheelBone.Transform;
            _rightBackWheelTransform = _rightBackWheelBone.Transform;
            _leftFrontWheelTransform = _leftFrontWheelBone.Transform;
            _rightFrontWheelTransform = _rightFrontWheelBone.Transform;
            _leftSteerTransform = _leftSteerBone.Transform;
            _rightSteerTransform = _rightSteerBone.Transform;
            _turretTransform = _turretBone.Transform;
            _cannonTransform = _cannonBone.Transform;
            _hatchTransform = _hatchBone.Transform;

            // Allocate the transform matrix array.
            _boneTransforms = new Matrix[_tankModel.Bones.Count];
        }

        /// <summary>
        ///     Draws the tank model, using the current animation settings.
        /// </summary>
        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            // Set the world matrix as the root transform of the model.
            _tankModel.Root.Transform = world;

            // Calculate matrices based on the current animation position.
            var wheelRotation = Matrix.CreateRotationX(WheelRotation);
            var steerRotation = Matrix.CreateRotationY(SteerRotation);
            var turretRotation = Matrix.CreateRotationY(TurretRotation);
            var cannonRotation = Matrix.CreateRotationX(CannonRotation);
            var hatchRotation = Matrix.CreateRotationX(HatchRotation);

            // Apply matrices to the relevant bones.
            _leftBackWheelBone.Transform = wheelRotation * _leftBackWheelTransform;
            _rightBackWheelBone.Transform = wheelRotation * _rightBackWheelTransform;
            _leftFrontWheelBone.Transform = wheelRotation * _leftFrontWheelTransform;
            _rightFrontWheelBone.Transform = wheelRotation * _rightFrontWheelTransform;
            _leftSteerBone.Transform = steerRotation * _leftSteerTransform;
            _rightSteerBone.Transform = steerRotation * _rightSteerTransform;
            _turretBone.Transform = turretRotation * _turretTransform;
            _cannonBone.Transform = cannonRotation * _cannonTransform;
            _hatchBone.Transform = hatchRotation * _hatchTransform;

            // Look up combined bone matrices for the entire model.
            _tankModel.CopyAbsoluteBoneTransformsTo(_boneTransforms);

            // Draw the model.
            foreach (var mesh in _tankModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = _boneTransforms[mesh.ParentBone.Index];
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();
                }

                mesh.Draw();
            }
        }

        #region Fields

        // The XNA framework Model object that we are going to display.
        private Model _tankModel;

        // Shortcut references to the bones that we are going to animate.
        // We could just look these up inside the Draw method, but it is more efficient to do the lookups while loading and cache the results.
        private ModelBone _leftBackWheelBone;

        private ModelBone _rightBackWheelBone;
        private ModelBone _leftFrontWheelBone;
        private ModelBone _rightFrontWheelBone;
        private ModelBone _leftSteerBone;
        private ModelBone _rightSteerBone;
        private ModelBone _turretBone;
        private ModelBone _cannonBone;
        private ModelBone _hatchBone;

        // Store the original transform matrix for each animating bone.
        private Matrix _leftBackWheelTransform;

        private Matrix _rightBackWheelTransform;
        private Matrix _leftFrontWheelTransform;
        private Matrix _rightFrontWheelTransform;
        private Matrix _leftSteerTransform;
        private Matrix _rightSteerTransform;
        private Matrix _turretTransform;
        private Matrix _cannonTransform;
        private Matrix _hatchTransform;

        // Array holding all the bone transform matrices for the entire model.
        // We could just allocate this locally inside the Draw method, but it is more efficient to reuse a single array, as this avoids creating unnecessary garbage.
        private Matrix[] _boneTransforms;

        // Current animation positions.

        #endregion Fields

        #region Properties

        /// <summary>
        ///     Gets or sets the wheel rotation amount.
        /// </summary>
        public float WheelRotation { get; set; }

        /// <summary>
        ///     Gets or sets the steering rotation amount.
        /// </summary>
        public float SteerRotation { get; set; }

        /// <summary>
        ///     Gets or sets the turret rotation amount.
        /// </summary>
        public float TurretRotation { get; set; }

        /// <summary>
        ///     Gets or sets the cannon rotation amount.
        /// </summary>
        public float CannonRotation { get; set; }

        /// <summary>
        ///     Gets or sets the entry hatch rotation amount.
        /// </summary>
        public float HatchRotation { get; set; }

        #endregion Properties
    }
}