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
    ///     Bone in a model that this keyframe bone is assigned to.
    /// </summary>
    private Bone _assignedBone;

    /// <summary>
    ///     The current keyframe. Our position is a time such that the we are greater than or equal to this keyframe's time and
    ///     less than the next keyframes time.
    /// </summary>
    private int _currentKeyframe;

    /// <summary>
    ///     Current animation Rotation.
    /// </summary>
    private Quaternion _rotation;

    /// <summary>
    ///     Constructor.
    /// </summary>
    public BoneInfo(AnimationBone animationBone)
    {
        ClipAnimationBone = animationBone;
        SetKeyframes();
        SetPosition(0);
    }

    /// <summary>
    ///     We are at a location between KeyframeFrom and KeyframeTo such that KeyframeFrom time is less than or equal to the
    ///     current position.
    /// </summary>
    public Keyframe KeyframeFrom { get; set; }

    /// <summary>
    ///     Second keyframe value.
    /// </summary>
    public Keyframe KeyframeTo { get; set; }

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
    public AnimationBone ClipAnimationBone { get; }

    /// <summary>
    ///     Set the bone based on the supplied position value.
    /// </summary>
    public void SetPosition(float position)
    {
        var keyframes = ClipAnimationBone.Keyframes;
        if (keyframes.Count == 0)
        {
            return;
        }

        // If our current position is less that the first keyframe we move the position backward until we get to the right keyframe.
        while (position < KeyframeFrom.Time && _currentKeyframe > 0)
        {
            // We need to move backwards in time.
            _currentKeyframe--;
            SetKeyframes();
        }

        // If our current position is greater than the second keyframe we move the position forward until we get to the right keyframe.
        while (position >= KeyframeTo.Time && _currentKeyframe < ClipAnimationBone.Keyframes.Count - 2)
        {
            // We need to move forwards in time.
            _currentKeyframe++;
            SetKeyframes();
        }

        if (KeyframeFrom == KeyframeTo)
        {
            // Keyframes are equal.
            _rotation = KeyframeFrom.Rotation;
            Translation = KeyframeFrom.Translation;
        }
        else
        {
            // Interpolate between keyframes.
            var t = (float)((position - KeyframeFrom.Time) / (KeyframeTo.Time - KeyframeFrom.Time));
            _rotation = Quaternion.Slerp(KeyframeFrom.Rotation, KeyframeTo.Rotation, t);
            Translation = Vector3.Lerp(KeyframeFrom.Translation, KeyframeTo.Translation, t);
        }

        Valid = true;
        if (_assignedBone == null)
        {
            return;
        }

        // Send to the model.
        // Make it a matrix first.
        var m = Matrix.CreateFromQuaternion(_rotation);
        m.Translation = Translation;
        _assignedBone.SetCompleteTransform(m);
    }

    /// <summary>
    ///     Set the keyframes to a Valid value relative to the current keyframe.
    /// </summary>
    private void SetKeyframes()
    {
        if (ClipAnimationBone.Keyframes.Count > 0)
        {
            KeyframeFrom = ClipAnimationBone.Keyframes[_currentKeyframe];
            KeyframeTo = _currentKeyframe == ClipAnimationBone.Keyframes.Count - 1
                ? KeyframeFrom
                : ClipAnimationBone.Keyframes[_currentKeyframe + 1];
        }
        else
        {
            // If there are no keyframes, set both to null.
            KeyframeFrom = null;
            KeyframeTo = null;
        }
    }

    /// <summary>
    ///     Assign this bone to the correct bone in the model.
    /// </summary>
    public void SetModel(AnimatedModel model)
    {
        // Find this bone.
        _assignedBone = model.FindBone(ClipAnimationBone.Name);
    }
}