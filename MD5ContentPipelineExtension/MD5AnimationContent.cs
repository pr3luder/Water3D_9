///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - MD5 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using XNAQ3Lib.MD5;

namespace MD5ContentPipelineExtension
{
    public class MD5AnimationContent
    {
        int[] flags;
        MD5FrameSkeleton[] skeletons;

        public MD5Bounds[] Bounds;
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
        public MD5FrameSkeleton[] FrameSkeletons
        {
            get { return skeletons;  }
        }
        public BoundingBox MaximumBounds
        {
            set { maximumBounds = value; }
            get { return maximumBounds; }
        }
        #endregion

        public MD5AnimationContent(string filename)
        {
            LoadAnimation(filename);
        }

        public MD5AnimationContent(float spf, BoundingBox maximumBounds, int[] flags, MD5FrameSkeleton[] frameSkeletons)
        {
            this.flags = flags;
            this.maximumBounds = maximumBounds;
            secondsPerFrame = spf;
            numberOfFrames = frameSkeletons.Length;
            skeletons = frameSkeletons;
        }

        public MD5FrameSkeleton GetFrameSkeleton(int index)
        {
            if (index >= skeletons.Length || index < 0)
                return null;

            return skeletons[index];
        }

        public void LoadAnimation(string filename)
        {
            int i, j;
            int version;
            string animFilename = filename;
            int numberOfFrames;
            int numberOfJoints;
            int numberOfAnimatedComponents;
            animFilename = filename;
            string line;
            string[] split;

            MD5Hierarchy[] hierarchy;
            MD5Bounds[] bounds;
            MD5Frame[] baseFrames;
            MD5Frame[] frames;

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("File " + filename + " not found in directory " + Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + ".");
            }

            FileStream fs = new FileStream(filename, FileMode.Open);
            StreamReader reader = new StreamReader(fs);

            #region LINE 1 - MD5Version
            line = MD5MeshContent.ReadLine(reader);
            if (!line.StartsWith("MD5Version"))
            {
                throw new Exception("File " + filename + "is not a valid .md5anim.");
            }
            version = int.Parse(line.Substring(10));
            #endregion

            #region LINE 2 - commandline (discard)
            line = MD5MeshContent.ReadLine(reader);
            #endregion

            #region LINE 3 - Number Of Frames
            line = MD5MeshContent.LookForLine(reader, "numFrames");
            numberOfFrames = int.Parse(line.Substring(10));
            #endregion

            #region LINE 4 - Number Of Joints
            line = MD5MeshContent.LookForLine(reader, "numJoints");
            numberOfJoints = int.Parse(line.Substring(10));
            #endregion

            #region LINE 5 - Frames per Second
            line = MD5MeshContent.LookForLine(reader, "frameRate");
            secondsPerFrame = 1.0f / int.Parse(line.Substring(10));
            #endregion

            #region LINE 6 - Number Of AnimatedComponents
            line = MD5MeshContent.LookForLine(reader, "numAnimatedComponents");
            numberOfAnimatedComponents = int.Parse(line.Substring(22));
            #endregion

            #region CHUNK - Hierarchy
            line = MD5MeshContent.LookForLine(reader, "hierarchy");
            hierarchy = new MD5Hierarchy[numberOfJoints];
            flags = new int[numberOfJoints];

            for (i = 0; i < numberOfJoints; i++)
            {
                MD5Hierarchy joint = new MD5Hierarchy();
                line = MD5MeshContent.ReadLine(reader);

                if (line.StartsWith("}"))
                {
                    throw new Exception("MD5Anim " + animFilename + " is invalid: Hierarchy chuck has missing joints.");
                }

                split = line.Split(new char[] { '"', ' ' });
                for (j = 0; j < split.Length; j++)
                {
                    split[j] = split[j].Trim();
                }

                joint.Name = split[1];
                joint.Parent = int.Parse(split[2]);
                joint.Flags = int.Parse(split[3]);
                joint.StartIndex = int.Parse(split[4]);

                hierarchy[i] = joint;
                flags[i] = joint.Flags;
            }
            #endregion

            #region CHUNK - Bounds
            line = MD5MeshContent.LookForLine(reader, "bounds");
            bounds = new MD5Bounds[numberOfFrames];

