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
    public enum MD5AnimationRepeatSetting
    {
        Looping,
        OneTime
    }

    class MD5AnimationTrack
    {
        MD5AnimationController parent;

        int trackPosition;
        MD5Animation animation;
        int bias;
        MD5AnimationRepeatSetting looping; 

        public int currentFrame;
        int nextFrame = 1;
        float timeInCurrentFrame;

        Vector3 rootPosition;
        Vector3 rootMovement;
        Vector3 lastRootPosition = Vector3.Zero;

        #region Properties
        public int Bias
        {
            get { return bias; }
        }
        public Vector3 RootMovement
        {
            get { return rootMovement; }
        }

        public Vector3 RootPosition
        {
            get { return animation.GetFrameSkeleton(0).RootPosition; }
        }
        public int NumberOfJoints
        {
            get { return animation.NumberOfJoints; }
        }
        #endregion
        
        public MD5AnimationTrack(int position, MD5AnimationController parent, MD5Animation animation, MD5AnimationRepeatSetting looping, int bias)
        {
            this.trackPosition = position;
            this.parent = parent;
            this.animation = animation;
            this.looping = looping;
            this.bias = bias;
        }

        public void Update(GameTime gameTime)
        {
            if (animation == null)
                return;

            timeInCurrentFrame += (float)gameTime.ElapsedGameTime.TotalSeconds;

            while (timeInCurrentFrame > animation.SecondsPerFrame)
            {
                timeInCurrentFrame -= animation.SecondsPerFrame;

                currentFrame++;
                nextFrame++;

                if (currentFrame == animation.NumberOfFrames)
                {
                    currentFrame = 0;
                }

                if (nextFrame == animation.NumberOfFrames)
                {
                    if (looping == MD5AnimationRepeatSetting.OneTime)
                    {
                        parent.SetAnimationTrack(trackPosition, null);
                        return;
                    }
                    lastRootPosition -= animation.GetFrameSkeleton(currentFrame).RootPosition - animation.GetFrameSkeleton(0).RootPosition;
                    currentFrame = 0;
                    nextFrame = 1;
                }
            }

            rootPosition = Vector3.Lerp(animation.GetFrameSkeleton(currentFrame).RootPosition, animation.GetFrameSkeleton(nextFrame).RootPosition, timeInCurrentFrame/animation.SecondsPerFrame);
            rootMovement =  rootPosition - lastRootPosition;
            lastRootPosition = rootPosition;
        }

        internal BoundingBox GetMaximumBounds()
        {
            BoundingBox maximumBounds = new BoundingBox();
            maximumBounds.Max = animation.MaximumBounds.Max - animation.GetFrameSkeleton(0).RootPosition;
            maximumBounds.Min = animation.MaximumBounds.Min - animation.GetFrameSkeleton(0).RootPosition;
            return maximumBounds;
        }

        internal MD5BoneTransforms GetBoneTransforms(int boneNumber)
        {
            MD5BoneTransforms boneTransforms = new MD5BoneTransforms();
            boneTransforms.Translation = Vector3.Lerp(animation.GetFrameSkeleton(currentFrame).Joints[boneNumber].Position, animation.GetFrameSkeleton(nextFrame).Joints[boneNumber].Position, timeInCurrentFrame / animation.SecondsPerFrame);
            boneTransforms.Rotation = Quaternion.Slerp(animation.GetFrameSkeleton(currentFrame).Joints[boneNumber].Rotation, animation.GetFrameSkeleton(nextFrame).Joints[boneNumber].Rotation, timeInCurrentFrame / animation.SecondsPerFrame);
            return boneTransforms;
        }

        internal int GetBoneFlags(int boneNumber)
        {
            return animation.Flags[boneNumber];
        }
    }
}
