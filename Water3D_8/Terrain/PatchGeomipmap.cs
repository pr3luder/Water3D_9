using System;
using System.Collections.Generic;
using System.Text;
using Assimp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Water3D.VertexDeclarations;
using PrimitiveType = Microsoft.Xna.Framework.Graphics.PrimitiveType;

namespace Water3D
{
    class PatchGeomipmap : Object3D
    {
       
        private int patchSize;
        private int mapSize;
        private int mapDetail;
        private LandscapeGeomipmap landscape;
        private int heightX, heightZ;
        private PositionNormalMultiTexture[] indexedVerts;
        private VertexBuffer vbIndexed;
        private float maxHeight;
        private int vertexDetail;
        private int[] index;
        int indexX;
        int indexZ;
        float xRatioFull;
        float xRatioPatch;
        float zRatioFull;
        float zRatioPatch;
        public PatchGeomipmap(LandscapeGeomipmap landscape, Vector3 pos, Matrix rotation, int heightX, int heightZ, int patchSize, int mapSize, int mapDetail)
            : base(landscape.Scene, pos, rotation, Vector3.One)
        {
            this.landscape = landscape;
            this.patchSize = patchSize;
            this.mapSize = mapSize;
            this.mapDetail = mapDetail;
            this.vertexDetail = 1;
            this.xRatioFull = 1.0f  / (mapSize + 1);
            this.zRatioFull = 1.0f / (mapSize + 1);
            this.xRatioPatch = 16.0f / (patchSize + 1);
            this.zRatioPatch = 16.0f / (patchSize + 1);

            
	        // Store pointer to first byte of the height data for this patch.
	        this.heightX = heightX;
            this.heightZ = heightZ;
            //this.worldX = worldX;
            //this.worldZ = worldZ;
            maxHeight = landscape.getHeightMap(heightX, heightZ);
            indexX = (heightX * mapSize) / (mapSize * patchSize);
            indexZ = (heightZ * mapSize) / (mapSize * patchSize);
            // Initialize flags
            initVertexBuffer();
            drawIndexedPrimitives();
            calculateBoundingSphere();

            // Store Patch offsets for the heightmap.
            setObject(pos.X, pos.Y, pos.Z);
        }

        public void render(int detailVal)
        {
            int mapDetail = (int)Math.Pow((double)2, (double)detailVal);
            
            scene.Game.GraphicsDevice.Indices = landscape.IndexBuffer[detailVal];
            scene.Game.GraphicsDevice.SetVertexBuffer(VertexBuffer);
            
            updateEffects();
            foreach (EffectPass pass in effectContainer.getEffect().CurrentTechnique.Passes)
            {
                pass.Apply();
                // just for basic effect
                //landscape.EffectContainer.drawUniform();
                /*EffectContainer.drawUniform();*/
                /*
                RasterizerState rStateWireFrame = new RasterizerState() { FillMode = FillMode.WireFrame };
                scene.Game.GraphicsDevice.RasterizerState = rStateWireFrame;
                */
                //scene.Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ((patchSize / mapDetail) + 1) * ((patchSize / mapDetail) + 1) + ((patchSize / mapDetail) + 1) * 4, 0, ((patchSize / mapDetail)) * ((patchSize / mapDetail)) * 2 + ((patchSize / mapDetail)) * 8);
                scene.Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, landscape.IndexBuffer[detailVal].IndexCount / 3);
                //scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, ((patchSize / mapDetail)) * ((patchSize / mapDetail)) * 2 + ((patchSize / mapDetail)) * 8);
            }
        }
        
