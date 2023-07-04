///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - BSP
// Author: Craig Sniffen
// Copyright (c) 2006-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAQ3Lib.Q3BSP
{
    public struct VertexPosition : IVertexType
    {
        public Vector3 Position;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
        );

        public static int SizeInBytes = sizeof(float) * 3;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }

    class Q3BSPSkybox
    {
        VertexPosition[] vertices;
        short[] indices;
        VertexDeclaration vertexDeclaration;

        Q3BSPShaderManager shaderManager;

        public Q3BSPSkybox(float length, int tessNumber, GraphicsDevice graphicsDevice, Q3BSPShaderManager shaderManager)
        {
            if (tessNumber < 2)
                tessNumber = 2;

            float halfLength = length / 2;

            int verticesOnEdges = tessNumber - 2;
            int verticesOnFaces = verticesOnEdges * verticesOnEdges;
            // # on corners + # on one edge * 12 + # on one face * 6
            int numberOfVertices = 8 + verticesOnEdges * 12 + verticesOnFaces * 6;

            int[,,] indexMatrices = new int[6, tessNumber, tessNumber];

            #region Build Skybox Geometry
            vertices = new VertexPosition[numberOfVertices];
            indices = new short[(tessNumber - 1) * (tessNumber - 1) * 2 * 3 * 6];
            vertices.Initialize();            

            // Top: 0-3-2-1     Bottom: 4-7-6-5
            // Front: 0-4-5-1   Back: 3-7-6-2
            // Left: 0-3-7-4    Right: 1-2-6-5

            #region Corners
            vertices[0].Position = new Vector3( halfLength, halfLength, halfLength);
            vertices[1].Position = new Vector3(-halfLength, halfLength, halfLength);
            vertices[2].Position = new Vector3(-halfLength, halfLength, -halfLength);
            vertices[3].Position = new Vector3( halfLength, halfLength, -halfLength);

            vertices[4].Position = new Vector3( halfLength, -halfLength, halfLength);
            vertices[5].Position = new Vector3(-halfLength, -halfLength, halfLength);
            vertices[6].Position = new Vector3(-halfLength, -halfLength, -halfLength);
            vertices[7].Position = new Vector3( halfLength, -halfLength, -halfLength);

            // Fill out index matrices for these corners
            int end = verticesOnEdges + 1;

            // Top
            indexMatrices[0, 0, 0] = 0;
            indexMatrices[0, 0, end] = 3;
            indexMatrices[0, end, end] = 2;
            indexMatrices[0, end, 0] = 1;
            // Front
            indexMatrices[1, 0, 0] = 0;
            indexMatrices[1, 0, end] = 4;
            indexMatrices[1, end, end] = 5;
            indexMatrices[1, end, 0] = 1;
            // Left
            indexMatrices[2, 0, 0] = 0;
            indexMatrices[2, 0, end] = 3;
            indexMatrices[2, end, end] = 7;
            indexMatrices[2, end, 0] = 4;

            // Bottom
            indexMatrices[3, 0, 0] = 4;
            indexMatrices[3, 0, end] = 7;
            indexMatrices[3, end, end] = 6;
            indexMatrices[3, end, 0] = 5;
            // Back
            indexMatrices[4, 0, 0] = 3;
            indexMatrices[4, 0, end] = 7;
            indexMatrices[4, end, end] = 6;
            indexMatrices[4, end, 0] = 2;
            // Right
            indexMatrices[5, 0, 0] = 1;
            indexMatrices[5, 0, end] = 2;
            indexMatrices[5, end, end] = 6;
            indexMatrices[5, end, 0] = 5;

            #endregion

            #region Edges
            BuildEdgeVertexPositions(0, 1, 8 + (0 * verticesOnEdges), tessNumber, length, ref indexMatrices);
            BuildEdgeVertexPositions(1, 2, 8 + (1 * verticesOnEdges), tessNumber, length, ref indexMatrices);
            BuildEdgeVertexPositions(3, 2, 8 + (2 * verticesOnEdges), tessNumber, length, ref indexMatrices);
            BuildEdgeVertexPositions(0, 3, 8 + (3 * verticesOnEdges), tessNumber, length, ref indexMatrices);

            BuildEdgeVertexPositions(0, 4, 8 + (4 * verticesOnEdges), tessNumber, length, ref indexMatrices);
            BuildEdgeVertexPositions(1, 5, 8 + (5 * verticesOnEdges), tessNumber, length, ref indexMatrices);
            BuildEdgeVertexPositions(2, 6, 8 + (6 * verticesOnEdges), tessNumber, length, ref indexMatrices);
            BuildEdgeVertexPositions(3, 7, 8 + (7 * verticesOnEdges), tessNumber, length, ref indexMatrices);

            BuildEdgeVertexPositions(4, 5, 8 + (8 * verticesOnEdges), tessNumber, length, ref indexMatrices);
            BuildEdgeVertexPositions(5, 6, 8 + (9 * verticesOnEdges), tessNumber, length, ref indexMatrices);
            BuildEdgeVertexPositions(7, 6, 8 + (10 * verticesOnEdges), tessNumber, length, ref indexMatrices);
            BuildEdgeVertexPositions(4, 7, 8 + (11 * verticesOnEdges), tessNumber, length, ref indexMatrices);
            #endregion

            #region Faces
            BuildFaceVertexPositions(0, 8 + (12 * verticesOnEdges) + (0 * verticesOnFaces), tessNumber, length, indexMatrices);
            BuildFaceVertexPositions(1, 8 + (12 * verticesOnEdges) + (1 * verticesOnFaces), tessNumber, length, indexMatrices);
            BuildFaceVertexPositions(2, 8 + (12 * verticesOnEdges) + (2 * verticesOnFaces), tessNumber, length, indexMatrices);

            BuildFaceVertexPositions(3, 8 + (12 * verticesOnEdges) + (3 * verticesOnFaces), tessNumber, length, indexMatrices);
            BuildFaceVertexPositions(4, 8 + (12 * verticesOnEdges) + (4 * verticesOnFaces), tessNumber, length, indexMatrices);
            BuildFaceVertexPositions(5, 8 + (12 * verticesOnEdges) + (5 * verticesOnFaces), tessNumber, length, indexMatrices);
            #endregion

            BuildIndexBuffer(indexMatrices);
            #endregion
            this.shaderManager = shaderManager;
        }

        private void BuildEdgeVertexPositions(int edgeHead, int edgeTail, int bufferPosition, int tessNumber, float length, ref int[,,] indexMatrices)
        {
            int direction;
            int verticesOnEdges = tessNumber - 2;
            float stepSize = (1.0f / (tessNumber - 1)) * length;

            //determine what direction the edge is along (0 = x, 1 = y, 2 = z)
            if (vertices[edgeHead].Position.X - vertices[edgeTail].Position.X > 0.001)
            {
                direction = 0;
            }
            else if (vertices[edgeHead].Position.Y - vertices[edgeTail].Position.Y > 0.001)
            {
                direction = 1;
            }
            else
            {
                direction = 2;
            }

            //build the positions for the vertices on these edges
            for (int i = 0; i < verticesOnEdges; i++)
            {
                float changeInPosition = (i + 1) * stepSize;
                vertices[bufferPosition + i].Position = vertices[edgeHead].Position;
                switch (direction)
                {
                    case 0: vertices[bufferPosition + i].Position.X -= changeInPosition; break;
                    case 1: vertices[bufferPosition + i].Position.Y -= changeInPosition; break;
                    case 2: vertices[bufferPosition + i].Position.Z -= changeInPosition; break;

                }
            }

            // Fill in the index matrices for this edge, each edge touches two faces
            int[] faces = new int[2];
            faces[0] = faces[1] = -1;

            #region Determine what faces touch this edge
            switch (edgeHead)
            {
                case 0: switch (edgeTail)
                    {
                        case 1: faces[0] = 0; faces[1] = 1; break;
                        case 3: faces[0] = 0; faces[1] = 2; break;
                        case 4: faces[0] = 1; faces[1] = 2; break;
                    }
                    break;
                case 1: switch (edgeTail)
                    {
                        case 2: faces[0] = 0; faces[1] = 5; break;
                        case 5: faces[0] = 1; faces[1] = 5; break;
                    }
                    break;
                case 2: faces[0] = 4; faces[1] = 5; break;
                case 3: switch (edgeTail)
                    {
                        case 2: faces[0] = 0; faces[1] = 4; break;
                        case 7: faces[0] = 2; faces[1] = 4; break;
                    }
                    break; 
                case 4: switch (edgeTail)
                    {
                        case 5: faces[0] = 1; faces[1] = 3; break;
                        case 7: faces[0] = 2; faces[1] = 3; break;
                    }
                    break;
                case 5: faces[0] = 3; faces[1] = 5; break;
                case 7: faces[0] = 4; faces[1] = 3; break;
            }
            #endregion

            for (int i = 0; i < verticesOnEdges; i++)
            {
                // Find the edge within the faces
                for (int j = 0; j < 2; j++)
                {
                    int endPosition = verticesOnEdges + 1;
                    int dx = 1, dy = 1;

                    int[] start = new int[2], end = new int[2];
                    start[0] = start[1] = end[0] = end[1] = -1;

                    #region Determine Start Position
                    if (indexMatrices[faces[j], 0, 0] == edgeHead)
                    {
                        start = new int[2] { 0, 0 };
                    }
                    else if (indexMatrices[faces[j], 0, endPosition] == edgeHead)
                    {
                        start = new int[2] { 0, endPosition };
                    }
                    else if (indexMatrices[faces[j], endPosition, endPosition] == edgeHead)
                    {
                        start = new int[2] { endPosition, endPosition };
                    }
                    else if (indexMatrices[faces[j], endPosition, 0] == edgeHead)
                    {
                        start = new int[2] { endPosition, 0 };
                    }
                    #endregion

                    #region Determine End Position
                    if (indexMatrices[faces[j], 0, 0] == edgeTail)
                    {
                        end = new int[2] { 0, 0 };
                    }
                    else if (indexMatrices[faces[j], 0, endPosition] == edgeTail)
                    {
                        end = new int[2] { 0, endPosition };
                    }
                    else if (indexMatrices[faces[j], endPosition, endPosition] == edgeTail)
                    {
                        end = new int[2] { endPosition, endPosition };
                    }
                    else if (indexMatrices[faces[j], endPosition, 0] == edgeTail)
                    {
                        end = new int[2] { endPosition, 0 };
                    }
                    #endregion

                    //Determine Direction
                    if (start[0] == end[0])
                        dx = 0;
                    else if (start[0] > end[0])
                        dx = -1;
                    if (start[1] == end[1])
                        dy = 0;
                    else if (start[1] > end[1])
                        dy = -1;

                    // Insert into the matrix
                    indexMatrices[faces[j], start[0] + (i + 1) * dx, start[1] + (i + 1) * dy] = bufferPosition + i;
                }
            }
        }

        private void BuildFaceVertexPositions(int faceNumber, int bufferPosition, int tessNumber, float length, int[,,] indexMatrices)
        {
            int planeDirection1 = -1;
            int planeDirection2 = -1;            
            int verticesOnEdges = tessNumber - 2;
            float stepSize = (1.0f / (tessNumber - 1)) * length;
            int vertex0 = indexMatrices[faceNumber, 0, 0];
            int vertex1 = indexMatrices[faceNumber, tessNumber - 1, 0];
            int vertex2 = indexMatrices[faceNumber, tessNumber - 1, tessNumber - 1];
            int vertex3 = indexMatrices[faceNumber, 0, tessNumber - 1];
            
            //determine what direction plane the edge is along (0 = x, 1 = y, 2 = z)
            if (vertices[vertex0].Position.X - vertices[vertex2].Position.X > 0.001)
            {
                planeDirection1 = 0;
            }
            if (vertices[vertex0].Position.Y - vertices[vertex2].Position.Y > 0.001)
            {
                if (planeDirection1 == -1)
                    planeDirection1 = 1;
                else
                    planeDirection2 = 1;
            }
            if (vertices[vertex0].Position.Z - vertices[vertex2].Position.Z > 0.001)
            {
                planeDirection2 = 2;
            }

            for (int i = 0; i < verticesOnEdges; i++)
            {
                float changeInPlaneDirection1 = (i + 1) * stepSize;

                for (int j = 0; j < verticesOnEdges; j++)
                {
                    float changeInPlaneDirection2 = (j + 1) * stepSize;

                    vertices[bufferPosition + i * verticesOnEdges + j].Position = vertices[vertex0].Position;

                    switch (planeDirection1)
                    {
                        case 0: vertices[bufferPosition + i * verticesOnEdges + j].Position.X -= changeInPlaneDirection1; break;
                        case 1: vertices[bufferPosition + i * verticesOnEdges + j].Position.Y -= changeInPlaneDirection1; break;
                    }

                    switch (planeDirection2)
                    {
                        case 1: vertices[bufferPosition + i * verticesOnEdges + j].Position.Y -= changeInPlaneDirection2; break;
                        case 2: vertices[bufferPosition + i * verticesOnEdges + j].Position.Z -= changeInPlaneDirection2; break;
                    }

                    indexMatrices[faceNumber, i + 1, j + 1] = bufferPosition + i * verticesOnEdges + j;
                }
            }
        }

        private void BuildIndexBuffer(int[, ,] indexMatrices)
        {
            int tessNumber = indexMatrices.GetLength(2);
            int bufferPosition = 0;

            for (int face = 0; face < 6; face++)
            {
                for (int i = 0; i < tessNumber - 1; i++)
                {
                    for (int j = 0; j < tessNumber - 1; j++) 
                    {
                        // Faces 1-3 are wound backwards for some reason, too complicated to fix up there
                        if (face == 0 || face > 3)
                        {
                            indices[bufferPosition + 0] = (short)indexMatrices[face, i, j];
                            indices[bufferPosition + 1] = (short)indexMatrices[face, i, j + 1];
                            indices[bufferPosition + 2] = (short)indexMatrices[face, i + 1, j + 1];

                            indices[bufferPosition + 3] = (short)indexMatrices[face, i, j];
                            indices[bufferPosition + 4] = (short)indexMatrices[face, i + 1, j + 1];
                            indices[bufferPosition + 5] = (short)indexMatrices[face, i + 1, j];
                        }
                        else
                        {
                            indices[bufferPosition + 2] = (short)indexMatrices[face, i, j];
                            indices[bufferPosition + 1] = (short)indexMatrices[face, i, j + 1];
                            indices[bufferPosition + 0] = (short)indexMatrices[face, i + 1, j + 1];

                            indices[bufferPosition + 5] = (short)indexMatrices[face, i, j];
                            indices[bufferPosition + 4] = (short)indexMatrices[face, i + 1, j + 1];
                            indices[bufferPosition + 3] = (short)indexMatrices[face, i + 1, j];
                        
                        }
                        bufferPosition += 6;

                    }
                }
            }
        }

        public void Render(GraphicsDevice graphicsDevice, Matrix view, Matrix projection, GameTime gameTime)
        {

            Effect effect = shaderManager.GetSkyEffect(view, projection, gameTime);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives<VertexPosition>(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
            }
        }
    }
}
