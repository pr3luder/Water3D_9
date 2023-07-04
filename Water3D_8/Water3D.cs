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

#region Using directives

using System;
using System.Drawing;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Timers;

#endregion

namespace Water3D
{
	public class Water3D : Object3D
	{
		private VertexBuffer vb;
        private VertexBuffer vbIndexed;
        private IndexBuffer ib;
        private Vector3[] tempNormal;
        private Vector3[] tempDiffVec;
        private Vector3 normal;
		float x, z, xCoord, zCoord;
		float zTex = 0.0f;
		float xTex = 0.0f;

        private Timer timer;
		double counter = 0.0; // increased when time ticks
		float[,] heightmap;

		//properties that can be influenced by console (textfield)
        private double counterStep = 0.1; //time on, time off, time 1.0
        private int timerTick = 10;    //how fast is time (in ms), timevelocity 10 
        private float xgrid = 32.0f; // grid in x-direction, xgrid 1
        private float zgrid = 32.0f; // grid in z direction  (lh coordinate system) zgrid 1
        private float xStep = 1.0f; // x-unit of coordinate system
        private float zStep = 1.0f; // z-unit of coordinate system
        private ushort quadNum = 64; //number of quads in wave, quads 40
		//properties
        public int gNumTrisRendered = 0;

        private ushort[] index;
        private VertexPositionNormalTexture[] verts;
        private VertexPositionNormalTexture[] indexedVerts;
        private FunctionCompiler compiler;


        public Water3D(SceneContainer scene, RenderEngine engine, Vector3 pos, Matrix rotation, Vector3 scale, float xgrid, float zgrid, ushort quadNum) 
            : base(scene, pos, rotation, scale)
		{
            this.quadNum = quadNum;
            this.xgrid = xgrid;
            this.zgrid = zgrid;
            tempDiffVec = new Vector3[8];
			tempNormal = new Vector3[8];

			// Compiler of function code and parser of console commands
			// must have all objects that you want to manipulate
			compiler = new FunctionCompiler(this, engine);

			// initialize vertex buffer
            this.vb = new VertexBuffer(scene.Game.GraphicsDevice, typeof(VertexPositionNormalTexture), quadNum * quadNum * 6, BufferUsage.None);
			heightmap = new float[quadNum, quadNum];

			worldMatrix = Matrix.Identity;
			transMatrix = Matrix.Identity;

			//initialize timer
			timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
			timer.Enabled = true;
			timer.Interval = timerTick;
			timer.Start();

			//initialize vertices and texture maps
            verts = new VertexPositionNormalTexture[quadNum * quadNum * 6];

			RenderEngine.Compile.OnCompileHandler += new CompileEvent.CompileEventHandler(CompileEventProcessor);
			initVertexBuffer();
            initIndexBuffer();
            setObject(pos.X, pos.Y, pos.Z); 
            scaleObject(scale.X, scale.Y, scale.Z);
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            counter += counterStep;
        }

		public FunctionCompiler getCompiler()
		{
			return compiler;
		}

		/// <summary>
		/// Chapter 4.3.2, Listing 4.3.15
		/// create waves in vertex buffer
		/// </summary>
        public override void Draw(GameTime time)
        {
            base.Draw(time);
            drawIndexedPrimitives();
        }

