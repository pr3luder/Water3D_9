/*
 * This file is a part of the work done by Andreas Zimmer for the 
 * University of Trier
 * Systemsoftware and Distributed Systems
 * D-54286 Trier
 * Email: andizimmer@gmx.de
 * You are free to use the code in any way you like, modified, unmodified or copy'n'pasted 
 * into your own work. However, I expect you to respect these points:  
 * - If you use this file and its contents unmodified, or use a major 
     part of this file, please credit the author and leave this note. 
 * - Please don't use it in any commercial manner.   
 * - Share your work and ideas too as much as you can.  
 * - It would be nice, if you sent me your modifications of this project
*/

using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Water3D.VertexDeclarations;

namespace Water3D
{
    /// <summary>
	/// renders the landscape out of a bitmap called heightmap
	/// method update is overloaded to provide arbitrary
	/// projection view and world matrix (for render to texture)
	/// </summary>
	public class Landscape : LandscapeBase
	{
        private FileStream fs;
        private BinaryReader r;
        private String fileName;
        private Bitmap heightFile;
		private float[] heightmap;
		private float xgrid = 1.0f; // grid in x-direction, xgrid 1
		private float zgrid = 1.0f; // grid in z direction, zgrid 1

        private Vector3[] tempDiffVec;
        private Vector3[] tempNormal;
        private bool raw;

        private int mapSize;
        private int patchSize;
        private int numPatchesPerSide;
        private Patch[,] m_Patches;

        private int numTrisRendered = 0;
        private int numPatchesRendered = 0;
        private int mapDetail;
        private bool doClip;

        private PositionNormalMultiTexture[] indexedVerts;
        private VertexBuffer vbIndexed;
        private IndexBuffer ib;
        private int[] index;
        private float maxHeight;

        private int maxVertexBuffer;
		/// <summary>
		/// Chapter 4.3.3, Listing 4.3.19
		/// read heightmap which is an 8 bit greyscale bitmap
		/// </summary>
		/// <param name="device"></param>
		/// <param name="fileName"></param>
		/// <param name="pos"></param>
        public Landscape(SceneContainer scene, String fileName, Vector3 pos, Vector3 scale, int mapDetail, int mapSize, int patchSize, bool doClip) 
            : base(scene, fileName, pos, Matrix.Identity, scale, mapSize)
        {

            this.scene = scene;
            this.fileName = fileName;
            tempDiffVec = new Vector3[8];
            tempNormal = new Vector3[8];
            if (fileName.Split('.').Length > 1 && fileName.Split('.')[1].ToLower() == "raw")
            {
                this.raw = true;
            }
            else
            {
                this.raw = false;
            }
            maxHeight = getHeightMap(0, 0);
            this.mapSize = mapSize;
            this.patchSize = patchSize;
            this.mapDetail = mapDetail;
            this.numPatchesPerSide = mapSize / patchSize;
            this.doClip = doClip;
            setObject(pos.X, pos.Y, pos.Z);
            readHeightmap();
            initVertexBuffer();
            initIndexBuffer();
            fillVertexBuffer();
            
            m_Patches = new Patch[numPatchesPerSide, numPatchesPerSide];
            for (int j = 0; j < numPatchesPerSide; j++)
            {
                for (int i = 0; i < numPatchesPerSide; i++)
                {
                    m_Patches[i, j] = new Patch(this, i * patchSize, j * patchSize, i * patchSize * (int)scale.X, -j * patchSize * (int)scale.Z, patchSize, mapSize, mapDetail);
                }
            }
            maxVertexBuffer = 0;
        }

		/// <summary>
		/// render landscape
        /// render all in one pass with one 
        /// ore more texture over the whole 
        /// landcape
		/// </summary>
        public override void Draw(GameTime time)
        {
            scene.Game.GraphicsDevice.Indices = IndexBuffer;
            scene.Game.GraphicsDevice.SetVertexBuffer(VertexBuffer);

            base.Draw(time);

            drawIndexedPrimitives();
        }

