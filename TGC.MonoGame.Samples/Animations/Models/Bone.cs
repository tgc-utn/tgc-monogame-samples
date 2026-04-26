using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.Samples.Animations.Models;

/// <summary>
///     Bones in this model are represented by this class, which allows a bone to have more detail associate with it.
///     This class allows you to manipulate the local coordinate system for objects by changing the scaling, Translation,
///     and Rotation.
///     These are independent of the bind transformation originally supplied for the model.
///     So, the actual transformation for a bone is the product of the:
///     * Scaling.
///     * Bind scaling (scaling removed from the bind transform).
///     * Rotation.
///     * Translation.
///     * Bind Transformation.
///     * Parent Absolute Transformation.
/// </summary>
public class Bone
{
    /// <summary>
    ///     The bind scaling component extracted from the bind transform.
    /// </summary>
    private readonly Vector3 _bindScale;

    /// <summary>
    ///     The bind transform is the transform for this bone as loaded from the original model. It's the base pose.
    ///     I do remove any scaling, though.
    /// </summary>
    private readonly Matrix _bindTransform;

    /// <summary>
    ///     The Children of this bone.
    /// </summary>
    private readonly List<Bone> _children = new();

    /// <summary>
    ///     The Parent bone or null for the root bone.
    /// </summary>
    private readonly Bone _parent;

    /// <summary>
    ///     Any scaling applied to the bone.
    /// </summary>
    private readonly Vector3 _scale = Vector3.One;

    /// <summary>
    ///     Any Rotation applied to the bone.
    /// </summary>
    private Quaternion _rotation = Quaternion.Identity;

    /// <summary>
    ///     Any Translation applied to the bone.
    /// </summary>
    private Vector3 _translation = Vector3.Zero;

    /// <summary>
    ///     Constructor for a bone object.
    /// </summary>
    /// <param name="name">The name of the bone.</param>
    /// <param name="bindTransform">The initial bind transform for the bone.</param>
    /// <param name="parent">A Parent for this bone.</param>
    public Bone(string name, Matrix bindTransform, Bone parent)
    {
        Name = name;
        _parent = parent;
        parent?._children.Add(this);

        // In this example, scaling in animation is not supported. The bind scaling is separated from the bind transform and saved.
        _bindScale = new Vector3(bindTransform.Right.Length(), bindTransform.Up.Length(),
            bindTransform.Backward.Length());

        bindTransform.Right /= _bindScale.X;
        bindTransform.Up /= _bindScale.Y;
        bindTransform.Backward /= _bindScale.Y;
        _bindTransform = bindTransform;

        // Set the skinning bind transform.
        // That is the inverse of the absolute transform in the bind pose.
        ComputeAbsoluteTransform();
        SkinTransform = Matrix.Invert(AbsoluteTransform);
    }

    /// <summary>
    ///     The bone absolute transform.
    /// </summary>
    public Matrix AbsoluteTransform { get; set; } = Matrix.Identity;

    /// <summary>
    ///     The bone name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Inverse of absolute bind transform for skinning.
    /// </summary>
    public Matrix SkinTransform { get; set; }

    /// <summary>
    ///     Compute the absolute transformation for this bone.
    /// </summary>
    public void ComputeAbsoluteTransform()
    {
        var transform = Matrix.CreateScale(_scale * _bindScale) * Matrix.CreateFromQuaternion(_rotation) *
                        Matrix.CreateTranslation(_translation) * _bindTransform;

        if (_parent != null)
            // This bone has a Parent bone.
        {
            AbsoluteTransform = transform * _parent.AbsoluteTransform;
        }
        else
            // The root bone.
        {
            AbsoluteTransform = transform;
        }
    }

    /// <summary>
    ///     This sets the Rotation and Translation such that the Rotation times the Translation times the bind after set equals
    ///     this matrix. This is used to set animation values.
    /// </summary>
    /// <param name="m">A matrix include Translation and Rotation</param>
    public void SetCompleteTransform(Matrix m)
    {
        var setTo = m * Matrix.Invert(_bindTransform);

        _translation = setTo.Translation;
        _rotation = Quaternion.CreateFromRotationMatrix(setTo);
    }
}