		public void drawPrimitives()
		{
            int i = 0;
			// compute vertices (esp. y-direction) dependent on function,
			// put them into vertex buffer and draw them
			// xTex and zTex for correct texture coordinates,pos is middle of water plane
            x = 0.0f;
            z = 0.0f;
			xTex = 0.0f;
			zTex = 0.0f;

			// first calculate heightmap
			// perhaps we can use it later in shader, and just factor 2 slower
            for (int zDir = 0; zDir < quadNum; zDir += 1)
			{
				for (int xDir = 0; xDir < quadNum; xDir += 1)
				{
					heightmap[xDir, zDir] = pos.Y + (float)compiler.plotFunction(x, z, counter);
					x += xgrid;
				}
				z += zgrid;
				x = 0.0f;
			}

			//   a quad, order of vertices 1-6
			//     3---2,6 
			//      |\  |
			//      | \ |	
			//      |  \|	
			//     1,4--5

			x = 0.0f;
            z = 0.0f;
			for (int zDir = 0; zDir < quadNum - 1; zDir += 1)
			{
				for (int xDir = 0; xDir < quadNum - 1; xDir += 1)
				{
                    if ((scene != null && scene.Camera != null) 
                        && (Tools3D.isVertexInFrustum( scene.Camera, new Vector3(x, heightmap[xDir, zDir], z), -30.0f)
                        || Tools3D.isVertexInFrustum( scene.Camera, new Vector3(x + xgrid, heightmap[xDir + 1, zDir], z), -30.0f)
                        || Tools3D.isVertexInFrustum( scene.Camera, new Vector3(x, heightmap[xDir, zDir + 1], z + zgrid), -30.0f)
                        || Tools3D.isVertexInFrustum( scene.Camera, new Vector3(x + xgrid, heightmap[xDir + 1, zDir + 1], z + zgrid), -30.0f)))
                    {
                        //first triangle of quad
                        gNumTrisRendered++;

                        verts[i].Position.X = x;
                        verts[i].Position.Z = -z + zgrid;
                        verts[i].Position.Y = heightmap[xDir, zDir + 1];
                        verts[i].TextureCoordinate.X = xTex / ((float)quadNum * xgrid);
                        verts[i].TextureCoordinate.Y = (zTex + zgrid) / ((float)quadNum * zgrid);
                        
                        verts[i + 1].Position.X = x + xgrid;
                        verts[i + 1].Position.Z = -z;
                        verts[i + 1].Position.Y = heightmap[xDir + 1, zDir];
                        verts[i + 1].TextureCoordinate.X = (xTex + xgrid) / ((float)quadNum * xgrid);
                        verts[i + 1].TextureCoordinate.Y = zTex / ((float)quadNum * zgrid);
                        
                        verts[i + 2].Position.X = x;
                        verts[i + 2].Position.Z = -z;
                        verts[i + 2].Position.Y = heightmap[xDir, zDir];
                        verts[i + 2].TextureCoordinate.X = xTex / ((float)quadNum * xgrid);
                        verts[i + 2].TextureCoordinate.Y = zTex / ((float)quadNum * zgrid);
                        
                        // compute normal of first triangle (counter clockwise)
                        normal = Vector3.Cross(verts[i + 1].Position - verts[i].Position, verts[i + 2].Position - verts[i].Position);
                        //normal = new Vector3(0.0f, 1.0f, 0.0f);
                        normal.Normalize();
                        verts[i].Normal = normal;
                        verts[i + 1].Normal = normal;
                        verts[i + 2].Normal = normal;
                        
                        //next triangle
                        gNumTrisRendered++;
                        i += 3;
                        verts[i].Position.X = x;
                        verts[i].Position.Z = -z + zgrid;
                        verts[i].Position.Y = heightmap[xDir, zDir + 1];
                        verts[i].TextureCoordinate.X = xTex / ((float)quadNum * xgrid);
                        verts[i].TextureCoordinate.Y = (zTex + zgrid) / ((float)quadNum * zgrid);

                        verts[i + 1].Position.X = x + xgrid;
                        verts[i + 1].Position.Z = -z + zgrid;
                        verts[i + 1].Position.Y = heightmap[xDir + 1, zDir + 1];
                        verts[i + 1].TextureCoordinate.X = (xTex + xgrid) / ((float)quadNum * xgrid);
                        verts[i + 1].TextureCoordinate.Y = (zTex + zgrid) / ((float)quadNum * zgrid);

                        verts[i + 2].Position.X = x + xgrid;
                        verts[i + 2].Position.Z = -z;
                        verts[i + 2].Position.Y = heightmap[xDir + 1, zDir];
                        verts[i + 2].TextureCoordinate.X = (xTex + xgrid) / ((float)quadNum * xgrid);
                        verts[i + 2].TextureCoordinate.Y = zTex / ((float)quadNum * zgrid);
                        
                        // compute normal of second triangle (counter clockwise)
                        normal = Vector3.Cross(verts[i + 1].Position - verts[i].Position, verts[i + 2].Position - verts[i].Position);
                        //normal = new Vector3(0.0f, 1.0f, 0.0f);
                        normal.Normalize();
                        verts[i].Normal = normal;
                        verts[i + 1].Normal = normal;
                        verts[i + 2].Normal = normal;
                        
                       i += 3;                      
                    }
                    xTex += xgrid;
					x += xgrid;
				}
				z += zgrid;     // begins at position minus quad_num / 2
				zTex += zgrid; // // begins at position 0
				xTex = 0.0f;
				x = 0.0f;
			}

            // Chapter 4.3.2, Listing 4.3.14
            // write vertices to vertex buffer and use index buffer
            vb.SetData<VertexPositionNormalTexture>(verts);
            scene.Game.GraphicsDevice.SetVertexBuffer(vb);
            render();
            
		}
         
