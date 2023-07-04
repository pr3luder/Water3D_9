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
    public class MD5Animation
    {
        int[] flags;
        MD5FrameSkeleton[] skeletons;

        BoundingBox maximumBounds;

        int numberOfFrames;
        float secondsPerFrame;     

        #region Properties
        public int NumberOfFrames
        {
            get { return numberOfFrames; }
        }
        public int NumberOfJoints
        {
            get { return skeletons[0].Joints.Length; }
        }
        public int[] Flags
        {
            get { return flags; }
        }
        public float SecondsPerFrame
        {
            get { return secondsPerFrame; }
        }
        public BoundingBox MaximumBounds
        {
            get { return maximumBounds; }
        }
        #endregion

        internal MD5Animation(float spf, BoundingBox maximumBounds, int[] flags, MD5FrameSkeleton[] frameSkeletons)
        {
            this.flags = flags;
            this.maximumBounds = maximumBounds;
            this.secondsPerFrame = spf;
            this.numberOfFrames = frameSkeletons.Length;
            this.skeletons = frameSkeletons;
        }

        public MD5FrameSkeleton GetFrameSkeleton(int index)
        {
            if (index >= skeletons.Length || index < 0)
                return null;

            return skeletons[index];
        }
    }
}
