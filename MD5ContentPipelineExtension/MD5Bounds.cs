///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - MD5 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using Microsoft.Xna.Framework;

namespace MD5ContentPipelineExtension
{
    public struct MD5Bounds
    {
        public Vector3 Minimum;
        public Vector3 Maximum;

        public MD5Bounds(Vector3 max, Vector3 min)
        {
            Maximum = max;
            Minimum = min;
        }

        public override string ToString()
        {
            return Minimum + "," + Maximum;
        }
    }
}
