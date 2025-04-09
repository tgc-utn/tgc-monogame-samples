﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Viewer.GUI.Modifiers;

namespace TGC.MonoGame.Samples.Samples.RenderPipeline.DeferredLighting
{
    public class DeferredLighting : TGCSample
    {
        public DeferredLighting(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.RenderPipeline;
            Name = "Deferred Lighting";
            Description = "technique for rendering scenes with multiple lights.";
        }
        private float _time;
        private Camera _camera;
        private Model _cityModel;
        private Texture2D _cityTexture;
        private Effect _effectModel;
        private Effect _effectDeferred;
        private FullScreenQuad _fullScreenQuad;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _colorTarget;
        private RenderTarget2D _positionTarget;
        private RenderTarget2D _normalTarget;
        private RenderTarget2D _materialTarget;
        private RenderTarget2D _lightTarget;
        private RenderTarget2D _currentTarget;
        private bool _showScene = true;
        private Vector3 _ambientLightPosition = new Vector3(180, 180, 50);
        private Vector3 _ambientLightDiffuseColor = new Vector3(.6f, .6f, .6f);
        private Vector3 _ambientLightSpecularColor = new Vector3(.6f, .6f, .6f);
        private Vector3 _lightCursorPosition = new Vector3(-345f, -20f, 80f);
        private Vector3 _lightCursorColor = Color.White.ToVector3();
        private float _pointRadius = 50f;
        private List<PointLight> _pointLights = new List<PointLight>();
        private Model _sphere;
        
        public override void Initialize()
        {
            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-280f, 190f, 660f));
            
            _time = 0;

            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            _cityModel = Game.Content.Load<Model>(ContentFolder3D + "scene/city");
            // From the effect of the model I keep the texture.
            _cityTexture = ((BasicEffect)_cityModel.Meshes.FirstOrDefault()?.MeshParts.FirstOrDefault()?.Effect)?.Texture;

            // Load model drawer shader using Content pipeline.
            _effectModel = Game.Content.Load<Effect>(ContentFolderEffects + "DeferredModel");

            // Load deferred shader using Content pipeline.
            _effectDeferred = Game.Content.Load<Effect>(ContentFolderEffects + "Deferred");
            //_effectDeferred.Parameters["screenSize"].SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));

            _sphere = Game.Content.Load<Model>(ContentFolder3D  + "geometries/sphere");
            // Asign the effect to the meshes
            AssignEffectToModel(_effectModel, _cityModel);
            
            // Create the targets we are going to use
            InitializeRenderTargets();
            
            // To draw any render target
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _fullScreenQuad = new FullScreenQuad(GraphicsDevice);
            
            ModifierController.AddVector("Ambient position", OnAmbientPositionChange, _ambientLightPosition);
            ModifierController.AddColor("Ambient color", OnAmbientColorChange, new Color(_ambientLightDiffuseColor));
            ModifierController.AddVector("Light position", OnPointPositionChange, new Vector3(-345f, -20f, 80f));
            ModifierController.AddColor("Light color", OnPointColorChange, Color.White);
            ModifierController.AddFloat("Point radius", OnPointRadiusChange, 20f);
            ModifierController.AddButton("Add point", OnPointAddClick);
            ModifierController.AddButton("Remove point", OnPointRemoveClick);
            ModifierController.AddOptions("Show Target", [ "Scene", "Color", "Normal", "Position", "Material", "Light" ], Target.Scene, OnTargetChange);

            // Base scene lights
            GeneratePointLights();

