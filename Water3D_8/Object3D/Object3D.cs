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
	/// <summary>
	/// base class for all 3D objects
	/// implements the most important methods
	/// and declares variables that are needed
	/// by all objects
	/// </summary>
    public abstract class Object3D : DrawableGameComponent
	{
		protected SceneContainer scene;
		protected Vector3 pos;
        protected Vector3 oldPos;
        protected Vector3 scale;
		protected Matrix worldMatrix;
		protected Matrix transMatrix;
        protected Matrix rotMatrix;
        protected Matrix scaleMatrix;
        protected Quaternion rotationQuat;
        protected EffectContainer effectContainer;
        protected float yaw;
        protected float pitch;
        protected float roll;
        protected Vector3 viewVector;
        protected Vector3 rightVector;
        protected Vector3 upVector;
        protected Vector3 localViewVector;
        protected Ray viewRay;
        protected Ray rightRay;
        protected Ray upRay;

        protected Random rand;
        protected Physics physics;
        protected BoundingSphere bs;
        protected BoundingBox bb;
        protected float currentTime;
        protected bool moving;
        protected bool fipZAxis;

        protected String mode;
        protected Model model;
        protected Matrix[] boneTransforms;
        protected float maxForwardSpeed;
        protected float maxBackwardSpeed;
        protected float maxTurningSpeed;

        protected bool isDebugMode = false;
        protected DebugDraw debugDraw = null;

        protected Object3DSettings settings;
        private List<Object3D> colliderList;

        public Object3D(Vector3 pos, Matrix rotation, Vector3 scale, Object3DSettings renderSettings, bool flipZAxis=false) : base(RenderEngine.Game)
		{
            if (renderSettings == null)
            {
                settings = new Object3DSettings();
            }
            else
            {
                settings = renderSettings;
            }
          
            maxForwardSpeed = 0.1f;
            maxBackwardSpeed = 0.1f;
            maxTurningSpeed = 0.1f;
            
            this.pos = pos;
            this.oldPos = pos; 
            this.yaw = 0.0f;
            this.pitch = 0.0f;
            this.roll = 0.0f;
            this.scale = scale;
            this.fipZAxis = flipZAxis;
          
            this.physics = null;
            this.currentTime = 0.0f;

            this.worldMatrix = Matrix.Identity;
            this.transMatrix = Matrix.Identity;
            this.rotMatrix = rotation;

            //this.scaleMatrix = Matrix.CreateScale(scale);
            rotateObject(0.0f, 1.1415f, 0.0f);
            scaleObject(scale.X, scale.Y, scale.Z);
            this.rotationQuat = Quaternion.Identity;
            rotateObjectQuaternion(rotation);

            this.viewVector = new Vector3(0.0f, 0.0f, -1.0f);
            this.rightVector = new Vector3(1.0f, 0.0f, 0.0f);
            this.upVector = new Vector3(0.0f, 1.0f, 0.0f);
            this.localViewVector = Vector3.Zero;

            viewRay = new Ray(pos, viewVector);
            rightRay = new Ray(pos, rightVector);
            upRay = new Ray(pos, upVector);

            this.moving = false;
            this.mode = "nothing";

            bs = new BoundingSphere();
            bb = new BoundingBox();

            rand = new Random();
		}

        public Object3D(SceneContainer scene, Vector3 pos, Matrix rotation, Vector3 scale, Object3DSettings renderSettings, bool flipZAxis = false) : this(pos, rotation, scale, renderSettings, flipZAxis)
        {
            scene.addObject(this); //setScene is called in addObject
        }

        public Object3D(SceneContainer scene, Vector3 pos, Matrix rotation, Vector3 scale)
            : this(scene, pos, rotation, scale, null)
        {

        }

        public void addColliderObject(Object3D obj)
        {
            colliderList.Add(obj);
        }

        public void setDebugMode(bool isDebugMode)
        {
            this.isDebugMode = isDebugMode;
            if (isDebugMode == true)
            {
                debugDraw = new DebugDraw(GraphicsDevice);
            } else
            {
                debugDraw = null;
            }
        }
        public override void Initialize()
        {
            base.Initialize();
        }

        public virtual void setScene(SceneContainer scene)
        {
            this.scene = scene;
        }

        public virtual SceneContainer getScene()
        {
            return scene;
        }

        public virtual void rotateObjectQuaternion(float rotX, float rotY, float rotZ)
        {

            // Create rotation matrix from rotation amount
            Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), rotZ) * Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), rotY) * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), rotX);
            rotationQuat *= additionalRot;

            worldMatrix = Matrix.CreateFromQuaternion(rotationQuat) * Matrix.CreateTranslation(pos);
            
            
            viewVector = worldMatrix.Forward;
            rightVector = worldMatrix.Right;
            upVector = worldMatrix.Up;
            pos = worldMatrix.Translation;
            
            
        }

        public virtual void rotateObjectQuaternion(Matrix rotation)
        {

            // Create rotation matrix from rotation amount
            Quaternion additionalRot = Quaternion.CreateFromRotationMatrix(rotation);
            rotationQuat *= additionalRot;

            worldMatrix = Matrix.CreateFromQuaternion(rotationQuat) * Matrix.CreateTranslation(pos);

            
            viewVector = worldMatrix.Forward;
            rightVector = worldMatrix.Right;
            upVector = worldMatrix.Up;
            pos = worldMatrix.Translation;
            

        }

        public virtual void moveObjectQuaternion(float transX, float transY, float transZ)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(transX, transY, transZ), rotationQuat);
            pos += addVector;
        }

        public virtual void rotateObject(float rotX, float rotY, float rotZ)
        {
            pitch += rotX;
            yaw += rotY; 
            roll += rotZ;

            Matrix rotMatrix = Matrix.CreateFromYawPitchRoll(rotY, rotX, rotZ);
            
            //rotate local coordinate system
            viewVector = Vector3.TransformNormal(viewVector, rotMatrix);
            upVector = Vector3.TransformNormal(upVector, rotMatrix);
            
            // re-calculate rightVector
            rightVector = Vector3.Cross(upVector, viewVector);

            // The same instability may cause the 3 orientation vectors may
            // also diverge. Either the Up or Direction vector needs to be
            // re-computed with a cross product to ensure orthagonality
            upVector = Vector3.Cross(viewVector, rightVector);
            
            //normalize
            viewVector.Normalize();
            upVector.Normalize();
            rightVector.Normalize();
            
        }
        
        public virtual void rotateObjectFollow(float rotX, float rotY, float rotZ)
        {   
            // Create rotation matrix from rotation amount
            
            Matrix rot = Matrix.CreateFromAxisAngle(rightVector, rotX) * Matrix.CreateRotationY(rotY) * Matrix.CreateRotationZ(rotZ);
            
            //rotate local coordinate system
            viewVector = Vector3.TransformNormal(viewVector, rot);
            upVector = Vector3.TransformNormal(upVector, rot);

            // re-calculate rightVector
            rightVector = Vector3.Cross(upVector, viewVector);

            // The same instability may cause the 3 orientation vectors may
            // also diverge. Either the Up or Direction vector needs to be
            // re-computed with a cross product to ensure orthagonality
            upVector = Vector3.Cross(viewVector, rightVector);

            //normalize
            viewVector.Normalize();
            upVector.Normalize();
            rightVector.Normalize();

        }

        public virtual void rotateObject(Vector3 v, float angle)
        {
            Matrix rot = Matrix.CreateFromAxisAngle(v, angle);

            //rotate local coordinate system
            viewVector = Vector3.TransformNormal(viewVector, rot);
            upVector = Vector3.TransformNormal(upVector, rot);

            // re-calculate rightVector
            rightVector = Vector3.Cross(upVector, viewVector);

            // The same instability may cause the 3 orientation vectors may
            // also diverge. Either the Up or Direction vector needs to be
            // re-computed with a cross product to ensure orthagonality
            upVector = Vector3.Cross(viewVector, rightVector);
            
            //normalize
            viewVector.Normalize();
            rightVector.Normalize();
            upVector.Normalize();

        }

		/// <summary>
		/// Chapter 4.3, Listing 4.3.2
		/// move object in x-, y- and z- direction relative to old position
		/// </summary>
		/// <param name="transX">offset to move in x direction</param>
		/// <param name="transY">offset to move in y direction</param>
		/// <param name="transZ">offset to move in z direction</param>
		public virtual void moveObject(float transX, float transY, float transZ)
		{
            localViewVector.X = transX;
            localViewVector.Y = transY;
            localViewVector.Z = transZ;
            
            pos.X += rightVector.X * transX;
            pos.Y += rightVector.Y * transX;
            pos.Z += rightVector.Z * transX;

            pos.X += upVector.X * transY;
            pos.Y += upVector.Y * transY;
            pos.Z += upVector.Z * transY;

            pos.X -= viewVector.X * transZ;
            pos.Y -= viewVector.Y * transZ;
            pos.Z -= viewVector.Z * transZ;
            
            
		}

        public Vector3 GetObjectFuturePosition(float transX, float transY, float transZ)
        {

            Vector3 futurePos = pos;
            futurePos.X += rightVector.X * transX;
            futurePos.Y += rightVector.Y * transX;
            futurePos.Z += rightVector.Z * transX;

            futurePos.X += upVector.X * transY;
            futurePos.Y += upVector.Y * transY;
            futurePos.Z += upVector.Z * transY;

            futurePos.X -= viewVector.X * transZ;
            futurePos.Y -= viewVector.Y * transZ;
            futurePos.Z -= viewVector.Z * transZ;

            return pos;
        }

        /// <summary>
        /// Chapter 4.3, Listing 4.3.1
        /// set object on position in world space
        /// </summary>
        /// <param name="posX">x position in world space</param>
        /// <param name="posY">y position in world space</param>
        /// <param name="posZ">z position in world space</param>
        public virtual void setObject(float posX, float posY, float posZ)
		{
            pos.X = posX;
			pos.Y = posY;
			pos.Z = posZ;       
		}

        public virtual void scaleObject(float scaleX, float scaleY, float scaleZ)
        {
            scaleMatrix = Matrix.CreateScale(scaleX, scaleY, scaleZ);
        }

        public virtual void updateEffects(Effect effect)
        {
            if (effectContainer == null || effect == null)
                return;
            effectContainer.updateMutable(this);
            effectContainer.drawMutable(effect);
        }

        public virtual void updateEffects()
        {
            if (effectContainer == null)
                return;
            effectContainer.updateMutable(this);
            effectContainer.drawMutable();
        }
        
        public override void Update(GameTime gameTime)
        {
            checkCollisions();
            base.Update(gameTime);
        }
        
        public virtual void DrawDebug()
        {
            if (debugDraw != null)
            {
                debugDraw.Begin(scene.Camera.MView, scene.Camera.MProjection);
                debugDraw.DrawWireSphere(BoundingSphere, Color.Red);
                debugDraw.DrawWireBox(BoundingBox, Color.Red);
                debugDraw.End();
            }
        }

		/// <summary>
		/// Chapter 4.3, Listing 4.3.3
		/// apply world matrix
		/// </summary>
        public override void Draw(GameTime gameTime)
        {

            DrawDebug();
            if (oldPos != pos)
            {
                moving = true;
            }
            else
            {
                moving = false;
            }
            oldPos = pos;


            
            // do translations of whole object
            if(this.fipZAxis)
            {
                worldMatrix.Forward = -viewVector;
            } else
            {
                worldMatrix.Forward = viewVector;
            }
            
            worldMatrix.Right = rightVector;
            worldMatrix.Up = upVector;
            worldMatrix.Translation = pos;

            worldMatrix = Matrix.Multiply(rotMatrix, worldMatrix);
            worldMatrix = Matrix.Multiply(scaleMatrix, worldMatrix);
            

            base.Draw(gameTime);


        }
        public abstract void initVertexBuffer();

        public abstract void initIndexBuffer();

		public abstract void drawIndexedPrimitives();

        public virtual void setSamplerStates()
        {
            this.GraphicsDevice.SamplerStates[0] = settings.SamplerState;
            this.GraphicsDevice.SamplerStates[1] = settings.SamplerState;
        }

        public virtual void resetSamplerStates()
        {
            this.GraphicsDevice.SamplerStates[0] = settings.StdSamplerState;
            this.GraphicsDevice.SamplerStates[1] = settings.StdSamplerState;
        }
        
        public void setRasterizerStates()
        {
            this.GraphicsDevice.RasterizerState = settings.RasterizerState;
        }

        public void resetRasterizerStates()
        {
            this.GraphicsDevice.RasterizerState = settings.StdRasterizerState;
        }

        public virtual void setRenderStates()
        {
            this.GraphicsDevice.BlendState = settings.BlendState;
            this.GraphicsDevice.DepthStencilState = settings.DepthStencilState;
        }

        public virtual void resetRenderStates()
        {
            this.GraphicsDevice.DepthStencilState = settings.StdDepthStencilState;
            this.GraphicsDevice.BlendState = settings.StdBlendState;
        }

        public virtual void setEffect(EffectContainer effectContainer)
        {
            this.effectContainer = effectContainer;
            this.effectContainer.updateUniform(this);
            this.effectContainer.drawUniform();
        }

        public SceneContainer Scene
        {
            get
            {
                return this.scene;
            }
        }

        public EffectContainer EffectContainer
        {
            get
            {
                return effectContainer;
            }
        }

        public Object3DSettings Object3DSettings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = value;
            }
        }

        public virtual void jump()
        {
        }
		
        public Vector3 ViewVector
        {
            get
            {
                return viewVector;
            }
            set
            {
                viewVector = value;
            }
        }

        public Vector3 RightVector
        {
            get
            {
                return rightVector;
            }
            set
            {
                rightVector = value;
            }
        }

        public Vector3 UpVector
        {
            get
            {
                return upVector;
            }
            set
            {
                upVector = value;
            }
        }

        public virtual float Time
        {
            get
            {
                return currentTime;
            }
        }

        public virtual float Random
        {
            get
            {
                return Convert.ToSingle(rand.NextDouble());
            }
        }

        public Matrix World
        {
            get
            {
                //return Matrix.Multiply(scaleMatrix, worldMatrix);
                return worldMatrix;
            }
        }

        public Matrix View
        {
            get
            {
                if(scene != null && scene.Camera != null)
                    return scene.Camera.MView;
                return Matrix.Identity;
            }
        }

        public Matrix Projection
        {
            get
            {
                if (scene != null && scene.Camera != null)
                    return scene.Camera.MProjection;
                return Matrix.Identity;
            }
        }

        public Vector4 Eye
        {
            get
            {
                if (scene != null && scene.Camera != null)    
                    return new Vector4(scene.Camera.VEye.X, scene.Camera.VEye.Y, scene.Camera.VEye.Z, 1.0f);
                return Vector4.Zero;
            }
        }

        public Vector3 Position
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
            }
        }

        public Matrix ViewInverse
        {
            get
            {
                return Matrix.Invert(getViewMatrix());
            }
        }

        public Matrix InverseTranspose
        {
            get
            {
                return Matrix.Transpose(Matrix.Invert(World));
            }
        }

        public Matrix WorldViewInverseTranspose
        {
            get
            {
                return Matrix.Transpose(Matrix.Invert(World));
            }
        }

        public Matrix ViewProjection
        {
            get
            {
                return Matrix.Multiply(View, Projection);
            }
        }

        public Matrix WorldView
        {
            get
            {
                return Matrix.Multiply(World, View);
            }
        }

        public Matrix WorldViewProjection
        {
            get
            {
                return Matrix.Multiply(WorldView, Projection);
            }
        }

        public int ViewportHeight
        {
            get
            {
                return this.GraphicsDevice.Viewport.Height;
            }
        }

        public Vector2 ViewportScale
        {
            get
            {
                return new Vector2(0.5f / this.GraphicsDevice.Viewport.AspectRatio, -0.5f);
            }
        }

        /// <summary>
        /// used for planar local reflections on water
        /// </summary>
        /// <returns></returns>
        public Matrix ProjectiveTexture
        {
            get
            {
                Matrix matTexScale = Matrix.Identity;
                float fOffset = 0.5f;
                matTexScale.M11 = 0.5f;
                matTexScale.M22 = -0.5f;
                matTexScale.M33 = 0.5f;
                matTexScale.M44 = 1.0f;
                matTexScale.M41 = fOffset;
                matTexScale.M42 = fOffset;
                Matrix matWorld = getWorldMatrix();
                Matrix matView = getViewMatrix();
                Matrix matProj = getProjectionMatrix();
                Matrix mat = Matrix.Multiply(matProj, matTexScale);
                mat = Matrix.Multiply(matView, mat);
                return mat;
            }
        }

        public Matrix ProjectiveScale
        {
            get
            {
                Matrix matTexScale = Matrix.Identity;
                float fOffset = 0.5f;
                matTexScale.M11 = 0.5f;
                matTexScale.M22 = -0.5f;
                matTexScale.M33 = 0.5f;
                matTexScale.M44 = 1.0f;
                matTexScale.M41 = fOffset;
                matTexScale.M42 = fOffset;
                return matTexScale;
            }
        }

        public virtual float getTime()
        {
            return currentTime;
        }

        public virtual float getRandom()
        {
            float val = Convert.ToSingle(rand.NextDouble());
            return val;
        }

        public Matrix getWorldMatrix()
        {
            return World;
        }

        public Vector3 getPosition()
        {
            return pos;
        }

        public void setPosition(Vector3 pos)
        {
            this.pos = pos;
        }

        public Matrix getViewMatrix()
        {
            if (scene != null && scene.Camera != null)    
                return scene.Camera.MView;
            return Matrix.Identity;
        }

        public Matrix getProjectionMatrix()
        {
            if (scene != null && scene.Camera != null)    
                return scene.Camera.MProjection;
            return Matrix.Identity;
        }

        public Matrix getViewInverseMatrix()
        {
            return Matrix.Invert(getViewMatrix());
        }

        public Matrix getWorldInverseTranspose()
        {
            return Matrix.Transpose(Matrix.Invert(getWorldMatrix()));
        }

        public Matrix getWorldViewInverseTranspose()
        {
            return Matrix.Transpose(Matrix.Invert(getWorldViewMatrix()));
        }

        public Matrix getViewProjectionMatrix()
        {
            return Matrix.Multiply(getViewMatrix(), getProjectionMatrix());
        }

        public Matrix getWorldViewMatrix()
        {
            return Matrix.Multiply(getWorldMatrix(), getViewMatrix());
        }

        public Matrix getWorldViewProjectionMatrix()
        {
            return Matrix.Multiply(getWorldViewMatrix(), getProjectionMatrix());
        }

        public int getViewportHeight()
        {
            return this.GraphicsDevice.Viewport.Height;
        }

        public Vector2 getViewportScale()
        {
            return new Vector2(0.5f / this.GraphicsDevice.Viewport.AspectRatio, -0.5f);
        }

        /// <summary>
        /// used for planar local reflections on water
        /// </summary>
        /// <returns></returns>
        public Matrix getProjectiveTextureMatrix()
        {
            Matrix matTexScale = Matrix.Identity;
            float fOffset = 0.5f;
            matTexScale.M11 = 0.5f;
            matTexScale.M22 = -0.5f;
            matTexScale.M33 = 0.5f;
            matTexScale.M44 = 1.0f;
            matTexScale.M41 = fOffset;
            matTexScale.M42 = fOffset;
            Matrix matWorld = getWorldMatrix();
            Matrix matView = getViewMatrix();
            Matrix matProj = getProjectionMatrix();
            Matrix mat = Matrix.Multiply(matProj, matTexScale);
            mat = Matrix.Multiply(matView, mat);
            return mat;
        }

        public Vector4 getEyeVector()
        {
            if (scene != null && scene.Camera != null)  
                return new Vector4(scene.Camera.VEye.X, scene.Camera.VEye.Y, scene.Camera.VEye.Z, 1.0f);
            return Vector4.Zero;
        }
        
        public Vector3 Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }

        public Quaternion RotationQuat
        {
            get
            {
                return rotationQuat;
            }
        }

        public float Yaw
        {
            get
            {
                return yaw;
            }
            set
            {
                yaw = value;
            }
        }

        public float Pitch
        {
            get
            {
                return pitch;
            }
            set
            {
                pitch = value;
            }
        }

        public float Roll
        {
            get
            {
                return roll;
            }
            set
            {
                roll = value;
            }
        }

        public float PositionX
        {
            get
            {
                return pos.X;
            }
            set
            {
                pos.X = value;
            }
        }

        public float PositionY
        {
            get
            {
                return pos.Y;
            }
            set
            {
                pos.Y = value;
            }
        }

        public float PositionZ
        {
            get
            {
                return pos.Z;
            }
            set
            {
                pos.Z = value;
            }
        }

        public BoundingSphere BoundingSphere
        {
            get
            {
                return bs;
            }
            set
            {
                bs = value;
            }
        }

        public BoundingBox BoundingBox
        {
            get
            {
                return bb;
            }
            set
            {
                bb = value;
            }
        }

        public Physics Physics
        {
            get
            {
                return physics;
            }
            set
            {
                physics = value;
            }
        }

        public bool Moving
        {
            get
            {
                return moving;
            }
        }

        public bool isVisible()
        {
            if (scene.Camera.ViewFrustum.Intersects(bs))
            {
                return true;
            }
            return false;
        }

        public void checkCollisions()
        {
            //for each obj in colliderList
            if (mode == "go")
            {
                // gravitation
                if (scene.Landscape != null)
                {
                    float height = scene.Landscape.getHeight(this.getPosition()) + scene.Landscape.getPosition().Y;
                    this.PositionY = height;
                }
                /*
                if(camera.getObjective().collides(level))
                {
                //TODO: collider for Quake level schreiben 
                camera.getObjective().PositionY = landscape.getHeight(camera.getObjective().getPosition()) + landscape.getPosition().Y + 0.2f;
                }
                */
            }
            if (mode == "fly")
            {
                if(this.collides(scene.Landscape))
                {
                    float height = scene.Landscape.getHeight(this.getPosition()) + scene.Landscape.getPosition().Y;
                    this.PositionY = height;
                }
            }
        }

        public virtual bool collides(Object3D obj)
        {
            if (oldPos != pos)
            {
                if (BoundingSphere.Intersects(obj.BoundingSphere))
                {
                    return true;
                }

                if (obj.GetType() == typeof(Level3D))
                {
                    Q3BSPCollisionData collision = ((Level3D)obj).Level.TraceSphere(oldPos, pos, BoundingSphere.Radius);
                    if (collision.inSolid)
                    {
                        return true;
                    }

                }
                if (obj.GetType() == typeof(LandscapeGeomipmap))
                {
                    if (this.PositionY < scene.Landscape.getHeight(this.getPosition()) + scene.Landscape.getPosition().Y)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public String Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
            }
        }

        public Model Model
        {
            get
            {
                return model;
            }
        }

        public Matrix[] BoneTransforms
        {
            get
            {
                return boneTransforms;
            }
            set
            {
                boneTransforms = value;
            }
        }

        // steering of object
        public void strafeLeft() 
        {
            moveObject(maxForwardSpeed, 0.0f, 0.0f);
        }

        public void strafeRight()
        {
            moveObject(-maxForwardSpeed, 0.0f, 0.0f);
        }

        public void turnRight()
        {
            rotateObjectFollow(0.0f, -0.1f, 0.0f);
            if (scene.Camera != null)
            {
                scene.Camera.followObjective();
            }
        }

        public void turnLeft()
        {
            rotateObject(0.0f, 0.1f, 0.0f);
            if (scene.Camera != null)
            {
                scene.Camera.followObjective();
            }
            
        }

        public void goForward()
        {
            moveObject(0.0f, 0.0f, -maxForwardSpeed);
            if (scene.Camera != null)
            {
                scene.Camera.followObjective();
            }
        }

        public void goBackwards()
        {
            moveObject(0.0f, 0.0f, maxBackwardSpeed);
            if (scene.Camera != null)
            {
                scene.Camera.followObjective();
            }
        }

        public void goUp()
        {

        }

        public void goDown()
        {

        }
    }
}
