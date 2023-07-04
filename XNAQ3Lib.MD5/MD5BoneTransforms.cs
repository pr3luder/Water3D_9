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
    public class MD5BoneTransforms
    {
        public Vector3 Translation;
        public Quaternion Rotation;

        public Matrix Transform
        {
            get
            {
                Matrix m = Matrix.CreateFromQuaternion(Rotation);
                m *= Matrix.CreateTranslation(Translation);
                return m;
            }
        }
    }
}