		/// <summary>
		/// Chapter 4.3.2, Lisitng 4.3.11
		/// compute vertices (esp. y-direction) dependent on function and normals,
		/// put them into vertex buffer and draw them.
		/// xTex and zTex are for correct texture coordinates,
		/// pos is middle of water plane,
		/// xCoord, zCoord are for function coordinate system
		/// </summary>
		public override void drawIndexedPrimitives()
		{
			// counter for vertices
			int i = 0;
   
            x = 0.0f;
            z = 0.0f;
            // first calculate heightmap
            // perhaps we can use it later in shader, and just factor 2 slower
            for (int zDir = 0; zDir < quadNum; zDir += 1)
            {
                for (int xDir = 0; xDir < quadNum; xDir += 1)
                {
                    heightmap[xDir, zDir] = pos.Y + (float)compiler.plotFunction(x, z, counter);
                    x += xgrid;
                }
                z += zgrid;
                x = 0.0f;
            }

			// Chapter 4.3.2, Listing 4.3.12
			// calculate position of vertices
            x = 0.0f;
            z = 0.0f;
			// points of function coordinate system
            xCoord = 0.0f;
            zCoord = 0.0f;
			// texture coordinates
			xTex = 0.0f;
			zTex = 0.0f;
			for (int zDir = 0; zDir < quadNum; zDir += 1)
			{
				for (int xDir = 0; xDir < quadNum; xDir += 1)
				{
					//heightmap[xDir, zDir] = pos.Y + (float)compiler.plotFunction(xCoord, zCoord, counter);
                    // just draw vertices once, the rest does index buffer 
					// take xCoord and zCoord as argument(function coordinate system)
                    gNumTrisRendered++;
                    indexedVerts[i].Position.X = x;
                    indexedVerts[i].Position.Z = -z;
                    indexedVerts[i].Position.Y = heightmap[xDir, zDir];
                    indexedVerts[i].TextureCoordinate.X = xTex / ((float)quadNum * xgrid);
                    indexedVerts[i].TextureCoordinate.Y = zTex / ((float)quadNum * zgrid);
                    //indexedVerts[i].Normal = new Vector3(0.0f, 1.0f, 0.0f);
                    i++;
                    x += xgrid; // side length of one quad
                    xCoord += xStep; // one unit in function coordinate system
                    xTex += xgrid; // texure coordinates
				}
				z += zgrid;
				zCoord += zStep;
				zTex += zgrid;

				xTex = 0.0f;
                x = 0.0f;
                xCoord = 0.0f;
			}
            computeGouraudNormals(indexedVerts);
			
			// Chapter 4.3.2, Listing 4.3.14
			// write vertices to vertex buffer and use index buffer
            vbIndexed.SetData<VertexPositionNormalTexture>(indexedVerts);
            scene.Game.GraphicsDevice.Indices = ib;
            scene.Game.GraphicsDevice.SetVertexBuffer(vbIndexed);
            render();
		}

