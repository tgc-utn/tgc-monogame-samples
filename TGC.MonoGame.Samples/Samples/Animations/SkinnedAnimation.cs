using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Xna.Framework;
using TGC.MonoGame.Samples.Animations.Models;
using TGC.MonoGame.Samples.Animations.PipelineExtension;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Animations;

/// <summary>
///     Skinned Animation:
///     Example of skeletal animation.
///     To build the animations we use CustomPipelineManager class.
///     Animations can be easily generated in Mixamo - https://www.mixamo.com.
///     Author: Rene Juan Rico Mendoza.
/// </summary>
public class SkinnedAnimation : TGCSample
{
    public SkinnedAnimation(TGCViewer game) : base(game)
    {
        Category = TGCSampleCategory.Animations;
        Name = "Skinned Skeletal Animation";
        Description = "A Better Skinned Sample.";
    }

    /// <summary>
    ///     The Camera we use.
    /// </summary>
    private Camera _camera;

    /// <summary>
    ///     Application configuration file.
    /// </summary>
    private IConfigurationRoot _configuration;

    private List<string> _modelAnimationFileNames;

    private List<string> _modelFileNames;

    /// <summary>
    ///     This Model is loaded solely for the animation.
    /// </summary>
    private AnimatedModel _animation;

    /// <summary>
    ///     The animated Model we are displaying.
    /// </summary>
    private AnimatedModel _model;

    /// <summary>
    ///     Allows the game to perform any initialization it needs to before starting to run.
    ///     This is where it can query for any required services and load any non-graphic related content.
    ///     Calling base.Initialize will enumerate through any components and initialize them as well.
    /// </summary>
    public override void Initialize()
    {
        // Configuration file.
        var configurationFileName = "app-settings.json";
        _configuration = new ConfigurationBuilder().AddJsonFile(configurationFileName, true, true).Build();

        _camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 100, 500), 30, 0.5f);

        base.Initialize();
    }

    /// <summary>
    ///     LoadContent will be called once per game and is the place to load all of your content.
    /// </summary>
    protected override void LoadContent()
    {
        // File names of models and animations.
        var skeletonFolder = ContentFolder3D + "tgcito-classic/skeleton/";
        _modelFileNames = new List<string> { skeletonFolder + "T-Pose" };
        _modelAnimationFileNames = new List<string>
        {
            skeletonFolder + "Idle",
            skeletonFolder + "Idle-2",
            skeletonFolder + "Standard-Run",
            skeletonFolder + "Standard-Walk"
        };

        // Build content.
        var manager = CustomPipelineManager.CreateCustomPipelineManager(_configuration);

        foreach (var model in _modelFileNames)
        {
            manager.BuildAnimationContent(model);
        }

        foreach (var animation in _modelAnimationFileNames)
        {
            manager.BuildAnimationContent(animation);
        }

        // Load the Model we will display.
        _model = new AnimatedModel(_modelFileNames[0]);
        _model.LoadContent(Game.Content);

        ModifierController.AddOptions("Animation", AnimationType.Idle, OnAnimationChange);
    }

    /// <summary>
    ///     Allows the game to run logic such as updating the world, checking for collisions, gathering input, and playing
    ///     audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public override void Update(GameTime gameTime)
    {
        _model.Update(gameTime);
        _camera.Update(gameTime);

        base.Update(gameTime);
    }

    /// <summary>
    ///     This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public override void Draw(GameTime gameTime)
    {
        Game.Background = Color.CornflowerBlue;

        _model.Draw(Matrix.Identity, _camera.View, _camera.Projection);

        base.Draw(gameTime);
    }

    /// <summary>
    ///     Processes a change in the animation selected.
    /// </summary>
    /// <param name="animation">The animation to play.</param>
    private void OnAnimationChange(AnimationType animation)
    {
        // Load the Model that has an animation clip it in.
        _animation = new AnimatedModel(_modelAnimationFileNames[Convert.ToInt32(animation)]);
        _animation.LoadContent(Game.Content);

        // Obtain the clip we want to play.
        // I'm using an absolute index, because XNA 4.0 won't allow you to have more than one animation associated with a Model, anyway.
        // It would be easy to add code to look up the clip by name and to index it by name in the Model.
        var clip = _animation.Clips[0];

        // And play the clip.
        var player = _model.PlayClip(clip);
        player.Looping = true;
    }

    /// <summary>
    ///     UnloadContent will be called once per game and is the place to unload all content.
    /// </summary>
    protected override void UnloadContent()
    {
        // Unload any non ContentManager content here.
    }
}