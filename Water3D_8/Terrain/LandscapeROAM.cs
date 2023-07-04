using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Water3D.VertexDeclarations;

namespace Water3D
{
    public class LandscapeROAM : LandscapeBase
    {
        private FileStream fs;
        private BinaryReader r;
        private float[] heightmap;
        public static VertexBuffer vb;
        private PositionNormalMultiTexture[] verts;
        private bool raw;
        private String fileName;     
        private int poolSize = 50000;
        private int mapSize;
        private int patchSize;
        private int numPatchesPerSide;
        private int desiredTris = 10000;
        private int nextTriNode;
        private bool withVariance;
        
        private TriTreeNode[] triPool;
        private PatchROAM[,] patches;

        private int numTrisRendered = 0;
        private int numVertsRendered = 0;
        private int numPatchesRendered = 0;
        private float frameVariance = 50.0f;
        private int initPatch = 0;

        public LandscapeROAM(SceneContainer scene, String fileName, Vector3 pos, Vector3 scale, int mapSize, int patchSize, bool withVariance)
            : base(scene, fileName, pos, Matrix.Identity, scale, mapSize)
        {
            this.scene = scene;
            this.fileName = fileName;
            this.scale = scale;
            if (fileName.Split('.').Length > 1 && fileName.Split('.')[1].ToLower() == "raw")
            {
                this.raw = true;
            }
            else
            {
                this.raw = false;
            }
            
            this.mapSize = mapSize;
            this.patchSize = patchSize;
            this.withVariance = withVariance;
            this.numPatchesPerSide = mapSize / patchSize;
            triPool = new TriTreeNode[poolSize];
            patches = new PatchROAM[numPatchesPerSide, numPatchesPerSide];
            vb = new VertexBuffer(scene.Game.GraphicsDevice, typeof(PositionNormalMultiTexture), 50000, BufferUsage.None);
            verts = new PositionNormalMultiTexture[50000];
            //setObject(pos.X, pos.Y, pos.Z);
            readHeightmap();
            smoothTerrain(1);
            init();
            
        }
        
        public int getNextTriNode()
        { 
            return nextTriNode; 
        }
        
        public void setNextTriNode(int nNextNode) 
        { 
            nextTriNode = nNextNode;
        }

        public int getMapSize()
        {
            return mapSize;
        }

        public int getPatchSize()
        {
            return patchSize;
        }
        
        public void drawLandscape()
        {
        }

        private void initIndexVertexBuffer()
        {         
        }

        public override void Draw(GameTime time)
        {
            base.Draw(time);
            drawIndexedPrimitives();
        }

        public override void drawIndexedPrimitives()
        {
            reset();
            tessellate();
            render();
        }
        
       
        // ---------------------------------------------------------------------
        // Allocate a TriTreeNode from the pool.
        //
        public TriTreeNode allocateTri()
        {
            // If we've run out of TriTreeNodes, just return NULL (this is handled gracefully)
            if (nextTriNode >= poolSize - 1)
                return null;
            nextTriNode++;
            triPool[nextTriNode] = new TriTreeNode();
            return triPool[nextTriNode];
        }

        // ---------------------------------------------------------------------
        // Initialize all patches
        //
        public void init()
        {
            int x, y;
            // Initialize all terrain patches
            for (y = 0; y < numPatchesPerSide; y++)
                for (x = 0; x < numPatchesPerSide; x++)
                {
                    patches[y, x] = new PatchROAM(this, scene.Camera , x*patchSize, y*patchSize, x*patchSize, y*patchSize, patchSize, mapSize);
                    if(withVariance)
                        patches[y, x].computeVariance();
                    this.initPatch++;
                }
        }
        // ---------------------------------------------------------------------
        // Reset all patches, recompute variance if needed
        //
        void reset()
        {
	        int x, y;

	        // Set the next free triangle pointer back to the beginning
	        setNextTriNode(0);

	        // Go through the patches performing resets, compute variances, and linking.
	        for ( y = 0; y < numPatchesPerSide; y++ )
		        for ( x = 0; x < numPatchesPerSide; x++)
		        {
			        // Reset the patch
			        patches[y, x].reset();
                    patches[y, x].setVisibility();
        			
			        // Check to see if this patch has been deformed since last frame.
			        // If so, recompute the varience tree for it.
			        if (patches[y, x].isDirty() != 0 )
				        if(withVariance)
                            patches[y, x].computeVariance();

			        if (patches[y, x].isVisibile() != 0)
			        {
				        // Link all the patches together.
				        if ( x > 0 )
                            patches[y, x].getBaseLeft().setLeftNeighbor(patches[y,x - 1].getBaseRight());
				        else
                            patches[y, x].getBaseLeft().setLeftNeighbor(null);		// Link to bordering Landscape here..

				        if ( x < (numPatchesPerSide-1) )
					        patches[y, x].getBaseRight().setLeftNeighbor(patches[y,x + 1].getBaseLeft());
				        else
					        patches[y, x].getBaseRight().setLeftNeighbor(null);		// Link to bordering Landscape here..

				        if ( y > 0 )
					        patches[y, x].getBaseLeft().setRightNeighbor(patches[y - 1,x].getBaseRight());
				        else
					        patches[y, x].getBaseLeft().setRightNeighbor(null);		// Link to bordering Landscape here..

				        if ( y < (numPatchesPerSide-1) )
					        patches[y, x].getBaseRight().setRightNeighbor(patches[y + 1,x].getBaseLeft());
				        else
					        patches[y, x].getBaseRight().setRightNeighbor(null);	// Link to bordering Landscape here..
			        }
		    }
        }

