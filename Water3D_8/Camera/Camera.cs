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

#endregion

namespace Water3D
{
	public class Camera
	{
		private int viewportWidth;
        private int viewportHeight;
        private Matrix viewMatrix, projMatrix;
		private Vector3 vEye, vDest, vUp;
        private float saveHeight;
        private BoundingFrustum viewFrustum;
        private String mode;
        private Vector3 desiredPositionOffset;
        private Vector3 lookAtOffset;
        private Quaternion rotation;
        private Object3D objective;
        private SceneContainer scene;

        public Camera(int viewportWidth, int viewportHeight, Vector3 vEye, Vector3 vDest, Vector3 vUp)
		{
            this.viewportWidth = viewportWidth;
            this.viewportHeight = viewportHeight;
            this.vEye = vEye;
			this.vDest = vDest;
			this.vUp = vUp;
            this.saveHeight = vEye.Y;
            this.mode = "follow";
            this.rotation = Quaternion.Identity;
            this.desiredPositionOffset = new Vector3(0.0f, 10.0f, 20.0f);
            this.lookAtOffset = new Vector3(0.0f, 10.0f, 10.0f);
            reset();
		}

        public Camera(SceneContainer scene, Vector3 vEye, Vector3 vDest, Vector3 vUp) : this(scene.Game.GraphicsDevice.Viewport.Width, scene.Game.GraphicsDevice.Viewport.Height, vEye, vDest, vUp)
        { 
            this.scene = scene;
            scene.setCamera(this);
        }

        public void setScene(SceneContainer scene)
        {
            this.scene = scene;
        }

        public void setObjective(Object3D objective)
        {
            this.objective = objective;
        }

        public Object3D getObjective()
        {
            return this.objective;
        }

        public void reset()
        {

            this.viewportWidth = RenderEngine.Game.GraphicsDevice.Viewport.Width;
            this.viewportHeight = RenderEngine.Game.GraphicsDevice.Viewport.Height;

            viewMatrix = Matrix.CreateLookAt(vEye, vDest, vUp);
            projMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, this.viewportWidth / this.viewportHeight, 0.1f, 16384.0f);
            buildViewFrustum();
        }


		/// <summary>
		/// Chapter 4.4, Listing 4.4.2
		/// rotate camera around arbitrary axis 
		/// </summary>
		/// <param name="angle"></param>
		public void rotateObject(float angle, Vector3 axis)
		{
            Vector3 vNewView;
			Vector3 vView;
			axis.Normalize();
            
            float x = axis.X;
			float y = axis.Y;
			float z = axis.Z;

			// Get our view vector (The direction we are facing)
			vView = VView;
			
            // Calculate the sine and cosine of the angle once
			float cosTheta = (float)Math.Cos((double)angle);
			float sinTheta = (float)Math.Sin((double)angle);

			// Find the new x position for the new rotated point
			vNewView.X = (cosTheta + (1 - cosTheta) * x * x) * vView.X;
			vNewView.X += ((1 - cosTheta) * x * y - z * sinTheta) * vView.Y;
			vNewView.X += ((1 - cosTheta) * x * z + y * sinTheta) * vView.Z;

			// Find the new y position for the new rotated point
			vNewView.Y = ((1 - cosTheta) * x * y + z * sinTheta) * vView.X;
			vNewView.Y += (cosTheta + (1 - cosTheta) * y * y) * vView.Y;
			vNewView.Y += ((1 - cosTheta) * y * z - x * sinTheta) * vView.Z;

			// Find the new z position for the new rotated point
			vNewView.Z = ((1 - cosTheta) * x * z - y * sinTheta) * vView.X;
			vNewView.Z += ((1 - cosTheta) * y * z + x * sinTheta) * vView.Y;
			vNewView.Z += (cosTheta + (1 - cosTheta) * z * z) * vView.Z;

			// Now we just add the newly rotated vector to our position to set
			// our new rotated view of our camera.
			vDest.X = vEye.X + vNewView.X;
			vDest.Y = vEye.Y + vNewView.Y;
			vDest.Z = vEye.Z + vNewView.Z;

		}

        public void rotateAroundObjective(float angle, Vector3 axis)
        {
            if (objective != null)
            {
                Matrix transform = Matrix.Identity;
                axis.Normalize();
                transform = Matrix.CreateFromAxisAngle(axis, angle);
                Vector3 viewVector = Vector3.Transform(VView, transform);
                vDest = objective.getPosition();
                vEye = vDest - viewVector;
            }
        }


