﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.Samples.Samples.PBR
{
    public struct Light
    {
        public Vector3 Position { get; set; }
        public Vector3 Color { get; set; }
        public Vector3 ShowColor { get; set; }

        private Light(Vector3 position, Vector3 color, Vector3 showColor) 
        {
            Position = position;
            Color = color;
            ShowColor = showColor;
        }

        internal Light GenerateShowColor()
        {
            return new Light(Position, Color, Vector3.Normalize(Color) * 2f);
        }
    }
}