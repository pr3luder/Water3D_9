using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Water3D.VertexDeclarations
{
    public struct PositionNormalColor : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement( sizeof(float)*6, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );

        public PositionNormalColor(Vector3 pos, Vector3 n, Color c)
        {
            Position = pos;
            Normal = n;
            Color = c;
        }

        public static int SizeInBytes = sizeof(float) * 7;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

    }
}
