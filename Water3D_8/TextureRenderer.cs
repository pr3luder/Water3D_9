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
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;

namespace Water3D
{
    // Chapter 4.3.3, Listing 4.3.20
    // render landscape to texture   
    public class TextureRenderer
	{
        private GraphicsDevice device;
        private Camera camera;
        private Matrix renderProj;
        private Matrix renderView;
        private Matrix renderWorld;
        private RenderTarget2D textureTarget;
		/// <summary>
		/// handles rendering into a 2D - texture
		/// code for rendering into cube map is existend
		/// but not ready yet
		/// </summary>
		/// <param name="device">directx device</param>
		/// <param name="path">path to texture (not used)</param>
		/// <param name="camera">used to get projection and view matrix</param>
        public TextureRenderer(GraphicsDevice device, Camera camera)
		{
			this.device = device;
			this.camera = camera;
            //this.mirrorHeight = mirrorHeight; 
			renderProj = renderView = renderWorld = Matrix.Identity;
            reset();
        }
        
        public void reset()
        {
            PresentationParameters pp = device.PresentationParameters;
            textureTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        }

		/// <summary>
		/// Chapter 4.3.3, Listing 4.3.21
		/// render the local landscape to texture from view matrix
        /// and clipping projection matrix
		/// </summary>
		/// <param name="obj">objects to render</param>
		/// <param name="renderProj">projection matrix</param>
		/// <param name="renderView">view matrix</param>
        /// <param name="renderProj">projection matrix</param>
        /// <param name="backgrndC">color of texure</param>
		/// <returns>texture with rendered object</returns>
		public Texture2D renderEnvironmentToTexture(GameTime gameTime, List<Object3D> objects, Matrix renderView, Matrix renderProj, Microsoft.Xna.Framework.Color backgrndC)
		{
			Matrix saveProj = camera.MProjection;
			Matrix saveView = camera.MView;
            this.renderView = renderView;
            this.renderProj = renderProj;

			// set render target for texture
            device.SetRenderTarget(textureTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, backgrndC, 1.0f, 0);
            foreach (Object3D o in objects)
            {
                this.renderWorld = o.getWorldMatrix();
                camera.MView = renderView;
                camera.MProjection = renderProj;
                o.Draw(gameTime);
                camera.MView = saveView;
                camera.MProjection = saveProj;
            }
            // main render target
            device.SetRenderTarget(null);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Microsoft.Xna.Framework.Color.White, 1.0f, 0);
           
            return textureTarget;
		}

        public void screenshot(String filename)
        {
            FileStream f = new FileStream("test.png", FileMode.Create);
            textureTarget.SaveAsPng(f, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight);
        }

		public Matrix getRenderViewMatrix()
		{
			return this.renderView;
		}

		public Matrix getRenderProjectionMatrix()
		{
			return this.renderProj;
		}
		
		public Matrix getRenderWorldMatrix()
		{
			return this.renderWorld;
		}
	}
}
