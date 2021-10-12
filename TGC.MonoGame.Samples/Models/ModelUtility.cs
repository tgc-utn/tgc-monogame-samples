using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Models
{
    public static class ModelUtility
    {

        public static Texture2D[] GetTextures(Model model)
        {
            var textures = new List<Texture2D>();

            foreach (var modelMesh in model.Meshes)
                foreach (var effect in modelMesh.Effects)
                {
                    var basicEffect = effect as BasicEffect;
                    if (basicEffect != null && basicEffect.Texture != null)
                        textures.Add(basicEffect.Texture);
                }

            return textures.ToArray();
        }


        public static void SetEffect(Model model, Effect effect)
        {
            foreach (var modelMesh in model.Meshes)
                foreach (var meshPart in modelMesh.MeshParts)
                    meshPart.Effect = effect;
        }



        public static void Draw(Model model, Matrix worldViewProjection, Matrix[] meshesBaseTransforms, EffectParameter worldViewProjectionEffectParameter)
        {
            foreach (var modelMesh in model.Meshes)
            {
                // We set the main matrices for each mesh to draw
                var meshRelativeWorld = meshesBaseTransforms[modelMesh.ParentBone.Index];

                // World View Projection is used to transform from model space to screen space
                worldViewProjectionEffectParameter.SetValue(meshRelativeWorld * worldViewProjection);

                // Once we set this matrix we draw
                modelMesh.Draw();
            }
        }

        public static void Draw(Model model, Matrix world, Matrix worldViewProjection, Matrix[] meshesBaseTransforms,
            EffectParameter worldViewProjectionEffectParameter,
            EffectParameter normalMatrixEffectParameter)
        {
            foreach (var modelMesh in model.Meshes)
            {
                // We set the main matrices for each mesh to draw
                var meshRelativeWorld = meshesBaseTransforms[modelMesh.ParentBone.Index];

                // World View Projection is used to transform from model space to screen space
                worldViewProjectionEffectParameter.SetValue(meshRelativeWorld * worldViewProjection);

                // Inverse Transpose World is used to transform normals to world space
                normalMatrixEffectParameter.SetValue(Matrix.Invert(Matrix.Transpose(meshRelativeWorld * world)));

                // Once we set these matrices we draw
                modelMesh.Draw();
            }
        }
        public static void Draw(Model model, Matrix world, Matrix worldViewProjection, Matrix[] meshesBaseTransforms,
            EffectParameter worldViewProjectionEffectParameter,
            EffectParameter worldEffectParameter,
            EffectParameter normalMatrixEffectParameter)
        {
            foreach (var modelMesh in model.Meshes)
            {
                // We set the main matrices for each mesh to draw
                var meshRelativeWorld = meshesBaseTransforms[modelMesh.ParentBone.Index];

                var finalWorld = meshRelativeWorld * world;
                // World is used to transform from model space to screen space
                worldEffectParameter.SetValue(finalWorld);

                // World View Projection is used to transform from model space to screen space
                worldViewProjectionEffectParameter.SetValue(meshRelativeWorld * worldViewProjection);

                // Inverse Transpose World is used to transform normals to world space
                normalMatrixEffectParameter.SetValue(Matrix.Invert(Matrix.Transpose(finalWorld)));

                // Once we set these matrices we draw
                modelMesh.Draw();
            }
        }
    }
}
