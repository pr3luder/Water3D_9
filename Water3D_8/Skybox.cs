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

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
#endregion

namespace Water3D
{
	/// <summary>
	/// the horizon of the scene
	/// sets a cube around camera with
	/// the illusion of a sphere (special textures)
	/// it moves with camera so its infinitely far away
	/// </summary>
	public class Skybox : Object3D
	{
        private VertexBuffer vb;
		private Texture2D frontTexture;
        private Texture2D leftTexture;
        private Texture2D topTexture;
        private Texture2D bottomTexture;
        private Texture2D rightTexture;
        private Texture2D backTexture;
        private TextureCube cube;
        private float width;
        private float depth;

        public Skybox(SceneContainer scene, Vector3 pos, Matrix rotation, Vector3 scale, float width, float depth, Object3DSettings renderSettings,
            Texture2D backTexture,
            Texture2D frontTexture,
            Texture2D rightTexture,
            Texture2D leftTexture,
            Texture2D topTexture,
            Texture2D bottomTexture) : base(scene, pos, rotation, scale, renderSettings)
        {
            this.scene = scene;
            this.width = width;
            this.depth = depth;
            initVertexBuffer();
            // Chapter 4.3.1, Listing 4.3.7
            // load textures
            this.backTexture = backTexture;
            this.frontTexture = frontTexture;
            this.rightTexture = rightTexture;
            this.leftTexture = leftTexture;
            this.topTexture = topTexture;
            this.bottomTexture = bottomTexture;
        }

        public Skybox(SceneContainer scene, Vector3 pos, Matrix rotation, Vector3 scale, float width, float depth) : base(scene, pos, rotation, scale)
		{
            this.scene = scene;
            this.width = width;
            this.depth = depth;
            initVertexBuffer();
			// Chapter 4.3.1, Listing 4.3.7
			// load textures
            backTexture = scene.Game.Content.Load<Texture2D>("Textures/skybox0001");
            frontTexture = scene.Game.Content.Load<Texture2D>("Textures/skybox0002");
            rightTexture = scene.Game.Content.Load<Texture2D>("Textures/skybox0003");
            leftTexture = scene.Game.Content.Load<Texture2D>("Textures/skybox0004");
            topTexture = scene.Game.Content.Load<Texture2D>("Textures/skybox0005");
            bottomTexture = scene.Game.Content.Load<Texture2D>("Textures/skybox0006");
		}

        public Skybox(SceneContainer scene, Vector3 pos, Matrix rotation, Vector3 scale, float width, float depth, Object3DSettings renderSettings) : base(scene, pos, rotation, scale, renderSettings)
		{
            this.scene = scene;
            this.width = width;
            this.depth = depth;
            initVertexBuffer();
			// Chapter 4.3.1, Listing 4.3.7
			// load textures
            backTexture = scene.Game.Content.Load<Texture2D>("Textures/skybox0001");
            frontTexture = scene.Game.Content.Load<Texture2D>("Textures/skybox0002");
            rightTexture = scene.Game.Content.Load<Texture2D>("Textures/skybox0003");
            leftTexture = scene.Game.Content.Load<Texture2D>("Textures/skybox0004");
            topTexture = scene.Game.Content.Load<Texture2D>("Textures/skybox0005");
            bottomTexture = scene.Game.Content.Load<Texture2D>("Textures/skybox0006");
            //cube = new TextureCube(scene.GraphicsDevice, 1024, true, SurfaceFormat.Dxt5);
		}

        public override void Initialize() {          
            base.Initialize();
        }
        /// <summary>
        /// Chapter 4.3.1, Listing 4.3.9 
        /// render skybox with lighting and z - buffer off
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            setSamplerStates();
            setRasterizerStates();
            setRenderStates();


            base.Draw(gameTime);
            
            drawIndexedPrimitives();
                

            resetSamplerStates();
            resetRasterizerStates();
            resetRenderStates();
        }

		/// <summary>
		/// Chapter 4.3.1, Listing 4.3.10
		/// draw the primitives and set textures
		/// </summary>
       
