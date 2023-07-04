using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Water3D.VertexDeclarations;

namespace Water3D
{
    public class LandscapeBase : Object3D
    {
        private FileStream fs;
        private BinaryReader r;
        private String fileName;
        private Bitmap heightFile;
        protected float[] heightmap;
        protected float[] displacementmap;
        private int mapSize;

        public LandscapeBase(SceneContainer scene, String fileName, Vector3 pos, Matrix rotation, Vector3 scale, int mapSize) : base(scene, pos, rotation, scale)
        {
            this.scene = scene;
            this.fileName = fileName;
            this.mapSize = mapSize;          
            setObject(pos.X, pos.Y, pos.Z);
            /*
            initVertexBuffer();
            initIndexBuffer();
            */
        }

        public override void drawIndexedPrimitives()
        {
            throw new NotImplementedException();
        }

        public override void initIndexBuffer()
        {
            throw new NotImplementedException();
        }
        public override void initVertexBuffer()
        {
            throw new NotImplementedException();
        }

        public void readHeightmap()
        {
            bool raw = false;
            if (fileName.Split('.').Length > 1 && fileName.Split('.')[1].ToLower() == "raw")
            {
                raw = true;
            }
            else
            {
                raw = false;
            }
            if (raw)
            {
                //raw file
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                //mapSize = (int)Math.Sqrt(fs.Length) - 1;         
                r = new BinaryReader(fs);
                heightmap = new float[mapSize * mapSize];
                displacementmap = new float[mapSize * mapSize];
                for (int i = 0; i < mapSize; i++)
                {
                    for (int j = 0; j < mapSize; j++)
                    {
                        float height = r.ReadByte();
                        if (i == 0 || j == 0 || i == mapSize - 1 || j == mapSize - 1)
                        {
                            height = 0.0f;
                        }
                        heightmap[(mapSize - 1 - i) * mapSize + (mapSize - 1 - j)] = displacementmap[(mapSize - 1 - i) * i + (mapSize - 1 - j)] = height * scale.Y;
                    }
                }
                r.Close();
            }
            else
            {
                heightmap = new float[mapSize * mapSize];
                displacementmap = new float[mapSize * mapSize];
                Microsoft.Xna.Framework.Color[] heightmapColor = new Microsoft.Xna.Framework.Color[mapSize * mapSize];
                Texture2D t = (Texture2D)scene.TextureManager.getTexture(fileName);
                t.GetData<Microsoft.Xna.Framework.Color>(heightmapColor);
                int c = (int)Math.Sqrt(heightmapColor.Length);
                if (c < mapSize)
                {
                    //interpolate
                }
                for (int i = 0; i < mapSize; i++)
                {
                    for (int j = 0; j < mapSize; j++)
                    {
                        heightmap[j + i * mapSize] = displacementmap[j + i * mapSize] = heightmapColor[j + i * mapSize].R * scale.Y;
                    }
                }
            }
        }

        public void smoothTerrain(int smoothingPasses)
        {
            if (smoothingPasses <= 0)
            {
                return;
            }
            float[] newHeightData;

            for (int passes = (int)smoothingPasses; passes > 0; --passes)
            {
                newHeightData = new float[mapSize * mapSize];

                for (int x = 0; x < this.mapSize; ++x)
                {
                    for (int z = 0; z < this.mapSize; ++z)
                    {
                        int adjacentSections = 0;
                        float sectionsTotal = 0.0f;

                        int xMinusOne = x - 1;
                        int zMinusOne = z - 1;
                        int xPlusOne = x + 1;
                        int zPlusOne = z + 1;
                        bool bAboveIsValid = zMinusOne > 0;
                        bool bBelowIsValid = zPlusOne < mapSize;

                        // =================================================================
                        if (xMinusOne > 0)            // Check to left
                        {
                            sectionsTotal += this.heightmap[xMinusOne + z * mapSize];
                            ++adjacentSections;

                            if (bAboveIsValid)        // Check up and to the left
                            {
                                sectionsTotal += this.heightmap[xMinusOne + zMinusOne * mapSize];
                                ++adjacentSections;
                            }

                            if (bBelowIsValid)        // Check down and to the left
                            {
                                sectionsTotal += this.heightmap[xMinusOne + zPlusOne * mapSize];
                                ++adjacentSections;
                            }
                        }
                        if (xPlusOne < mapSize)     // Check to right
                        {
                            sectionsTotal += this.heightmap[xPlusOne + z * mapSize];
                            ++adjacentSections;

                            if (bAboveIsValid)        // Check up and to the right
                            {
                                sectionsTotal += this.heightmap[xPlusOne + zMinusOne * mapSize];
                                ++adjacentSections;
                            }

                            if (bBelowIsValid)        // Check down and to the right
                            {
                                sectionsTotal += this.heightmap[xPlusOne + zPlusOne * mapSize];
                                ++adjacentSections;
                            }
                        }
                        if (bAboveIsValid)            // Check above
                        {
                            sectionsTotal += this.heightmap[x + zMinusOne * mapSize];
                            ++adjacentSections;
                        }
                        if (bBelowIsValid)    // Check below
                        {
                            sectionsTotal += this.heightmap[x + zPlusOne * mapSize];
                            ++adjacentSections;
                        }
                        newHeightData[x + z * mapSize] = (this.heightmap[x + z * mapSize] + (sectionsTotal / adjacentSections)) * 0.5f;
                    }
                }

                // Overwrite the HeightData info with our new smoothed info
                for (int x = 0; x < this.mapSize; ++x)
                    for (int z = 0; z < this.mapSize; ++z)
                    {
                        this.heightmap[x + z * mapSize] = newHeightData[x + z * mapSize];
                    }
            }
        }

        public void hole(float x, float z)
        {
            int iH = (int)(x / scale.X);
            int jH = (int)(-z / scale.Z);
            for (int k = 0; k < 20; k++)
            {
                for (int l = 0; l < 20; l++)
                {
                    displacementmap[(iH + l) + (jH + k) * mapSize] -= 1.0f;
                }
            }
            effectContainer.updateUniformData("displacement", displacementmap);
            /*
            for (int j = 0; j < numPatchesPerSide; j++)
            {
                for (int i = 0; i < numPatchesPerSide; i++)
                {
                    m_Patches[i, j].drawIndexedPrimitives();
                }
            }
            */
        }
    }
}
