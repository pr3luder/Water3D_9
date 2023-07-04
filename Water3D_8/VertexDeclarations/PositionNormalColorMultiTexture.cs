using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Water3D.VertexDeclarations
{
    public struct PositionNormalColorMultiTexture : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;
        public Vector2 TextureCoordinate;
        public Vector2 TextureCoordinate1;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 7, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 9, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
        );

        public PositionNormalColorMultiTexture(Vector3 pos, Vector3 n, Color c, Vector2 tc, Vector2 tc1)
        {
            Position = pos;
            Normal = n;
            Color = c;
            TextureCoordinate = tc;
            TextureCoordinate1 = tc1;
        }

        public static int SizeInBytes = sizeof(float) * 11;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }
}