		/// <summary>
		/// draw indexed primitives
		/// </summary>
		public override void drawIndexedPrimitives()
		{
            NumPatchesRendered = 0;
            
            for (int i = 0; i < numPatchesPerSide; i++)
            {
                for (int j = 0; j < numPatchesPerSide; j++)
                {
                    if (doClip)
                    {
                        if (m_Patches[i, j].isVisible())
                        {
                            NumPatchesRendered++;
                            m_Patches[i, j].render();
                        }
                    }
                    else
                    {
                        NumPatchesRendered++;
                        m_Patches[i, j].render();
                    }
                }
            }
            
            /*
            m_Patches[0, 0].render();
            m_Patches[1, 0].render();
            m_Patches[2, 0].render();
            m_Patches[3, 0].render();
            m_Patches[4, 0].render();
            m_Patches[5, 0].render();
            m_Patches[6, 0].render();
            m_Patches[7, 0].render();
            m_Patches[0, 1].render();
            m_Patches[1, 1].render();
            m_Patches[2, 1].render();
            m_Patches[3, 1].render();
            m_Patches[4, 1].render();
            m_Patches[5, 1].render();
            m_Patches[6, 1].render();
            m_Patches[7, 1].render();
            m_Patches[0, 2].render();
            m_Patches[1, 2].render();
            m_Patches[2, 2].render();
            m_Patches[3, 2].render();
            m_Patches[4, 2].render();
            m_Patches[5, 2].render();
            m_Patches[6, 2].render();
            m_Patches[7, 2].render();
            m_Patches[0, 3].render();
            m_Patches[1, 3].render();
            m_Patches[2, 3].render();
            m_Patches[3, 3].render();
            m_Patches[4, 3].render();
            m_Patches[5, 3].render();
            m_Patches[6, 3].render();
            m_Patches[7, 3].render();
            m_Patches[0, 4].render();
            m_Patches[1, 4].render();
            m_Patches[2, 4].render();
            m_Patches[3, 4].render();
            m_Patches[4, 4].render();
            m_Patches[5, 4].render();
            m_Patches[6, 4].render();
            m_Patches[7, 4].render();
            
            m_Patches[0, 5].render();
            m_Patches[1, 5].render();
            m_Patches[2, 5].render();
            m_Patches[3, 5].render();
            m_Patches[4, 5].render();
            m_Patches[5, 5].render();
            m_Patches[6, 5].render();
            m_Patches[7, 5].render();
            
            m_Patches[0, 6].render();
            m_Patches[1, 6].render();
            m_Patches[2, 6].render();
            m_Patches[3, 6].render();
            m_Patches[4, 6].render();
            m_Patches[5, 6].render();
            m_Patches[6, 6].render();
            m_Patches[7, 6].render();
            
            m_Patches[0, 7].render();
            m_Patches[1, 7].render();
            m_Patches[2, 7].render();
            m_Patches[3, 7].render();
            m_Patches[4, 7].render();
            m_Patches[5, 7].render();
            m_Patches[6, 7].render();
            m_Patches[7, 7].render();
           */
            /*
            //device.RenderState.FillMode = FillMode.WireFrame;
            device.Indices = IndexBuffer;
            device.Vertices[0].SetSource(VertexBuffer, 0, PositionNormalMultiTexture.SizeInBytes);
            device.VertexDeclaration = new VertexDeclaration(device, PositionNormalMultiTexture.VertexElements);
            //just needed if shader effect
            EffectContainer.updateMutable(this);
            EffectContainer.getEffect().Begin();
            foreach (EffectPass pass in EffectContainer.getEffect().CurrentTechnique.Passes)
            {
                pass.Begin();
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

                    device.Textures[0] = getBaseTexture();
                }
                //device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,  4, 0, 2);
                //device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 6, 2);
                
                //device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ((patchSize / mapDetail) + 1) * ((patchSize / mapDetail) + 1), 0, (patchSize / mapDetail) * (patchSize / mapDetail) * 2);
                for (int i = 0; i < 64;i++ )
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ((patchSize / mapDetail) + 1) * ((patchSize / mapDetail) + 1), (patchSize / mapDetail) * (patchSize / mapDetail) * 6 * i, (patchSize / mapDetail) * (patchSize / mapDetail) * 2);
                //((patchSize / mapDetail) + 1) * ((patchSize / mapDetail) + 1) / 2 - 1) * 6 * i
                pass.End();
            }
            EffectContainer.getEffect().End();
            device.Vertices[0].SetSource(null, 0, 0);
            */
        }

        public override void scaleObject(float scaleX, float scaleY, float scaleZ)
        {
            
        }

		public void changeXGrid(float xgrid)
		{
			this.xgrid = xgrid;
            readHeightmap();;
            setObject(pos.X, pos.Y, pos.Z);
		}

		public void changeZGrid(float zgrid)
		{
			this.zgrid = zgrid;
            readHeightmap(); 
            setObject(pos.X, pos.Y, pos.Z);
		}

