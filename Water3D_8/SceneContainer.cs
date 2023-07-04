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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections;

#endregion

namespace Water3D
{
	public class SceneContainer : DrawableGameComponent
	{
        private Camera camera;
        private LandscapeGeomipmap landscape;
        private List<Object3D> objectsList;
        private Hashtable reflectionsList;
        private Hashtable refractionsList;
        private Hashtable objectsMirrorList;
        private Hashtable objectsRefractList;
        private TextureRenderer reflect;
        private TextureRenderer refract;
        private TextureManager textureManager;
        private float seaHeight = 0.0f;
        public SceneContainer(Camera camera, TextureManager textureManager) : base(RenderEngine.Game)
		{
            this.camera = camera;
            this.camera.setScene(this);
            this.textureManager = textureManager;
            objectsList = new List<Object3D>();
            reflectionsList = new Hashtable();
            refractionsList = new Hashtable();
            objectsMirrorList = new Hashtable();
            objectsRefractList = new Hashtable();
		}

        public SceneContainer(TextureManager textureManager) : this(null, textureManager)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            reflect = new TextureRenderer(this.GraphicsDevice, camera);
            refract = new TextureRenderer(this.GraphicsDevice, camera);
        }

        public void reset()
        {
            reflect.reset();
            refract.reset();
        }

        public void setCamera(Camera camera)
        {
            this.camera = camera;
        }

        public List<Object3D> getObjects()
		{
            return objectsList;
		}

        public void addObject(Object3D obj)
        {
            if (!objectsList.Contains(obj))
            {
                obj.Initialize();
                if(obj.GetType() == typeof(LandscapeGeomipmap))
                {
                    landscape = (LandscapeGeomipmap)obj;
                }
                objectsList.Add(obj);
                obj.setScene(this);
            }
        }

        public void renderReflections(GameTime gameTime)
        {
            //reflections
            foreach (String textureName in reflectionsList.Keys)
            {
                Texture2D t1 = reflect.renderEnvironmentToTexture(gameTime, (List<Object3D>)reflectionsList[textureName], viewMatrixForMirroring(((Object3D)objectsMirrorList[textureName]).getPosition().Y), projMatrixForClipping(new Plane(0.0f, -1.0f, 0.0f, ((Object3D)objectsMirrorList[textureName]).getPosition().Y)), Microsoft.Xna.Framework.Color.White);
                textureManager.addTexture(textureName, t1);
            }
        }

        public void renderRefractions(GameTime gameTime)
        {
           //refractions
            foreach (String textureName in refractionsList.Keys)
            {
                Texture2D t2 = refract.renderEnvironmentToTexture(gameTime, (List<Object3D>)refractionsList[textureName], camera.MView, projMatrixForClipping(new Plane(0.0f, -1.0f, 0.0f, -((Object3D)objectsRefractList[textureName]).getPosition().Y + 0.6f)), Microsoft.Xna.Framework.Color.White);
                textureManager.addTexture(textureName, t2);
            }
        }
        /* here we can have a control over rendered objetcs, e.g. dont draw invisible objects */
        public void renderObjects(GameTime gameTime)
        {
            foreach (Object3D o in objectsList)
            {
                o.Draw(gameTime);
            }
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Object3D o in objectsList)
            {
                o.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            renderReflections(gameTime);
            
            renderRefractions(gameTime);
            
            renderObjects(gameTime);
            /*
            debugDraw.Begin(camera.MView, camera.MProjection);
            debugDraw.DrawWireSphere(BoundingSphere, Color.Red);
            debugDraw.DrawWireBox(BoundingBox, Color.Green);
            debugDraw.End();
            */
        }


		public void initializeCamera()
		{
			
		}

        public TextureManager TextureManager
        {
            get
            {
                return textureManager;
            }
        }

        public TextureManager getTextureManager()
        {
            return textureManager;
        }

        public LandscapeGeomipmap Landscape
        {
            get
            {
                return landscape;
            }
        }


        public Camera Camera 
        {
            get
            {
                return camera;
            }
        }

