///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - MD5 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using XNAQ3Lib.MD5;

namespace MD5ContentPipelineExtension
{
    public class MD5MeshContent
    {
        public string Filename;
        public MD5Joint[] Joints;
        public MD5Submesh[] Submeshes;
        public ModelContent CapsuleContent;

        public MD5MeshContent(string filename)
        {
            int i, j;
            string line;
            string[] split;

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
                throw new Exception("File " + filename + "is not a valid .md5mesh.");
            }
            int Version = int.Parse(line.Substring(10));
            #endregion

            #region LINE 2 - commandline (discard)
            line = MD5MeshContent.ReadLine(reader);
            #endregion

            #region LINE 3 - numJoints
            line = MD5MeshContent.LookForLine(reader, "numJoints ");
            int NumberOfJoints = int.Parse(line.Substring(10));
            #endregion

            #region LINE 4 - numMeshes
            line = MD5MeshContent.LookForLine(reader, "numMeshes ");
            int NumberOfMeshes = int.Parse(line.Substring(10));
            #endregion

            #region CHUNK - Joints
            line = MD5MeshContent.LookForLine(reader, "joints");
            MD5Joint[] joints = new MD5Joint[NumberOfJoints];
            for (i = 0; i < joints.Length; i++)
            {
                MD5Joint joint = new MD5Joint();
                line = MD5MeshContent.ReadLine(reader);

                if (line.StartsWith("}"))
                {
                    throw new Exception("MD5Mesh " + filename + " is invalid: Joints chuck has missing joints.");
                }

                //Line format: "name" parent ( x y z ) ( rX rY rZ )
                split = line.Split(new Char[] { '"', '"', '(', ')', '(', ')' });
                for (j = 0; j < split.Length; j++)
                {
                    split[j] = split[j].Trim();
                }

                if (split.Length != 7)
                {
                    throw new Exception("MD5Mesh " + filename + " is invalid: Joint " + i + " appears to be malformed. ");
                }

                joint.Name = split[1];
                joint.Parent = int.Parse(split[2]);

                // Position
                string[] pos = split[3].Split(' ', ' ');
                if (pos.Length != 3)
                {
                    throw new Exception("MD5Mesh " + filename + " is invalid: Joint " + i + "'s position data appears to be malformed. ");
                }

                joint.Position = new Vector3(float.Parse(pos[0]), float.Parse(pos[2]), float.Parse(pos[1]));

                // Rotation
                string[] rot = split[5].Split(' ', ' ');
                if (rot.Length != 3)
                {
                    throw new Exception("MD5Mesh " + filename + " is invalid: Joint " + i + "'s rotation data appears to be malformed. ");
                }
                float x, y, z, w;
                x = float.Parse(rot[0]);
                y = float.Parse(rot[2]);
                z = float.Parse(rot[1]);
                w = MD5AnimationContent.ComputeW(x, y, z);

                joint.Rotation = new Quaternion(x, y, z, w);

                joints[i] = joint;
            }
            #endregion

