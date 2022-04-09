using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TGC.MonoGame.Samples.Utils;

namespace TGC.MonoGame.Samples.Models.Drawers
{
    internal partial class EffectInspector
    {


        internal static EffectPass[] GetDefaultPasses(EffectTechnique technique)
        {
            return technique.Passes.Cast<EffectPass>().ToArray();
        }


        internal static void SetModelActions(ModelDrawer drawer, EffectInspectionType inspection)
        {
            if (inspection.Equals(EffectInspectionType.NONE))
                return;

            var hasMultipleGeometry = !drawer.HasSingleWorldMatrix;
            var hasSingleTexture = drawer.HasSingleTexture;

            AddMatricesActions(drawer, hasMultipleGeometry, inspection);
            AddTextureActions(drawer, hasSingleTexture, inspection);
        }

        private static void AddMatricesActions(ModelDrawer modelDrawer, bool hasMultipleGeometry, EffectInspectionType inspection)
        {
            if (!FlagsHelper.IsSet(inspection, EffectInspectionType.MATRICES))
                return;

            var parameters = modelDrawer.Effect.Parameters;

            var worldParameter = Find(EffectParameterClass.Matrix, "WORLD", parameters);
            var worldViewProjectionParameter = FindLike(EffectParameterClass.Matrix, "WORLDVIEWPROJ", parameters);
            var inverseTransposeWorldParameter = Find(EffectParameterClass.Matrix, "INVERSETRANSPOSEWORLD", parameters);

            IActionCollection collection;

            if (hasMultipleGeometry)
                collection = modelDrawer.GeometryActionCollection;
            else
                collection = modelDrawer.ModelActionCollection;

            if (worldParameter != null)
                collection.AddSetWorld(worldParameter);

            if (inverseTransposeWorldParameter != null)
                collection.AddSetInverseTranspose(inverseTransposeWorldParameter);

            if (worldViewProjectionParameter != null)
                collection.AddSetWorldViewProjection(worldViewProjectionParameter);
        }

        private static void AddTextureActions(ModelDrawer modelDrawer, bool hasSingleTexture, EffectInspectionType inspection)
        {
            if (!FlagsHelper.IsSet(inspection, EffectInspectionType.TEXTURES))
                return;

            var parameters = modelDrawer.Effect.Parameters;            
            var textureParameter = Find(EffectParameterType.Texture2D, parameters);
            
            if (textureParameter == null)
                return;

            IActionCollection collection;

            if (hasSingleTexture)
                collection = modelDrawer.ModelActionCollection;
            else
                collection = modelDrawer.GeometryActionCollection;


            collection.AddMainTexture(textureParameter);
        }



        private static EffectParameter Find(EffectParameterType type, EffectParameterCollection parameters)
        {
            return parameters.FirstOrDefault(parameter => parameter.ParameterType.Equals(type));
        }
        private static EffectParameter Find(EffectParameterClass parameterClass, EffectParameterCollection parameters)
        {
            return parameters.FirstOrDefault(parameter => parameter.ParameterClass.Equals(parameterClass));
        }
        private static EffectParameter Find(EffectParameterClass parameterClass, string name, EffectParameterCollection parameters)
        {
            var upperName = name.ToUpper();
            return parameters.FirstOrDefault(parameter =>
                parameter.ParameterClass.Equals(parameterClass) && parameter.Name.ToUpper().Equals(upperName));
        }
        private static EffectParameter Find(EffectParameterType type, string name, EffectParameterCollection parameters)
        {
            var upperName = name.ToUpper();
            return parameters.FirstOrDefault(parameter => 
                parameter.ParameterType.Equals(type) && parameter.Name.ToUpper().Equals(upperName));
        }

        private static EffectParameter FindLike(EffectParameterType type, string name, EffectParameterCollection parameters)
        {
            var upperName = name.ToUpper();
            return parameters.FirstOrDefault(parameter =>
                parameter.ParameterType.Equals(type) && parameter.Name.ToUpper().Contains(upperName));
        }
        private static EffectParameter FindLike(EffectParameterClass parameterClass, string name, EffectParameterCollection parameters)
        {
            var upperName = name.ToUpper();
            return parameters.FirstOrDefault(parameter =>
                parameter.ParameterClass.Equals(parameterClass) && parameter.Name.ToUpper().Contains(upperName));
        }
    }
}
