using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TGC.MonoGame.Samples.Models.Drawers
{
    public class ModelData
    {
        internal Matrix World
        {
            get => _world;
            set
            {
                _world = value;
                _worldViewProjectionCalculated = false;
            }
        }

        internal Matrix ViewProjection 
        { 
            get => _viewProjection;
            set
            {
                _viewProjection = value;
                _worldViewProjectionCalculated = false;
            } 
        }


        internal List<Texture> Textures;

        private Matrix _world;

        private Matrix _viewProjection;

        private Matrix _worldViewProjection;

        private bool _worldViewProjectionCalculated;

        public ModelData()
        {
            World = Matrix.Identity;
            ViewProjection = Matrix.Identity;
            Textures = new List<Texture>();
            _worldViewProjectionCalculated = false;
        }


        public Matrix GetWorldViewProjection()
        {
            if (!_worldViewProjectionCalculated)
            {
                _worldViewProjection = _world * _viewProjection;
                _worldViewProjectionCalculated = true;
            }
            return _worldViewProjection;
        }

        public ModelData Clone()
        {
            var clone = MemberwiseClone() as ModelData;
            clone.Textures = new List<Texture>();
            for (var index = 0; index < Textures.Count; index++)
                clone.Textures.Add(Textures[index]);

            return clone;
        }
    }
}