            #region CHUNKS - Meshes
            MD5Submesh[] submeshes = new MD5Submesh[NumberOfMeshes];
            for (i = 0; i < NumberOfMeshes; i++)
            {
                #region Pull the Mesh Chunk Out
                string chunk = "";
                line = MD5MeshContent.LookForLine(reader, "mesh {");

                while (!line.Contains("}"))
                {
                    if (reader.EndOfStream)
                    {
                        throw new Exception("MD5Mesh " + filename + " is invalid: Mesh chunk " + i + " is malformed.");
                    }

                    line = MD5MeshContent.ReadLine(reader);
                    chunk += line + '\n';
                }
                StringReader sr = new StringReader(chunk);
                #endregion

                MD5Submesh mesh = new MD5Submesh();

                #region mesh.Shader
                line = MD5MeshContent.LookForLine(sr, "shader");
                line = line.Substring(8);
                if (line.LastIndexOf('.') > 0)
                {
                    line = line.Substring(0, line.LastIndexOf('.'));
                }
                if (line.Length > 1 && line[0] == '/' && line[1] == '/')
                {
                    line = line.Substring(2);
                }
                if (line.Length > 1 && line[line.Length - 1] == '"')
                {
                    line = mesh.Shader.Substring(0, line.Length - 1);
                }

                mesh.Shader = line;
                #endregion

                line = MD5MeshContent.LookForLine(sr, "numverts");
                mesh.NumberOfVertices = int.Parse(line.Substring(9));
                mesh.Vertices = new MD5Vertex[mesh.NumberOfVertices];

                #region mesh.Vertices[j]
                for (j = 0; j < mesh.NumberOfVertices; j++)
                {
                    MD5Vertex vert;

                    line = MD5MeshContent.LookForLine(sr, "vert");
                    split = line.Split(new char[] { ' ', '(', ' ', ')', ' ' });

                    if (int.Parse(split[1]) != j)
                    {
                        throw new Exception("MD5Mesh " + filename + " is invalid: Vertex " + j + " in mesh chunk " + i + " is missing.");
                    }

                    vert.S = float.Parse(split[4]);
                    vert.T = float.Parse(split[5]);
                    vert.FirstWeight = int.Parse(split[8]);
                    vert.NumberOfWeights = int.Parse(split[9]);

                    mesh.Vertices[j] = vert;
                }
                #endregion

                line = MD5MeshContent.LookForLine(sr, "numtris");
                mesh.NumberOfTriangles = int.Parse(line.Substring(8));
                mesh.Triangles = new MD5Triangle[mesh.NumberOfTriangles];

                #region mesh.Triangles[j]
                for (j = 0; j < mesh.NumberOfTriangles; j++)
                {
                    MD5Triangle tri = new MD5Triangle();

                    line = MD5MeshContent.LookForLine(sr, "tri");
                    split = line.Split(new char[] { ' ' });

                    if (int.Parse(split[1]) != j)
                    {
                        throw new Exception("MD5Mesh " + filename + " is invalid: Triangle " + j + " in mesh chunk " + i + " is missing.");
                    }

                    tri.Indices = new int[] { int.Parse(split[2]), int.Parse(split[3]), int.Parse(split[4]) };

                    mesh.Triangles[j] = tri;
                }
                #endregion

                line = MD5MeshContent.LookForLine(sr, "numweights");
                mesh.NumberOfWeights = int.Parse(line.Substring(10));
                mesh.Weights = new MD5Weight[mesh.NumberOfWeights];

                #region mesh.Weights[j]
                for (j = 0; j < mesh.NumberOfWeights; j++)
                {
                    MD5Weight weight = new MD5Weight();

                    line = MD5MeshContent.LookForLine(sr, "weight");
                    split = line.Split(new char[] { ' ', '(', ')' });

                    if (int.Parse(split[1]) != j)
                    {
                        throw new Exception("MD5Mesh " + filename + " is invalid: Weight " + j + " in mesh chunk " + i + " is missing.");
                    }

                    weight.Joint = int.Parse(split[2]);
                    weight.Weight = float.Parse(split[3]);
                    weight.Position = new Vector3(float.Parse(split[6]), float.Parse(split[8]), float.Parse(split[7]));

                    mesh.Weights[j] = weight;
                }
                #endregion

                submeshes[i] = mesh;
            }
            #endregion

            this.Filename = filename.Substring(filename.LastIndexOf("Content") + 8);
            this.Joints = joints;
            this.Submeshes = submeshes;
        }

        #region Static Methods
        static internal Matrix MatrixInvertX
        {
            get
            {
                Matrix m = Matrix.Identity;
                m.M11 = -1.0f;
                return m;
            }
        }



        /// <summary>
        /// Advances the StreamReader until it finds a line that starts with the input string, and returns that line.
        /// </summary>
        public static string LookForLine(TextReader reader, string startsWith)
        {
            string line = ReadLine(reader);

            while (line != null)
            {
                if (line.StartsWith(startsWith))
                {
                    return line;
                }

                line = ReadLine(reader);
            }

            throw new Exception("MD5Mesh " + /*filename + */" is invalid: Cannot find line \"" + startsWith + "\".");
        }

        /// <summary>
        /// Reads the next line in the StreamReader and strips out comments and whitespace.
        /// </summary>
        public static string ReadLine(TextReader reader)
        {
            string line = reader.ReadLine();
            if (line == null)
                return line;

            line = line.Trim();

            int commentPosition = line.IndexOf("//");
            if (commentPosition > 0 && line[commentPosition - 1] != '"')
            {
                line = line.Substring(0, commentPosition - 1);
            }

            return line;
        }
        #endregion

    }
}
