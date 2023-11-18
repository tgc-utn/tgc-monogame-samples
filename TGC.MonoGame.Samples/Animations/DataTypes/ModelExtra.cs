using System.Collections.Generic;

namespace TGC.MonoGame.Samples.Animations.DataTypes;

/// <summary>
///     Class that contains additional information attached to the model and shared with the runtime.
/// </summary>
public class ModelExtra
{
    /// <summary>
    ///     Animation clips associated with this model.
    /// </summary>
    public List<AnimationClip> Clips { get; set; } = new();

    /// <summary>
    ///     The bone indices for the skeleton associated with any skinned model.
    /// </summary>
    public List<int> Skeleton { get; set; } = new();
}