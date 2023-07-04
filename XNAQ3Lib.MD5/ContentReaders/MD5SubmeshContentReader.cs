///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - MD5 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// TODO: replace this with the type you want to read.
using TRead = XNAQ3Lib.MD5.MD5Submesh;

namespace XNAQ3Lib.MD5.ContentReaders
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class MD5SubmeshContentReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            TRead mesh = new TRead();
            mesh.Shader = input.ReadString();
            mesh.NumberOfVertices = input.ReadInt32();
            mesh.NumberOfTriangles = input.ReadInt32();
            mesh.NumberOfWeights = input.ReadInt32();

            mesh.Vertices = input.ReadObject<MD5Vertex[]>();
            mesh.Triangles = input.ReadObject<MD5Triangle[]>();
            mesh.Weights = input.ReadObject<MD5Weight[]>();

            return mesh;
        }
    }
}
