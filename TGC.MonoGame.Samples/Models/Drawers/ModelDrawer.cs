using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace TGC.MonoGame.Samples.Models.Drawers
{

    public class ModelDrawer
    {
        protected EffectTechnique _effectTechnique;

        protected EffectPass[] _passes;

        protected ModelData _modelData;

        protected List<GeometryData> _geometryData;

        public Effect Effect { get; private set; }
        public List<GeometryDrawer> GeometryDrawers { get; private set; }
        public List<GeometryData> GeometryData { get => _geometryData; }
        public ModelActionCollection ModelActionCollection { get; private set; }
        public GeometryActionCollection GeometryActionCollection { get; private set; }
        public Matrix World { get => _modelData.World; set => _modelData.World = value; }
        public Matrix ViewProjection { get => _modelData.ViewProjection; set => _modelData.ViewProjection = value; }
        public List<Texture> Textures { get => _modelData.Textures; }
       
        private Dictionary<string, int> _indices;


        public bool SingleWorldMatrix 
        {
            get => _geometryData == null || _geometryData.All(geometry => geometry.Matrices.Count == 0);
        }

        public bool SingleTexture 
        {
            get => _geometryData == null || _geometryData.All(geometry => geometry.Textures.Count == 0);
        }

        protected ModelDrawer()
        {
            ModelActionCollection = new ModelActionCollection();
            GeometryActionCollection = new GeometryActionCollection();
            _modelData = new ModelData();
            _geometryData = new List<GeometryData>();
            GeometryDrawers = new List<GeometryDrawer>();
            _indices = new Dictionary<string, int>();
        }



        internal ModelDrawer(List<GeometryDrawer> drawers, ModelData data, List<GeometryData> geometryData)
        {
            ModelActionCollection = new ModelActionCollection();
            GeometryActionCollection = new GeometryActionCollection();
            _modelData = data;
            _geometryData = geometryData;
            GeometryDrawers = drawers;
            _indices = new Dictionary<string, int>();
        }

        internal ModelDrawer(Dictionary<string, GeometryDrawer> drawers, ModelData data, List<GeometryData> geometryData)
        {
            ModelActionCollection = new ModelActionCollection();
            GeometryActionCollection = new GeometryActionCollection();
            _modelData = data;
            _geometryData = geometryData;
            GeometryDrawers = drawers.Values.ToList();
            CreateIndexDictionary(drawers);
        }


        public ModelDrawer(GeometryDrawer drawer, Effect effect, EffectTechnique effectTechnique,
            EffectInspectionType inspectionType = EffectInspectionType.NONE) : this()
        {
            Effect = effect;
            _effectTechnique = effectTechnique;

            GeometryDrawers.Add(drawer);

            SetDefaultPasses();

            EffectInspector.SetModelActions(this, inspectionType);
        }

        public ModelDrawer(GeometryDrawer drawer, Effect effect, EffectInspectionType inspectionType = EffectInspectionType.NONE) 
            : this(drawer, effect, effect.CurrentTechnique, inspectionType)
        {

        }

        public ModelDrawer(List<GeometryDrawer> drawers, Effect effect, EffectTechnique effectTechnique, 
            EffectInspectionType inspectionType = EffectInspectionType.NONE) : this()
        {
            Effect = effect;
            _effectTechnique = effectTechnique;
            GeometryDrawers.AddRange(drawers);

            for (var index = 0; index < GeometryDrawers.Count; index++)
                GeometryData.Add(new GeometryData(null));

            SetDefaultPasses();
            EffectInspector.SetModelActions(this, inspectionType);
        }

        public ModelDrawer(List<GeometryDrawer> drawers, Effect effect,
            EffectInspectionType inspectionType = EffectInspectionType.NONE)
            : this (drawers, effect, effect.CurrentTechnique, inspectionType)
        {

        }


        public ModelDrawer(Dictionary<string, GeometryDrawer> drawers, Effect effect, EffectTechnique effectTechnique,
            EffectInspectionType inspectionType = EffectInspectionType.NONE) : this()
        {
            Effect = effect;
            _effectTechnique = effectTechnique;
            GeometryDrawers.AddRange(drawers.Values);

            for (var index = 0; index < GeometryDrawers.Count; index++)
                GeometryData.Add(new GeometryData(null));

            SetDefaultPasses();
            EffectInspector.SetModelActions(this, inspectionType);
            CreateIndexDictionary(drawers);
        }


        public ModelDrawer(Dictionary<string, GeometryDrawer> drawers, Effect effect,
            EffectInspectionType inspectionType = EffectInspectionType.NONE)
            : this(drawers, effect, effect.CurrentTechnique, inspectionType)
        {

        }


        public GeometryDrawer FindDrawer(string name)
        {
            return GeometryDrawers[_indices[name]];
        }

        public GeometryData FindData(string name)
        {
            return GeometryData[_indices[name]];
        }


        public void SetEffect(Effect effect, EffectInspectionType inspectionType = EffectInspectionType.NONE)
        {
            SetEffect(effect, effect.CurrentTechnique, inspectionType);
        }

        public void UpdateTechnique(EffectInspectionType inspectionType = EffectInspectionType.NONE)
        {
            SetEffect(Effect, Effect.CurrentTechnique, inspectionType);
        }

        public void SetEffect(Effect effect, EffectTechnique effectTechnique,
            EffectInspectionType inspectionType = EffectInspectionType.NONE)
        {
            Effect = effect;
            _effectTechnique = effectTechnique;
            SetDefaultPasses();

            ModelActionCollection.Clear();
            GeometryActionCollection.Clear();

            EffectInspector.SetModelActions(this, inspectionType);
        }

        private void CreateIndexDictionary(Dictionary<string, GeometryDrawer> drawers)
        {
            var index = 0;
            _indices = drawers.Keys
                .Select(key =>
                {
                    var keyValue = new KeyValuePair<string, int>(key, index);
                    index++;
                    return keyValue;
                })
                .ToDictionary(t => t.Key, t => t.Value);
        }

        private void SetDefaultPasses()
        {
            _passes = EffectInspector.GetDefaultPasses(_effectTechnique);
        }

        public void Draw()
        {
            ModelActionCollection.Execute(_modelData);

            if (GeometryActionCollection.Empty)
                for (var index = 0; index < GeometryDrawers.Count; index++)
                    GeometryDrawers[index].Draw(_passes);
            else
            {
                GeometryData geometryData;
                for (var index = 0; index < GeometryDrawers.Count; index++)
                {
                    geometryData = GeometryData[index];
                    GeometryActionCollection.Execute(_modelData, geometryData);
                    GeometryDrawers[index].Draw(_passes);
                }
            }
        }

        public ModelDrawer Clone(bool withActions = false, bool withData = false)
        {
            var clone = new ModelDrawer(GeometryDrawers, Effect, _effectTechnique, EffectInspectionType.NONE);

            if (withActions)
            {
                clone.ModelActionCollection.AddRange(ModelActionCollection);
                clone.GeometryActionCollection.AddRange(GeometryActionCollection);
            }

            if(withData)
            {
                clone._modelData = _modelData.Clone();
                var geometryData = new List<GeometryData>();
                for (var index = 0; index < _geometryData.Count; index++)
                    geometryData.Add(_geometryData[index].Clone());

                clone._geometryData = geometryData;
            }

            return clone;
        }

        public void Dispose()
        {
            for (var index = 0; index < GeometryDrawers.Count; index++)
                GeometryDrawers[index].Dispose();
        }
    }
}