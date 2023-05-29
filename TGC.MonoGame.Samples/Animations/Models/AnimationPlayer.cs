using Microsoft.Xna.Framework;
using TGC.MonoGame.Samples.Animations.DataTypes;

namespace TGC.MonoGame.Samples.Animations.Models;

/// <summary>
///     Animation Clip player.
///     It maps an animation Clip onto a Model.
/// </summary>
public class AnimationPlayer
{
    /// <summary>
    ///     Current position in time in the clip.
    /// </summary>
    private float _position;

    /// <summary>
    ///     Constructor for the animation player.
    ///     It makes the association between a Clip and a Model and sets up for playing.
    /// </summary>
    public AnimationPlayer(AnimationClip clip, AnimatedModel model)
    {
        Clip = clip;
        Model = model;

        // Create the bone information classes.
        BonesCount = clip.Bones.Count;
        BonesInfo = new BoneInfo[BonesCount];

        for (var b = 0; b < BonesInfo.Length; b++)
        {
            // Create it.
            BonesInfo[b] = new BoneInfo(clip.Bones[b]);

            // Assign it to a Model bone.
            BonesInfo[b].SetModel(model);
        }

        Rewind();
    }

    /// <summary>
    ///     The number of bones.
    /// </summary>
    private int BonesCount { get; }

    /// <summary>
    ///     We maintain a BoneInfo class for each bone.
    ///     This class does most of the work in playing the animation.
    /// </summary>
    private BoneInfo[] BonesInfo { get; }

    /// <summary>
    ///     The Clip we are playing.
    /// </summary>
    private AnimationClip Clip { get; }

    /// <summary>
    ///     A Model this animation is assigned to.
    ///     It will play on that Model.
    /// </summary>
    private AnimatedModel Model { get; }

    /// <summary>
    ///     The Looping option.
    ///     Set to true if you want the animation to loop back at the end.
    /// </summary>
    public bool Looping { get; set; }

    /// <summary>
    ///     Current Position in time in the Clip.
    /// </summary>
    private float Position
    {
        get => _position;
        set
        {
            if (value > Duration)
            {
                value = Duration;
            }

            _position = value;
            foreach (var bone in BonesInfo)
            {
                bone.SetPosition(_position);
            }
        }
    }

    /// <summary>
    ///     The Clip duration.
    /// </summary>
    public float Duration => (float)Clip.Duration;

    /// <summary>
    ///     Reset back to time zero.
    /// </summary>
    public void Rewind()
    {
        Position = 0;
    }

    /// <summary>
    ///     Update the Clip Position.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        Position += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (Looping && Position >= Duration)
        {
            Position = 0;
        }
    }
}