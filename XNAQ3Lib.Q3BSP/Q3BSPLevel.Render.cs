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
        VertexBuffer vertexBuffer;
        IndexBuffer[] indexBuffers;
        int[] indexBufferLengths;
        Vector2[] textureAndLightMapIndices;
        public bool Intersect;
        public BoundingBox tempBB;
        private OrientedBoundingBox obb;

        public void RenderLevel(Vector3 cameraPosition, Matrix worldMatrix, Matrix viewMatrix, Matrix projMatrix, GameTime gameTime, GraphicsDevice graphics, bool renderSkyBox)
        {
            graphics.RasterizerState = rStateSolid;

            // Render Skybox before everything else
            if (renderSkyBox && skybox != null)
            {
                graphics.DepthStencilState = DepthStencilState.None;
                Matrix skyboxView = viewMatrix;
                skyboxView.Translation = Vector3.Zero;
                skybox.Render(graphics, skyboxView, projMatrix, gameTime);
            }

            if (renderType == Q3BSPRenderType.StaticBuffer)
            {
                RenderLevelStatic(worldMatrix, viewMatrix, projMatrix, graphics, gameTime);
            }
            else
            {
                RenderLevelBSP(cameraPosition, worldMatrix, viewMatrix, projMatrix, gameTime, graphics);
            }
        }

        public void RenderLevelStatic(Matrix worldMatrix, Matrix viewMatrix, Matrix projMatrix, GraphicsDevice graphics, GameTime gameTime)
        {
            Effect effect;
            Matrix matrixWorldViewProjection = worldMatrix * viewMatrix * projMatrix;

            graphics.DepthStencilState = DepthStencilState.Default;
            /*graphics.RasterizerState = rStateCullNoneWireFrame;*/
            
            graphics.SetVertexBuffer(vertexBuffer);

            for (int i = 0; i < indexBuffers.Length; ++i)
            {
                if (!shaderManager.IsMaterialDrawable((int)textureAndLightMapIndices[i].X))
                {
                    continue;
                }

                graphics.Indices = indexBuffers[i];
                effect = shaderManager.GetEffect((int)textureAndLightMapIndices[i].X, (int)textureAndLightMapIndices[i].Y, viewMatrix, matrixWorldViewProjection, gameTime);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indexBufferLengths[i] / 3);
                }
            }
        }

        public void RenderLevelBSP(Vector3 cameraPosition, Matrix worldMatrix, Matrix viewMatrix, Matrix projMatrix, GameTime gameTime, GraphicsDevice graphics)
        {
            int cameraLeaf = GetCameraLeaf(Vector3.Transform(cameraPosition, Matrix.Invert(worldMatrix))); /* transform to world coords  */
            //int cameraLeaf = GetCameraLeaf(Vector3.Transform(cameraPosition, worldMatrix));
            //int cameraLeaf = GetCameraLeaf(cameraPosition); /* transform to world coords  */
            int cameraCluster = leafs[cameraLeaf].Cluster;
            CurrentCluster = cameraCluster;
            CurrentLeaf = cameraLeaf;

            if (0 > cameraCluster)
            {
                //cameraCluster = lastGoodCluster; /* if we are outside, we want to render all*/
            }
            lastGoodCluster = cameraCluster;
            
            ResetFacesToDraw();
            BoundingFrustum frustum = new BoundingFrustum(Matrix.Multiply(viewMatrix, projMatrix));
            ArrayList visibleFaces = new ArrayList();
            VisibleLeafs = 0;
            Intersect = false;
            foreach (Q3BSPLeaf leaf in leafs)
            {
                
                if (!visData.FastIsClusterVisible(cameraCluster, leaf.Cluster))
                {
                    continue;
                }
                
                //// Culls visible leafs. Unsure as to why.
                /*
                if (!frustum.Intersects(leaf.Bounds))
                {
                    continue;
                }
                */
                /* oriented bounding boxes fix the upper problem */
                // create oriented bounding box in world space

                
                obb = new OrientedBoundingBox(leaf.Bounds, worldMatrix);
                if (!frustum.Intersects(obb.AABBWorld))
                {
                    continue;
                }
                else
                {
                    Intersect = true;
                }
                
                VisibleLeafs++;   

                for (int i = 0; i < leaf.LeafFaceCount; i++)
                {
                    int faceIndex = leafFaces[leaf.StartLeafFace + i];
                    Q3BSPFace face = faces[faceIndex];
                    if (face.FaceType != Q3BSPFaceType.Billboard && !facesToDraw[faceIndex])
                    {
                        
                        facesToDraw[faceIndex] = true;
                        visibleFaces.Add(face);
                    }
                }
            }

            if (0 >= visibleFaces.Count)
            {
                return;
            }

            Q3BSPFaceComparer fc = new Q3BSPFaceComparer();
            visibleFaces.Sort(fc);

            Matrix matrixWorldViewProjection = worldMatrix * viewMatrix * projMatrix;
            Effect effect;

            short[] indexArray = new short[maximumNumberOfIndicesToDraw];
            
            int lastTextureIndex = 0;
            int lastLightMapIndex = 0;
            int accumulatedIndexCount = 0;

            graphics.DepthStencilState = DepthStencilState.Default;

            foreach (Q3BSPFace face in visibleFaces)
            {

                if (face.FaceType == Q3BSPFaceType.Patch)
                {
                    effect = shaderManager.GetEffect(face.TextureIndex, face.LightMapIndex, viewMatrix, matrixWorldViewProjection, gameTime);

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        patches[face.PatchIndex].Draw(graphics);
                    }

                    continue;
                }

                if ((face.TextureIndex != lastTextureIndex || face.LightMapIndex != lastLightMapIndex) && accumulatedIndexCount > 0)
                {
                    if (shaderManager.IsMaterialDrawable(lastTextureIndex))
                    {
                        effect = shaderManager.GetEffect(lastTextureIndex, lastLightMapIndex, viewMatrix, matrixWorldViewProjection, gameTime);

                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            graphics.DrawUserIndexedPrimitives<Q3BSPVertex>(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indexArray, 0, accumulatedIndexCount / 3);
                        }
                    }

                    //indexArray = new int[maximumNumberOfIndicesToDraw];
                    accumulatedIndexCount = 0;
                }

                lastTextureIndex = face.TextureIndex;
                lastLightMapIndex = face.LightMapIndex;

                for (int i = 0; i < face.MeshVertexCount; ++i)
                {
                    indexArray[accumulatedIndexCount] = (short)(face.StartVertex + meshVertices[face.StartMeshVertex + i]);
                    accumulatedIndexCount++;
                }
            }

            // Draw the final batch of faces
            if (indexArray.Length != 0 && shaderManager.IsMaterialDrawable(lastTextureIndex))
            {
                effect = shaderManager.GetEffect(lastTextureIndex, lastLightMapIndex, viewMatrix, matrixWorldViewProjection, gameTime);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives<Q3BSPVertex>(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indexArray, 0, accumulatedIndexCount / 3);
                }
            }
        }
    }
}
