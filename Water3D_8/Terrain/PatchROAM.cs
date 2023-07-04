using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Water3D.VertexDeclarations;

namespace Water3D
{
    // -------------------------------------------------------------------------------------------------
    //	PATCH CLASS
    // -------------------------------------------------------------------------------------------------
    class PatchROAM
    {

        private int varianceDepth;
        private int patchSize;
        private int mapSize;
        private LandscapeROAM landscape;
        private Camera cam;
        private TriTreeNode m_BaseLeft, m_BaseRight;
        private int worldX, worldY;
        private int m_isVisible, heightX, heightY;
        private ushort m_VarianceDirty;
        ushort[] m_VarianceLeft, m_VarianceRight;
        private BoundingSphere bs;
        public PatchROAM(LandscapeROAM landscape, Camera cam, int heightX, int heightY, int worldX, int worldY, int patchSize, int mapSize)
        {
            this.landscape = landscape;
            this.cam = cam;
            this.varianceDepth = 8;
            this.patchSize = patchSize;
            this.mapSize = mapSize;
            
            // Clear all the relationships
            m_BaseLeft = new TriTreeNode();
            m_BaseRight = new TriTreeNode();
           
            m_BaseLeft.setBaseNeighbor(m_BaseRight);
            m_BaseRight.setBaseNeighbor(m_BaseLeft);
	        
            // Store Patch offsets for the world and heightmap.
	        this.worldX = worldX;
	        this.worldY = worldY;

	        // Store pointer to first byte of the height data for this patch.
	        this.heightX = heightX;
            this.heightY = heightY;

	        // Initialize flags
	        m_VarianceDirty = 1;
	        m_isVisible = 0;

            m_VarianceLeft = new ushort[1 << (varianceDepth)];			// Left variance tree
            m_VarianceRight = new ushort[1 << (varianceDepth)];		// Right variance tree

            Vector3 center = new Vector3(worldX + patchSize / 2, 0.0f, -worldY - patchSize / 2);
            bs = new BoundingSphere(center, patchSize);
        }

        public TriTreeNode getBaseLeft() 
        { 
            return m_BaseLeft;
        }

        public TriTreeNode getBaseRight() 
        { 
            return m_BaseRight; 
        }

        public ushort isDirty() 
        { 
            return m_VarianceDirty; 
        }

