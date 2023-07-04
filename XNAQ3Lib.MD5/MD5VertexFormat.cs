///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - MD5 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace XNAQ3Lib.MD5
{
    /// <summary>
    /// Vertex format that contains a V4 Position, V3 Normal, V2 TexCoord, V4 Indices, and a V4 Weights
    /// </summary>
    public struct MD5VertexFormat : IVertexType
    {
        public Vector4 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
        public Vector4 BoneIndices;
        public Vector4 BoneWeights;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
            new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(36, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0),
            new VertexElement(52, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0)
        );

        public MD5VertexFormat(Vector4 pos, Vector3 normal, Vector2 tc, Vector4 bi, Vector4 bw)
        {
            Position = pos;
            Normal = normal;
            TexCoord = tc;
            BoneIndices = bi;
            BoneWeights = bw;
        }

        public static int SizeInBytes = 68;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }

}
