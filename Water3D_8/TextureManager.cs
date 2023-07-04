using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;

namespace Water3D
{
    public class TextureManager
    {
        private Hashtable textures;
        public TextureManager()
        {
            textures = new Hashtable();
        }

        public void addTexture(String textureName, Texture texture)
        {
            if (textures.ContainsKey(textureName))
            {
                textures[textureName] = texture;
            }
            else
            {
                textures.Add(textureName, texture);
            }
        }

        public Texture getTexture(String textureName)
        {
            return (Texture)textures[textureName];
        }
    }
}
