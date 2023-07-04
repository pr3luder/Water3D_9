///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - MD5 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace XNAQ3Lib.MD5
{
    public class MD5Submesh
    {
        public string Shader;
        public int NumberOfVertices;
        public int NumberOfTriangles;
        public int NumberOfWeights;

        public MD5Vertex[] Vertices;
        public MD5Triangle[] Triangles;
        public MD5Weight[] Weights;

        public Texture2D Texture;
    }
}