        /// <summary>
        /// draw landscape in local space
        /// (fill vertex array with data)
        /// </summary>
        public override void drawIndexedPrimitives()
        {
            int vertCount = 0;
            float x = 0.0f;
            float z = 0.0f;
            float xTex = 0.0f;
            float zTex = 0.0f;
            float xTexFull = 0.0f;
            float zTexFull = 0.0f;

            xTex = 0.0f;
            zTex = 0.0f;
            xTexFull = heightX * xRatioFull;
            zTexFull = heightZ * zRatioFull;
            //patchSize = 201;
            for (int zDir = 0; zDir <= patchSize; zDir += vertexDetail)
            {
                for (int xDir = 0; xDir <= patchSize; xDir += vertexDetail)
                {
                    // just draw vertices once, rest does index buffer 
                    indexedVerts[vertCount].Position.X = x;
                    indexedVerts[vertCount].Position.Z = z;

                    //save maximum height of patch for bounding box
                    float height = landscape.getHeightMap(heightX + xDir, heightZ + zDir);

                    if (maxHeight < height)
                    {
                        maxHeight = height*scale.Z;
                    }
                    indexedVerts[vertCount].Position.Y = height;

                    // normals for lightning
                    indexedVerts[vertCount].Normal.X = 0.0f;
                    indexedVerts[vertCount].Normal.Y = 1.0f;
                    indexedVerts[vertCount].Normal.Z = 0.0f;

                    //indexedVerts[vertCount].Color = Color.Red;
                    // base texture coords

                    indexedVerts[vertCount].TextureCoordinate.X = xTexFull;
                    indexedVerts[vertCount].TextureCoordinate.Y = zTexFull;

                    // detail texture coords
                    indexedVerts[vertCount].TextureCoordinate1.X = xTex;
                    indexedVerts[vertCount].TextureCoordinate1.Y = zTex;
                    
                    x += vertexDetail * landscape.Scale.X;
                    xTex += xRatioPatch;
                    xTexFull += xRatioFull;
                    vertCount++;
                }
                z -= vertexDetail * landscape.Scale.Z;
                //zTex += vertexDetail * landscape.Scale.Z;
                zTex += zRatioPatch;
                zTexFull += zRatioFull;
                xTex = 0.0f;
                xTexFull = heightX * xRatioFull;
                x = 0.0f;
            }
            
            //skirt
            
            x = 0.0f;
            z = 0.0f;
            xTex = 0.0f;
            zTex = 0.0f;
            xTexFull = heightX * xRatioFull;
            zTexFull = heightZ * zRatioFull;
            for (int xDir = 0; xDir <= patchSize; xDir += vertexDetail)
            {
                indexedVerts[vertCount].Position.X = x;
                indexedVerts[vertCount].Position.Y = -1.0f;
                indexedVerts[vertCount].Position.Z = z;
                
                // base texture coords
                indexedVerts[vertCount].TextureCoordinate.X = xTexFull;
                indexedVerts[vertCount].TextureCoordinate.Y = zTexFull;
                // detail texture coords
                indexedVerts[vertCount].TextureCoordinate1.X = xTex;
                indexedVerts[vertCount].TextureCoordinate1.Y = zTex;
                
                x += vertexDetail * landscape.Scale.X;
                xTex += xRatioPatch;
                xTexFull += xRatioFull;
                vertCount++;
            }
            x -= vertexDetail * landscape.Scale.X;
            xTex -= xRatioPatch;
            xTexFull -= xRatioFull;
            for (int zDir = 0; zDir <= patchSize; zDir += vertexDetail)
            {
                indexedVerts[vertCount].Position.X = x;
                indexedVerts[vertCount].Position.Y = -1.0f;
                indexedVerts[vertCount].Position.Z = z;
                // base texture coords
                indexedVerts[vertCount].TextureCoordinate.X = xTexFull;
                indexedVerts[vertCount].TextureCoordinate.Y = zTexFull;
                // detail texture coords
                indexedVerts[vertCount].TextureCoordinate1.X = xTex;
                indexedVerts[vertCount].TextureCoordinate1.Y = zTex;

                z -= vertexDetail * landscape.Scale.Z;
                zTex += zRatioPatch;
                zTexFull += zRatioFull;
                vertCount++;
            }
            z += vertexDetail * landscape.Scale.Z;
            zTex -= zRatioPatch;
            zTexFull -= zRatioFull;
            for (int xDir = patchSize; xDir >= 0; xDir -= vertexDetail)
            {
                indexedVerts[vertCount].Position.X = x;
                indexedVerts[vertCount].Position.Y = -1.0f;
                indexedVerts[vertCount].Position.Z = z;
                // base texture coords
                indexedVerts[vertCount].TextureCoordinate.X = xTexFull;
                indexedVerts[vertCount].TextureCoordinate.Y = zTexFull;
                // detail texture coords
                indexedVerts[vertCount].TextureCoordinate1.X = xTex;
                indexedVerts[vertCount].TextureCoordinate1.Y = zTex;
                x -= vertexDetail * landscape.Scale.X;
                xTex -= xRatioPatch;
                xTexFull -= xRatioFull;
                vertCount++;
            }
            x += vertexDetail * landscape.Scale.X;
            xTex -= xRatioPatch;
            xTexFull -= xRatioFull;
            for (int zDir = patchSize; zDir >= 0; zDir -= vertexDetail)
            {
                indexedVerts[vertCount].Position.X = x;
                indexedVerts[vertCount].Position.Y = -1.0f;
                indexedVerts[vertCount].Position.Z = z;
                // base texture coords
                indexedVerts[vertCount].TextureCoordinate.X = xTexFull;
                indexedVerts[vertCount].TextureCoordinate.Y = zTexFull;
                // detail texture coords
                indexedVerts[vertCount].TextureCoordinate1.X = xTex;
                indexedVerts[vertCount].TextureCoordinate1.Y = zTex;
                z += vertexDetail * landscape.Scale.Z;
                zTex -= zRatioPatch;
                zTexFull -= zRatioFull;
                vertCount++;
            }
            
            calculateNormals();
            vbIndexed.SetData<PositionNormalMultiTexture>(indexedVerts);
        }

