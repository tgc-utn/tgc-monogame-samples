#region File Description

//-----------------------------------------------------------------------------
// TeapotPatch.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion File Description

#region Using Statements

using System.Diagnostics;

#endregion Using Statements

namespace TGC.MonoGame.Samples.Geometries
{
    /// <summary>
    ///     The teapot model consists of 10 bezier patches. Each patch has 16 control points, plus a flag indicating whether it
    ///     should be mirrored in the Z axis as well as in X (all of the teapot is symmetrical from left to right, but only
    ///     some parts are symmetrical from front to back). The control points are stored as integer indices into the
    ///     TeapotControlPoints array.
    /// </summary>
    internal class TeapotPatch
    {
        /// <summary>
        ///     Creates a patch from its 16 control point indices.
        /// </summary>
        /// <param name="mirrorZ">Whether the patch is also mirrored in the Z axis.</param>
        /// <param name="indices">The 16 indices into the TeapotControlPoints array.</param>
        internal TeapotPatch(bool mirrorZ, int[] indices)
        {
            Debug.Assert(indices.Length == 16, "A teapot patch must have exactly 16 control point indices.");

            Indices = indices;
            MirrorZ = mirrorZ;
        }

        /// <summary>
        ///     Gets the 16 indices into the TeapotControlPoints array.
        /// </summary>
        internal int[] Indices { get; }

        /// <summary>
        ///     Gets a value indicating whether the patch is also mirrored in the Z axis.
        /// </summary>
        internal bool MirrorZ { get; }
    }
}
