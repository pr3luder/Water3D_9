using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Water3D
{
    public class Physics
    {
        private float timer;
        private float time;
        private float mass;
        
        public Physics(float mass)
        {
            this.time = 0.0f;
            this.timer = 0.0f;
            this.mass = 0.0f;
        }

        public Physics()
        {
            this.time = 0.0f;
            this.timer = 0.0f;
            this.mass = 0.0f;
        }
        
        public void updateObject()
        {
            timer += time;
        }

        public void begin(float time)
        {
            this.time = time;
        }

        public float accellerate(float a, float max)
        {
            float s = a * timer;
            if (s > max)
                return max;
            return s;
        }
        
        public void end()
        {
            this.time = 0.0f;
            this.timer = 0.0f;
        }
    }
}
