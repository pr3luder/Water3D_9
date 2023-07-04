using Microsoft.Xna.Framework;
using System;
namespace Water3D
{
    public class QuadtreeNode
    {
        private float x;
        private float z;
        private float width;
        private float min;
        private int level;
        private bool hasChildNodes;
        private QuadtreeNode lowerLeft;
        private QuadtreeNode lowerRight;
        private QuadtreeNode upperLeft;
        private QuadtreeNode upperRight;
        public QuadtreeNode(float x, float z, float width, float min)
        {
            this.x = x;
            this.z = z;
            this.width = width;
            float helperWidth = width;
            this.min = min;
            //this.hasChildNodes = isInside(root.Pos.X, root.Pos.Z) || isNear(root.Pos.X, root.Pos.Z);
            while ((helperWidth = helperWidth / 2) >= min)
            {
                level++;
            }
            if (width > min)
            {
                float halfwidth = width / 2;
                lowerLeft = new QuadtreeNode(x, z, halfwidth, min);
                lowerRight = new QuadtreeNode(x + halfwidth, z, halfwidth, min);
                upperLeft = new QuadtreeNode(x, z - halfwidth, halfwidth, min);
                upperRight = new QuadtreeNode(x + halfwidth, z - halfwidth, halfwidth, min);
            }
        }

        public void updateObject(Vector3 pos)
        {
            this.hasChildNodes = isInside(pos.X, pos.Z) || isNear(pos.X, pos.Z);
            if (lowerLeft != null)
            {
                lowerLeft.updateObject(pos);
            }
            if (lowerRight != null)
            {
                lowerRight.updateObject(pos);
            }
            if (upperLeft != null)
            {
                upperLeft.updateObject(pos);
            }
            if (upperRight != null)
            {
                upperRight.updateObject(pos);
            }
        }

        public QuadtreeNode find(float x, float z)
        {
            QuadtreeNode ret = null;
            if(isInside(x, z))
            {
                if (lowerLeft != null)
                {
                    ret = lowerLeft.find(x, z);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
                if (lowerRight != null)
                {
                    ret = lowerRight.find(x, z);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
                if (upperLeft != null)
                {
                    ret = upperLeft.find(x, z);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
                if (upperRight != null)
                {
                    ret = upperRight.find(x, z);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
                if (ret == null)
                {
                    return this;
                }
            }
            return ret;
        }

        public bool isInside(float x, float z)
        {
            //inside
            if (x >= this.x && z <= this.z && x <= this.x + this.width && z >= this.z - this.width)
            {
                return true;
            }
            return false;
        }

        public bool isNear(float x, float z)
        {
            float middleX = this.x + width / 2;
            float middleZ = this.z - width / 2;
            float distance = (float)Math.Sqrt(Math.Pow((double)(middleX - x), 2.0) + Math.Pow((double)(middleZ - z), 2.0));
            float diagonal = (float)Math.Sqrt(2.0 * Math.Pow((double)(width), 2.0));
            if(distance < 0.7f*diagonal)
            {
                return true;
            }
            return false;
        }


        public QuadtreeNode LowerLeft
        {
            get
            {
                return lowerLeft;
            }
        }

        public QuadtreeNode LowerRight
        {
            get
            {
                return lowerRight;
            }
        }

        public QuadtreeNode UpperLeft
        {
            get
            {
                return upperLeft;
            }
        }

        public QuadtreeNode UpperRight
        {
            get
            {
                return upperRight;
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

        public bool HasChildNodes
        {
            get
            {
                return hasChildNodes;
            }
        }
    }
}
