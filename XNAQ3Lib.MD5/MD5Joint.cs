///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - MD5 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using Microsoft.Xna.Framework;

namespace XNAQ3Lib.MD5
{
    public struct MD5Joint
    {
        public string Name;
        public int Parent;
        public Vector3 Position;
        public Quaternion Rotation;

        public Matrix Transform 
        {
            get
            {
                Matrix m = Matrix.CreateFromQuaternion(Rotation);
                m *= Matrix.CreateTranslation(Position);

                return m;
            }
        }
    }
}