        public float getHeight(float x, float y)
        {
            float xHeight = 0.0f;
            float yHeight = 0.0f;
            /*
            xHeight = (x - pos.X) / scale;
            yHeight = (y + pos.Z) / scale;
            */
            xHeight = x / scale.X - pos.X;
            yHeight = y / scale.Z + pos.Z;
            if (xHeight < 0 || xHeight > mapSize - 1)
            {
                xHeight = 0;
            }
            if (yHeight < 0 || yHeight > mapSize - 1)
            {
                yHeight = 0;
            }
            /*
            x = (x + pos.X) * scale;
            y = (y - pos.Z) * scale;
            */
            x = x * scale.X + pos.X;
            y = y * scale.Z - pos.Z;
            float z = (float)((heightmap[(int)xHeight + (int)yHeight * mapSize] * (1 - (x - (int)x)) + heightmap[(int)xHeight + 1 +  (int)yHeight * mapSize] * (x - (int)x)) * (1 - (y - (int)y)) + (heightmap[(int)xHeight + (int)(yHeight + 1) * mapSize] * (1 - (x - (int)x)) + heightmap[(int)xHeight + 1 + (int)(yHeight + 1) * mapSize] * (x - (int)x)) * (y - (int)y));
            return z;
        }

        public float getHeightMap(float x, float y)
        {
            if (x > mapSize - 1 || y > mapSize - 1)
                return 0.0f;
            return heightmap[(int)x + (int)y * mapSize];
        }

        // This function takes in a position, and returns the heightmap's height at that
        // point. Be careful - this function will throw an IndexOutOfRangeException if
        // position isn't on the heightmap!
        // This function is explained in more detail in the accompanying doc.
        public float getHeight(Vector3 position)
        {
            Vector3 positionOnHeightmap = new Vector3();
            // the first thing we need to do is figure out where on the heightmap
            // "position" is. This'll make the math much simpler later.
            positionOnHeightmap = position - pos;
            if (positionOnHeightmap.Z < 0)
            {
                positionOnHeightmap.Z = -positionOnHeightmap.Z;
            }
            // we'll use integer division to figure out where in the "heights" array
            // positionOnHeightmap is. Remember that integer division always rounds
            // down, so that the result of these divisions is the indices of the "upper
            // left" of the 4 corners of that cell.
            int left, top;
            left = (int)positionOnHeightmap.X / (int)scale.X;
            top = (int)positionOnHeightmap.Z / (int)scale.Z;

            // next, we'll use modulus to find out how far away we are from the upper
            // left corner of the cell. Mod will give us a value from 0 to terrainScale,
            // which we then divide by terrainScale to normalize 0 to 1.
            float xNormalized = (positionOnHeightmap.X % scale.X) / scale.X;
            float zNormalized = (positionOnHeightmap.Z % scale.Z) / scale.Z;

            // Now that we've calculated the indices of the corners of our cell, and
            // where we are in that cell, we'll use bilinear interpolation to calculuate
            // our height. This process is best explained with a diagram, so please see
            // the accompanying doc for more information.
            // First, calculate the heights on the bottom and top edge of our cell by
            // interpolating from the left and right sides.
            float topHeight = 0.0f;
            float bottomHeight = 0.0f;
            if (left < 0 || left > mapSize - 2)
            {
                left = 0;
            }
            if (top < 0 || top > mapSize - 2)
            {
                top = 0;
            }
            // next, interpolate between those two values to calculate the height at our
            // position.
            topHeight = MathHelper.Lerp(heightmap[left + top * mapSize], heightmap[left + 1 + top * mapSize], xNormalized);
            bottomHeight = MathHelper.Lerp(heightmap[left + (top + 1) * mapSize], heightmap[left + 1 + (top + 1) * mapSize], xNormalized);
            
            float height = MathHelper.Lerp(topHeight, bottomHeight, zNormalized);
           
            return height;
        }

        public int NumTrisRendered
        {
            get
            {
                return numTrisRendered;
            }
            set
            {
                numTrisRendered = value;
            }
        }

        public int NumPatchesRendered
        {
            get
            {
                return numPatchesRendered;
            }
            set
            {
                numPatchesRendered = value;
            }
        }

