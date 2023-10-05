using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using TGC.MonoGame.Samples.Animations.DataTypes;

namespace TGC.MonoGame.Samples.Animations.PipelineExtension;

/// <summary>
///     This class extends the standard ModelProcessor to include code that extracts a skeleton, pulls any animations, and
///     does any necessary prep work to support animation and skinning.
/// </summary>
[ContentProcessor(DisplayName = "Animation Processor")]
public class AnimationProcessor : ModelProcessor
{
    private const float TinyLength = 1e-8f;
    private const float TinyCosAngle = 0.9999999f;

    /// <summary>
    ///     Bones lookup table, converts bone names to indices.
    /// </summary>
    private readonly Dictionary<string, int> _bones = new();

    /// <summary>
    ///     A dictionary so we can keep track of the clips by name.
    /// </summary>
    private readonly Dictionary<string, AnimationClip> _clips = new();

    /// <summary>
    ///     Extra content to associated with the model. This is where we put the stuff that is unique to this project.
    /// </summary>
    private readonly ModelExtra _modelExtra = new();

    /// <summary>
    ///     A lookup dictionary that remembers when we changes a material to skinned material.
    /// </summary>
    private readonly Dictionary<MaterialContent, SkinnedMaterialContent> _toSkinnedMaterial = new();

    /// <summary>
    ///     This will keep track of all of the bone transforms for a base pose.
    /// </summary>
    private Matrix[] _boneTransforms;

    /// <summary>
    ///     The model we are reading.
    /// </summary>
    private ModelContent _model;

    /// <summary>
    ///     The function to process a model from original content into model content for export.
    /// </summary>
    public override ModelContent Process(NodeContent input, ContentProcessorContext context)
    {
        // Skeleton Support.
        // Process the skeleton for skinned character animation.
        ProcessSkeleton(input);

        // Skinned Support.
        SwapSkinnedMaterial(input);

        // Base Model process.
        _model = base.Process(input, context);

        // Animation Support.
        ProcessAnimations(_model, input);

        // Add the extra content to the model.
        _model.Tag = _modelExtra;

        return _model;
    }

    /// <summary>
    ///     Process the skeleton in support of skeletal animation.
    /// </summary>
    private void ProcessSkeleton(NodeContent input)
    {
        // Find the skeleton.
        var skeleton = MeshHelper.FindSkeleton(input);

        if (skeleton == null)
        {
            return;
        }

        // We don't want to have to worry about different parts of the model being in different local coordinate systems, so let's just bake everything.
        FlattenTransforms(input, skeleton);

        // 3D Studio Max includes helper bones that end with "Nub" These are not part of the skinning system and can be discarded. TrimSkeleton removes them from the geometry.
        TrimSkeleton(skeleton);

        // Convert the hierarchy of nodes and bones into a list.
        var nodes = FlattenHierarchy(input);
        var bones = MeshHelper.FlattenSkeleton(skeleton);

        // Create a dictionary to convert a node to an index into the array of nodes.
        var nodeToIndex = new Dictionary<NodeContent, int>();
        for (var i = 0; i < nodes.Count; i++)
        {
            nodeToIndex[nodes[i]] = i;
        }

        // Now create the array that maps the bones to the nodes.
        foreach (var bone in bones)
        {
            _modelExtra.Skeleton.Add(nodeToIndex[bone]);
        }
    }

    /// <summary>
    ///     Convert a tree of nodes into a list of nodes in topological order.
    /// </summary>
    /// <param name="item">The root of the hierarchy.</param>
    private List<NodeContent> FlattenHierarchy(NodeContent item)
    {
        var nodes = new List<NodeContent>();
        nodes.Add(item);

        foreach (var child in item.Children)
        {
            FlattenHierarchy(nodes, child);
        }

        return nodes;
    }

    private void FlattenHierarchy(ICollection<NodeContent> nodes, NodeContent item)
    {
        nodes.Add(item);

        foreach (var child in item.Children)
        {
            FlattenHierarchy(nodes, child);
        }
    }

    /// <summary>
    ///     Bakes unwanted transforms into the model geometry, so everything ends up in the same coordinate system.
    /// </summary>
    private void FlattenTransforms(NodeContent node, BoneContent skeleton)
    {
        foreach (var child in node.Children)
        {
            // Don't process the skeleton, because that is special.
            if (child == skeleton)
            {
                continue;
            }

            // This is important: Don't bake in the transforms except for geometry that is part of a skinned mesh.
            if (IsSkinned(child))
            {
                FlattenAllTransforms(child);
            }
        }
    }

