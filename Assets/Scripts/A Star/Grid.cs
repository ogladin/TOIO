﻿using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{

    public Transform StartPosition;
    public LayerMask ObstacleMask; 
    public LayerMask BlueCrateMask;
    public LayerMask GreenCrateMask;
    public LayerMask BlueDotMask;
    public LayerMask GreenDotMask;
    public Vector2 vGridWorldSize;
    public float fNodeRadius;
    public float fDistanceBetweenNodes;

    Node[,] NodeArray;
    public List<Node> FinalPath;


    float fNodeDiameter;
    int iGridSizeX, iGridSizeY;

    public Pathfinding Pathfinding;

    public void Init()
    {
          CreateGrid();
          Pathfinding = GetComponent<Pathfinding>();
    }

    public int GetGridWidth()
    {
	    return iGridSizeX;
    }

    public int GetGridHeight()
    {
	    return iGridSizeY;
    }

    public void CreateGrid()
    {
	    fNodeDiameter = fNodeRadius * 2;
	    iGridSizeX = Mathf.RoundToInt(vGridWorldSize.x / fNodeDiameter);
	    iGridSizeY = Mathf.RoundToInt(vGridWorldSize.y / fNodeDiameter);

        NodeArray = new Node[iGridSizeX, iGridSizeY];
        Vector3 bottomLeft = transform.position - Vector3.right * vGridWorldSize.x / 2 - Vector3.forward * vGridWorldSize.y / 2;//Get the real world position of the bottom left of the grid.
        for (int x = 0; x < iGridSizeX; x++)
        {
            for (int y = 0; y < iGridSizeY; y++)
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * fNodeDiameter + fNodeRadius) + Vector3.forward * (y * fNodeDiameter + fNodeRadius);//Get the world co ordinates of the bottom left of the graph
                bool Wall = true;
                Node.NodeLayer Crate = Node.NodeLayer.None;
                Node.NodeLayer Dot= Node.NodeLayer.None;

                //If the node is not being obstructed
                //Quick collision check against the current node and anything in the world at its position. If it is colliding with an object with a ObstacleMask,
                //The if statement will return false.
                if (Physics.CheckSphere(worldPoint, fNodeRadius/2, ObstacleMask))
                {
                    Wall = false;//Object is not a wall
                }
                if (Physics.CheckSphere(worldPoint, fNodeRadius/2, BlueCrateMask))
                {
	                Crate = Node.NodeLayer.Blue;
                }
                if (Physics.CheckSphere(worldPoint, fNodeRadius/2, GreenCrateMask))
                {
	                Crate = Node.NodeLayer.Green;
                }
                if (Physics.CheckSphere(worldPoint, fNodeRadius/2, BlueDotMask))
                {
	                Dot = Node.NodeLayer.Blue;
                }
                if (Physics.CheckSphere(worldPoint, fNodeRadius/2, GreenDotMask))
                {
	                Dot = Node.NodeLayer.Green;
                }


                NodeArray[x, y] = new Node(Wall, Crate, Dot,worldPoint, x, y);//Create a new node in the array.
            }
        }

    }

    //Function that gets the neighboring nodes of the given node.
    public List<Node> GetNeighboringNodes(Node a_NeighborNode)
    {
        List<Node> NeighborList = new List<Node>();
        int icheckX;
        int icheckY;

        //Check the right side of the current node.
        icheckX = a_NeighborNode.iGridX + 1;
        icheckY = a_NeighborNode.iGridY;
        if (icheckX >= 0 && icheckX < iGridSizeX)
        {
            if (icheckY >= 0 && icheckY < iGridSizeY)
            {
                NeighborList.Add(NodeArray[icheckX, icheckY]);
            }
        }
        //Check the Left side of the current node.
        icheckX = a_NeighborNode.iGridX - 1;
        icheckY = a_NeighborNode.iGridY;
        if (icheckX >= 0 && icheckX < iGridSizeX)
        {
            if (icheckY >= 0 && icheckY < iGridSizeY)
            {
                NeighborList.Add(NodeArray[icheckX, icheckY]);
            }
        }
        //Check the Top side of the current node.
        icheckX = a_NeighborNode.iGridX;
        icheckY = a_NeighborNode.iGridY + 1;
        if (icheckX >= 0 && icheckX < iGridSizeX)
        {
            if (icheckY >= 0 && icheckY < iGridSizeY)
            {
                NeighborList.Add(NodeArray[icheckX, icheckY]);
            }
        }
        //Check the Bottom side of the current node.
        icheckX = a_NeighborNode.iGridX;
        icheckY = a_NeighborNode.iGridY - 1;
        if (icheckX >= 0 && icheckX < iGridSizeX)
        {
            if (icheckY >= 0 && icheckY < iGridSizeY)
            {
                NeighborList.Add(NodeArray[icheckX, icheckY]);
            }
        }

        return NeighborList;
    }

    //Gets the closest node to the given world position.
    public Node NodeFromWorldPoint(Vector3 a_vWorldPos)
    {
        float ixPos = ((a_vWorldPos.x + vGridWorldSize.x / 2) / vGridWorldSize.x);
        float iyPos = ((a_vWorldPos.z + vGridWorldSize.y / 2) / vGridWorldSize.y);

        ixPos = Mathf.Clamp01(ixPos);
        iyPos = Mathf.Clamp01(iyPos);

        int ix = Mathf.RoundToInt((iGridSizeX - 1) * ixPos);
        int iy = Mathf.RoundToInt((iGridSizeY - 1) * iyPos);

        return NodeArray[ix, iy];
    }

    public bool isWon()
    {
	    if (NodeArray != null)
	    {
		    for (int x = 0; x < iGridSizeX; x++)
		    {
			    for (int y = 0; y < iGridSizeY; y++) 
			    {
				    if (NodeArray[x,y].bIsCrate==Node.NodeLayer.Green && NodeArray[x,y].bIsDot != Node.NodeLayer.Green)
					    return false;
				    if (NodeArray[x,y].bIsCrate==Node.NodeLayer.Blue && NodeArray[x,y].bIsDot != Node.NodeLayer.Blue)
					    return false;
			    }
		    }

		    return true;
	    }

	    return false;
    }

    public Vector2Int getGridPositionFromUnityPosition(Vector3 unityPos)
    {
	    Vector2Int gridPos = new Vector2Int(-1,-1);//OutOfGrid
	    Vector3 bottomLeft = transform.position - Vector3.right * vGridWorldSize.x / 2 - Vector3.forward * vGridWorldSize.y / 2;
	    for (int x = 0; x < iGridSizeX; x++)
	    {
		    for (int y = 0; y < iGridSizeY; y++)
		    {
			    Vector3 worldPoint = bottomLeft + Vector3.right * (x * fNodeDiameter + fNodeRadius) +
			                         Vector3.forward *
			                         (y * fNodeDiameter +
			                          fNodeRadius);
			    Vector3 BottomLeft = bottomLeft + Vector3.right * (x * fNodeDiameter ) + Vector3.forward * (y * fNodeDiameter);
			    Vector3 TopRight = bottomLeft + Vector3.right * ((x+1) * fNodeDiameter ) + Vector3.forward * ((y+1) * fNodeDiameter);

			    if (unityPos.x >= BottomLeft.x && unityPos.x < TopRight.x && unityPos.z >= BottomLeft.z &&
			        unityPos.z < TopRight.z)
			    {
				    gridPos.x = x;
				    gridPos.y = y;
				    return gridPos;
			    }
		    }
	    }
	    return gridPos;
    }

    public Vector3 getUnityPositionFromGridPosition(Vector2Int gridPos) //return the middle of the node pos
    {
	    if (gridPos.x < 0 && gridPos.y< 0)
		    return Vector3.zero;
	    Vector3 bottomLeft = transform.position - Vector3.right * vGridWorldSize.x / 2 - Vector3.forward * vGridWorldSize.y / 2;
	    return bottomLeft + Vector3.right * (gridPos.x * fNodeDiameter + fNodeRadius) + Vector3.forward * (gridPos.y * fNodeDiameter + fNodeRadius);
    }

    public string LogNodeArray(Vector2Int [] playerpositions)
    {
	    string serializedArray = "";
	    for (int y = 0; y < iGridSizeY; y++) 
	    {
		    for (int x = iGridSizeX-1; x >=0; x--)
		    {
			    bool playerPositionFound = false;
			    for (int i = 0; i < playerpositions.Length; i++)
			    {
				    if (playerpositions[i].x == x && playerpositions[i].y == y)
				    {
					    serializedArray += ""+(i+1);
					    playerPositionFound = true;
					    break;
				    }
			    }
			    if (!playerPositionFound)
			    {
				    Node node = NodeArray[x, y];
				    if (node.bIsCrate == Node.NodeLayer.Blue)
				    {
					    serializedArray += "£";
				    }
				    else if (node.bIsCrate == Node.NodeLayer.Green)
				    {
					    serializedArray += "$";
				    }
				    else if (!node.bIsObstacle)
				    {
					    serializedArray += "#";
				    }
				    else
				    {
					    serializedArray += " ";
				    }
			    }
		    }
		    if (y < iGridSizeY-1)
				serializedArray += "\n"; //No trailing \n
	    }

	    return serializedArray;

    }


    //Function that draws the wireframe for debugging purpose
    // private void OnDrawGizmos()
    // {
	   //  Gizmos.DrawWireCube(transform.position, new Vector3(vGridWorldSize.x, 1, vGridWorldSize.y));//Draw a wire cube with the given dimensions from the Unity inspector
    //
    //     if (NodeArray != null)//If the grid is not empty
    //     {
    //         foreach (Node n in NodeArray)//Loop through every node in the grid
    //         {
    //             if (n.bIsObstacle)//If the current node is a wall node
    //             {
    //                 Gizmos.color = Color.white;//Set the color of the node
    //             }
    //             else
    //             {
    //                 Gizmos.color = Color.yellow;//Set the color of the node
    //             }
    //
    //
    //             if (FinalPath != null)//If the final path is not empty
    //             {
    //                 if (FinalPath.Contains(n))//If the current node is in the final path
    //                 {
    //                     Gizmos.color = Color.red;//Set the color of that node
    //                 }
    //
    //             }
    //
				// if (Gizmos.color != Color.white && Gizmos.color != Color.yellow)
				// 	Gizmos.DrawCube(n.vPosition, Vector3.one * (fNodeDiameter - fDistanceBetweenNodes));//Draw the node at the position of the node.
    //         }
    //     }
    // }
}