		public Camera getCamera()
		{
            return camera;
		}
        

        public void setReflectionObject(String where, Object3D obj)
        {
            objectsMirrorList[where] = obj;
        }

        public void setRefractionObject(String where, Object3D obj)
        {
            objectsRefractList[where] = obj;
        }

        public void addReflection(String where, Object3D what)
        {
            reflectionsList[where] = what;
        }

        public void addRefraction(String where, Object3D what)
        {
            refractionsList[where] = what;
        }

        public void setReflection(String where, List<Object3D> what, Object3D obj)
        {
            // projection matrix for clipping away everything under water in mirrored texture
            reflectionsList[where] = what;
            objectsMirrorList[where] = obj;
        }

        public void setRefraction(String where, List<Object3D> what, Object3D obj)
        {
            refractionsList[where] = what;
            objectsRefractList[where] = obj;
        }

        private float sgn(float a)
        {
            if (a > 0.0f) return (1.0f);
            if (a < 0.0f) return (-1.0f);
            return (0.0f);
        }

        /// <summary>
        /// sets the projection matrix, so that just the landscape above water can be rendered 
        /// to texture
        /// </summary>
        /// <param name="clipPlane"></param>
        /// <returns></returns>
        public Matrix projMatrixForClipping(Plane clipPlane)
        {
            //translate to camera space
            clipPlane = Plane.Transform(clipPlane, camera.MView);

            Vector4 planeVec = new Vector4(clipPlane.Normal.X, clipPlane.Normal.Y, clipPlane.Normal.Z, clipPlane.D);
            Matrix clipProjMat;
            Vector4 q;
            // Grab the current projection matrix from D3D
            clipProjMat = camera.MProjection;

            // Calculate the clip-space corner point opposite the clipping plane
            // as (sgn(clipPlane.x), sgn(clipPlane.y), 1, 1) and
            // transform it into camera space by multiplying it
            // by the inverse of the projection matrix
            q.X = sgn(planeVec.X) / clipProjMat.M11;
            q.Y = sgn(planeVec.Y) / clipProjMat.M22;
            q.Z = 1.0f;
            q.W = (1.0f - clipProjMat.M33) / clipProjMat.M43;

            // Calculate the scaled plane vector
            Vector4 c = planeVec * (1.0f / Vector4.Dot(planeVec, q));

            // Replace the third column of the projection matrix
            clipProjMat.M13 = c.X;
            clipProjMat.M23 = c.Y;
            clipProjMat.M33 = c.Z;
            clipProjMat.M43 = c.W;

            // Return changed projection matrix
            return clipProjMat;
        }

        /// <summary>
        /// converts the plane to clip space
        /// </summary>
        /// <param name="clipPlane"></param>
        /// <returns></returns>
        public Plane planeForClipping(Plane clipPlane)
        {
            Matrix worldViewProjection = Matrix.Multiply(camera.MView, camera.MProjection);
            clipPlane = Plane.Transform(clipPlane, worldViewProjection);
            return clipPlane;
        }

        /// <summary>
        /// sets the view matrix, so that just the landscape above water can be mirrored 
        /// to texture
        /// </summary>
        /// <param name="clipPlane"></param>
        /// <returns></returns>
        public Matrix viewMatrixForMirroring(float mirrorHeight)
        {
            Matrix reflDir = Matrix.Identity;
            Matrix viewDir = camera.MView;
            reflDir.M11 = 1.0f;
            reflDir.M22 = -1.0f;
            reflDir.M33 = 1.0f;
            reflDir.M42 = 2.0f * mirrorHeight;
            reflDir.M44 = 1.0f;

            viewDir = Matrix.Multiply(reflDir, viewDir);
            return viewDir;
        }

        public TextureRenderer Reflect
        {
            get
            {
                return reflect;
            }
        }

        public TextureRenderer Refract
        {
            get
            {
                return refract;
            }
        }

        public float SeaHeight
        {
            get
            {
                return seaHeight;
            }
        }
	}
}
