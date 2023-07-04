///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - MD5 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace XNAQ3Lib.MD5
{
    public struct MD5Vertex
    {
        public float S;
        public float T;
        public int FirstWeight;
        public int NumberOfWeights;

        public Vector2 TexCoord
        {
            get { return new Vector2(S, T); }
        }
    }
}
