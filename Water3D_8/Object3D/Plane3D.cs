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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Timers;

#endregion

namespace Water3D
{
	public class Plane3D : Object3D
	{
        private VertexBuffer vb;
        private float width, length;
        private Timer timer;
        private float counter;
        private float counterStep;
        public Plane3D(SceneContainer scene, Vector3 pos, Matrix rotation, Vector3 scale, float width, float length)
            : base(scene, pos, rotation, scale)
		{
            this.scene = scene;
            this.pos = pos;
            this.width = width;
            this.length = length;
            this.counter = 0.0f;
            this.counterStep = 0.1f;
            this.vb = new VertexBuffer(scene.Game.GraphicsDevice, typeof(VertexPositionNormalTexture), 6, BufferUsage.None);
            VertexPositionNormalTexture[] verts = 
			{
				new VertexPositionNormalTexture(new Vector3(-width,0.0f,-width), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3(-width,0.0f,width), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3(width,0.0f,width), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3(width,0.0f,width), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f,0.0f)),
			    new VertexPositionNormalTexture(new Vector3(width,0.0f,-width), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f,1.0f)),
			    new VertexPositionNormalTexture(new Vector3(-width,0.0f,-width), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f,1.0f))
			};
            vb.SetData<VertexPositionNormalTexture>(verts);
            setObject(pos.X, pos.Y, pos.Z);
            scaleObject(scale.X, 1.0f, scale.Z);
            //initialize timer
            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;
            timer.Interval = 1;
            timer.Start();
		}

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            counter += counterStep;
        }

        public override void Draw(GameTime time)
        {
            base.Draw(time);
            drawIndexedPrimitives();
        } 

		public override void drawIndexedPrimitives()
		{
            scene.Game.GraphicsDevice.SetVertexBuffer(vb);
            render();
		}

        public void render()
        {
            if (effectContainer.getEffect().GetType() != typeof(BasicEffect))
            {
                effectContainer.updateMutable(this);
                effectContainer.drawMutable();
            }

            foreach (EffectPass pass in effectContainer.getEffect().CurrentTechnique.Passes)
            {
                pass.Apply();
                if (effectContainer.getEffect().GetType() == typeof(BasicEffect))
                {
                    scene.Game.GraphicsDevice.Textures[0] = scene.TextureManager.getTexture("baseWater");
                }
                scene.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }

        public override float getTime()
        {
            return counter;
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
