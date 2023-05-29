using Microsoft.Xna.Framework;
using TGC.MonoGame.Samples.Animations.DataTypes;

namespace TGC.MonoGame.Samples.Animations.Models;

/// <summary>
///     Information about a bone we are animating.
///     This class connects a bone in the clip to a bone in the model.
/// </summary>
public class BoneInfo
{
    /// <summary>
    ///     Constructor.
    /// </summary>
    public BoneInfo(DataTypes.Bone bone)
    {
        ClipBone = bone;
        SetKeyframes();
        SetPosition(0);
    }

    /// <summary>
    ///     Bone in a model that this keyframe bone is assigned to.
    /// </summary>
    private Bone AssignedBone { get; set; }

    /// <summary>
    ///     The current keyframe. Our position is a time such that the we are greater than or equal to this keyframe's time and
    ///     less than the next keyframes time.
    /// </summary>
    private int CurrentKeyframe { get; set; }

    /// <summary>
    ///     We are at a location between Keyframe1 and Keyframe2 such that Keyframe1's time is less than or equal to the
    ///     current position.
    /// </summary>
    public Keyframe Keyframe1 { get; set; }

    /// <summary>
    ///     Second keyframe value.
    /// </summary>
    public Keyframe Keyframe2 { get; set; }

    /// <summary>
    ///     Current animation Rotation.
    /// </summary>
    private Quaternion Rotation { get; set; }

    /// <summary>
    ///     Current animation Translation.
    /// </summary>
    public Vector3 Translation { get; set; }

    /// <summary>
    ///     We are not Valid until the Rotation and Translation are set.
    ///     If there are no keyframes, we will never be Valid.
    /// </summary>
    public bool Valid { get; set; }

    /// <summary>
    ///     The bone in the actual animation clip.
    /// </summary>
    public DataTypes.Bone ClipBone { get; }

    /// <summary>
    ///     Set the bone based on the supplied position value.
    /// </summary>
    public void SetPosition(float position)
    {
        var keyframes = ClipBone.Keyframes;
        if (keyframes.Count == 0)
        {
            return;
        }

        // If our current position is less that the first keyframe we move the position backward until we get to the right keyframe.
        while (position < Keyframe1.Time && CurrentKeyframe > 0)
        {
            // We need to move backwards in time.
            CurrentKeyframe--;
            SetKeyframes();
        }

        // If our current position is greater than the second keyframe we move the position forward until we get to the right keyframe.
        while (position >= Keyframe2.Time && CurrentKeyframe < ClipBone.Keyframes.Count - 2)
        {
            // We need to move forwards in time.
            CurrentKeyframe++;
            SetKeyframes();
        }

        if (Keyframe1 == Keyframe2)
        {
            // Keyframes are equal.
            Rotation = Keyframe1.Rotation;
            Translation = Keyframe1.Translation;
        }
        else
        {
            // Interpolate between keyframes.
            var t = (float)((position - Keyframe1.Time) / (Keyframe2.Time - Keyframe1.Time));
            Rotation = Quaternion.Slerp(Keyframe1.Rotation, Keyframe2.Rotation, t);
            Translation = Vector3.Lerp(Keyframe1.Translation, Keyframe2.Translation, t);
        }

        Valid = true;
        if (AssignedBone == null)
        {
            return;
        }

        // Send to the model.
        // Make it a matrix first.
        var m = Matrix.CreateFromQuaternion(Rotation);
        m.Translation = Translation;
        AssignedBone.SetCompleteTransform(m);
    }

    /// <summary>
    ///     Set the keyframes to a Valid value relative to the current keyframe.
    /// </summary>
    private void SetKeyframes()
    {
        if (ClipBone.Keyframes.Count > 0)
        {
            Keyframe1 = ClipBone.Keyframes[CurrentKeyframe];
            Keyframe2 = CurrentKeyframe == ClipBone.Keyframes.Count - 1
                ? Keyframe1
                : ClipBone.Keyframes[CurrentKeyframe + 1];
        }
        else
        {
            // If there are no keyframes, set both to null.
            Keyframe1 = null;
            Keyframe2 = null;
        }
    }

    /// <summary>
    ///     Assign this bone to the correct bone in the model.
    /// </summary>
    public void SetModel(AnimatedModel model)
    {
        // Find this bone.
        AssignedBone = model.FindBone(ClipBone.Name);
    }
}