        public void rotateAroundObjectiveY(float yaw)
        {
            if (objective != null)
            {
                // vEye muss sich auf Kreisbahn um vDest drehen
                Matrix transform = Matrix.CreateRotationY(yaw);
                Vector3 viewVector = Vector3.Transform(VView, transform);
                vDest = objective.getPosition();
                vEye = vDest - viewVector;
            }
        }

        /// <summary>
        /// move camera in x, y or z direction of world space by keeping destination
        /// </summary>
        /// <param name="following">object to follow</param>
        /// <param name="x">x-direction</param>
        /// <param name="y">y-direction</param>
        /// <param name="z">z-direction</param>
        public void followObjective(float x, float y, float z)
        {
            if(objective != null)
            { 
                vEye.X += x;
                vEye.Y += y;
                vEye.Z += z;
                vDest = objective.getPosition();
            }
        }

        /// <summary>
        /// follow an object by keeping desiredPositionOffset, lookAtOffset, direction
        /// of an object
        /// </summary>
        /// <param name="following">object to follow</param>
        public void followObjective()
        {
            if (objective != null)
            {
                // Construct a matrix to transform from object space to worldspace
                Matrix transform = Matrix.Identity;
                transform.Forward = objective.ViewVector;
                transform.Up = objective.UpVector;
                transform.Right = Vector3.Cross(objective.UpVector, objective.ViewVector);

                Vector3 objPos = objective.getPosition();
                // Calculate desired camera properties in world space
                vEye = objPos + Vector3.TransformNormal(desiredPositionOffset, transform);
                vDest = objPos + Vector3.TransformNormal(lookAtOffset, transform);
            }
        }

        /// <summary>
        /// zoom to or away from object
        /// </summary>
        /// <param name="following"></param>
        /// <param name="amount"></param>
        public void zoomObjective(float amount)
        {
            if (objective != null)
            {
                vEye.X += VView.X * amount;
                vEye.Y += VView.Y * amount;
                vEye.Z += VView.Z * amount;
                vDest = objective.getPosition();
            }
        }

       

        /// <summary>
        /// follow an object by changing desiredPositionOffset, lookAtOffset, direction
        /// of an object
        /// </summary>
        /// <param name="following">object to follow</param>
        public void followObjective(Vector3 dPO, Vector3 lAO)
        {
            desiredPositionOffset += dPO;
            lookAtOffset += lAO;
            if (objective != null)
            {
                // Construct a matrix to transform from object space to worldspace
                Matrix transform = Matrix.Identity;
                transform.Forward = objective.ViewVector;
                transform.Up = objective.UpVector;
                transform.Right = Vector3.Cross(objective.UpVector, objective.ViewVector);

                // Calculate desired camera properties in world space
                vEye = objective.getPosition() + Vector3.TransformNormal(desiredPositionOffset, transform);
                vDest = objective.getPosition() + Vector3.TransformNormal(lookAtOffset, transform);
            }
        }