    /// <summary>
    ///     Recursively flatten all transforms from this node down.
    /// </summary>
    private void FlattenAllTransforms(NodeContent node)
    {
        // Bake the local transform into the actual geometry.
        MeshHelper.TransformScene(node, node.Transform);

        // Having baked it, we can now set the local coordinate system back to identity.
        node.Transform = Matrix.Identity;

        foreach (var child in node.Children)
        {
            FlattenAllTransforms(child);
        }
    }

    /// <summary>
    ///     3D Studio Max includes an extra help bone at the end of each IK chain that doesn't effect the skinning system and
    ///     is redundant as far as any game is concerned. This function looks for children who's name ends with "Nub" and
    ///     removes them from the hierarchy.
    /// </summary>
    /// <param name="skeleton">Root of the skeleton tree.</param>
    private void TrimSkeleton(NodeContent skeleton)
    {
        var toDelete = new List<NodeContent>();

        foreach (var child in skeleton.Children)
        {
            if (child.Name.EndsWith("Nub") || child.Name.EndsWith("Footsteps"))
            {
                toDelete.Add(child);
            }
            else
            {
                TrimSkeleton(child);
            }
        }

        foreach (var child in toDelete)
        {
            skeleton.Children.Remove(child);
        }
    }

    /// <summary>
    ///     Determine if a node is a skinned node, meaning it has bone weights associated with it.
    /// </summary>
    private bool IsSkinned(NodeContent node)
    {
        // It has to be a MeshContent node.
        if (node is MeshContent mesh)
            // In the geometry we have to find a vertex channel that has a bone weight collection.
        {
            foreach (var geometry in mesh.Geometry)
            {
                foreach (var vertexChannel in geometry.Vertices.Channels)
                {
                    if (vertexChannel is VertexChannel<BoneWeightCollection>)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    ///     If a node is skinned, we need to use the skinned model effect rather than basic effect. This function runs through
    ///     the geometry and finds the meshes that have bone weights associated and swaps in the skinned effect.
    /// </summary>
    private void SwapSkinnedMaterial(NodeContent node)
    {
        // It has to be a MeshContent node.
        if (node is MeshContent mesh)
            // In the geometry we have to find a vertex channel that has a bone weight collection.
        {
            foreach (var geometry in mesh.Geometry)
            {
                var swap = false;
                foreach (var vertexChannel in geometry.Vertices.Channels)
                {
                    if (vertexChannel is VertexChannel<BoneWeightCollection>)
                    {
                        swap = true;
                        break;
                    }
                }

                if (swap)
                {
                    if (_toSkinnedMaterial.ContainsKey(geometry.Material))
                    {
                        // We have already swapped it.
                        geometry.Material = _toSkinnedMaterial[geometry.Material];
                    }
                    else
                    {
                        var skinnedMaterial = new SkinnedMaterialContent();

                        // Copy over the data.
                        if (geometry.Material is BasicMaterialContent basicMaterial)
                        {
                            skinnedMaterial.Alpha = basicMaterial.Alpha;
                            skinnedMaterial.DiffuseColor = basicMaterial.DiffuseColor;
                            skinnedMaterial.EmissiveColor = basicMaterial.EmissiveColor;
                            skinnedMaterial.SpecularColor = basicMaterial.SpecularColor;
                            skinnedMaterial.SpecularPower = basicMaterial.SpecularPower;
                            skinnedMaterial.Texture = basicMaterial.Texture;
                        }

                        skinnedMaterial.WeightsPerVertex = 4;

                        _toSkinnedMaterial[geometry.Material] = skinnedMaterial;
                        geometry.Material = skinnedMaterial;
                    }
                }
            }
        }

        foreach (var child in node.Children)
        {
            SwapSkinnedMaterial(child);
        }
    }

    /// <summary>
    ///     Entry point for animation processing.
    /// </summary>
    private void ProcessAnimations(ModelContent model, NodeContent input)
    {
        // First build a lookup table so we can determine the index into the list of bones from a bone name.
        for (var i = 0; i < model.Bones.Count; i++)
        {
            _bones[model.Bones[i].Name] = i;
        }

        // For saving the bone transforms.
        _boneTransforms = new Matrix[model.Bones.Count];

        // Collect up all of the animation data.
        ProcessAnimationsRecursive(input);

        // Ensure there is always a clip, even if none is included in the FBX.
        // That way we can create poses using FBX files as one-frame animation clips.
        if (_modelExtra.Clips.Count == 0)
        {
            var clip = new AnimationClip();
            _modelExtra.Clips.Add(clip);

            var clipName = "Take 001";

            // Retain by name.
            _clips[clipName] = clip;

            clip.Name = clipName;
            foreach (var bone in model.Bones)
            {
                var clipBone = new AnimationBone();
                clipBone.Name = bone.Name;

                clip.Bones.Add(clipBone);
            }
        }

        // Ensure all animations have a first key frame for every bone.
        foreach (var clip in _modelExtra.Clips)
        {
            for (var b = 0; b < _bones.Count; b++)
            {
                var keyframes = clip.Bones[b].Keyframes;
                if (keyframes.Count == 0 || keyframes[0].Time > 0)
                {
                    var keyframe = new Keyframe();
                    keyframe.Time = 0;
                    keyframe.Transform = _boneTransforms[b];
                    keyframes.Insert(0, keyframe);
                }
            }
        }
    }

    /// <summary>
    ///     Recursive function that processes the entire scene graph, collecting up all of the animation data.
    /// </summary>
    private void ProcessAnimationsRecursive(NodeContent input)
    {
        // Look up the bone for this input channel.
        if (_bones.TryGetValue(input.Name, out var inputBoneIndex))
        {
            // Save the transform.
            _boneTransforms[inputBoneIndex] = input.Transform;
        }

        foreach (var animation in input.Animations)
        {
            // Do we have this animation before?
            var clipName = animation.Key;

            if (!_clips.TryGetValue(clipName, out var clip))
            {
                // Never seen before clip.
                clip = new AnimationClip();
                _modelExtra.Clips.Add(clip);

                // Retain by name.
                _clips[clipName] = clip;

                clip.Name = clipName;
                foreach (var bone in _model.Bones)
                {
                    var clipBone = new AnimationBone();
                    clipBone.Name = bone.Name;

                    clip.Bones.Add(clipBone);
                }
            }

            // Ensure the duration is always set.
            if (animation.Value.Duration.TotalSeconds > clip.Duration)
            {
                clip.Duration = animation.Value.Duration.TotalSeconds;
            }

            // For each channel, determine the bone and then process all of the keyframes for that bone.

            foreach (var channel in animation.Value.Channels)
            {
                // What is the bone index?
                if (!_bones.TryGetValue(channel.Key, out var boneIndex))
                {
                    continue; // Ignore if not a named bone.
                }

                // An animation is useless if it is for a bone not assigned to any meshes at all.
                if (UselessAnimationTest(boneIndex))
                {
                    continue;
                }

                // I'm collecting up in a linked list so we can process the data and remove redundant keyframes.
                var keyframes = new LinkedList<Keyframe>();
                foreach (var keyframe in channel.Value)
                {
                    // Keyframe transformation.
                    var transform = keyframe.Transform;

                    var newKeyframe = new Keyframe();
                    newKeyframe.Time = keyframe.Time.TotalSeconds;
                    newKeyframe.Transform = transform;

                    keyframes.AddLast(newKeyframe);
                }

                LinearKeyframeReduction(keyframes);
                foreach (var keyframe in keyframes)
                {
                    clip.Bones[boneIndex].Keyframes.Add(keyframe);
                }
            }
        }

        foreach (var child in input.Children)
        {
            ProcessAnimationsRecursive(child);
        }
    }

    /// <summary>
    ///     This function filters out keyframes that can be approximated well with linear interpolation.
    /// </summary>
    private void LinearKeyframeReduction(LinkedList<Keyframe> keyframes)
    {
        if (keyframes.Count < 3)
        {
            return;
        }

        var node = keyframes.First.Next;

        while (node != null)
        {
            var next = node.Next;
            if (next == null)
            {
                break;
            }

            // Determine nodes before and after the current node.
            var a = node.Previous.Value;
            var b = node.Value;
            var c = next.Value;

            var t = (float)((node.Value.Time - a.Time) / (next.Value.Time - a.Time));

            var translation = Vector3.Lerp(a.Translation, c.Translation, t);
            var rotation = Quaternion.Slerp(a.Rotation, c.Rotation, t);

            if ((translation - b.Translation).LengthSquared() < TinyLength &&
                Quaternion.Dot(rotation, b.Rotation) > TinyCosAngle)
            {
                keyframes.Remove(node);
            }

            node = next;
        }
    }

    /// <summary>
    ///     Discard any animation not assigned to a mesh or the skeleton.
    /// </summary>
    private bool UselessAnimationTest(int boneId)
    {
        // If any mesh is assigned to this bone, it is not useless.
        foreach (var mesh in _model.Meshes)
        {
            if (mesh.ParentBone.Index == boneId)
            {
                return false;
            }
        }

        // If this bone is in the skeleton, it is not useless.
        foreach (var b in _modelExtra.Skeleton)
        {
            if (boneId == b)
            {
                return false;
            }
        }

        // Otherwise, it is useless.
        return true;
    }
}