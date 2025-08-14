using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.PostProcessing;

public class PlanarReflections : TGCSample
{
    private FreeCamera _camera;

    private QuadPrimitive _quad;
    private Matrix _quadWorld;
    private RenderTarget2D _reflectionRenderTarget;
    private Effect _planarReflectionEffect;

    private Model _robotModel;
    private Matrix _robotWorld;

    private Model _chairModel;
    private Matrix _chairWorld;

    private BoxPrimitive _box;
    private Matrix _frameTop, _frameBottom, _frameLeft, _frameRight;

    public PlanarReflections(TGCViewer game) : base(game)
    {
        Category = TGCSampleCategory.PostProcessing;
        Name = "Planar Reflections";
        Description = "Reflections on a flat surface";
    }

    public override void Initialize()
    {
        var screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
        _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.Zero, screenSize);

        var robotPosition = new Vector3(0f, 0f, -250f);
        _robotWorld = Matrix.CreateTranslation(robotPosition);

        var chairTranslation = Matrix.CreateTranslation(-100f, -50f, -150f);
        var chairRotation = Matrix.CreateRotationY(MathHelper.PiOver2);
        _chairWorld = chairRotation * chairTranslation;

        var quadTranslation = Matrix.CreateTranslation(0, 0, -350f);
        var quadRotation = Matrix.CreateRotationX(MathHelper.PiOver2);
        var quadScale = Matrix.CreateScale(50f, 100f, 100f);
        _quad = new QuadPrimitive(GraphicsDevice);
        _quadWorld = quadScale * quadRotation * quadTranslation;

        _frameTop = Matrix.CreateScale(10f, 1f, 1f)
                    * Matrix.CreateTranslation(0f, 100f, -350f);

        _frameBottom = Matrix.CreateScale(10f, 1f, 1f)
                       * Matrix.CreateTranslation(0f, -100f, -350f);

        _frameRight = Matrix.CreateScale(1f, 20f, 1f)
                      * Matrix.CreateTranslation(50f, 0f, -350f);

        _frameLeft = Matrix.CreateScale(1f, 20f, 1f)
                     * Matrix.CreateTranslation(-50f, 0f, -350f);

        _reflectionRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height,
            true, SurfaceFormat.Color, DepthFormat.Depth24);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _robotModel = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
        ((BasicEffect)_robotModel.Meshes.FirstOrDefault()?.Effects.FirstOrDefault())?.EnableDefaultLighting();

        _chairModel = Game.Content.Load<Model>(ContentFolder3D + "chair/chair");
        ((BasicEffect)_chairModel.Meshes.FirstOrDefault()?.Effects.FirstOrDefault())?.EnableDefaultLighting();

        _box = new BoxPrimitive(GraphicsDevice, Vector3.One * 10f, null);

        _planarReflectionEffect = Game.Content.Load<Effect>(ContentFolderEffects + "PlanarReflections");

        ModifierController.AddTexture("Reflection Render Target", _reflectionRenderTarget);
        
        base.LoadContent();
    }

    private void DrawReflection(Matrix world, Matrix view, Matrix projection)
    {
        // Set the render target to the Reflection Texture
        GraphicsDevice.SetRenderTarget(_reflectionRenderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        var quadNormal = Vector3.Backward;
        var viewDirection = _camera.Position - _quadWorld.Translation;
        var projLength = Vector3.Dot(quadNormal, viewDirection);
        var reflectionCamPos = _camera.Position - 2 * quadNormal * projLength;
        var reflectionCamForward = Vector3.Reflect(_camera.FrontDirection,
            quadNormal);
        var reflectionCamUp = Vector3.Reflect(_camera.UpDirection, quadNormal);
        var reflectionCamView = Matrix.CreateLookAt(reflectionCamPos,
            reflectionCamPos + reflectionCamForward, reflectionCamUp);

        // Draw the scene from the reflection camera point of view
        DrawScene(reflectionCamView, projection);

        // Reset the render target to the default (screen)
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Draw the quad/reflective surface
        DrawQuad(world, view, projection, reflectionCamView);
    }

    private void DrawQuad(Matrix world, Matrix view, Matrix projection, Matrix reflectionView)
    {
        _planarReflectionEffect.Parameters["ReflectionView"].SetValue(reflectionView);
        _planarReflectionEffect.Parameters["Projection"].SetValue(projection);
        _planarReflectionEffect.Parameters["WorldViewProjection"].SetValue(world * view * projection);
        _planarReflectionEffect.Parameters["World"].SetValue(world);
        _planarReflectionEffect.Parameters["ReflectionTexture"].SetValue(_reflectionRenderTarget);

        _quad.Draw(_planarReflectionEffect);

        _box.Draw(_frameTop, _camera.View, _camera.Projection);
        _box.Draw(_frameBottom, _camera.View, _camera.Projection);
        _box.Draw(_frameRight, _camera.View, _camera.Projection);
        _box.Draw(_frameLeft, _camera.View, _camera.Projection);
    }

    private void DrawScene(Matrix view, Matrix projection)
    {
        _robotModel.Draw(_robotWorld, view, projection);
        _chairModel.Draw(_chairWorld, view, projection);
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.Clear(Color.CornflowerBlue);

        DrawReflection(_quadWorld, _camera.View, _camera.Projection);

        DrawScene(_camera.View, _camera.Projection);

        base.Draw(gameTime);
    }

    public override void Update(GameTime gameTime)
    {
        _camera.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void UnloadContent()
    {
        _reflectionRenderTarget.Dispose();
        
        base.UnloadContent();
    }
}