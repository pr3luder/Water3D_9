///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - MD5 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAQ3Lib.MD5
{
    public class MD5Mesh
    {
        string filename;
        MD5Joint[] joints;
        MD5Submesh[] submeshes;

        Matrix[] inverseBindPoseTransforms;

        #region Properties
        public string Filename
        {
            get { return filename; }
        }
        public MD5Submesh[] Submeshes
        {
            get { return submeshes; }
        }
        public MD5Joint[] Joints
        {
            get { return joints; }
        }
        public Matrix[] InverseBindPoseTransforms
        {
            get { return inverseBindPoseTransforms; }
        }

        public int[] Hierarchy
        {
            get
            {
                int[] hiearchy = new int[joints.Length];

                for(int i = 0; i < hiearchy.Length; i++)
                {
                    hiearchy[i] = joints[i].Parent;
                }

                return hiearchy;
            }
        }
        #endregion

        public MD5Mesh(string filename, MD5Joint[] inJoints, MD5Submesh[] inMeshes)
        {
            this.filename = filename;
            this.joints = inJoints;
            this.submeshes = inMeshes;

            BuildInverseBindPoseTransforms();
        }

        void BuildInverseBindPoseTransforms()
        {
            int i;
            inverseBindPoseTransforms = new Matrix[joints.Length];

            //Calculate the root inverse transform
            inverseBindPoseTransforms[0] = Matrix.Invert(joints[0].Transform);

            for (i = 1; i < inverseBindPoseTransforms.Length; i++)
            {
                Matrix transform = joints[i].Transform;// *joints[joints[i].Parent].Transform;
                inverseBindPoseTransforms[i] = Matrix.Invert(transform);
            }
        }
    }
}
