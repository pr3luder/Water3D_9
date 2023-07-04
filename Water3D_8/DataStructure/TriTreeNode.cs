using System;
using System.Collections.Generic;
using System.Text;

namespace Water3D
{
    public class TriTreeNode
    {
        public TriTreeNode leftChild;
        public TriTreeNode rightChild;
        public TriTreeNode baseNeighbor;
        public TriTreeNode leftNeighbor;
        public TriTreeNode rightNeighbor;
        public float lz, rz, az;
        public TriTreeNode()
        {
            lz = rz = az = 0.0f;
            this.leftChild = null;
            this.rightChild = null;
            this.baseNeighbor = null;
            this.leftNeighbor = null;
            this.rightNeighbor = null;
        }

        public void setLeftChild(TriTreeNode tri)
        {
            if(tri == null)
                this.leftChild = new TriTreeNode();
            this.leftChild = tri;
        }

        public void setRightChild(TriTreeNode tri)
        {
            if (tri == null)
                this.rightChild = new TriTreeNode();
            this.rightChild = tri;
        }

        public void setBaseNeighbor(TriTreeNode tri)
        {
            if (tri == null)
                this.baseNeighbor = new TriTreeNode();
            this.baseNeighbor = tri;
        }

        public void setLeftNeighbor(TriTreeNode tri)
        {
            if (tri == null)
                this.leftNeighbor = new TriTreeNode();
            this.leftNeighbor = tri;
        }

        public void setRightNeighbor(TriTreeNode tri)
        {
            if (tri == null)
                this.rightNeighbor = new TriTreeNode();
            this.rightNeighbor = tri;
        }

        public TriTreeNode getLeftChild()
        {
            /*if (this.leftChild == null)
                return new TriTreeNode();*/
            return this.leftChild;
        }

        public TriTreeNode getRightChild()
        {
            /*if (this.rightChild == null)
                return new TriTreeNode();*/
            return this.rightChild;
        }

        public TriTreeNode getBaseNeighbor()
        {
            /*if (this.baseNeighbor == null)
                return new TriTreeNode();*/
            return this.baseNeighbor;
        }

        public TriTreeNode getLeftNeighbor()
        {
            /*if (this.leftNeighbor == null)
                return new TriTreeNode();*/
            return this.leftNeighbor;
        }

        public TriTreeNode getRightNeighbor()
        {
            /*if (this.rightNeighbor == null)
                return new TriTreeNode();*/
            return this.rightNeighbor;
        }
    }
}
