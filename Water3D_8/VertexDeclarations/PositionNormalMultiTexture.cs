using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Water3D.VertexDeclarations
{
    public struct PositionNormalMultiTexture : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector2 TextureCoordinate1;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
        );

        public PositionNormalMultiTexture(Vector3 pos, Vector3 n, Vector2 tc, Vector2 tc1)
        {
            Position = pos;
            Normal = n;
            TextureCoordinate = tc;
            TextureCoordinate1 = tc1;
        }

        public static int SizeInBytes = sizeof(float) * 10;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }
}
