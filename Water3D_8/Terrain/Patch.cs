using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Water3D.VertexDeclarations;

namespace Water3D
{
    class Patch : Object3D
    {
        private int patchSize;
        private int mapSize;
        private int mapDetail;
        private Landscape landscape;
        private int heightX, heightZ;
        private PositionNormalMultiTexture[] indexedVerts;
        private VertexBuffer vbIndexed;
        private IndexBuffer ib;
        private int[] index;
        private float maxHeight;
        private int patchIndex;
        public Patch(Landscape landscape, int heightX, int heightZ, int worldX, int worldZ, int patchSize, int mapSize, int mapDetail)
            : base(landscape.Scene, new Vector3(worldX, landscape.getPosition().Y, worldZ), Matrix.Identity, Vector3.One)
        {
            this.landscape = landscape;
            this.patchSize = patchSize;
            this.mapSize = mapSize;
            this.mapDetail = mapDetail;
            //setObject(pos.X, pos.Y, pos.Z);
	        // Store pointer to first byte of the height data for this patch.
	        this.heightX = heightX;
            this.heightZ = heightZ;
            pos.X = landscape.getPosition().X + worldX;
            pos.Y = landscape.getPosition().Y;
            pos.Z = landscape.getPosition().Z + worldZ;
            patchIndex = worldX / patchSize - (worldZ / patchSize)*(mapSize / patchSize);
            maxHeight = landscape.getHeightMap(heightX, heightZ);
            calculateBoundingSphere();
            // Initialize flags
            //initVertexBuffer();
            //initIndexBuffer();
            //drawIndexedPrimitives();
        }
 
        public void render()
        {
            //just needed if shader effect
            EffectContainer.updateMutable(landscape);
            EffectContainer.drawMutable();
            foreach (EffectPass pass in EffectContainer.getEffect().CurrentTechnique.Passes)
            {
                pass.Apply();
                //just needed if BasicEffect
                if (EffectContainer.getEffect().GetType() == typeof(BasicEffect))
                {
                    ((BasicEffect)EffectContainer.getEffect()).EnableDefaultLighting();
                    ((BasicEffect)EffectContainer.getEffect()).LightingEnabled = true;
                    ((BasicEffect)EffectContainer.getEffect()).PreferPerPixelLighting = true;

                    ((BasicEffect)EffectContainer.getEffect()).DirectionalLight0.Enabled = true;
                    ((BasicEffect)EffectContainer.getEffect()).DirectionalLight0.DiffuseColor = new Vector3(0.5f, 0, 0);
                    ((BasicEffect)EffectContainer.getEffect()).DirectionalLight0.SpecularColor = new Vector3(0, 1, 0);
                    ((BasicEffect)EffectContainer.getEffect()).DirectionalLight0.Direction = new Vector3(1, -1, -1);
                    ((BasicEffect)EffectContainer.getEffect()).AmbientLightColor = new Vector3(0.5f, 0.7f, 0.7f);
                    ((BasicEffect)EffectContainer.getEffect()).EmissiveColor = new Vector3(1, 0, 0);

                    landscape.Scene.Game.GraphicsDevice.Textures[0] = landscape.Scene.TextureManager.getTexture("base");
                }
                landscape.Scene.Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, (patchSize / mapDetail) * (patchSize / mapDetail) * 6 * patchIndex, ((patchSize / mapDetail) + 1) * ((patchSize / mapDetail) + 1), (patchSize / mapDetail) * (patchSize / mapDetail) * 6 * patchIndex, (patchSize / mapDetail) * (patchSize / mapDetail) * 2);
                //device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, (patchSize / mapDetail) * (patchSize / mapDetail) * 6, ((patchSize / mapDetail) + 1) * ((patchSize / mapDetail) + 1), (patchSize / mapDetail) * (patchSize / mapDetail) * 6, (patchSize / mapDetail) * (patchSize / mapDetail) * 2);
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
            float xTexFull = 0.0f;
            float zTex = 0.0f;
            float zTexFull = 0.0f;

            xTex = 0.0f;
            xTexFull = getPosition().X;

            zTex = 0.0f;
            zTexFull = -getPosition().Z;
            for (int zDir = 0; zDir <= patchSize; zDir += mapDetail)
            {
                for (int xDir = 0; xDir <= patchSize; xDir += mapDetail)
                {
                    //save maximum height of patch for bounding box
                    float height = landscape.getHeightMap(heightX + xDir, heightZ + zDir);
                    if (maxHeight < height)
                    {
                        maxHeight = height;
                    }
                    // just draw vertices once, rest does index buffer 

                    indexedVerts[vertCount].Position = new Vector3(x, height, z);
                    
                    // normals for lightning
                    indexedVerts[vertCount].Normal.X = 0.0f;
                    indexedVerts[vertCount].Normal.Y = 1.0f;
                    indexedVerts[vertCount].Normal.Z = 0.0f;

                    //indexedVerts[vertCount].Color = Color.Red;
                    // base texture coords
                    indexedVerts[vertCount].TextureCoordinate.X = xTexFull / ((float)(mapSize + 1) * landscape.Scale.X);
                    indexedVerts[vertCount].TextureCoordinate.Y = zTexFull / ((float)(mapSize + 1) * landscape.Scale.Z);
                    
                    // detail texture coords
                    indexedVerts[vertCount].TextureCoordinate1.X = xTex / ((float)(patchSize + 1) * landscape.Scale.X);
                    indexedVerts[vertCount].TextureCoordinate1.Y = zTex / ((float)(patchSize + 1) * landscape.Scale.Z);

                    x += mapDetail * landscape.Scale.X;
                    xTex += mapDetail * landscape.Scale.X;
                    xTexFull += mapDetail * landscape.Scale.X;
                    vertCount++;
                }
                z -= mapDetail * landscape.Scale.Z;
                zTex += mapDetail * landscape.Scale.Z;
                zTexFull += mapDetail * landscape.Scale.Z;
                xTex = 0.0f;
                xTexFull = getPosition().X;
                x = 0.0f;
            }
            //bounding box
            /*
            Vector3 min = new Vector3(worldX, landscape.getPosition().Y, worldZ);
            Vector3 max = new Vector3(worldX + (patchSize + 1), maxHeight, worldZ + (patchSize + 1));
            bb = new BoundingBox(min, max);
            */
            calculateNormals();
            vbIndexed.SetData<PositionNormalMultiTexture>(indexedVerts);
        }