        // ---------------------------------------------------------------------
        // Create an approximate mesh of the landscape.
        //
        void tessellate()
        {
            // Perform Tessellation
            for (int y = 0; y < numPatchesPerSide; y++)
            {
                for (int x = 0; x < numPatchesPerSide; x++)
                {
                    if (patches[y, x].isVisibile() != 0)
                        patches[y, x].tessellate();
                }
            }
        }
        // ---------------------------------------------------------------------
        // Render each patch of the landscape & adjust the frame variance.
        //
        void render()
        {
            numPatchesRendered = 0;
            numTrisRendered = 0;
            numVertsRendered = 0;
            for (int y = 0; y < numPatchesPerSide; y++)
            {
                for (int x = 0; x < numPatchesPerSide; x++)
                {
                    if (patches[y, x].isVisibile() != 0)
                    {
                        this.numPatchesRendered++;
                        patches[y, x].render();
                    }
                }
            }
            if(numTrisRendered > 0)
            {
                vb.SetData<PositionNormalMultiTexture>(verts);
                scene.Game.GraphicsDevice.SetVertexBuffer(vb);
                foreach (EffectPass pass in ((BasicEffect)effectContainer.getEffect()).CurrentTechnique.Passes)
                {
                    pass.Apply();
                    scene.Game.GraphicsDevice.Textures[0] = scene.TextureManager.getTexture("landscapecolor");
                    //landscape.Device.Textures[1] = landscape.getDetailTexture();
                    scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, numTrisRendered);
                }
            }
            // Check to see if we got close to the desired number of triangles.
            // Adjust the frame variance to a better value.
            if (getNextTriNode() != desiredTris)
                this.frameVariance += ((float)getNextTriNode() - (float)desiredTris) / (float)desiredTris;
           
            // Bounds checking.
            if (this.frameVariance < 0)
                this.frameVariance = 0;
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
            float z = (float)((heightmap[(int)xHeight + (int)yHeight * mapSize] * (1 - (x - (int)x)) + heightmap[(int)xHeight + 1 + (int)yHeight * mapSize] * (x - (int)x)) * (1 - (y - (int)y)) + (heightmap[(int)xHeight + (int)(yHeight + 1)*mapSize] * (1 - (x - (int)x)) + heightmap[(int)xHeight + 1 + (int)(yHeight + 1) * mapSize] * (x - (int)x)) * (y - (int)y));
            return z;
        }

        public float getHeightMap(float x, float y)
        {
            if (x > mapSize - 1 || y > mapSize - 1)
                return 0.0f;
            return heightmap[(int)x + (int)(y * mapSize)];
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

        public PositionNormalMultiTexture[] Verts
        {
            get
            {
                return verts;
            }
        }

        public float FrameVariance
        {
            get
            {
                return frameVariance;
            }
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

        public int NumVertsRendered
        {
            get
            {
                return numVertsRendered;
            }
            set
            {
                numVertsRendered = value;
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

        public override void initVertexBuffer()
        {
            throw new NotImplementedException();
        }

        public override void initIndexBuffer()
        {
            throw new NotImplementedException();
        }
    }
}
