using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TGC.MonoGame.Samples.Models.Drawers
{
    class ModelInspector
    {
        internal static Dictionary<string, GeometryDrawer> GetGeometries(Model model)
        {
            var device = GetGraphicsDevice(model);
            var meshes = model.Meshes;
            var meshCount = meshes.Count;

            int meshPartCount;
            ModelMeshPartCollection meshParts;

            ModelMesh mesh;
            var list = new Dictionary<string, GeometryDrawer>();
            for (var index = 0; index < meshCount; index++)
            {
                mesh = meshes[index];
                meshParts = mesh.MeshParts;
                meshPartCount = meshParts.Count;
                if (meshPartCount == 1)
                    list.Add(mesh.Name, new GeometryDrawer(meshParts.First(), device));
                else
                    for (var subIndex = 0; subIndex < meshPartCount; subIndex++)
                        list.Add(mesh.Name + subIndex.ToString(), new GeometryDrawer(meshParts[subIndex], device));
            }

            return list;
        }

        internal static void FindModelGeometryData(Model model, ref ModelData modelData, ref List<GeometryData> geometryData)
        {
            var meshes = model.Meshes;
            var meshCount = meshes.Count;
            int meshPartCount;
            ModelMeshPartCollection meshParts;


            var hasTheSameTexture = true;
            Texture commonTexture = null;
            for (var index = 0; index < meshCount; index++)
            {
                var effects = meshes[index].Effects;
                for (var effectIndex = 0; effectIndex < effects.Count; effectIndex++)
                {
                    var texture = ((BasicEffect)effects[effectIndex]).Texture;
                    if (texture == null)
                        continue;

                    if (commonTexture != null && !texture.Equals(commonTexture))
                    {
                        hasTheSameTexture = false;
                        break;
                    }
                    commonTexture = texture;
                }
            }

            var matrices = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(matrices);

            if (hasTheSameTexture)
            {
                if (commonTexture != null)
                    modelData.Textures.Add(commonTexture);

                if (matrices.Length == 1 && matrices.First().Equals(Matrix.Identity))
                    return;

                for (var index = 0; index < meshCount; index++)
                {
                    meshParts = meshes[index].MeshParts;
                    meshPartCount = meshParts.Count;

                    var currentMatrix = matrices[meshes[index].ParentBone.Index];

                    for (var subIndex = 0; subIndex < meshPartCount; subIndex++)
                        geometryData.Add(new GeometryData(new List<Matrix> { currentMatrix }));
                }


                return;
            }

            for (var index = 0; index < meshCount; index++)
            {
                meshParts = meshes[index].MeshParts;
                meshPartCount = meshParts.Count;

                var currentMatrix = matrices[meshes[index].ParentBone.Index];

                for (var subIndex = 0; subIndex < meshPartCount; subIndex++)
                {
                    var texture = ((BasicEffect)meshParts[subIndex].Effect).Texture;
                    if (texture == null)
                        geometryData.Add(new GeometryData(new List<Matrix> { currentMatrix }));
                    else
                        geometryData.Add(new GeometryData(new List<Matrix> { currentMatrix }, new List<Texture> { texture }));
                }
            }



        }



        internal static GraphicsDevice GetGraphicsDevice(Model model)
        {
            return model.Meshes.FirstOrDefault().Effects.FirstOrDefault().GraphicsDevice;
        }

        internal static Effect GetDefaultEffect(Model model)
        {
            return model.Meshes.FirstOrDefault().Effects.FirstOrDefault();
        }

        public static ModelDrawer CreateDrawerFrom(Model model, Effect effect, EffectInspectionType type = EffectInspectionType.NONE)
        {
            var drawer = CreateDrawerFrom(model);
            drawer.SetEffect(effect, type);
            return drawer;
        }

        public static ModelDrawer CreateDrawerFrom(Model model, Effect effect, EffectTechnique technique, EffectInspectionType type = EffectInspectionType.NONE)
        {
            var drawer = CreateDrawerFrom(model);
            drawer.SetEffect(effect, technique, type);
            return drawer;
        }

        public static ModelDrawer CreateDrawerFrom(Model model)
        {
            var drawers = GetGeometries(model);

            var geometryData = new List<GeometryData>();
            var modelData = new ModelData();

            FindModelGeometryData(model, ref modelData, ref geometryData);

            return new ModelDrawer(drawers, modelData, geometryData);
        }

    }
}