            base.LoadContent();
        }

        

        public override void Update(GameTime gameTime)
        {
            _camera.Update(gameTime);

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _effectModel.Parameters["view"].SetValue(_camera.View);
            _effectModel.Parameters["projection"].SetValue(_camera.Projection);
            _effectDeferred.Parameters["view"].SetValue(_camera.View);
            _effectDeferred.Parameters["projection"].SetValue(_camera.Projection);

            // STEP 1: Instead of calculating lighting on the first pass, we save the following data per pixel 
            // into different render targets (G-Buffer). We can write to up to 4 render targets at once in the 
            // GraphicsDevice. The order is important, it must be the same as in the shader.
            
            // [Color] RGB = texture color
            // [Normal] RGB = world normal
            // [Position] RGB = world position
            // [Material] R = KD, G = KS, B = shininess
            GraphicsDevice.SetRenderTargets(_colorTarget, _normalTarget, _positionTarget, _materialTarget);
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            DrawCity();
            DrawCursor(); // A small sphere where the light is going to be placed when hitting 'add point'

            // STEP 2: Draw light 3D volumes as you would any 3D model
            // Lighting will only be calculated for the pixels inside the light volume,
            // color from each will be blended additively

            GraphicsDevice.SetRenderTargets(_lightTarget);
            GraphicsDevice.BlendState = BlendState.Additive;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            
            // We set the targets from STEP 1 as textures we will sample
            _effectDeferred.Parameters["colorMap"].SetValue(_colorTarget);
            _effectDeferred.Parameters["normalMap"].SetValue(_normalTarget);
            _effectDeferred.Parameters["positionMap"].SetValue(_positionTarget);
            _effectDeferred.Parameters["materialMap"].SetValue(_materialTarget);

            _effectDeferred.Parameters["cameraPosition"].SetValue(_camera.Position);
            
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            
            // We draw a fullscreen quad for an ambient light, to affect all pixels
            DrawAmbient();
            
            DrawPointLights();

            GraphicsDevice.SetRenderTarget(null);
            
            if(_showScene)
            {
                // STEP 3: We integrate the calculated light texture with the base color 
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.None;
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                _effectDeferred.Parameters["lightMap"].SetValue(_lightTarget);
                _effectDeferred.CurrentTechnique = _effectDeferred.Techniques["integrate"];
                _fullScreenQuad.Draw(_effectDeferred);
            }
            else
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(_currentTarget, Vector2.Zero, Color.White);
                _spriteBatch.End();
            }

            // STEP 4: As deferred rendering doesnt support transparency, we need to draw
            // transparent objects afterwards as a last step.

            base.Draw(gameTime);
        }
        void DrawCity()
        {
            _effectModel.CurrentTechnique = _effectModel.Techniques["textured"];
            var bones = _cityModel.Bones.Count;
            var modelTransforms = new Matrix[bones];

            _cityModel.CopyAbsoluteBoneTransformsTo(modelTransforms);

            var cityW = Matrix.CreateScale(0.5f);
            foreach (var mesh in _cityModel.Meshes)
            {
                var w = modelTransforms[mesh.ParentBone.Index] * cityW;
                _effectModel.Parameters["colorTexture"].SetValue(_cityTexture);
                _effectModel.Parameters["world"].SetValue(w);
                _effectModel.Parameters["inverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(w)));
                _effectModel.Parameters["KD"].SetValue(.8f);
                _effectModel.Parameters["KS"].SetValue(.6f);
                _effectModel.Parameters["shininess"].SetValue(10f);

                mesh.Draw();
            }
        }

        void DrawAmbient()
        {
            _effectDeferred.CurrentTechnique = _effectDeferred.Techniques["ambient_light"];

            _effectDeferred.Parameters["KA"].SetValue(.10f);
            _effectDeferred.Parameters["lightPosition"].SetValue(_ambientLightPosition);
            _effectDeferred.Parameters["lightDiffuseColor"].SetValue(_ambientLightDiffuseColor);
            _effectDeferred.Parameters["lightSpecularColor"].SetValue(_ambientLightSpecularColor);

            _fullScreenQuad.Draw(_effectDeferred);
        }

        void DrawCursor()
        {
            _effectModel.CurrentTechnique = _effectModel.Techniques["plain_color"];
            _effectModel.Parameters["color"].SetValue(_lightCursorColor);
            AssignEffectToModel(_effectModel, _sphere);
            foreach (var mesh in _sphere.Meshes)
            {
                var w = Matrix.CreateTranslation(_lightCursorPosition);

                _effectModel.Parameters["world"].SetValue(w);
                _effectModel.Parameters["inverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(w)));

                mesh.Draw();
            }
        }
        void DrawPointLights()
        {
            _effectDeferred.CurrentTechnique = _effectDeferred.Techniques["point_light"];
            AssignEffectToModel(_effectDeferred, _sphere);
            foreach (var light in _pointLights)
            {
                foreach(var mesh in _sphere.Meshes)
                {
                    var w = Matrix.CreateScale(light.Radius * .9f) * Matrix.CreateTranslation(light.Position);

                    _effectDeferred.Parameters["world"].SetValue(w);
                    _effectDeferred.Parameters["radius"].SetValue(light.Radius);
                    _effectDeferred.Parameters["lightPosition"].SetValue(light.Position);
                    _effectDeferred.Parameters["lightDiffuseColor"].SetValue(light.DiffuseColor);
                    _effectDeferred.Parameters["lightSpecularColor"].SetValue(light.SpecularColor);

                    mesh.Draw();
                }
                
            }
        }
        
        void InitializeRenderTargets()
        {
            // A high SurfaceFormat is recommended in position and light targets, as they can drastically
            // improve the quality of the lighting, but there is a significant performance cost at
            // high resolutions.
            _colorTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
               GraphicsDevice.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 0,
               RenderTargetUsage.DiscardContents);
            _positionTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            _normalTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            _materialTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            _lightTarget= new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
        }
        private void AssignEffectToModel(Effect e, Model m)
        {
            foreach (var mesh in m.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = e;
                }
            }
        }

        private void OnTargetChange(Target target)
        {
            _showScene = false;
            switch (target)
            {
                case Target.Scene:
                    _showScene = true;
                    break;
                case Target.Color:
                    _currentTarget = _colorTarget;
                    break;
                case Target.Normal:
                    _currentTarget = _normalTarget;
                    break;
                case Target.Position:
                    _currentTarget = _positionTarget;
                    break;
                case Target.Material:
                    _currentTarget = _materialTarget;
                    break;
                case Target.Light:
                    _currentTarget = _lightTarget;
                    break;
            }
        }
        private void OnPointRemoveClick()
        {
            if(_pointLights.Count > 0)
                _pointLights.RemoveAt(_pointLights.Count - 1);
            
        }
        private void OnPointPositionChange(Vector3 position)
        {
            _lightCursorPosition = position;
        }
        private void OnPointColorChange(Color color)
        {
            _lightCursorColor = color.ToVector3();
        }
        private void OnPointRadiusChange(float r)
        {
            _pointRadius = r;
        }
        private void OnPointAddClick()
        {
            _pointLights.Add(new PointLight(_lightCursorPosition, _lightCursorColor, _lightCursorColor, _pointRadius));
        }
        private void OnAmbientColorChange(Color color)
        {
            _ambientLightDiffuseColor = color.ToVector3();
            _ambientLightSpecularColor = _ambientLightDiffuseColor;
        }

        private void OnAmbientPositionChange(Vector3 pos)
        {
            _ambientLightPosition = pos;
        }

        private void GeneratePointLights()
        {
            Vector3 pos, col;
            float r;
            //cars
            pos = new Vector3(-367, -49, 294);
            col = Color.Cyan.ToVector3();
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-367, -49, 264);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-447, -49, 267);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-447, -49, 287);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(-109, -49, 231);
            col = Color.Cyan.ToVector3();
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-109, -49, 202);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(39, -49, 202);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(39, -49, 223);
            _pointLights.Add(new PointLight(pos, col, col, r));

            
            pos = new Vector3(-712, -49, 112);
            col = Color.Yellow.ToVector3();
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-739, -49, 112);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-735, -49, 28);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-714, -49, 28);
            _pointLights.Add(new PointLight(pos, col, col, r));

            
            pos = new Vector3(-455, -49, -180);
            col = Color.Blue.ToVector3();
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-455, -49, -203);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-537, -49, -198);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-537, -49, -178);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(-603, -49, -277);
            col = new Vector3(1, 0, 0.67f);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-607, -49, -255);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-521, -49, -279);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-521, -49, -253);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(-601, -49, -697);
            col = Vector3.One;
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-595, -49, -674);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-674, -49, -661);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-665, -49, -641);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(-175,-49, 32);
            col = new Vector3(1, 0.5f, 0);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-199, -49, 32);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-200, -49, 127);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-175, -49, 126);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(-202, -49, -86);
            col = new Vector3(1, 0.8f, 0f);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-174, -49, -86);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-175, -49, 12);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-201, -49, 12);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(-201, -49, -249);
            col = Vector3.One;
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-201, -49, -272);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-119, -49, -272);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-119, -49, -253);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(103, -49, -184);
            col = new Vector3(1, 0.64f, 0);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(103, -49, -207);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(16, -49, -207);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(16, -49, -186);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(273, -49, -105);
            col = Vector3.One;
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(298, -49, -105);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(298, -49, 43);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(275, -49, 43);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(225, -49, -416);
            col = new Vector3(1, 0.9f, 0);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(202, -49, -416);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(204, -49, -503);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(226, -49, -503);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(273, -49, -664);
            col = new Vector3(0.55f, 0, 1);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(294, -49, -664);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(281, -49, -558);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(301, -49, -561);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(50, -49, -542);
            col = new Vector3(1,0,0.43f);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(44, -49, -564);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(124, -49, -583);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(130, -49, -556);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(123, -49, -661);
            col = new Vector3(1, 0.94f, 0);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(123, -49, -663);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-24, -49, -677);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-24, -49, -656);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(-221, -49, -665);
            col = new Vector3(1, 0, 0.94f);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-206, -49, -650);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-152, -49, -708);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-168, -49, -723);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(-295, -49, -500);
            col = new Vector3(0, 0.32f, 1);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-300, -49, -526);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-397, -49, -504);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-394, -49, -480);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(-388, -49, -422);
            col = new Vector3(1, 0.9f, 0);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-389, -49, -399);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-304, -49, -399);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-304, -49, -419);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(-397, -49, -346);
            col = new Vector3(1, 0.7f, 0);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-397, -49, -323);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-314, -49, -323);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-313, -49, -346);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(-495, -49, -728);
            col = new Vector3(0,0.73f,1);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-495, -49, -753);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-395, -49, -728);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-395, -49, -750);
            _pointLights.Add(new PointLight(pos, col, col, r));

            pos = new Vector3(-466, -49, -681);
            col = new Vector3(0, 0.96f, 1f);
            r = 40f;
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-465, -49, -659);
            _pointLights.Add(new PointLight(pos, col, col, r));
            col = Color.Red.ToVector3();
            r = 20f;
            pos = new Vector3(-385, -49, -659);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-385, -49, -682);
            _pointLights.Add(new PointLight(pos, col, col, r));

            //lamps
            r = 60f;
            col = Vector3.One;
            pos = new Vector3(-390, 87, -602);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-386, 87, -329);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-271, 72, -113);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-271, 72, 130);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-101, 72, 203);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(113, 72, 203);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(205, 72, 19);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(210, 72, -480);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(110, 72, -665);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-103, 72, -665);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-677, 86, -472);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-664, 72, 131);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-664, 72, -114);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-98, 72, -195);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(-98, 73, -270);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(109, 72, -270);
            _pointLights.Add(new PointLight(pos, col, col, r));
            pos = new Vector3(114, 73, -194);
            _pointLights.Add(new PointLight(pos, col, col, r));
        }
    }
    enum Target
    {
        Scene,
        Color,
        Normal,
        Position,
        Material,
        Light
    }
    class PointLight
    {
        public Vector3 Position;
        public Vector3 DiffuseColor;
        public Vector3 SpecularColor;
        public float Radius;

        public PointLight(Vector3 position, Vector3 diffuseColor, Vector3 specularColor, float radius)
        {
            Position = position;
            DiffuseColor = diffuseColor;
            SpecularColor = specularColor;
            Radius = radius;
        }
    }
}