        private void render()
        {
            updateEffects();
            foreach (EffectPass pass in effectContainer.getEffect().CurrentTechnique.Passes)
            {
                pass.Apply();
                if (effectContainer != null && effectContainer.getEffect().GetType() == typeof(BasicEffect))
                {
                    scene.Game.GraphicsDevice.Textures[0] = scene.TextureManager.getTexture("baseWater");
                }
                scene.Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, quadNum * quadNum, 0, quadNum * quadNum * 2);
            }
        }

        private void computeGouraudNormals(VertexPositionNormalTexture[] vertexArray)
        {
            // compute (gouraud) normals
            // use 8 vertices around vertex to compute normals	
            for (int zDir = 0; zDir < quadNum; zDir += 1)
            {
                for (int xDir = 0; xDir < quadNum; xDir += 1)
                {

                    // for the edge
                    if (xDir == 0 || zDir == 0 || xDir == quadNum - 1 || zDir == quadNum - 1)
                    {
                        vertexArray[zDir * quadNum + xDir].Normal = new Vector3(0.0f, 1.0f, 0.0f);
                    }
                    else
                    {
                        //6---7---8
                        //|   |   |
                        //3---4---5
                        //|   |   |
                        //0---1---2

                        //0-4
                        tempDiffVec[0] = vertexArray[(zDir - 1) * quadNum + xDir - 1].Position - vertexArray[zDir * quadNum + xDir].Position;
                        //1-4
                        tempDiffVec[1] = vertexArray[(zDir - 1) * quadNum + xDir].Position - vertexArray[zDir * quadNum + xDir].Position;
                        //2-4
                        tempDiffVec[2] = vertexArray[(zDir - 1) * quadNum + xDir + 1].Position - vertexArray[zDir * quadNum + xDir].Position;
                        //5-4
                        tempDiffVec[3] = vertexArray[zDir * quadNum + xDir + 1].Position - vertexArray[zDir * quadNum + xDir].Position;
                        //8-4
                        tempDiffVec[4] = vertexArray[(zDir + 1) * quadNum + xDir + 1].Position - vertexArray[zDir * quadNum + xDir].Position;
                        //6-4
                        tempDiffVec[5] = vertexArray[(zDir + 1) * quadNum + xDir - 1].Position - vertexArray[zDir * quadNum + xDir].Position;
                        //7-4
                        tempDiffVec[6] = vertexArray[(zDir + 1) * quadNum + xDir].Position - vertexArray[zDir * quadNum + xDir].Position;
                        //3-4
                        tempDiffVec[7] = vertexArray[zDir * quadNum + xDir - 1].Position - vertexArray[zDir * quadNum + xDir].Position;

                        //0-4 x 1-4
                        tempNormal[0] = Vector3.Cross(tempDiffVec[0], tempDiffVec[1]);
                        //3-4 x 0-4
                        tempNormal[1] = Vector3.Cross(tempDiffVec[7], tempDiffVec[0]);
                        //6-4 x 3-4
                        tempNormal[2] = Vector3.Cross(tempDiffVec[5], tempDiffVec[7]);
                        //7-4 x 6-4
                        tempNormal[3] = Vector3.Cross(tempDiffVec[6], tempDiffVec[5]);
                        //8-4 x 7-4
                        tempNormal[4] = Vector3.Cross(tempDiffVec[4], tempDiffVec[6]);
                        //5-4 x 8-4
                        tempNormal[5] = Vector3.Cross(tempDiffVec[3], tempDiffVec[4]);
                        //2-4 x 5-4
                        tempNormal[6] = Vector3.Cross(tempDiffVec[2], tempDiffVec[3]);
                        //1-4 x 2-4
                        tempNormal[7] = Vector3.Cross(tempDiffVec[1], tempDiffVec[2]);

                        normal = (1.0f / 8.0f) * (tempNormal[0] + tempNormal[1] + tempNormal[2] + tempNormal[3] + tempNormal[4] + tempNormal[5] + tempNormal[6] + tempNormal[7]);
                        normal.Normalize();
                        vertexArray[zDir * quadNum + xDir].Normal = normal;
                    }
                }
            }
        }

