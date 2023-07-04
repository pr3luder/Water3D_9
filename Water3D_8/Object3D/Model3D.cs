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

namespace Water3D
{
	/// <summary>
	/// load and draw directx .x - files that
	/// describe a 3D object
	/// </summary>
	public class Model3D : Object3D
	{
        private String modelName;
        private float saveHeight;
        private BoundingSphere bsLocal;

        public Model3D(SceneContainer scene, Vector3 pos, Matrix rotation, Vector3 scale, String modelName, bool flipZAxis=false)
            : base(scene, pos, rotation, scale, null, flipZAxis)
		{
            this.scene = scene;
            this.modelName = modelName;

            this.saveHeight = pos.Y;
            /* load model */
            model = scene.Game.Content.Load<Model>(modelName);
            boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            setObject(pos.X, pos.Y, pos.Z);
            bsLocal = new BoundingSphere(pos, 10.0f); // fixme welcher radius?
            foreach (ModelMesh mesh in model.Meshes)
            {
                bsLocal = BoundingSphere.CreateMerged(bsLocal, mesh.BoundingSphere);
            }
		}
        
        public override void Draw(GameTime time)
        {
            Matrix wm = worldMatrix;
            //worldMatrix = Matrix.CreateFromQuaternion(rotationQuat) * Matrix.CreateTranslation(pos);
            foreach (ModelMesh mesh in model.Meshes)
            {
                
                worldMatrix = boneTransforms[mesh.ParentBone.Index] * wm;
                
                foreach (Effect currentEffect in mesh.Effects)
                {
                    updateEffects(currentEffect);
                }
                
                /*
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * worldMatrix;
                    //effect.World = scaleMatrix * effect.World;
                    effect.View = View;
                    effect.Projection = Projection;
                }*/
                mesh.Draw();
 
            }
            //update bounding sphere
            bs = bsLocal.Transform(getWorldMatrix());
            base.Draw(time);
        }

		public override void drawIndexedPrimitives()
		{
            
		}

        public override void setEffect(EffectContainer effectContainer)
        {
            base.setEffect(effectContainer);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    if (effectContainer.getEffect().GetType() != typeof(BasicEffect))
                    {
                        meshPart.Effect = effectContainer.getEffect();
                    }
                    else
                    {
                    }
                }
            }
        }

        public String ModelName
        {
            get
            {
                return modelName;
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
	}
}
