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
    public class MD5AnimationController
    {
        const int MAX_NUMBER_OF_TRACKS = 4;
        int[] hiearchy;
        int numberOfBones;

        MD5AnimationTrack[] animationTracks = new MD5AnimationTrack[MAX_NUMBER_OF_TRACKS];
        //public Vector3 RootMovement;

        MD5BoneTransforms[] finalBoneTransforms;
        Matrix[] boneToWorldTransforms;

        #region Properties
        public int NumberOfActiveTracks
        {
            get
            {
                int i = 0;
                foreach (MD5AnimationTrack track in animationTracks)
                {
                    if (track != null)
                        i++;
                }
                return i;
            }
        }

        public Vector3 RootMovement
        {
            get
            {
                Vector3 dt = Vector3.Zero;
                float bias = 0;

                if (NumberOfActiveTracks == 0)
                    return dt;

                foreach (MD5AnimationTrack track in animationTracks)
                {
                    if (track != null)
                    {
                        bias += track.Bias;
                        dt = Vector3.Lerp(dt, track.RootMovement, track.Bias / bias);
                    }
                }

                return dt;
            }
        }

        public Vector3 RootPosition
        {
            get
            {
                Vector3 dt = Vector3.Zero;
                float bias = 0;

                if (NumberOfActiveTracks == 0)
                    return dt;

                foreach (MD5AnimationTrack track in animationTracks)
                {
                    if (track != null)
                    {
                        bias += track.Bias;
                        dt = Vector3.Lerp(dt, track.RootPosition, track.Bias / bias);
                    }
                }

                return dt;
            }
        }

        public Matrix[] BoneToWorldTransforms
        {
            get { return boneToWorldTransforms; }
        }
        #endregion

        public MD5AnimationController(int[] hiearchy)
        {
            this.hiearchy = hiearchy;
            this.numberOfBones = hiearchy.Length;
            finalBoneTransforms = new MD5BoneTransforms[this.numberOfBones];
            boneToWorldTransforms = new Matrix[this.numberOfBones];
        }

        public bool SetAnimationTrack(int position, MD5Animation animation, MD5AnimationRepeatSetting repeat, int bias)
        {
            // Input track position is out of bounds
            if (position < 0 || position >= MAX_NUMBER_OF_TRACKS)
            {
                return false;
            }


            // User wants to clear the animation on the input track
            if (animation == null)
            {
                animationTracks[position] = null;
                return true;
            }

            MD5AnimationTrack track = new MD5AnimationTrack(position, this, animation, repeat, bias);

            // The animation isn't compatable with this model
            if (numberOfBones != track.NumberOfJoints)
            {
                return false;
            }

            // Everything is a-ok
            animationTracks[position] = track;
            return true;
        }
        public bool SetAnimationTrack(int track, MD5Animation animation, MD5AnimationRepeatSetting repeat)
        {
            return SetAnimationTrack(track, animation, repeat, 1);
        }
        public bool SetAnimationTrack(int track, MD5Animation animation)
        {
             return SetAnimationTrack(track, animation, MD5AnimationRepeatSetting.Looping, 1);
        }

        public void AdvanceTime(GameTime gameTime)
        {
            int i;

            foreach (MD5AnimationTrack track in animationTracks)
            {
                if(track != null)
                {
                    track.Update(gameTime);
                }
            }

            for (i = 0; i < numberOfBones; i++)
            {
                finalBoneTransforms[i] = GetFinalBoneTransforms(i);
            }

        }

        public void BuildBoneToWorldTransforms(Matrix world)
        {
            int i;

            world = Matrix.CreateScale(-1, 1, 1) * world;

            // transform the root bone
            boneToWorldTransforms[0] = Matrix.CreateFromQuaternion(finalBoneTransforms[0].Rotation) * world;

            // transform the child bones
            for (i = 1; i < numberOfBones; i++)
            {
                boneToWorldTransforms[i] = finalBoneTransforms[i].Transform * world;// * boneToWorldTransforms[hiearchy[i]];
            }
        }

        internal BoundingBox GetBoundingBox()
        {
            BoundingBox bounds = new BoundingBox();
            bounds.Max = bounds.Min = Vector3.Zero;

            foreach (MD5AnimationTrack track in animationTracks)
            {
                if (track == null)
                {
                    continue;
                }

                BoundingBox trackBounds = track.GetMaximumBounds();

                bounds.Max.X = Math.Max(trackBounds.Max.X, bounds.Max.X);
                bounds.Max.Y = Math.Max(trackBounds.Max.Y, bounds.Max.Y);
                bounds.Max.Z = Math.Max(trackBounds.Max.Z, bounds.Max.Z);

                bounds.Min.X = Math.Min(trackBounds.Min.X, bounds.Min.X);
                bounds.Min.Y = Math.Min(trackBounds.Min.Y, bounds.Min.Y);
                bounds.Min.Z = Math.Min(trackBounds.Min.Z, bounds.Min.Z);

                if (NumberOfActiveTracks == 1)
                {
                    break;
                }
            }

            return bounds;
        }

        public BoundingBox GetMaximumBounds()
        {
            BoundingBox bounds = new BoundingBox();

            foreach (MD5AnimationTrack track in animationTracks)
            {
                if (track == null)
                {
                    continue;
                }

                BoundingBox trackBounds = track.GetMaximumBounds();
                bounds.Max.X = Math.Max(trackBounds.Max.X, bounds.Max.X);
                bounds.Max.Y = Math.Max(trackBounds.Max.Y, bounds.Max.Y);
                bounds.Max.Z = Math.Max(trackBounds.Max.Z, bounds.Max.Z);

                bounds.Min.X = Math.Min(trackBounds.Min.X, bounds.Min.X);
                bounds.Min.Y = Math.Min(trackBounds.Min.Y, bounds.Min.Y);
                bounds.Min.Z = Math.Min(trackBounds.Min.Z, bounds.Min.Z);

                if (NumberOfActiveTracks == 1)
                {
                    break;
                }
            }

            return bounds;
        }

        public MD5BoneTransforms GetFinalBoneTransforms(int boneIndex)
        {
            MD5BoneTransforms trackBT;
            int trackFlags;
            int totalBias = 0;
            int bufferedBias = 0;
            bool boneExplicitlyAnimated = false;
            MD5BoneTransforms bt = new MD5BoneTransforms();
            MD5BoneTransforms bufferBT = new MD5BoneTransforms();
            int numberOfActiveTracks = NumberOfActiveTracks;

            bt.Translation = bufferBT.Translation = Vector3.Zero;
            bt.Rotation = bufferBT.Rotation = Quaternion.Identity;

            foreach (MD5AnimationTrack track in animationTracks)
            {
                if (track != null)
                {
                    trackBT = track.GetBoneTransforms(boneIndex);
                    trackFlags = track.GetBoneFlags(boneIndex);

                    if (numberOfActiveTracks == 1)
                    {
                        return trackBT;
                    }

                    bufferedBias += track.Bias;
                    
                    bufferBT.Translation = Vector3.Lerp(bufferBT.Translation, trackBT.Translation, (float)track.Bias / (float)bufferedBias);
                    bufferBT.Rotation = Quaternion.Slerp(bufferBT.Rotation, trackBT.Rotation, (float)track.Bias / (float)bufferedBias);

                    if (trackFlags != 0)
                    {
                        boneExplicitlyAnimated = true;
                        totalBias += track.Bias;
                        bt.Translation = Vector3.Lerp(bt.Translation, trackBT.Translation, (float)track.Bias / (float)totalBias);
                        bt.Rotation = Quaternion.Slerp(bt.Rotation, trackBT.Rotation, (float)track.Bias / (float)totalBias);
                    } 
                }
            }

            if (!boneExplicitlyAnimated)
            {
                return bufferBT;
            }

            return bt;
        }
    }
}
