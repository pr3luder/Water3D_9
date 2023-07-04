﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Water3D
{
    public class FpsCounter : DrawableGameComponent
    {
        private float updateInterval = 1.0f;
        private float timeSinceLastUpdate = 0.0f;
        private float framecount = 0;

        private float fps = 0;
        /// <summary>
        /// The frames per second.
        /// </summary>
        public float FPS
        {
            get { return fps; }
        }

        public FpsCounter(Game game): base(game)
        {
            Enabled = true;
        }

        /// <summary>
        /// Update the fps.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            base.Update(gameTime);
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            framecount++;
            timeSinceLastUpdate += elapsed;

            if (timeSinceLastUpdate > updateInterval)
            {
                fps = framecount / timeSinceLastUpdate; //mean fps over updateIntrval
                framecount = 0;
                timeSinceLastUpdate -= updateInterval;

                if (Updated != null)
                    Updated(this, new EventArgs());
            }
        }

        /// <summary>
        /// FpsCounter Updated Event.
        /// </summary>
        public event EventHandler<EventArgs> Updated;
    }
}