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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;
using CustomModelAnimation;

namespace Water3D
{
	/// <summary>
	/// load and draw directx .x - files that
	/// describe a 3D object
	/// </summary>
	public class Model3DSkinned : Object3D
	{     
        private String modelName;
        private SkinnedAnimationPlayer skinnedAnimationPlayer;
        private RootAnimationPlayer rootAnimationPlayer;
        private ModelData skinningData;
        private ModelAnimationClip clip;
        private bool animate;
        private bool jumping;
        private bool rising;
        private float jumpHeight;
        private BoundingSphere bsLocal;
        public Model3DSkinned(SceneContainer scene, Vector3 pos, Matrix rotation, Vector3 scale, String modelName, bool flipZAxis = false)
            : base(scene, pos, rotation, scale, null, flipZAxis)
		{
            this.scene = scene;
            this.modelName = modelName;

            this.animate = false;
            this.jumping = false;
            this.rising = false;
            model = scene.Game.Content.Load<Model>(modelName);
            
            // Look up our custom skinning information.
            skinningData = model.Tag as ModelData;
            
            if (skinningData != null)
            {
                if (skinningData.RootAnimationClips != null)
                {
                    rootAnimationPlayer = new RootAnimationPlayer();
                }
                if (skinningData.ModelAnimationClips != null)
                {
                    skinnedAnimationPlayer = new SkinnedAnimationPlayer(skinningData.BindPose, skinningData.InverseBindPose, skinningData.SkeletonHierarchy);
                    //skinnedAnimationPlayer.StartClip(skinningData.ModelAnimationClips["Take 001"]);
                    updateClip("Take 001");
                    boneTransforms = skinnedAnimationPlayer.GetSkinTransforms();
                    //skinnedAnimationPlayer.Update(new GameTime(new TimeSpan(), new TimeSpan(2)));
                }
            }
            setSkinningEffect();

            bsLocal = new BoundingSphere();
            foreach (ModelMesh mesh in model.Meshes)
            {
                bsLocal = BoundingSphere.CreateMerged(bsLocal, mesh.BoundingSphere);
            }
            setObject(pos.X, pos.Y, pos.Z);
		}

        public void setSkinningEffect()
        {
            foreach(ModelMesh mesh in model.Meshes)
            {
                foreach(ModelMeshPart modelMeshPart in mesh.MeshParts)
                {
                    SkinnedEffect newEffect = new SkinnedEffect(scene.GraphicsDevice);
                    BasicEffect oldEffect = ((BasicEffect)modelMeshPart.Effect);
                    newEffect.EnableDefaultLighting();
                    newEffect.SpecularColor = Color.Black.ToVector3();
                    newEffect.AmbientLightColor = oldEffect.AmbientLightColor;
                    newEffect.DiffuseColor = oldEffect.DiffuseColor;
                    newEffect.Texture = oldEffect.Texture;
                    modelMeshPart.Effect = newEffect;

                }
            }
        }

        public void updateAnimation(GameTime gameTime)
        {
            if (animate)
            {
                if (skinningData.RootAnimationClips != null)
                    rootAnimationPlayer.Update(gameTime);
                if (skinningData.ModelAnimationClips != null)
                    skinnedAnimationPlayer.Update(gameTime);
            } else
            {
                //skinnedAnimationPlayer.Update(TimeSpan.Zero, true, Matrix.Identity);
                skinnedAnimationPlayer.Update(new GameTime(TimeSpan.Zero, TimeSpan.Zero));
                //skinnedAnimationPlayer.Update(gameTime);
            }
        }

        public void startAnimation()
        {
            animate = true;
        }

        public void stopAnimation()
        {
            animate = false;
        }

        public void updateClip(String clip)
        {
            this.clip = skinningData.ModelAnimationClips[clip];
            if (skinnedAnimationPlayer.CurrentClip == null || this.clip != skinnedAnimationPlayer.CurrentClip)
                skinnedAnimationPlayer.StartClip(skinningData.ModelAnimationClips[clip]);
        }

        public void turnLeft(string animation)
        {
            updateClip(animation);
            base.turnLeft();
            this.startAnimation();
        }

        public void turnRight(string animation)
        {
            updateClip(animation);
            base.turnRight();
            this.startAnimation();
        }

        public void goForward(string animation)
        {
            updateClip(animation);
            base.goForward();
            this.startAnimation();
        }

        public void goBackwards(string animation)
        {
            updateClip(animation);
            base.goBackwards();
            this.startAnimation();
        }

        public override void Update(GameTime gameTime)
        {
            //if (moving)
            //updateAnimation(gameTime); // FIXME hier gibt es Animationsprobleme - aendern
            base.Update(gameTime);
        }

        public override void Draw(GameTime time)
        {
            //worldMatrix = Matrix.CreateFromQuaternion(rotationQuat) * Matrix.CreateTranslation(pos);
            scene.Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            scene.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            boneTransforms = skinnedAnimationPlayer.GetSkinTransforms();

            effectContainer.updateMutable(this);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    if (currentEffect.GetType() == typeof(SkinnedEffect))
                    {
                        effectContainer.drawMutable(currentEffect);
                    }
                    else if (currentEffect.GetType() == typeof(BasicEffect))
                    {
                        effectContainer.drawMutable(currentEffect);
                    }
                    else
                    {
                        currentEffect.Parameters["Bones"].SetValue(boneTransforms);
                        currentEffect.Parameters["View"].SetValue(World * scene.Camera.MView);
                        currentEffect.Parameters["Projection"].SetValue(scene.Camera.MProjection);
                    }
                }
                mesh.Draw();
                Matrix world = Matrix.Identity;
                world = boneTransforms[mesh.ParentBone.Index] * World;
                bs = bsLocal.Transform(world);
            }
            /* not tested */
            if (!moving)
            {
                stopAnimation();
            }
            else
            {
                startAnimation();
            }          
            updateAnimation(time); // FIXME hier gibt es Animationsprobleme - aendern
            base.Draw(time);

        }

        public override void jump()
        {
            jumpHeight = 0.0f;
            rising = true;
            jumping = true;
        }

		public override void drawIndexedPrimitives()
		{
            
		}

        public SkinnedAnimationPlayer SkinnedAnimationPlayer
        {
            get 
            {
                return skinnedAnimationPlayer;
            }
        }

        public String ModelName
        {
            get
            {
                return modelName;
            }
        }

        public bool Jumping
        {
            get
            {
                return jumping;
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

        public override bool collides(Object3D obj)
        {
            if (obj.GetType() == typeof(LandscapeGeomipmap))
            {
                if (pos.Y <= ((LandscapeGeomipmap)obj).getHeight(pos) + ((LandscapeGeomipmap)obj).getPosition().Y)
                {
                    return true;
                }
            }
            else if (this.bs.Intersects(obj.BoundingSphere))
            {
                return true;
            }
            return base.collides(obj);
        }
	}
}
