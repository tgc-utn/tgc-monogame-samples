using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TGC.MonoGame.Samples.Models
{
    class ModelDrawer
    {
        private GraphicsDevice GraphicsDevice { get; set; }

        private Model Model { get; set; }

        private Dictionary<Texture2D, Dictionary<ModelMesh, ModelMeshPart[]>> TextureMeshDictionary { get; set; }
        private Dictionary<ModelMesh, ModelMeshPart[]> NoTextureMeshParts { get; set; }


        private Matrix[] BoneTransforms { get; set; }


        private bool HasTextureParameter { get; set; } = false;
        private bool HasWorldParameter { get; set; } = false;
        private bool HasNormalMatrixParameter { get; set; } = false;

        public EffectParameter WorldViewProjectionMatrixParameter { get; set; }

        private ModelMeshCollection ModelMeshes { get; set; }


        private EffectParameter _textureParameter;
        private EffectParameter _normalMatrixParameter;
        private EffectParameter _worldParameter;


        public EffectParameter TextureParameter
        {
            get => _textureParameter;
            set
            {
                _textureParameter = value;
                HasTextureParameter = true;
            }
        }

        public EffectParameter WorldMatrixParameter
        {
            get => _worldParameter;
            set
            {
                _worldParameter = value;
                HasWorldParameter = true;
            }
        }

        public EffectParameter NormalMatrixParameter
        {
            get => _normalMatrixParameter;
            set
            {
                _normalMatrixParameter = value;
                HasNormalMatrixParameter = true;
            }
        }



        public ModelDrawer(Model model, GraphicsDevice device)
        {
            Model = model;
            ModelMeshes = model.Meshes;
            GraphicsDevice = device;
            ExtractTextures();
            ExtractMatrices();
        }


        public void SetEffect(Effect effect)
        {
            foreach (var modelMesh in ModelMeshes)
                foreach (var meshPart in modelMesh.MeshParts)
                    meshPart.Effect = effect;
        }


        private void ExtractTextures()
        {
            var basicTextureDictionary = new Dictionary<Texture2D, Dictionary<ModelMesh, List<ModelMeshPart>>>();
            var meshesWithoutTexture = new Dictionary<ModelMesh, List<ModelMeshPart>>();

            var index = 0;

            foreach (var modelMesh in ModelMeshes)
            {
                foreach (var effect in modelMesh.Effects)
                {
                    var basicEffect = effect as BasicEffect;
                    if (basicEffect == null)
                        continue;

                    var meshPart = modelMesh.MeshParts[index];

                    var texture = basicEffect.Texture;
                    if (texture != null)
                    {
                        if (basicTextureDictionary.ContainsKey(texture))
                        {
                            if (basicTextureDictionary[texture].ContainsKey(modelMesh))
                                basicTextureDictionary[texture][modelMesh].Add(meshPart);
                            else
                                basicTextureDictionary[texture].Add(modelMesh, new List<ModelMeshPart>() { meshPart });
                        }
                        else
                        {
                            var dictionary = new Dictionary<ModelMesh, List<ModelMeshPart>>();
                            dictionary.Add(modelMesh, new List<ModelMeshPart>() { meshPart });
                            basicTextureDictionary.Add(texture, dictionary);
                        }
                    }
                    else
                    {
                        if (meshesWithoutTexture.ContainsKey(modelMesh))
                            meshesWithoutTexture[modelMesh].Add(meshPart);
                        else
                            meshesWithoutTexture.Add(modelMesh, new List<ModelMeshPart>() { meshPart });
                    }

                    index++;
                }
                index = 0;
            }

            NoTextureMeshParts = new Dictionary<ModelMesh, ModelMeshPart[]>();
            foreach (var keyPair in meshesWithoutTexture)
                NoTextureMeshParts.Add(keyPair.Key, keyPair.Value.ToArray());

            TextureMeshDictionary = new Dictionary<Texture2D, Dictionary<ModelMesh, ModelMeshPart[]>>();

            foreach (var keyPair in basicTextureDictionary)
            {
                var dictionary = new Dictionary<ModelMesh, ModelMeshPart[]>();
                foreach (var keyPair2 in keyPair.Value)
                    dictionary.Add(keyPair2.Key, keyPair2.Value.ToArray());

                TextureMeshDictionary.Add(keyPair.Key, dictionary);
            }
        }

        private void ExtractMatrices()
        {
            // We get the base transform for each mesh
            BoneTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(BoneTransforms);
        }

        private void DrawMeshPart(ModelMeshPart part)
        {
            if (part.PrimitiveCount == 0)
                return;

            var effect = part.Effect;

            GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
            GraphicsDevice.Indices = part.IndexBuffer;

            for (int passIndex = 0; passIndex < effect.CurrentTechnique.Passes.Count; passIndex++)
            {
                effect.CurrentTechnique.Passes[passIndex].Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
        }

        public void Draw(Matrix worldViewProjection)
        {

            foreach(var entry in NoTextureMeshParts)
            {
                var mesh = entry.Key;
                var meshParts = entry.Value;
                WorldViewProjectionMatrixParameter.SetValue(BoneTransforms[mesh.ParentBone.Index] * worldViewProjection);
                for (var index = 0; index < meshParts.Length; index++)
                    DrawMeshPart(meshParts[index]);
            }

            foreach (var drawInstance in TextureMeshDictionary)
            {
                if (HasTextureParameter)
                    TextureParameter.SetValue(drawInstance.Key);

                foreach(var meshDrawInstance in drawInstance.Value)
                {
                    var mesh = meshDrawInstance.Key;
                    var meshParts = meshDrawInstance.Value;
                    WorldViewProjectionMatrixParameter.SetValue(BoneTransforms[mesh.ParentBone.Index] * worldViewProjection);
                    for (var index = 0; index < meshParts.Length; index++)
                        DrawMeshPart(meshParts[index]);
                }
            }
        }


        public void Draw(Matrix world, Matrix viewProjection)
        {
            
            foreach (var entry in NoTextureMeshParts)
            {
                var mesh = entry.Key;
                var meshParts = entry.Value;

                var modelMeshWorld = BoneTransforms[mesh.ParentBone.Index] * world;

                if (HasWorldParameter)
                    WorldMatrixParameter.SetValue(modelMeshWorld);

                if (HasNormalMatrixParameter)
                    NormalMatrixParameter.SetValue(Matrix.Invert(Matrix.Transpose(modelMeshWorld)));

                WorldViewProjectionMatrixParameter.SetValue(modelMeshWorld * viewProjection);

                for (var index = 0; index < meshParts.Length; index++)
                    DrawMeshPart(meshParts[index]);
            }

            foreach (var drawInstance in TextureMeshDictionary)
            {
                if (HasTextureParameter)
                    TextureParameter.SetValue(drawInstance.Key);

                foreach (var meshDrawInstance in drawInstance.Value)
                {
                    var mesh = meshDrawInstance.Key;
                    var meshParts = meshDrawInstance.Value;

                    var modelMeshWorld = BoneTransforms[mesh.ParentBone.Index] * world;

                    if (HasWorldParameter)
                        WorldMatrixParameter.SetValue(modelMeshWorld);

                    if (HasNormalMatrixParameter)
                        NormalMatrixParameter.SetValue(Matrix.Invert(Matrix.Transpose(modelMeshWorld)));

                    WorldViewProjectionMatrixParameter.SetValue(modelMeshWorld * viewProjection);

                    for (var index = 0; index < meshParts.Length; index++)
                        DrawMeshPart(meshParts[index]);
                }
            }
        }



    }
}