        /// <summary>
        /// initialize vertex and index buffer
        /// </summary>
        public override void initVertexBuffer()
        {
            indexedVerts = new PositionNormalMultiTexture[((patchSize / vertexDetail) + 1) * ((patchSize / vertexDetail) + 1) + ((patchSize / vertexDetail) + 1) * 4];
            vbIndexed = new VertexBuffer(scene.Game.GraphicsDevice, typeof(PositionNormalMultiTexture), ((patchSize / vertexDetail) + 1) * ((patchSize / vertexDetail) + 1) + ((patchSize / vertexDetail) + 1) * 4, BufferUsage.None);
        }

        public override void initIndexBuffer()
        {
            throw new NotImplementedException();
        }

        private void calculateBoundingSphere()
        {
            Vector3 center = new Vector3(getPosition().X + patchSize * landscape.Scale.X / 2, getPosition().Y, getPosition().Z - patchSize * landscape.Scale.Z / 2);
            bs = new BoundingSphere(center, Math.Max(getPosition().Y + maxHeight * landscape.Scale.X, patchSize * landscape.Scale.X));
        }

        private void calculateBoundingBox()
        {
            Vector3 min = getPosition();
            Vector3 max = new Vector3(getPosition().X + patchSize*scale.X, getPosition().Y + maxHeight, getPosition().Z + patchSize*scale.Z);
            bb = new BoundingBox(min, max);
        }
        
        private void calculateNormals()
        {
            //index = new int[landscape.IndexBuffer[0].IndexCount];
            //landscape.IndexBuffer[0].GetData(index);
            index = new int[2];
            for (int i = 0; i < indexedVerts.Length; i++)
                indexedVerts[i].Normal = new Vector3(0, 0, 0);
            
            for (int i = 0; i < index.Length / 3; i++)
            {

                int index1 = index[i * 3];
                int index2 = index[i * 3 + 1];
                int index3 = index[i * 3 + 2];
                
                Vector3 side1 = indexedVerts[index1].Position - indexedVerts[index3].Position;
                Vector3 side2 = indexedVerts[index1].Position - indexedVerts[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                indexedVerts[index1].Normal += normal;
                indexedVerts[index2].Normal += normal;
                indexedVerts[index3].Normal += normal;
            }

            for (int i = 0; i < indexedVerts.Length; i++)
            {
                indexedVerts[i].Normal.Normalize();
            }
        }

        public VertexBuffer VertexBuffer
        {
            get
            {
                return vbIndexed;
            }
            set
            {
                vbIndexed = value;
            }
        }
    }
}