        /// <summary>
        /// follow an object by following the object completely
        /// </summary>
        /// <param name="following">object to follow</param>
        public void followObjectiveQuaternion(float delay)
        {
            if (objective != null)
            {
                rotation = Quaternion.Lerp(rotation, objective.RotationQuat, delay);

                Vector3 campos = desiredPositionOffset;
                campos = Vector3.Transform(campos, Matrix.CreateFromQuaternion(rotation));
                campos += objective.Position;
                vEye = campos;

                Vector3 camup = new Vector3(0, 1, 0);
                camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(rotation));
                vUp = camup;

                VDest = objective.Position;
            }

        }

        /// <summary>
        /// follow an object by keeping distance, but not direction 
        /// </summary>
        /// <param name="following">object to follow</param>
        public void followObjectiveZ(Object3D following)
        {
            Vector3 v = VView;
            vDest = following.getPosition();
            vEye = vDest - v;
        }

        /// <summary>
        /// move offset behind object in x, y or z direction 
        /// just for follow mode
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void moveObjectiveOffset(float x, float y, float z)
        {
            this.desiredPositionOffset.X += x;
            this.desiredPositionOffset.Y += y;
            this.desiredPositionOffset.Z += z;
        }

		/// <summary>
		/// Chapter 4.4, Listing 4.4.1
		/// move camera along local axes
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public void moveObject(float x, float y, float z)
		{
			Vector3 vView = VView;
            Vector3 vRight = VRight;
            Vector3 vUp = VUp;
			
			vView.Normalize();
			vRight.Normalize();
            vUp.Normalize();
			
			// move forward (z-direction)
			vEye.X += vView.X * z;
			vEye.Z += vView.Z * z;
			
			// move right (x-direction)
			vEye.X += vRight.X * x;
			vEye.Z += vRight.Z * x;
		
			vDest.X += vView.X * z;
			vDest.Z += vView.Z * z;
			
			vDest.X += vRight.X * x;
			vDest.Z += vRight.Z * x;

			// move up (y-direction), keep destination
			vEye.X += vUp.X * y;
			vEye.Y += vUp.Y * y;		
		}
        
        
        /// <summary>
        /// Chapter 4.4, Listing 4.4.1
        /// move camera along local axes
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void moveObjectFly(float x, float y, float z)
        {
            Vector3 vView = VView;
            Vector3 vRight = VRight;
            Vector3 vUp = VUp;

            vView.Normalize();
            vRight.Normalize();
            vUp.Normalize();

            // move forward (z-direction)
            vEye.X += vView.X * z;
            vEye.Y += vView.Y * z;
            vEye.Z += vView.Z * z;

            // move right (x-direction)
            vEye.X += vRight.X * x;
            vEye.Y += vRight.Y * x;
            vEye.Z += vRight.Z * x;

            vDest.X += vView.X * z;
            vDest.Y += vView.Y * z;
            vDest.Z += vView.Z * z;

            vDest.X += vRight.X * x;
            vDest.Y += vRight.Y * x;
            vDest.Z += vRight.Z * x;

            // move up (y-direction), keep destination
            vEye.X += vUp.X * y;
            vEye.Y += vUp.Y * y;
        }


        public void Draw(LandscapeGeomipmap landscape, Object3D plane)
        {
            switch (Mode)
            {
                case "free":
                    updateObject();
                    break;
                case "follow":
                    float minimum = Math.Max(landscape.getHeight(VEye), objective.Scene.SeaHeight);
                    updateObject(minimum - 1.0f, true);
                    followObjective();
                    break;
                case "followFree":
                    float minimum1 = Math.Max(landscape.getHeight(VEye), objective.Scene.SeaHeight);
                    updateObject(minimum1 - 1.0f , true);
                    /*updateObject(getObjective().getPosition().Y + 0.5f);*/
                    break;
                case "go":
                    float minimum2 = Math.Max(landscape.getHeight(VEye), objective.Scene.SeaHeight);
                    updateObject(minimum2 + 2.0f, true);
                    //updateObject(landscape.getHeight(VEye));
                    followObjective();
                    break;
                default:
                    updateObject(landscape.getHeight(VEye));
                    break;
            }
        }
        
        /// <summary>
		/// Chapter 4.4, Listing 4.4.3
		/// apply view matrix
		/// </summary>
		public void updateObject()
		{
            viewMatrix = Matrix.CreateLookAt(vEye, vDest, vUp);
            updateViewFrustum();
		}
        
        public void updateObject(float height)
        {
            vEye.Y = height;
            if (saveHeight != height)
            {
                float deltaHeight = height - saveHeight;
                vDest.Y += deltaHeight;
                saveHeight = height;
            }
            updateObject();
        }

        public void updateObject(float minimumHeight, bool use)
        {
            if (use)
            {
                if (vEye.Y < minimumHeight)
                {
                    vEye.Y = minimumHeight;
                }
            }
            updateObject();
        }

        public void updateInput()
        {

        }

        /// <summary>
        /// Build the view frustum planes using the current view/projection matrices
        /// </summary>
        public void buildViewFrustum()
        {
            viewFrustum = new BoundingFrustum(Matrix.Multiply(MView, MProjection));
        }

        public void updateViewFrustum()
        {
            viewFrustum.Matrix = Matrix.Multiply(MView, MProjection);
        } 

		public Vector3 VEye
		{
			get
			{
				return vEye;
			}
			set
			{
				vEye = value;
			}
		}
		public Vector3 VDest
		{
			get
			{
				return vDest;
			}
			set
			{
				vDest = value;
			}
		}
		public Vector3 VUp
		{
			get
			{
				return vUp;
			}
			set
			{
				vUp = value;
			}
		}
		
		public Vector3 VView
		{
			get
			{
                return VDest - VEye;
			}
		}

        public Vector3 VRight
        {
            get
            {
                return Vector3.Cross(VUp, VView);
            }
        }
        public Matrix MView
        {
            get
            {
                return viewMatrix;
            }
            set 
            {
                viewMatrix = value;
            }
        }

        public Matrix MProjection
        {
            get
            {
                return projMatrix;
            }
            set
            {
                projMatrix = value;
            }
        }

        public BoundingFrustum ViewFrustum
        {
            get
            {
                return viewFrustum;
            }
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
	}
}