        public int isVisibile() 
        {
            return m_isVisible; 
        }
        // ---------------------------------------------------------------------
        // Split a single Triangle and link it into the mesh.
        // Will correctly force-split diamonds.
        //
        public void split(TriTreeNode tri)
        {
            if (tri != null)
            {
                // We are already split, no need to do it again.
                if (tri.getLeftChild() != null)
                    return;

                // If this triangle is not in a proper diamond, force split our base neighbor
                if (tri.getBaseNeighbor() != null && tri.getBaseNeighbor().getBaseNeighbor() != tri)
                    split(tri.getBaseNeighbor());

                // Create children and link into mesh
                tri.setLeftChild(landscape.allocateTri());
                tri.setRightChild(landscape.allocateTri());

                // If creation failed, just exit.
                if (tri.getLeftChild() == null)
                    return;
                if (tri.getRightChild() == null)
                    return;
                // Fill in the information we can get from the parent (neighbor pointers)
                tri.getLeftChild().setBaseNeighbor(tri.getLeftNeighbor());
                tri.getLeftChild().setLeftNeighbor(tri.getRightChild());

                tri.getRightChild().setBaseNeighbor(tri.getRightNeighbor());
                tri.getRightChild().setRightNeighbor(tri.getLeftChild());

                // Link our Left Neighbor to the new children
                if (tri.getLeftNeighbor() != null)
                {
                    if (tri.getLeftNeighbor().getBaseNeighbor() == tri)
                        tri.getLeftNeighbor().setBaseNeighbor(tri.getLeftChild());
                    else if (tri.getLeftNeighbor().getLeftNeighbor() == tri)
                        tri.getLeftNeighbor().setLeftNeighbor(tri.getLeftChild());
                    else if (tri.getLeftNeighbor().getRightNeighbor() == tri)
                        tri.getLeftNeighbor().setRightNeighbor(tri.getLeftChild());
                    else
                        ;// Illegal Left Neighbor!
                }

                // Link our Right Neighbor to the new children
                if (tri.getRightNeighbor() != null)
                {
                    if (tri.getRightNeighbor().getBaseNeighbor() == tri)
                        tri.getRightNeighbor().setBaseNeighbor(tri.getRightChild());
                    else if (tri.getRightNeighbor().getRightNeighbor() == tri)
                        tri.getRightNeighbor().setRightNeighbor(tri.getRightChild());
                    else if (tri.getRightNeighbor().getLeftNeighbor() == tri)
                        tri.getRightNeighbor().setLeftNeighbor(tri.getRightChild());
                    else
                        ;// Illegal Right Neighbor!
                }

                // Link our Base Neighbor to the new children
                if (tri.getBaseNeighbor() != null)
                {
                    if (tri.getBaseNeighbor().getLeftChild() != null)
                    {
                        tri.getBaseNeighbor().getLeftChild().setRightNeighbor(tri.getRightChild());
                        tri.getBaseNeighbor().getRightChild().setLeftNeighbor(tri.getLeftChild());
                        tri.getLeftChild().setRightNeighbor(tri.getBaseNeighbor().getRightChild());
                        tri.getRightChild().setLeftNeighbor(tri.getBaseNeighbor().getLeftChild());
                    }
                    else
                        split(tri.getBaseNeighbor());  // Base Neighbor (in a diamond with us) was not split yet, so do that now.
                }
                else
                {
                    // An edge triangle, trivial case.
                    tri.getLeftChild().setRightNeighbor(null);
                    tri.getRightChild().setLeftNeighbor(null);
                }
            }
        }
         // ---------------------------------------------------------------------
        // Tessellate a Patch.
        // Will continue to split until the variance metric is met.
        //
        public void recursTessellate(TriTreeNode tri, int leftX, int leftY, int rightX, int rightY, int apexX, int apexY, int node, ushort[] m_CurrentVariance)
        {
            float triVariance = 0.0f;
            int centerX = (leftX + rightX) >> 1; // Compute X coordinate of center of Hypotenuse
            int centerY = (leftY + rightY) >> 1; // Compute Y coord...

            if (node < (1 << varianceDepth))
            {
	            // Extremely slow distance metric (sqrt is used).
	            // Replace this with a faster one!
	            float distance = 1.0f + (float)Math.Sqrt(Math.Pow(centerX - cam.VEye.X, 2) + Math.Pow(centerY + cam.VEye.Z, 2));
              
                // Egads!  A division too?  What's this world coming to!
	            // This should also be replaced with a faster operation.
	            triVariance = ((float)m_CurrentVariance[node] * mapSize * 2) / distance;	// Take both distance and variance into consideration
                
            }

            if ( (node >= (1 << varianceDepth)) ||	// IF we do not have variance info for this node, then we must have gotten here by splitting, so continue down to the lowest level.
                 (triVariance > landscape.FrameVariance))	// OR if we are not below the variance tree, test for variance.
            {
                split(tri);                    											// Split this triangle.
	            if (tri != null && tri.getLeftChild() != null &&											// If this triangle was split, try to split it's children as well.
		            ((Math.Abs(leftX - rightX) >= 2) || (Math.Abs(leftY - rightY) >= 2)))	// Tessellate all the way down to one vertex per height field entry
	            {
		            recursTessellate(tri.getLeftChild(), apexX,  apexY, leftX, leftY, centerX, centerY, node << 1, m_CurrentVariance);
                    recursTessellate(tri.getRightChild(), rightX, rightY, apexX, apexY, centerX, centerY, 1 + (node << 1), m_CurrentVariance);
	            }
            }
         
        }
        // ---------------------------------------------------------------------
        // Render the tree.  Simple no-fan method.
        //
        public void recursRender(TriTreeNode tri, int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
        {
            if (tri!=null && tri.getLeftChild() != null)				// All non-leaf nodes have both children, so just check for one
	        {
                int centerX = (leftX + rightX) >> 1;	// Compute X coordinate of center of Hypotenuse
                int centerY = (leftY + rightY) >> 1;	// Compute Y coord...

		        recursRender(tri.getLeftChild(), apexX, apexY, leftX, leftY, centerX, centerY);
		        recursRender(tri.getRightChild(), rightX, rightY, apexX, apexY, centerX, centerY);
	        }
	        else									// A leaf node!  Output a triangle to be rendered.
	        {
                float leftZ = landscape.getHeightMap(heightX + leftX, heightY + leftY);
                float rightZ = landscape.getHeightMap(heightX + rightX, heightY + rightY);
                float apexZ = landscape.getHeightMap(heightX + apexX, heightY + apexY);
                
                // Actual number of rendered triangles...
                landscape.NumTrisRendered++;

                // Perform lighting calculations.
                Vector3[] vec = new Vector3[3];
                Vector3 normal = new Vector3();	
                
                // Create a vertex normal for this triangle.
                // NOTE: This is an extremely slow operation for illustration purposes only.
                //       You should use a texture map with the lighting pre-applied to the texture.
                
                vec[0].X = leftX;
                vec[0].Y = leftZ;
                vec[0].Z = leftY;
			
                vec[1].X = rightX;
                vec[1].Y = rightZ ;
                vec[1].Z = rightY;
			
                vec[2].X = apexX;
                vec[2].Y = apexZ ;
                vec[2].Z = apexY;

                normal = RenderEngine.calcNormal(vec);
                normal.Normalize();
                float u = ((float)leftX + (float)worldX) / (float)mapSize;
                float v = ((float)leftY + (float)worldY) / (float)mapSize;

                landscape.Verts[landscape.NumVertsRendered] = new PositionNormalMultiTexture(new Vector3((float)landscape.getPosition().X + worldX + (float)leftX, landscape.getPosition().Y + (float)leftZ, (float)-landscape.getPosition().Z - worldY - (float)leftY), normal, new Vector2(u, v), new Vector2(u, v));
                landscape.NumVertsRendered++;

                u = ((float)rightX + (float)worldX) / (float)mapSize;
                v = ((float)rightY + (float)worldY) / (float)mapSize;

                landscape.Verts[landscape.NumVertsRendered] = new PositionNormalMultiTexture(new Vector3((float)landscape.getPosition().X + worldX + (float)rightX, landscape.getPosition().Y + (float)rightZ, (float)-landscape.getPosition().Z - worldY - (float)rightY), normal, new Vector2(u, v), new Vector2(u, v));
                landscape.NumVertsRendered++;
                
                u = ((float)apexX + (float)worldX) / (float)mapSize;
                v = ((float)apexY + (float)worldY) / (float)mapSize;

                landscape.Verts[landscape.NumVertsRendered] = new PositionNormalMultiTexture(new Vector3((float)landscape.getPosition().X + worldX + (float)apexX, landscape.getPosition().Y + (float)apexZ, (float)-landscape.getPosition().Z - worldY - (float)apexY), normal, new Vector2(u, v), new Vector2(u, v));
                landscape.NumVertsRendered++;
            }
        }
        // ---------------------------------------------------------------------
        // Computes Variance over the entire tree.  Does not examine node relationships.
        //
        public ushort recursComputeVariance(float leftX, float leftY, float leftZ, float rightX, float rightY, float rightZ, float apexX, float apexY, float apexZ, int node, ushort[] m_CurrentVariance)
        {
	        //        /|\
	        //      /  |  \
	        //    /    |    \
	        //  /      |      \
	        //  ~~~~~~~*~~~~~~~  <-- Compute the X and Y coordinates of '*'
	        //
	        float centerX = (leftX + rightX) / 2;		// Compute X coordinate of center of Hypotenuse
	        float centerY = (leftY + rightY) / 2;		// Compute Y coord...
	        ushort myVariance;

	        // Get the height value at the middle of the Hypotenuse
            float centerZ = landscape.getHeightMap(centerX, centerY); //m_HeightMap[(centerY * MAP_SIZE) + centerX];
	        // Variance of this triangle is the actual height at it's hypotenuse midpoint minus the interpolated height.
	        // Use values passed on the stack instead of re-accessing the Height Field.
            myVariance = (ushort)Math.Abs(centerZ - ((leftZ + rightZ) / 2));

	        // Since we're after speed and not perfect representations,
	        //    only calculate variance down to an 8x8 block
	        if((Math.Abs(leftX - rightX) >= 16) || (Math.Abs(leftY - rightY) >= 16))
	        {
		        // Final Variance for this node is the max of it's own variance and that of it's children.
                myVariance = (ushort)Math.Max(myVariance, recursComputeVariance(apexX, apexY, apexZ, leftX, leftY, leftZ, centerX, centerY, centerZ, node << 1, m_CurrentVariance));
                myVariance = (ushort)Math.Max(myVariance, recursComputeVariance(rightX, rightY, rightZ, apexX, apexY, apexZ, centerX, centerY, centerZ, 1 + (node << 1),  m_CurrentVariance));
	        }

	        // Store the final variance for this node.  Note Variance is never zero.
	        if (node < ( 1 << varianceDepth))
		        m_CurrentVariance[node] = (ushort)(1 + myVariance);
	        return myVariance;
        }
        // ---------------------------------------------------------------------
        // Reset the patch.
        //
        public void reset()
        {
	        // Assume patch is not visible.
	        m_isVisible = 0;

	        // Reset the important relationships
	        m_BaseLeft.setLeftChild(null);
            m_BaseLeft.setRightChild(null);
            m_BaseRight.setLeftChild(null);
            m_BaseLeft.setLeftChild(null);

	        // Attach the two m_Base triangles together
	        m_BaseLeft.setBaseNeighbor(m_BaseRight);
	        m_BaseRight.setBaseNeighbor(m_BaseLeft);

	        // Clear the other relationships.
            m_BaseLeft.setRightNeighbor(null);
            m_BaseLeft.setLeftNeighbor(null);
            m_BaseRight.setRightNeighbor(null);
            m_BaseRight.setLeftNeighbor(null);
        }
        // ---------------------------------------------------------------------
        // Compute the variance tree for each of the Binary Triangles in this patch.
        //
        public void computeVariance()
        {
	        // Compute variance on each of the base triangles...
            recursComputeVariance(0, patchSize, landscape.getHeightMap(heightX, heightY + patchSize),
                                    patchSize, 0, landscape.getHeightMap(heightX + patchSize, heightY),
                                    0, 0, landscape.getHeightMap(heightX, heightY),
							        1, m_VarianceLeft);
            recursComputeVariance(patchSize, 0, landscape.getHeightMap(heightX + patchSize, heightY),
                                    0, patchSize, landscape.getHeightMap(heightX, heightY + patchSize),
                                    patchSize, patchSize, landscape.getHeightMap(heightX + patchSize, heightY + patchSize),
                                    1, m_VarianceRight);
            // Clear the dirty flag for this patch
            m_VarianceDirty = 0;
        }
        // ---------------------------------------------------------------------
        // Discover the orientation of a triangle's points:
        //
        // Taken from "Programming Principles in Computer Graphics", L. Ammeraal (Wiley)
        //
        private int orientation( int pX, int pY, int qX, int qY, int rX, int rY )
        {
	        int aX, aY, bX, bY;
	        float d;

	        aX = qX - pX;
	        aY = qY - pY;

	        bX = rX - pX;
	        bY = rY - pY;

	        d = (float)aX * (float)bY - (float)aY * (float)bX;
            return (d < 0) ? (-1) : 1;
        }

        // ---------------------------------------------------------------------
        // Set patch's visibility flag.
        //
        public void setVisibility()
        {
            // by now render all patches, view frustum test is made based on vertices
            /*
            m_isVisible = 1;
            return;
            // test corners of patch against view frustum
            int patchCenterX = worldX;
            int patchCenterY = worldY;
            float patchCenterZ = (int)landscape.getHeightMap(patchCenterX, patchCenterY);
            if (isVertexInFrustum(new Vector3(patchCenterX, patchCenterZ, patchCenterY), -30.0f))
            {
                m_isVisible = 1;
                return;
            }

            patchCenterX = worldX + patchSize;
            patchCenterY = worldY;
            patchCenterZ = (int)landscape.getHeightMap(patchCenterX, patchCenterY);
            
            if (isVertexInFrustum(new Vector3(patchCenterX, patchCenterZ, patchCenterY), -30.0f))
            {
                m_isVisible = 1;
                return;
            }

            patchCenterX = worldX;
            patchCenterY = worldY + patchSize;
            patchCenterZ = (int)landscape.getHeightMap(patchCenterX, patchCenterY);
            
            if (isVertexInFrustum(new Vector3(patchCenterX, patchCenterZ, patchCenterY), -30.0f))
            {
                m_isVisible = 1;
                return;
            }
            patchCenterX = worldX + patchSize;
            patchCenterY = worldY + patchSize;
            patchCenterZ = (int)landscape.getHeightMap(patchCenterX, patchCenterY);

            if (isVertexInFrustum(new Vector3(patchCenterX, patchCenterZ, patchCenterY), -30.0f))
            {
                m_isVisible = 1;
                return;
            }
            m_isVisible = 0;
            */
            /*
            // Set visibility flag (orientation of both triangles must be counter clockwise)
            int patchCenterX = m_WorldX + PATCH_SIZE / 2;
            int patchCenterY = m_WorldY + PATCH_SIZE / 2;
            if ((orientation(eyeX, eyeY, rightX, rightY, patchCenterX, patchCenterY) < 0) && (orientation(leftX, leftY, eyeX, eyeY, patchCenterX, patchCenterY) < 0))
                m_isVisible = 1;
            else
                m_isVisible = 0;
            */
            if (cam.ViewFrustum.Intersects(bs))
            {
                m_isVisible = 1;
            }
            else
            {
                m_isVisible = 0;
            }

        }
        public bool isVertexInFrustum(Vector3 v, float sensivity)
        {
            BoundingFrustum frustum = cam.ViewFrustum;
            //test each point against all planes
            Vector4 position4 = new Vector4(v.X, v.Y, v.Z, 1f);
            if (frustum.Far.Dot(position4) < sensivity)
            {
                return false;
            }
            if (frustum.Left.Dot(position4) < sensivity)
            {
                return false;
            }
            if (frustum.Near.Dot(position4) < sensivity)
            {
                return false;
            }
            if (frustum.Right.Dot(position4) < sensivity)
            {
                return false;
            }
            if (frustum.Top.Dot(position4) < sensivity)
            {
                return false;
            }
            if (frustum.Bottom.Dot(position4) < sensivity)
            {
                return false;
            }
            return true;
        }
        // ---------------------------------------------------------------------
        // Create an approximate mesh.
        //
        public void tessellate()
        {
	        // Split each of the base triangles
            recursTessellate(m_BaseLeft, worldX, worldY + patchSize, worldX + patchSize, worldY, worldX, worldY, 1, m_VarianceLeft);
            recursTessellate(m_BaseRight, worldX + patchSize, worldY, worldX, worldY + patchSize, worldX + patchSize, worldY + patchSize, 1, m_VarianceRight);         
        }
        // ---------------------------------------------------------------------
        // Render the mesh.
        //
        public void render()
        {
            recursRender(m_BaseLeft, 0, patchSize, patchSize, 0, 0, 0);
            recursRender(m_BaseRight, patchSize, 0, 0, patchSize, patchSize, patchSize);
        }
    }
}