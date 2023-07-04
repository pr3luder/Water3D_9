#region File Description
//-----------------------------------------------------------------------------
// ParticleVertex.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
#endregion

namespace Water3D.VertexDeclarations
{
    /// <summary>
    /// Custom vertex structure for drawing point sprite particles.
    /// </summary>
    public struct ParticleVertex : IVertexType
    {
        // Stores which corner of the particle quad this vertex represents.
        public Vector2 Corner;
        // Stores the starting position of the particle.
        public Vector3 Position;
        // Stores the starting velocity of the particle.
        public Vector3 Velocity;
        // Four random values, used to make each particle look slightly different.
        public Color Random;
        // The time (in seconds) at which this particle was created.
        public float Time;


        // Describe the layout of this vertex structure.
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
            new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(32, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(36, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0)
        );

        public ParticleVertex(Vector2 c, Vector3 pos, Vector3 v, Color r, float t)
        {
            Corner = c;
            Position = pos;
            Velocity = v;
            Random = r;
            Time = t;
        }
        public static int SizeInBytes = 40;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

    }
}
