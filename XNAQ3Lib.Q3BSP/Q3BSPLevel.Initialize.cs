///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - BSP
// Author: Aanand Narayanan and Craig Sniffen
// Copyright (c) 2006-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace XNAQ3Lib.Q3BSP
{
    public partial class Q3BSPLevel
    {
        const int skyboxTessellation = 8;
        const float skyboxSize = 4096.0f;

        /// <param name="shaderPath">Path to the directory that contains all compiled .shaders. Is relative to the Content.BasePath directory. Do not include a leading '/'.</param>
        public bool InitializeLevel(GraphicsDevice graphics, ContentManager content, string shaderPath, string quakePath)
        {
            bool bSuccess = true;

            lightMapManager = new Q3BSPLightMapManager();
            bSuccess = lightMapManager.GenerateLightMaps(lightMapData, graphics);

            if (bSuccess)
            {
                shaderManager = new Q3BSPShaderManager();
                shaderManager.ShaderPath = shaderPath;
                shaderManager.QuakePath = quakePath;
                bSuccess = shaderManager.LoadTextures(textureData, graphics, content);
            }

            if (bSuccess && shaderManager.SkyMaterial != null)
            {
                skybox = new Q3BSPSkybox(skyboxSize, skyboxTessellation, graphics, shaderManager);
            }

            if (bSuccess)
            {
                shaderManager.LightMapManager = lightMapManager;
            }

            levelInitialized = bSuccess;

            if (renderType == Q3BSPRenderType.StaticBuffer)
            {
                InitializeStatic(graphics);
            }

            else if (renderType == Q3BSPRenderType.BSPCulling)
            {
                Q3BSPFace[] tempFaces = new Q3BSPFace[faces.Length];
                faces.CopyTo(tempFaces, 0);

                System.Array.Sort(tempFaces, new Q3BSPFaceComparer());

                int indexCount = 0;
                int lastTextureIndex = 0;
                int lastLightMapIndex = 0;
                this.maximumNumberOfIndicesToDraw = 0;

                foreach (Q3BSPFace face in tempFaces)
                {
                    // The current index buffer is done and needs to be refreshed.
                    if ((face.TextureIndex != lastTextureIndex || face.LightMapIndex != lastLightMapIndex))
                    {
                        if (face.TextureIndex == 4)
                            indexCount += 0;
                        if (indexCount > maximumNumberOfIndicesToDraw)
                            maximumNumberOfIndicesToDraw = indexCount;
                        indexCount = 0;
                    }

                    lastTextureIndex = face.TextureIndex;
                    lastLightMapIndex = face.LightMapIndex;
                    indexCount += face.MeshVertexCount;
                }

                if (indexCount > maximumNumberOfIndicesToDraw)
                    maximumNumberOfIndicesToDraw = indexCount;
            }

            bspLogger.WriteLine("Level initialized: " + levelInitialized.ToString());
            return levelInitialized;
        }

        public void InitializeStatic(GraphicsDevice graphics)
        {
            List<short> indexList = new List<short>();
            List<short[]> indexBufferList = new List<short[]>();
            List<Vector2> textureAndLightMapList = new List<Vector2>();
            int lastTextureIndex = 0;
            int lastLightMapIndex = 0;

            Q3BSPFace[] tempFaces = new Q3BSPFace[faces.Length];
            faces.CopyTo(tempFaces, 0);

            System.Array.Sort(tempFaces, new Q3BSPFaceComparer());

            foreach (Q3BSPFace face in tempFaces)
            {
                // The current index buffer is done and needs to be refreshed.
                if ((face.TextureIndex != lastTextureIndex || face.LightMapIndex != lastLightMapIndex) && ((indexList.Count != 0 || indexList.Count > int.MaxValue)))
                {
                    indexBufferList.Add(indexList.ToArray());
                    textureAndLightMapList.Add(new Vector2(lastTextureIndex, lastLightMapIndex));
                    indexList.Clear();
                }

                if (face.FaceType == Q3BSPFaceType.Patch)
                {
                    Q3BSPVertex[] patchVertices = patches[face.PatchIndex].GetVertices();
                    short[] patchIndices = patches[face.PatchIndex].GetIndices();
                    Q3BSPVertex[] newVertexArray = new Q3BSPVertex[vertices.Length + patchVertices.Length];

                    vertices.CopyTo(newVertexArray, 0);
                    patchVertices.CopyTo(newVertexArray, vertices.Length);

                    foreach (short index in patchIndices)
                    {
                        indexList.Add((short)(vertices.Length + index));
                    }

                    vertices = newVertexArray;
                }

                for (int i = 0; i < face.MeshVertexCount; ++i)
                {
                    indexList.Add((short)(face.StartVertex + meshVertices[face.StartMeshVertex + i]));
                }

                lastTextureIndex = face.TextureIndex;
                lastLightMapIndex = face.LightMapIndex;
            }

            // Add the last index buffer
            indexBufferList.Add(indexList.ToArray());
            textureAndLightMapList.Add(new Vector2(lastTextureIndex, lastLightMapIndex));
            indexList.Clear();

            // Set the vertex and index buffers
            vertexBuffer = new VertexBuffer(graphics, typeof(Q3BSPVertex), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<Q3BSPVertex>(vertices);

            indexBuffers = new IndexBuffer[indexBufferList.Count];
            indexBufferLengths = new int[indexBufferList.Count];
            for (int i = 0; i < indexBuffers.Length; i++)
            {
                indexBuffers[i] = new IndexBuffer(graphics, typeof(short), indexBufferList[i].Length, BufferUsage.WriteOnly);
                indexBuffers[i].SetData<short>(indexBufferList[i]);
                indexBufferLengths[i] = indexBufferList[i].Length;
            }

            // Set the texture and lightmap array
            textureAndLightMapIndices = textureAndLightMapList.ToArray();
        }
    }
}