            for (i = 0; i < numberOfFrames; i++)
            {
                MD5Bounds bounding = new MD5Bounds();
                line = MD5MeshContent.ReadLine(reader);

                if (line.StartsWith("}"))
                {
                    throw new Exception("MD5Anim " + animFilename + " is invalid: Bounds chuck has missing frames.");
                }

                split = line.Split(new char[] { '(', ' ', ')' });
                for (j = 0; j < split.Length; j++)
                {
                    split[j] = split[j].Trim();
                }

                bounding.Minimum = new Vector3(float.Parse(split[2]), float.Parse(split[4]), float.Parse(split[3]));
                bounding.Maximum = new Vector3(float.Parse(split[9]), float.Parse(split[11]), float.Parse(split[10]));

                bounds[i] = bounding;
            }
            this.Bounds = bounds;
            #endregion

            #region CHUNK - BaseFrame
            line = MD5MeshContent.LookForLine(reader, "baseframe");
            baseFrames = new MD5Frame[numberOfJoints];

            for (i = 0; i < numberOfJoints; i++)
            {
                MD5Frame baseFrame = new MD5Frame();
                baseFrame.Data = new float[6];
                line = MD5MeshContent.ReadLine(reader);

                if (line.StartsWith("}"))
                {
                    throw new Exception("MD5Anim " + animFilename + " is invalid: Bounds chuck has missing frames.");
                }

                split = line.Split(new char[] { '(', ' ', ')' });
                for (j = 0; j < split.Length; j++)
                {
                    split[j] = split[j].Trim();
                }
                baseFrame.Data[0] = float.Parse(split[2]);
                baseFrame.Data[1] = float.Parse(split[3]);
                baseFrame.Data[2] = float.Parse(split[4]);

                baseFrame.Data[3] = float.Parse(split[9]);
                baseFrame.Data[4] = float.Parse(split[10]);
                baseFrame.Data[5] = float.Parse(split[11]);

                baseFrames[i] = baseFrame;
            }
            #endregion

            #region CHUNKS - Frame Data
            frames = new MD5Frame[numberOfFrames];
            for (i = 0; i < frames.Length; i++)
            {
                string chunk = "";
                MD5Frame frame = new MD5Frame();
                frame.Data = new float[numberOfAnimatedComponents];

                line = MD5MeshContent.LookForLine(reader, "frame " + i.ToString() + " {");

                while (!line.Contains("}"))
                {
                    if (reader.EndOfStream)
                    {
                        throw new Exception("MD5Anim " + animFilename + " is invalid: Frame chunk " + i + " is malformed.");
                    }

                    line = MD5MeshContent.ReadLine(reader);

                    if (!line.Contains("}"))
                        chunk += line + ' ';
                    else
                        chunk = chunk.Substring(0, chunk.Length - 1);
                }
                StringReader sr = new StringReader(chunk);

                split = chunk.Split(new char[] { ' ' });

                for (j = 0; j < frame.Data.Length; j++)
                {
                    frame.Data[j] = float.Parse(split[j]);
                }

                frames[i] = frame;

            }
            #endregion

            reader.Dispose();
            fs.Dispose();

            // If every joint's flag field is set to 63, recalculate the flags, as the flags were probably improperly exported
            if (numberOfAnimatedComponents == hierarchy.Length * 6)
            {
                flags = RecalculateFlags(numberOfJoints, baseFrames, frames);
            }

            ComputeFrameSkeletons(hierarchy, baseFrames, frames);
        }

        /// <summary>
        /// Tests to see what components of a joint are actually animated, and returns an int array containing this data
        /// </summary>
        static int[] RecalculateFlags(int numberOfJoints, MD5Frame[] baseframes, MD5Frame[] frames)
        {
            int i, j;
            int[] flags = new int[numberOfJoints];
            const float EPSILON = 0.00001f;

            for (i = 0; i < numberOfJoints; i++)
            {
                int flag = 0;

                for (j = 0; j < frames.Length; j++)
                {
                    // Tx
                    if (((flag & 1) != 1) && (Math.Abs(Math.Abs(frames[j].Data[i * 6 + 0]) - Math.Abs(baseframes[i].Data[0])) > EPSILON))
                        flag += 1;
                    // Tz
                    if (((flag & 2) != 2) && (Math.Abs(Math.Abs(frames[j].Data[i * 6 + 1]) - Math.Abs(baseframes[i].Data[1])) > EPSILON))
                        flag += 2;
                    // Ty
                    if (((flag & 4) != 4) && (Math.Abs(Math.Abs(frames[j].Data[i * 6 + 2]) - Math.Abs(baseframes[i].Data[2])) > EPSILON))
                        flag += 4;

                    // Rx
                    if (((flag & 8) != 8) && (Math.Abs(Math.Abs(frames[j].Data[i * 6 + 3]) - Math.Abs(baseframes[i].Data[3])) > EPSILON))
                        flag += 8;
                    // Rz
                    if (((flag & 16) != 16) && (Math.Abs(Math.Abs(frames[j].Data[i * 6 + 4]) - Math.Abs(baseframes[i].Data[4])) > EPSILON))
                        flag += 16;
                    // Ry
                    if (((flag & 32) != 32) && (Math.Abs(Math.Abs(frames[j].Data[i * 6 + 5]) - Math.Abs(baseframes[i].Data[5])) > EPSILON))
                        flag += 32;

                }

                flags[i] = flag;
            }

            return flags;
        }

