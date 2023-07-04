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
using XNAQ3Lib.Q3BSP;

#endregion

namespace Water3D
{
    public class Level3D : Object3D
    {
        private Q3BSPLevel level;
        private bool levelLoaded;
        private bool renderSkybox;

        public int CurrentLeaf
        {
            get { return level.CurrentLeaf; }
        }

        public int CurrentCluster
        {
            get { return level.CurrentCluster; }
        }

        public int VisibleLeafs
        {
            get { return level.VisibleLeafs; }
        }

        public Level3D(SceneContainer scene, Vector3 pos, Matrix rotation, Vector3 scale, string levelFile, string shaderPath, string contentPath, bool renderSkybox)
            : base(scene, pos, rotation, scale)
        {
            level = new Q3BSPLevel(levelFile, Q3BSPRenderType.BSPCulling);
            //level = new Q3BSPLevel(levelFile, Q3BSPRenderType.StaticBuffer);

            if (level.LoadFromFile(levelFile))
            {
                levelLoaded = level.InitializeLevel(GraphicsDevice, scene.Game.Content, shaderPath, contentPath);
            }
            this.renderSkybox = renderSkybox;
            setObject(pos.X, pos.Y, pos.Z);
        }

        public override void Draw(GameTime time)
        {
            if (levelLoaded)
            {
                base.Draw(time);     
                level.RenderLevel(scene.Camera.VEye, World, scene.Camera.MView, scene.Camera.MProjection, time, GraphicsDevice, renderSkybox);
            }
        }

        public override void drawIndexedPrimitives()
        {
            
        }

        public void render()
        {

        }

        public override float getTime()
        {
            return 0;
        }

        public override void initVertexBuffer()
        {
            throw new NotImplementedException();
        }

        public override void initIndexBuffer()
        {
            throw new NotImplementedException();
        }

        public Q3BSPLevel Level
        {
            get
            {
                return level;
            }
        }
    }
}