        /// <summary>
        /// draw landscape in local space
        /// (fill vertex array with data)
        /// </summary>
        public void fillVertexBuffer()
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
            for (int zDir = 0; zDir < mapSize; zDir += 1)
            {
                for (int xDir = 0; xDir < mapSize; xDir += 1)
                {
                    // just draw vertices once, rest does index buffer 
                    indexedVerts[vertCount].Position.X = x;
                    indexedVerts[vertCount].Position.Z = z;

                    //save maximum height of patch for bounding box
                    float height = getHeightMap(xDir, zDir);
                    if (maxHeight < height)
                    {
                        maxHeight = height;
                    }
                    indexedVerts[vertCount].Position.Y = height;

                    // normals for lightning
                    indexedVerts[vertCount].Normal.X = 0.0f;
                    indexedVerts[vertCount].Normal.Y = 1.0f;
                    indexedVerts[vertCount].Normal.Z = 0.0f;

                    //indexedVerts[vertCount].Color = Color.Red;
                    // base texture coords
                    indexedVerts[vertCount].TextureCoordinate.X = xTexFull / ((float)(mapSize + 1) * Scale.X);
                    indexedVerts[vertCount].TextureCoordinate.Y = zTexFull / ((float)(mapSize + 1) * Scale.Z);

                    // detail texture coords
                    indexedVerts[vertCount].TextureCoordinate1.X = xTex / ((float)(patchSize + 1) * Scale.X);
                    indexedVerts[vertCount].TextureCoordinate1.Y = zTex / ((float)(patchSize + 1) * Scale.Z);
                    
                    x += Scale.X;
                    xTex += Scale.X;
                    xTexFull += Scale.X;
                    vertCount++;
                }
                z -= Scale.Z;
                zTex += Scale.Z;
                zTexFull += Scale.Z;
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
            //bounding sphere
            Vector3 center = new Vector3(getPosition().X + mapSize * Scale.X / 2, getPosition().Y, getPosition().Z - mapSize * Scale.Z / 2);
            bs = new BoundingSphere(center, Math.Max(maxHeight * 2 * Scale.X, mapSize * Scale.X));
            calculateNormals();
            vbIndexed.SetData<PositionNormalMultiTexture>(indexedVerts);
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

        /// <summary>
        /// initialize vertex and index buffer
        /// </summary>
        public override void initVertexBuffer()
        {
            indexedVerts = new PositionNormalMultiTexture[mapSize * mapSize];
            vbIndexed = new VertexBuffer(scene.Game.GraphicsDevice, typeof(PositionNormalMultiTexture), mapSize * mapSize, BufferUsage.None);
        }

        public override void initIndexBuffer()
        {
            index = new int[(mapSize) * (mapSize) * 6];
            ib = new IndexBuffer(scene.Game.GraphicsDevice, typeof(int), (mapSize) * (mapSize) * 6, BufferUsage.None);
            int j = 0;
            //fill index buffer
            for (int pz = 0; pz < numPatchesPerSide; pz += 1)
            {
                for (int px = 0; px < numPatchesPerSide; px += 1)
                {
                    for (int z = 0; z < patchSize; z += mapDetail)
                    {
                        for (int x = 0; x < patchSize; x += mapDetail)
                        {
                            //basically Riemer
                            int lowerLeft = (x + (patchSize - mapDetail) * px) + (z + (patchSize - mapDetail) * pz) * mapSize;
                            int lowerRight = (x + mapDetail + (patchSize - mapDetail) * px) + (z + (patchSize - mapDetail) * pz) * mapSize;
                            int topLeft = (x + (patchSize - mapDetail) * px) + (z + mapDetail + (patchSize - mapDetail) * pz) * mapSize;
                            int topRight = (x + mapDetail + (patchSize - mapDetail) * px) + (z + mapDetail + (patchSize - mapDetail) * pz) * mapSize;
                            
                            index[j++] = topLeft;
                            index[j++] = lowerRight;
                            index[j++] = lowerLeft;

                            index[j++] = topLeft;
                            index[j++] = topRight;
                            index[j++] = lowerRight;
                        }
                    }
                }
            }
            // Lock the IndexBuffer
            ib.SetData<int>(index);
        }

        /*
        private void initIndexBuffer()
        {
            index = new int[mapSize * mapSize * 6];
            ib = new IndexBuffer(device, typeof(int), mapSize * mapSize * 6, BufferUsage.None);
            int j = 0;
            // for index buffer
            //fill index buffer
            for (int z = 0; z < mapSize; z += 1)
            {
                for (int x = 0; x < mapSize; x += 1)
                {
                    //basically Riemer
                    int lowerLeft = x + z * (mapSize + 1);
                    int lowerRight = (x  + 1) + z  * (mapSize + 1);
                    int topLeft = x + (z + 1) * (mapSize + 1);
                    int topRight = (x + 1) + (z + 1) * (mapSize + 1);

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
        */
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

        public IndexBuffer IndexBuffer
        {
            get
            {
                return ib;
            }
            set
            {
                ib = value;
            }
        }
	}
}