        /// <summary>
        /// initialize vertex and index buffer
        /// </summary>
        public override void initVertexBuffer()
        {
            indexedVerts = new PositionNormalMultiTexture[((patchSize / mapDetail) + 1) * ((patchSize / mapDetail) + 1)];
            vbIndexed = new VertexBuffer(landscape.Scene.Game.GraphicsDevice, typeof(PositionNormalMultiTexture), ((patchSize / mapDetail) + 1) * ((patchSize / mapDetail) + 1), BufferUsage.None);
        }

        public override void initIndexBuffer()
        {
            index = new int[((patchSize / mapDetail) + 1) * ((patchSize / mapDetail) + 1) * 6];
            ib = new IndexBuffer(landscape.Scene.Game.GraphicsDevice, typeof(int), ((patchSize / mapDetail) + 1) * ((patchSize / mapDetail) + 1) * 6, BufferUsage.None);
            int j = 0;
            // for index buffer
            //fill index buffer
            for (int z = 0; z < (patchSize / mapDetail); z += 1)
            {
                for (int x = 0; x < (patchSize / mapDetail); x += 1)
                {
                    //basically Riemer
                    int lowerLeft = x + z * ((patchSize / mapDetail) + 1);
                    int lowerRight = (x + 1) + z * ((patchSize / mapDetail) + 1);
                    int topLeft = x + (z + 1) * ((patchSize / mapDetail) + 1);
                    int topRight = (x + 1) + (z + 1) * ((patchSize / mapDetail) + 1);

                    index[j++] = topLeft;
                    index[j++] = lowerRight;
                    index[j++] = lowerLeft;

                    index[j++] = topLeft;
                    index[j++] = topRight;
                    index[j++] = lowerRight;
                }
            }
            // Lock the IndexBuffer
            ib.SetData<int>(index);
        }
        
        private void calculateBoundingSphere()
        {
            Vector3 center = new Vector3(getPosition().X + patchSize * landscape.Scale.X / 2, getPosition().Y, getPosition().Z - patchSize * landscape.Scale.Z / 2);
            bs = new BoundingSphere(center, Math.Max(patchSize * 2 * landscape.Scale.X, patchSize * landscape.Scale.X));
        }
        
        private void calculateNormals()
        {
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
    }
}
