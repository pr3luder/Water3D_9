using Microsoft.Xna.Framework;
namespace Water3D
{
    public class Quadtree
    {
        private float x;
        private float z;
        private float width;
        private float min;
        private int level;
        private QuadtreeNode root;
        public Quadtree(float x, float z, float width, float min)
        {
            this.x = x;
            this.z = z;
            this.width = width;
            this.min = min;
            this.level = 0;
            this.root = new QuadtreeNode(x, z, width, min);
        }

        public QuadtreeNode find(float x, float z)
        {
            return root.find(x, z);
        }

        public bool isInside(float x, float z)
        {
            return root.isInside(x, z);
        }

        public void updateObject(Vector3 pos)
        {
            this.root.updateObject(pos);
        }

        public QuadtreeNode Root
        {
            get
            {
                return root;
            }
        }

        public float X
        {
            get
            {
                return x;
            }
        }

        public float Z
        {
            get
            {
                return z;
            }
        }

        public float Width
        {
            get
            {
                return width;
            }
        }

        public float Min
        {
            get
            {
                return min;
            }
        }

        public int Level
        {
            get
            {
                return level;
            }
        }
    }
}
