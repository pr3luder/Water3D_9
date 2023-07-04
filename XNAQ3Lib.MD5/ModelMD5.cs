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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using XNAQ3Lib;

namespace XNAQ3Lib.MD5
{
    public class ModelMD5
    {
        GraphicsDevice graphicsDevice;
       
        MD5Mesh mesh;
        MD5AnimationController animationController;

        BoundingBox bounds;

        MD5VertexFormat[][] vertices;
        short[][] indices;

        bool drawBoundingBox = false;
        Color boundingBoxColor = Color.Red;
        VertexPositionColor[] boundingBoxVertices;
        DynamicVertexBuffer boundingBoxVertexBuffer;
        IndexBuffer boundingBoxIndexBuffer;

        Effect modelEffect;
        Effect boundingBoxEffect;

        Matrix world;
        Vector3 worldPosition = Vector3.Zero;
        Vector3 changeInWorldPositionThisFrame = Vector3.Zero;

        RasterizerState rStateWireFrame;
        RasterizerState rStateSolid;
        RasterizerState rStateCullNoneWireFrame;

        float rotation = 0.0f;

        #region Properties
        public BoundingBox Bounds
        {
            get { return bounds; }
        }
        public bool UseMaximumBounds
        {
            get;
            set;
        }
        public MD5Submesh[] Meshes
        {
            get { return mesh.Submeshes; }
        }
        public MD5Joint[] Joints
        {
            get { return mesh.Joints; }
        }
        public bool DrawBoundingBox
        {
            set { drawBoundingBox = value; }
            get { return drawBoundingBox; }
        }
        public MD5AnimationController AnimationController
        {
            get { return animationController; }
        }
        public Matrix[] FinalBoneTransforms
        {
            get 
            {
                int i;
                Matrix[] finalBoneTransforms = new Matrix[mesh.Joints.Length];
                Matrix[] inverseBindPoseTransforms = mesh.InverseBindPoseTransforms;
                Matrix[] boneToWorldTransforms = animationController.BoneToWorldTransforms;

                for (i = 0; i < finalBoneTransforms.Length; i++)
                {
                    finalBoneTransforms[i] = inverseBindPoseTransforms[i] * boneToWorldTransforms[i];
                }

                return finalBoneTransforms; 
            }
        }
        public Vector3 WorldPosition
        {
            set { worldPosition = value; }
            get { return worldPosition; }
        }
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        public Vector3 ChangeInWorldPositionThisFrame
        {
            get { return changeInWorldPositionThisFrame; }
        }
        #endregion

