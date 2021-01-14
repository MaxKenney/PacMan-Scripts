using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{

    public Transform StartPosition;
    public GameObject ThisGhost;

    //The mask that the program will look for when trying to find obstructions to the path.
    public LayerMask WallMask;
    public Vector2 vGridWorldSize;
    public float fNodeRadius;
    public float fDistanceBetweenNodes;

    Node[,] NodeArray;
    public List<Node> prevVisitedNodes = new List<Node>();
    public List<Node> FinalPath;

    public GameObject L_Teleporter;
    public GameObject R_Teleporter;

    //Nodes for the teleport themselves and the spots that the other teleporter throws them to
    Node leftTeleporterNode;
    Node leftTeleporterFrontNode;
    Node rightTeleporterNode;
    Node rightTeleporterFrontNode;



    float fNodeDiameter;
    int iGridSizeX, iGridSizeY;


    private void Start()
    {
        //Double the radius to get diameter
        fNodeDiameter = fNodeRadius * 2;

        //Divide the grids world co-ordinates by the diameter to get the size of the graph in array units.
        iGridSizeX = Mathf.RoundToInt(vGridWorldSize.x / fNodeDiameter);
        iGridSizeY = Mathf.RoundToInt(vGridWorldSize.y / fNodeDiameter);
        CreateGrid();

        InvokeRepeating("DeleteLocationOfGhost", 3.5f, 0.2f);

        leftTeleporterNode = getNearestFloorNode(NodeFromWorldPoint(L_Teleporter.transform.position));
        rightTeleporterNode = getNearestFloorNode(NodeFromWorldPoint(R_Teleporter.transform.position));
        leftTeleporterFrontNode = getNearestFloorNode(NodeFromWorldPoint(L_Teleporter.transform.GetChild(0).position));
        rightTeleporterFrontNode = getNearestFloorNode(NodeFromWorldPoint(R_Teleporter.transform.GetChild(0).position));
    }

    void CreateGrid()
    {
        //Declare the array of nodes.
        NodeArray = new Node[iGridSizeX, iGridSizeY];
        Vector3 bottomLeft = new Vector3(0,0,0) - Vector3.right * vGridWorldSize.x / 2 - Vector3.forward * vGridWorldSize.y / 2;//Get the real world position of the bottom left of the grid.
        
        //Loop through the array of nodes.
        for (int x = 0; x < iGridSizeX; x++)
        {
            for (int y = 0; y < iGridSizeY; y++)
            {
                //Get the world coordinates of the bottom left of the graph
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * fNodeDiameter + fNodeRadius) + Vector3.forward * (y * fNodeDiameter + fNodeRadius);
                bool Wall = true;
                

                //If the node is not being obstructed
                //quick collision check against the current node and anything in the world at its position. If it is colliding with an object with a WallMask,
                //the if statement will return false.
                if (Physics.CheckSphere(worldPoint, fNodeRadius, WallMask))
                {
                    Wall = false;
                }
                //Create a new node in the array.
                NodeArray[x, y] = new Node(Wall, worldPoint, x, y);
            }
        }
    }

    //Function that gets the neighboring nodes of the given node.
    public List<Node> GetNeighboringNodes(Node neighborNode) 
    {
        List<Node> NeighborList = new List<Node>();
        //Variables to check if the X and Y Positions are within range of the node array to avoid out of range errors.
        int icheckX;
        int icheckY;

        //Check the right side of the current node.
        icheckX = neighborNode.iGridX + 1;
        icheckY = neighborNode.iGridY;

        //If the XPosition is in range of the array
        if (icheckX >= 0 && icheckX < iGridSizeX)
        {
            //If the YPosition is in range of the array
            if (icheckY >= 0 && icheckY < iGridSizeY)
            {
                if (icheckX == rightTeleporterNode.iGridX + 2 && icheckY == rightTeleporterNode.iGridY)
                {
                    NeighborList.Add(leftTeleporterFrontNode);
                }
                else //Add the grid to the available neighbors list
                    NeighborList.Add(NodeArray[icheckX, icheckY]);
            }
        }
        //Check the Left side of the current node.
        icheckX = neighborNode.iGridX - 1;
        icheckY = neighborNode.iGridY;
        if (icheckX >= 0 && icheckX < iGridSizeX)
        {
            if (icheckY >= 0 && icheckY < iGridSizeY)
            {
                if (icheckX == leftTeleporterNode.iGridX - 2 && icheckY == leftTeleporterNode.iGridY)
                {
                    NeighborList.Add(rightTeleporterFrontNode);
                }
                else
                    NeighborList.Add(NodeArray[icheckX, icheckY]);
            }
        }
        //Check the Top side of the current node.
        icheckX = neighborNode.iGridX;
        icheckY = neighborNode.iGridY + 1;
        if (icheckX >= 0 && icheckX < iGridSizeX)
        {
            if (icheckY >= 0 && icheckY < iGridSizeY)
            {
                NeighborList.Add(NodeArray[icheckX, icheckY]);
            }
        }
        //Check the Bottom side of the current node.
        icheckX = neighborNode.iGridX;
        icheckY = neighborNode.iGridY - 1;
        if (icheckX >= 0 && icheckX < iGridSizeX)
        {
            if (icheckY >= 0 && icheckY < iGridSizeY)
            {
                NeighborList.Add(NodeArray[icheckX, icheckY]);
            }
        }
        //Return the neighbors list.
        return NeighborList;
    }

    public Vector3 GetRandomPosition()
    {
        
        float randX = Random.Range(1, iGridSizeX - 1);
        float randY = Random.Range(1, iGridSizeY - 1);

        return NodeArray[(int)randX, (int)randY].vPosition;
    }

    public Node getNearestFloorNode(Node targetNode)
    {

        List<Node> OpenList = new List<Node>();
        HashSet<Node> ClosedList = new HashSet<Node>();

        //Add the starting node to the open list to begin the program
        OpenList.Add(targetNode);

        //While there is something in the open list
        while (OpenList.Count > 0)
        {
            Node CurrentNode = OpenList[0];

            //If the neighbor is a wall or has already been checked skip it
            if (ClosedList.Contains(CurrentNode))
            {
                continue;
            }else if (CurrentNode.bIsWall)
            {
                return CurrentNode;
            }
            else
            {
                //Remove this node from the open list
                OpenList.Remove(CurrentNode);

                //Loop through each neighbor of the current node
                foreach (Node NeighborNode in GetNeighboringNodes(CurrentNode))
                {
                    //If the neighbor is a wall or has already been checked skip it
                    if (ClosedList.Contains(NeighborNode))
                    {
                        continue;
                    }

                    //If the neighbor is not in the openlist add it to the list
                    if (!OpenList.Contains(NeighborNode))
                    {
                        OpenList.Add(NeighborNode);
                    }
                    
                }
            }

            

        }

        Debug.Log("we couldnt find a walkable node. (Grid.getNearestFloorNode)");
        return targetNode;
    }

    //Gets the closest node to the given world position.
    public Node NodeFromWorldPoint(Vector3 a_vWorldPos)
    {
        float ixPos = ((a_vWorldPos.x + vGridWorldSize.x / 2) / vGridWorldSize.x);
        float iyPos = ((a_vWorldPos.z + vGridWorldSize.y / 2) / vGridWorldSize.y);

        ixPos = Mathf.Clamp01(ixPos);
        iyPos = Mathf.Clamp01(iyPos);

        if (ixPos < 0)
        {
            ixPos = 1;
            Debug.Log("Whoops, X under 0");
        }
        else if (ixPos > vGridWorldSize.x)
        {
            ixPos = vGridWorldSize.x - 1;
            Debug.Log("Whoops, X over size");
        }

        if (iyPos < 0)
        {
            iyPos = 1;
            Debug.Log("Whoops, Y under 0");
        }
        else if (iyPos > vGridWorldSize.y)
        {
            iyPos = vGridWorldSize.y - 1;
            Debug.Log("Whoops, Y over 0");
        }


        int ix = Mathf.RoundToInt((iGridSizeX - 1) * ixPos);
        int iy = Mathf.RoundToInt((iGridSizeY - 1) * iyPos);


        if (NodeArray[ix, iy] != null)
            return NodeArray[ix, iy];

        return NodeArray[30, 30];
    }

    public void ChangeNodePointToWall(Vector3 a_vWorldPos)
    {
        float ixPos = ((a_vWorldPos.x + vGridWorldSize.x / 2) / vGridWorldSize.x);
        float iyPos = ((a_vWorldPos.z + vGridWorldSize.y / 2) / vGridWorldSize.y);

        ixPos = Mathf.Clamp01(ixPos);
        iyPos = Mathf.Clamp01(iyPos);

        int ix = Mathf.RoundToInt((iGridSizeX - 1) * ixPos);
        int iy = Mathf.RoundToInt((iGridSizeY - 1) * iyPos);

        NodeArray[ix, iy].bIsWall = false;
    }


    //Function that draws the wireframe
    private void OnDrawGizmos()
    {
        //Draw a wire cube with the given dimensions from the Unity inspector
        Gizmos.DrawWireCube(transform.position, new Vector3(vGridWorldSize.x, 1, vGridWorldSize.y));

        //If the grid is not empty
        if (NodeArray != null)
        {
            //Loop through every node in the grid
            foreach (Node n in NodeArray)
            {
                //Set the colours of the nodes based on wall or not
                if (n.bIsWall)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.yellow;
                }

                //If the final path is not empty
                if (FinalPath != null)
                {
                    //If the current node is in the final path set the colour
                    if (FinalPath.Contains(n))
                    {
                        Gizmos.color = Color.red;
                    }

                }

                //Draw the node at the position of the node.
                Gizmos.DrawCube(n.vPosition, Vector3.one * (fNodeDiameter - fDistanceBetweenNodes));
            }
        }
    }

    public void AddLocationOfGhost(Vector3 ghostPosition)
    {
        prevVisitedNodes.Add(NodeFromWorldPoint(ghostPosition));
    }

    public void DeleteLocationOfGhost()
    {
        prevVisitedNodes.RemoveAt(0);
        prevVisitedNodes.RemoveAt(0);
        prevVisitedNodes.RemoveAt(0);
        prevVisitedNodes.RemoveAt(0);
        Debug.Log(prevVisitedNodes.Count);

    }

}
