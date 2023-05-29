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
    ///     Constructor for a bone object.
    /// </summary>
    /// <param name="name">The name of the bone.</param>
    /// <param name="bindTransform">The initial bind transform for the bone.</param>
    /// <param name="parent">A Parent for this bone.</param>
    public Bone(string name, Matrix bindTransform, Bone parent)
    {
        Name = name;
        Parent = parent;
        parent?.Children.Add(this);

        // I am not supporting scaling in animation in this example, so I extract the bind scaling from the bind transform and save it.
        BindScale = new Vector3(bindTransform.Right.Length(), bindTransform.Up.Length(),
            bindTransform.Backward.Length());

        bindTransform.Right /= BindScale.X;
        bindTransform.Up /= BindScale.Y;
        bindTransform.Backward /= BindScale.Y;
        BindTransform = bindTransform;

        // Set the skinning bind transform.
        // That is the inverse of the absolute transform in the bind pose.
        ComputeAbsoluteTransform();
        SkinTransform = Matrix.Invert(AbsoluteTransform);
    }

    /// <summary>
    ///     The bind scaling component extracted from the bind transform.
    /// </summary>
    private Vector3 BindScale { get; }

    /// <summary>
    ///     The bind transform is the transform for this bone as loaded from the original model. It's the base pose.
    ///     I do remove any scaling, though.
    /// </summary>
    private Matrix BindTransform { get; }

    /// <summary>
    ///     The Children of this bone.
    /// </summary>
    private List<Bone> Children { get; } = new();

    /// <summary>
    ///     The Parent bone or null for the root bone.
    /// </summary>
    private Bone Parent { get; }

    /// <summary>
    ///     The bone absolute transform.
    /// </summary>
    public Matrix AbsoluteTransform { get; set; } = Matrix.Identity;

    /// <summary>
    ///     The bone name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Any Rotation applied to the bone.
    /// </summary>
    private Quaternion Rotation { get; set; } = Quaternion.Identity;

    /// <summary>
    ///     Any scaling applied to the bone.
    /// </summary>
    private Vector3 Scale { get; } = Vector3.One;

    /// <summary>
    ///     Any Translation applied to the bone.
    /// </summary>
    private Vector3 Translation { get; set; } = Vector3.Zero;

    /// <summary>
    ///     Inverse of absolute bind transform for skinning.
    /// </summary>
    public Matrix SkinTransform { get; set; }

    /// <summary>
    ///     Compute the absolute transformation for this bone.
    /// </summary>
    public void ComputeAbsoluteTransform()
    {
        var transform = Matrix.CreateScale(Scale * BindScale) * Matrix.CreateFromQuaternion(Rotation) *
                        Matrix.CreateTranslation(Translation) * BindTransform;

        if (Parent != null)
            // This bone has a Parent bone.
        {
            AbsoluteTransform = transform * Parent.AbsoluteTransform;
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
        var setTo = m * Matrix.Invert(BindTransform);

        Translation = setTo.Translation;
        Rotation = Quaternion.CreateFromRotationMatrix(setTo);
    }
}