		public override void drawIndexedPrimitives()
		{
            if (effectContainer.getEffect().GetType() == typeof(BasicEffect))
            {
                // make the skybox be infinitely far away
                ((BasicEffect)effectContainer.getEffect()).World = Matrix.CreateTranslation(scene.Camera.VEye);
                ((BasicEffect)effectContainer.getEffect()).View = scene.Camera.MView;
                ((BasicEffect)effectContainer.getEffect()).Projection = scene.Camera.MProjection;

                ((BasicEffect)effectContainer.getEffect()).LightingEnabled = false;
                ((BasicEffect)effectContainer.getEffect()).TextureEnabled = true;

                scene.Game.GraphicsDevice.Indices = null;
                scene.Game.GraphicsDevice.SetVertexBuffer(vb);
                foreach (EffectPass pass in ((BasicEffect)effectContainer.getEffect()).CurrentTechnique.Passes)
                {
                    pass.Apply();
                    scene.Game.GraphicsDevice.Textures[0] = backTexture;
                    scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    scene.Game.GraphicsDevice.Textures[0] = rightTexture;
                    scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 6, 2);
                    scene.Game.GraphicsDevice.Textures[0] = topTexture;
                    scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 12, 2);
                    scene.Game.GraphicsDevice.Textures[0] = bottomTexture;
                    scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 18, 2);
                    scene.Game.GraphicsDevice.Textures[0] = leftTexture;
                    scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 24, 2);
                    scene.Game.GraphicsDevice.Textures[0] = frontTexture;
                    scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 30, 2);
                }
                ((BasicEffect)effectContainer.getEffect()).LightingEnabled = true;
            }
            
		}

        public void drawIndexedPrimitivesTest()
        {
            if (effectContainer.getEffect().GetType() == typeof(BasicEffect))
            {
                // make the skybox be infinitely far away
                ((BasicEffect)effectContainer.getEffect()).World = Matrix.CreateTranslation(scene.Camera.VEye);
                ((BasicEffect)effectContainer.getEffect()).View = scene.Camera.MView;
                ((BasicEffect)effectContainer.getEffect()).Projection = scene.Camera.MProjection;

                ((BasicEffect)effectContainer.getEffect()).LightingEnabled = false;
                ((BasicEffect)effectContainer.getEffect()).TextureEnabled = true;

                scene.Game.GraphicsDevice.Indices = null;
                scene.Game.GraphicsDevice.SetVertexBuffer(vb);
                foreach (EffectPass pass in ((BasicEffect)effectContainer.getEffect()).CurrentTechnique.Passes)
                {
                    pass.Apply();
                    scene.Game.GraphicsDevice.Textures[0] = backTexture;
                    scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    scene.Game.GraphicsDevice.Textures[0] = rightTexture;
                    scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 6, 2);
                    scene.Game.GraphicsDevice.Textures[0] = topTexture;
                    scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 12, 2);
                    scene.Game.GraphicsDevice.Textures[0] = bottomTexture;
                    scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 18, 2);
                    scene.Game.GraphicsDevice.Textures[0] = leftTexture;
                    scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 24, 2);
                    scene.Game.GraphicsDevice.Textures[0] = frontTexture;
                    scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 30, 2);
                }
                ((BasicEffect)effectContainer.getEffect()).LightingEnabled = true;
            }

        }

        public override float getTime()
		{
			return 0.0f;
		}

        public override void initVertexBuffer()
        {
            // Chapter 4.3.1, Listing 4.3.5
            // initialize vertex buffer
            this.vb = new VertexBuffer(scene.Game.GraphicsDevice, typeof(VertexPositionNormalTexture), 36, BufferUsage.None);
            // Chapter 4.3.1, Listing 4.3.6
            // fill vertex array with vertex attributes
            VertexPositionNormalTexture[] verts = 
			{
			    new VertexPositionNormalTexture(new Vector3( - width,+ width, - depth), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(1.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3( - width,- width, - depth), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(1.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( + width,- width, - depth), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( + width,- width, - depth), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( + width,+ width, - depth), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3( - width,+ width, - depth), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(1.0f,0.0f)), // front 
    			
			    new VertexPositionNormalTexture(new Vector3( + depth,+ width, - width), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(1.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3( + depth,- width, - width), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(1.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( + depth,- width, + width), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(0.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( + depth,- width, + width), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(0.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( + depth,+ width, + width), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(0.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3( + depth,+ width, - width), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(1.0f,0.0f)), // right

			    new VertexPositionNormalTexture(new Vector3( - width,+ depth, - width), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(0.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3( - width,+ depth, + width), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(0.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( + width,+ depth, + width), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(1.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( + width,+ depth, + width), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(1.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( + width,+ depth, - width), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(1.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3( - width,+ depth, - width), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(0.0f,0.0f)), // top 
    			
			    new VertexPositionNormalTexture(new Vector3( - width,- depth, - width), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( - width,- depth, + width), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3( + width,- depth, + width), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3( + width,- depth, + width), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3( + width,- depth, - width), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( - width,- depth, - width), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f,1.0f)), // bottom
    			
			    new VertexPositionNormalTexture(new Vector3( - depth,+ width, + width), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3( - depth,- width, + width), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( - depth,- width, - width), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(0.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( - depth,- width, - width), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(0.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( - depth,+ width, - width), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(0.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3( - depth,+ width, + width), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1.0f,0.0f)), // left 

                new VertexPositionNormalTexture(new Vector3( + width,+ width, + depth), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(1.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3( + width,- width, + depth), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(1.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( - width,- width, + depth), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(0.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( - width,- width, + depth), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(0.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3( - width,+ width, + depth), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(0.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3( + width,+ width, + depth), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(1.0f,0.0f)) // back
			};
            // Chapter 4.3.1, Listing 4.3.8
            // write vertices to vertex buffer
            vb.SetData<VertexPositionNormalTexture>(verts);
        }

        public override void initIndexBuffer()
        {
            throw new NotImplementedException();
        }
    }
}