        void ComputeFrameSkeletons(MD5Hierarchy[] hierarchy, MD5Frame[] baseFrames, MD5Frame[] frames)
        {
            int i, j;
            int numberOfJoints = hierarchy.Length;
            numberOfFrames = frames.Length;
           
            skeletons = new MD5FrameSkeleton[numberOfFrames];

            for (i = 0; i < numberOfFrames; i++)
            {
                skeletons[i] = new MD5FrameSkeleton();
                skeletons[i].Joints = new MD5Joint[numberOfJoints];

                for (j = 0; j < numberOfJoints; j++)
                {
                    int flags = hierarchy[j].Flags;
                    int dataIndex = hierarchy[j].StartIndex;
                    MD5Joint joint = new MD5Joint();

                    joint.Name = hierarchy[j].Name;
                    joint.Parent = hierarchy[j].Parent;
                    joint.Position = new Vector3(baseFrames[j].Data[0],baseFrames[j].Data[2], baseFrames[j].Data[1]);
                    joint.Rotation = new Quaternion(baseFrames[j].Data[3], baseFrames[j].Data[5], baseFrames[j].Data[4], ComputeW(baseFrames[j].Data[3], baseFrames[j].Data[5], baseFrames[j].Data[4]));

                    #region Replace Joint components with data from the Frame
                    if ((flags & 1) == 1)
                    {
                        joint.Position.X = frames[i].Data[dataIndex];
                        dataIndex++;
                    }
                    if ((flags & 2) == 2)
                    {
                        joint.Position.Z = frames[i].Data[dataIndex];
                        dataIndex++;
                    }
                    if ((flags & 4) == 4)
                    {
                        joint.Position.Y = frames[i].Data[dataIndex];
                        dataIndex++;
                    }

                    if ((flags & 8) == 8)
                    {
                        joint.Rotation.X = frames[i].Data[dataIndex]; 
                        dataIndex++;
                    }
                    if ((flags & 16) == 16)
                    {
                        joint.Rotation.Z = frames[i].Data[dataIndex]; 
                        dataIndex++;
                    }
                    if ((flags & 32) == 32)
                    {
                        joint.Rotation.Y = frames[i].Data[dataIndex]; 
                        dataIndex++;
                    }

                    joint.Rotation = new Quaternion(joint.Rotation.X, joint.Rotation.Y, joint.Rotation.Z, ComputeW(joint.Rotation.X, joint.Rotation.Y, joint.Rotation.Z));

                    if (joint.Parent == -1)
                    {
                        Vector3 scale;

                        Matrix m = Matrix.CreateFromQuaternion(joint.Rotation) * Matrix.CreateTranslation(joint.Position);
                        m *= Matrix.CreateScale(-1, 1, 1);

                        m.Decompose(out scale, out joint.Rotation, out joint.Position);
                    }
                    #endregion

                    #region Tranform by the parent joint's data
                    if (hierarchy[j].Parent >= 0)
                    {
                        Vector3 rotatedPosition = Vector3.Transform(joint.Position, skeletons[i].Joints[hierarchy[j].Parent].Rotation);
                        Quaternion concatenatedRotation = Quaternion.Concatenate(joint.Rotation, skeletons[i].Joints[hierarchy[j].Parent].Rotation);

                        joint.Position = skeletons[i].Joints[hierarchy[j].Parent].Position + rotatedPosition;
                        joint.Rotation = Quaternion.Normalize(concatenatedRotation);
                    }

                    // If this is the root bone, strip out the position data and store it to be used later
                    else
                    {
                        skeletons[i].RootPosition = joint.Position;
                        joint.Position = Vector3.Zero;//new Vector3(baseFrames[0].Data[0], baseFrames[0].Data[2], baseFrames[0].Data[1]);
                    }
                    #endregion

                    skeletons[i].Joints[j] = joint;
                }
            }
        }

        /// <summary>
        /// Computes the missing W component of a Quaternion given the normalized x, y, and z components.
        /// </summary>
        public static float ComputeW(float x, float y, float z)
        {
            float w = 1.0f - x * x - y * y - z * z;

            if (w < 0.0f)
            {
                w = 0.0f;
            }
            else
            {
                w = (float)Math.Sqrt(w);
            }

            return w;
        }
    }
}