        public ModelMD5(Game game, MD5Mesh meshFile)
        {
            ResourceContentManager rm = new ResourceContentManager(game.Services, XNAQ3Lib.MD5.Resources.MD5Resources.ResourceManager);

            mesh = meshFile;
            graphicsDevice = game.GraphicsDevice;
            animationController = new MD5AnimationController(mesh.Hierarchy);
            UseMaximumBounds = true;

            modelEffect = rm.Load<Effect>("SkinnedModelEffect");
            boundingBoxEffect= rm.Load<Effect>("BoundingBoxEffect");

            vertices = new MD5VertexFormat[mesh.Submeshes.Length][];
            indices = new short[mesh.Submeshes.Length][];
            MD5Logger logger = new MD5Logger(game.Content.RootDirectory + "\\" + meshFile.Filename + ".txt");

            for (int i = 0; i < mesh.Submeshes.Length; i++)
            {
                vertices[i] = new MD5VertexFormat[mesh.Submeshes[i].Vertices.Length];
                if (System.IO.File.Exists(game.Content.RootDirectory + @"\" + mesh.Submeshes[i].Shader + ".xnb"))
                {
                    mesh.Submeshes[i].Texture = game.Content.Load<Texture2D>(mesh.Submeshes[i].Shader);
                }
                else
                {
                    logger.WriteLine("Missing texture: " + mesh.Submeshes[i].Shader);
                }
            }
            SetIndices();
            SetVertices();

            rStateWireFrame = new RasterizerState() { FillMode = FillMode.WireFrame };
            rStateSolid = new RasterizerState() { FillMode = FillMode.Solid };
            rStateCullNoneWireFrame = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };

            SetBoundingBoxVerticesAndIndices(graphicsDevice);
        }

        private void SetVertices()
        {
            int i, j, k;

            for (i = 0; i < mesh.Submeshes.Length; i++)
            {
                for (j = 0; j < vertices[i].Length; j++)
                {
                    MD5VertexFormat vert = new MD5VertexFormat();
                    MD5Vertex input = mesh.Submeshes[i].Vertices[j];

                    vert.Position = new Vector4(BuildVertexPosition(i, j), 1.0f);
                    vert.Normal = Vector3.Zero;
                    vert.TexCoord = input.TexCoord;
                    vert.BoneIndices = Vector4.Zero;
                    vert.BoneWeights = Vector4.Zero;

                    // Determine the boneIndices and boneWeights, if there aren't four bones effecting this joint the default zeros will work
                    vert.BoneIndices.X = mesh.Submeshes[i].Weights[input.FirstWeight].Joint;
                    vert.BoneWeights.X = mesh.Submeshes[i].Weights[input.FirstWeight].Weight;

                    if (input.NumberOfWeights > 1)
                    {
                        vert.BoneIndices.Y = mesh.Submeshes[i].Weights[input.FirstWeight + 1].Joint;
                        vert.BoneWeights.Y = mesh.Submeshes[i].Weights[input.FirstWeight + 1].Weight;
                    }

                    if (input.NumberOfWeights > 2)
                    {
                        vert.BoneIndices.Z = mesh.Submeshes[i].Weights[input.FirstWeight + 2].Joint;
                        vert.BoneWeights.Z = mesh.Submeshes[i].Weights[input.FirstWeight + 2].Weight;
                    }

                    if (input.NumberOfWeights > 3)
                    {
                        vert.BoneIndices.W = mesh.Submeshes[i].Weights[input.FirstWeight + 3].Joint;
                        vert.BoneWeights.W = mesh.Submeshes[i].Weights[input.FirstWeight + 3].Weight;
                    }

                    vertices[i][j] = vert;
                }

                for (j = 0; j < mesh.Submeshes[i].NumberOfTriangles; j++)
                {
                    int index0, index1, index2;

                    index0 = indices[i][3 * j];
                    index1 = indices[i][3 * j + 1];
                    index2 = indices[i][3 * j + 2];

                    Vector3 side1 = BuildVertexPosition(i, index0) - BuildVertexPosition(i, index1);
                    Vector3 side2 = BuildVertexPosition(i, index0) - BuildVertexPosition(i, index2);
                    Vector3 normal = Vector3.Cross(side1, side2);

                    vertices[i][index0].Normal += normal;
                    vertices[i][index1].Normal += normal;
                    vertices[i][index2].Normal += normal;
                }

                for (j = 0; j < mesh.Submeshes[i].Vertices.Length; j++)
                {
                    MD5Vertex vertex = mesh.Submeshes[i].Vertices[j];
                    Vector3 normal = Vector3.Zero;

                    for (k = 0; k < vertex.NumberOfWeights; k++)
                    {
                        MD5Weight weight = mesh.Submeshes[i].Weights[vertex.FirstWeight + k];
                        normal = Vector3.Transform(normal, Quaternion.Inverse(mesh.Joints[weight.Joint].Rotation));

                        vertices[i][j].Normal += normal * weight.Weight;
                    }
                }
            }

        }

        private void SetIndices()
        {
            int i, j;

            for (i = 0; i < indices.Length; i++)
            {
                indices[i] = new short[mesh.Submeshes[i].NumberOfTriangles * 3];

                for (j = 0; j < mesh.Submeshes[i].NumberOfTriangles; j++)
                {
                    indices[i][3 * j] = (short)mesh.Submeshes[i].Triangles[j].Indices[0];
                    indices[i][3 * j + 1] = (short)mesh.Submeshes[i].Triangles[j].Indices[1];
                    indices[i][3 * j + 2] = (short)mesh.Submeshes[i].Triangles[j].Indices[2];
                }
            }

        }

        private void SetBoundingBoxVerticesAndIndices(GraphicsDevice device)
        {
            boundingBoxVertices = new VertexPositionColor[8];
            boundingBoxVertices.Initialize();
            short[] bbIndices = new short[36];

            boundingBoxVertices[0] = new VertexPositionColor(Vector3.Zero, boundingBoxColor);
            boundingBoxVertices[1] = new VertexPositionColor(Vector3.Zero, boundingBoxColor);
            boundingBoxVertices[2] = new VertexPositionColor(Vector3.Zero, boundingBoxColor);
            boundingBoxVertices[3] = new VertexPositionColor(Vector3.Zero, boundingBoxColor);

            boundingBoxVertices[4] = new VertexPositionColor(Vector3.Zero, boundingBoxColor);
            boundingBoxVertices[5] = new VertexPositionColor(Vector3.Zero, boundingBoxColor);
            boundingBoxVertices[6] = new VertexPositionColor(Vector3.Zero, boundingBoxColor);
            boundingBoxVertices[7] = new VertexPositionColor(Vector3.Zero, boundingBoxColor);

            #region Set Indices
            // TOP
            bbIndices[0] = 0;
            bbIndices[1] = 1;
            bbIndices[2] = 0;

            bbIndices[3] = 1;
            bbIndices[4] = 2;
            bbIndices[5] = 1;

            bbIndices[6] = 2;
            bbIndices[7] = 3;
            bbIndices[8] = 2;

            bbIndices[9] = 3;
            bbIndices[10] = 0;
            bbIndices[11] = 3;

            // SIDES
            bbIndices[21] = 0;
            bbIndices[22] = 4;
            bbIndices[23] = 0;
            
            bbIndices[18] = 1;
            bbIndices[19] = 5;
            bbIndices[20] = 1;
            
            bbIndices[15] = 2;
            bbIndices[16] = 6;
            bbIndices[17] = 2;

            bbIndices[12] = 3;
            bbIndices[13] = 7;
            bbIndices[14] = 3;            
            
            // BOTTOM
            bbIndices[24] = 4;
            bbIndices[25] = 5;
            bbIndices[26] = 4;

            bbIndices[27] = 5;
            bbIndices[28] = 6;
            bbIndices[29] = 5;

            bbIndices[30] = 6;
            bbIndices[31] = 7;
            bbIndices[32] = 6;

            bbIndices[33] = 7;
            bbIndices[34] = 4;
            bbIndices[35] = 7;
            #endregion
            boundingBoxVertexBuffer = new DynamicVertexBuffer(device, typeof(VertexPositionColor), 8, BufferUsage.WriteOnly);

            boundingBoxIndexBuffer = new IndexBuffer(device, typeof(short), 36, BufferUsage.WriteOnly);
            boundingBoxIndexBuffer.SetData<short>(bbIndices);
        }

        Vector3 BuildVertexPosition(int meshIndex, int vertexIndex)
        {
            int i;
            MD5Vertex vertex = mesh.Submeshes[meshIndex].Vertices[vertexIndex];
            Vector3 position = Vector3.Zero;

            for (i = 0; i < vertex.NumberOfWeights; i++)
            {
                MD5Weight weight = mesh.Submeshes[meshIndex].Weights[vertex.FirstWeight + i];
                position += Vector3.Transform(weight.Position, mesh.Joints[weight.Joint].Transform) * weight.Weight;
            }

            return position;
        }

        public void Update(GameTime gameTime)
        {
            animationController.AdvanceTime(gameTime);
            
            while (rotation > MathHelper.TwoPi)
            {
                rotation -= MathHelper.TwoPi;
            }

            changeInWorldPositionThisFrame = Vector3.Transform(animationController.RootMovement, Matrix.CreateRotationY(rotation));
        }

        public void FinalizeMovement(Vector3 finalChangeInWorldPositionThisFrame)
        {
            worldPosition += finalChangeInWorldPositionThisFrame;
            world = Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(worldPosition);

            animationController.BuildBoneToWorldTransforms(world);

            if (drawBoundingBox)
            {
                UpdateBoundingBoxVertices();
            }
        }


        private void UpdateBoundingBoxVertices()
        {
            if (UseMaximumBounds)
            {
                bounds = animationController.GetMaximumBounds();
            }
            else
            {
                bounds = animationController.GetBoundingBox();
            }

            boundingBoxVertices[0].Position = bounds.Max;
            boundingBoxVertices[1].Position = new Vector3(bounds.Max.X, bounds.Max.Y, bounds.Min.Z);
            boundingBoxVertices[2].Position = new Vector3(bounds.Min.X, bounds.Max.Y, bounds.Min.Z);
            boundingBoxVertices[3].Position = new Vector3(bounds.Min.X, bounds.Max.Y, bounds.Max.Z);

            boundingBoxVertices[4].Position = new Vector3(bounds.Max.X, bounds.Min.Y, bounds.Max.Z);
            boundingBoxVertices[5].Position = new Vector3(bounds.Max.X, bounds.Min.Y, bounds.Min.Z);
            boundingBoxVertices[6].Position = bounds.Min;
            boundingBoxVertices[7].Position = new Vector3(bounds.Min.X, bounds.Min.Y, bounds.Max.Z);

            for (int i = 0; i < boundingBoxVertices.Length; i++)
            {
                boundingBoxVertices[i].Position = Vector3.Transform(boundingBoxVertices[i].Position, Matrix.CreateRotationY(rotation));
            }

            bounds.Min = bounds.Max = boundingBoxVertices[0].Position;

            foreach (VertexPositionColor vertex in boundingBoxVertices)
            {
                bounds.Max.X = Math.Max(bounds.Max.X, vertex.Position.X);
                bounds.Max.Y = Math.Max(bounds.Max.Y, vertex.Position.Y);
                bounds.Max.Z = Math.Max(bounds.Max.Z, vertex.Position.Z);

                bounds.Min.X = Math.Min(bounds.Min.X, vertex.Position.X);
                bounds.Min.Y = Math.Min(bounds.Min.Y, vertex.Position.Y);
                bounds.Min.Z = Math.Min(bounds.Min.Z, vertex.Position.Z);
            }

            boundingBoxVertices[0].Position = bounds.Max;
            boundingBoxVertices[1].Position = new Vector3(bounds.Max.X, bounds.Max.Y, bounds.Min.Z);
            boundingBoxVertices[2].Position = new Vector3(bounds.Min.X, bounds.Max.Y, bounds.Min.Z);
            boundingBoxVertices[3].Position = new Vector3(bounds.Min.X, bounds.Max.Y, bounds.Max.Z);

            boundingBoxVertices[4].Position = new Vector3(bounds.Max.X, bounds.Min.Y, bounds.Max.Z);
            boundingBoxVertices[5].Position = new Vector3(bounds.Max.X, bounds.Min.Y, bounds.Min.Z);
            boundingBoxVertices[6].Position = bounds.Min;
            boundingBoxVertices[7].Position = new Vector3(bounds.Min.X, bounds.Min.Y, bounds.Max.Z);

            boundingBoxVertexBuffer.SetData<VertexPositionColor>(boundingBoxVertices);

            return;
        }

        public void Draw(Texture2D texture, Matrix view, Matrix projection)
        {
            graphicsDevice.RasterizerState = rStateSolid;
            
            modelEffect.Parameters["View"].SetValue(view);
            modelEffect.Parameters["Projection"].SetValue(projection);
            modelEffect.Parameters["Bones"].SetValue(FinalBoneTransforms);
            for (int i = 0; i < mesh.Submeshes.Length; i++)
            {
                if (vertices[i].Length == 0)
                {
                    continue;
                }

                 modelEffect.Parameters["Texture"].SetValue(mesh.Submeshes[i].Texture);
                 foreach (EffectPass pass in modelEffect.CurrentTechnique.Passes)
                 {
                     pass.Apply();
                     graphicsDevice.DrawUserIndexedPrimitives<MD5VertexFormat>(PrimitiveType.TriangleList, vertices[i], 0, vertices[i].Length, indices[i], 0, mesh.Submeshes[i].NumberOfTriangles);
                }
            }
            if (drawBoundingBox)
            {
                DrawBounds(view, projection);
            }
        }

        public void DrawBounds(Matrix view, Matrix projection)
        {
            graphicsDevice.Indices = boundingBoxIndexBuffer;
            graphicsDevice.RasterizerState = rStateWireFrame;
            graphicsDevice.SetVertexBuffer(boundingBoxVertexBuffer);
            boundingBoxEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(worldPosition));
            boundingBoxEffect.Parameters["View"].SetValue(view);
            boundingBoxEffect.Parameters["Projection"].SetValue(projection);
            boundingBoxEffect.Parameters["Color"].SetValue(new Vector4(1, 0, 0, 1));
            foreach (EffectPass pass in boundingBoxEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 12);
            }
            graphicsDevice.RasterizerState = rStateSolid;
                
        }
    }
}
