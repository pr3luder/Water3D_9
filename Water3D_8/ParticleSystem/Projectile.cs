#region File Description
//-----------------------------------------------------------------------------
// Projectile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Water3D
{
    /// <summary>
    /// This class demonstrates how to combine several different particle systems
    /// to build up a more sophisticated composite effect. It implements a rocket
    /// projectile, which arcs up into the sky using a ParticleEmitter to leave a
    /// steady stream of trail particles behind it. After a while it explodes,
    /// creating a sudden burst of explosion and smoke particles.
    /// </summary>
    class Projectile : Object3D
    {
        #region Constants

        const float trailParticlesPerSecond = 100;
        const int numExplosionParticles = 10;
        const int numExplosionSmokeParticles = 20;
        const float projectileLifespan = 1.5f;
        const float sidewaysVelocityRange = 60;
        const float verticalVelocityRange = 40;
        const float gravity = 0;

        #endregion

        #region Fields

        ParticleSystem explosionParticles;
        ParticleSystem explosionSmokeParticles;
        ParticleSystem projectileTrailParticles;
        ParticleEmitter trailEmitter;

        Vector3 velocity;
        float age;
        bool explode;
        bool flying;

        static Random random = new Random();

        #endregion


        /// <summary>
        /// Constructs a new projectile.
        /// </summary>
        public Projectile(SceneContainer scene, Vector3 pos, Matrix rotation, Vector3 scale,    
                            ParticleSystem explosionParticles,
                            ParticleSystem explosionSmokeParticles,
                            ParticleSystem projectileTrailParticles,
                            Vector3 direction) : base(scene, pos, rotation, scale)
        {
            this.explosionParticles = explosionParticles;
            this.explosionSmokeParticles = explosionSmokeParticles;
            this.projectileTrailParticles = projectileTrailParticles;
            this.pos = pos;
            this.age = 0.0f;
            this.explode = false;
            bs.Center = pos;
            bs.Radius = scale.X;
            velocity.X = direction.X * sidewaysVelocityRange;
            velocity.Y = direction.Y * verticalVelocityRange;
            velocity.Z = direction.Z * sidewaysVelocityRange;
            
            // Use the particle emitter helper to output our trail particles.
            trailEmitter = new ParticleEmitter(projectileTrailParticles,
                                               trailParticlesPerSecond, pos);
        }

        public override void Update(GameTime time)
        {

        }

        /// <summary>
        /// Updates the projectile.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            if (flying)
            {
                float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                // Simple projectile physics.
                pos += velocity * elapsedTime;
                velocity.Y -= elapsedTime * gravity;
                age += elapsedTime;

                // Update the particle emitter, which will create our particle trail.
                trailEmitter.Update(gameTime, pos);

                bs.Center = pos;
                bs.Radius = scale.X;
                // If enough time has passed, explode! Note how we pass our velocity
                // in to the AddParticle method: this lets the explosion be influenced
                // by the speed and direction of the projectile which created it.
                if (age > projectileLifespan || explode)
                {
                    for (int i = 0; i < numExplosionParticles; i++)
                        explosionParticles.AddParticle(pos, velocity);

                    for (int i = 0; i < numExplosionSmokeParticles; i++)
                        explosionSmokeParticles.AddParticle(pos, velocity);

                    flying = false;
                }
                base.Draw(gameTime);
            }
        }

        public override void drawIndexedPrimitives()
        {

        }

        public void reset()
        {
            age = 0.0f;
            explode = false;
        }

        public Vector3 Pos
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

        public Vector3 Dir
        {
            set
            {
                velocity.X = value.X * sidewaysVelocityRange;
                velocity.Y = value.Y * verticalVelocityRange;
                velocity.Z = value.Z * sidewaysVelocityRange;
            }
        }

        public bool Explode
        {
            get
            {
                return explode;
            }
            set
            {
                explode = value;
            }
        }

        public bool Flying
        {
            get
            {
                return flying;
            }
            set
            {
                flying = value;
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
            if(obj.GetType() == typeof(LandscapeGeomipmap))
            {
                
                float t1 = ((LandscapeGeomipmap)obj).getHeight(oldPos) + ((LandscapeGeomipmap)obj).getPosition().Y;
                float t2 = ((LandscapeGeomipmap)obj).getHeight(pos) + ((LandscapeGeomipmap)obj).getPosition().Y;
                if ((t1 <= oldPos.Y && t2 >= pos.Y) || (t1 >= oldPos.Y && t2 <= pos.Y))
                {
                    return true;
                }
                
                /*
                if (pos.Y <= ((LandscapeGeomipmap)obj).getHeight(pos) + ((LandscapeGeomipmap)obj).getPosition().Y)
                {
                    return true;
                }
                */
            }
            return false;
        }
    }
}
