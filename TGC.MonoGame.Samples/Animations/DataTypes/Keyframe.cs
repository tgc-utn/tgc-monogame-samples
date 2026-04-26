using Microsoft.Xna.Framework;

namespace TGC.MonoGame.Samples.Animations.DataTypes;

/// <summary>
///     An Keyframe is a rotation and Translation for a moment in time.
///     It would be easy to extend this to include scaling as well.
/// </summary>
public class Keyframe
{
    /// <summary>
    ///     The rotation for the bone.
    /// </summary>
    public Quaternion Rotation { get; set; }

    /// <summary>
    ///     The keyframe time.
    /// </summary>
    public double Time { get; set; }

    /// <summary>
    ///     The Translation for the bone.
    /// </summary>
    public Vector3 Translation { get; set; }

    public Matrix Transform
    {
        get => Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Translation);
        set
        {
            var transform = value;
            transform.Right = Vector3.Normalize(transform.Right);
            transform.Up = Vector3.Normalize(transform.Up);
            transform.Backward = Vector3.Normalize(transform.Backward);
            Rotation = Quaternion.CreateFromRotationMatrix(transform);
            Translation = transform.Translation;
        }
    }
}