		/// <summary>
		/// compile text in console to get function of wave, or execute
		/// other commands
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CompileEventProcessor(Object sender, CompileEventArgs myEventArgs)
		{
			compiler.compile(myEventArgs.Subject);
		}

		public override float getTime()
		{
			return (float)counter;
		}

        public override float Time
        {
            get
            {
                return (float)counter;
            }
        }

		public void setPosition(float x, float y, float z)
		{
			pos.X = x;
			pos.Y = y;
			pos.Z = z;
		}

		public void changeTimerProperty(bool timeSwitch)
		{
			if (timeSwitch)
				counterStep = 0.1;
			else
				counterStep = 0.0;
		}

        public void changeQuadNumber(ushort quad_num)
		{
			this.quadNum = quad_num;
			// initialize vertex buffer and vertices, 
			// important if new quad value is bigger than init value
			heightmap = new float[quad_num, quad_num];
            this.vb = new VertexBuffer(scene.Game.GraphicsDevice, typeof(VertexPositionNormalTexture), quad_num * quad_num * 6, BufferUsage.None);
            verts = new VertexPositionNormalTexture[quad_num * quad_num * 6];
            initIndexBuffer();
            initVertexBuffer();
		}

        public override void initVertexBuffer()
        {
            indexedVerts = new VertexPositionNormalTexture[quadNum * quadNum];
            vbIndexed = new VertexBuffer(scene.Game.GraphicsDevice, typeof(VertexPositionNormalTexture), quadNum * quadNum, BufferUsage.None);
        }
		/// <summary>
		/// Chapter 4.3.2, Listing 4.3.13
		/// initialize index and vertex buffer
		/// </summary>
		public override void initIndexBuffer()
		{
			// counter of indices 
			int j = 0;
			// for index buffer
			index = new ushort[quadNum * quadNum * 6];
            ib = new IndexBuffer(scene.Game.GraphicsDevice, typeof(ushort), quadNum * quadNum * 6, BufferUsage.None);

			//   a quad, order of vertices 0-5
			//    2,6---1,5 
			//      |  /|
			//      | / |	
			//      |/  |	
			//    0,3---4 
			//fill index buffer with these references
            for (ushort z = 0; z < quadNum - 1; z++)
			{
                for (ushort x = 0; x < quadNum - 1; x++)
				{
					// first triangle of first quad (counter clockwise)
					index[j] = (ushort)(z * quadNum + x);
                    index[j + 1] = (ushort)((z + 1) * quadNum + x + 1);
                    index[j + 2] = (ushort)((z + 1) * quadNum + x);
					j += 3;
					// second triangle of  first quad (counter clockwise)
                    index[j] = (ushort)(z * quadNum + x);
                    index[j + 1] = (ushort)(z * quadNum + x + 1);
                    index[j + 2] = (ushort)((z + 1) * quadNum + x + 1);
					j += 3;
				}
			}
            ib.SetData<ushort>(index);
		}

		public void changeXGrid(float xgrid)
		{
			this.xgrid = xgrid;
		}

		public void changeZGrid(float zgrid)
		{
			this.zgrid = zgrid;
		}

		public void changeXStep(float xStep)
		{
			this.xStep = xStep;
		}

		public void changeZStep(float zStep)
		{
			this.zStep = zStep;
		}

		protected void makeTexture(Texture2D tex, uint dx, uint dy)
		{
            uint[] buffer = new uint[dy * dx];                
			for (uint y = 0; y < dy; y++)
			{
				uint offset = y * dx;

				for (uint x = 0; x < dx; )
				{
					uint b = 127;
					uint g = 127;
					uint r = 127;
					uint a = 127;

					buffer[offset + x] = ((a << 24) + (r << 16) + (g << 8) + (b));
					x++;
				}
			}
            tex.SetData<uint>(buffer);
		}
	}
}

