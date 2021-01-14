using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour {

    public Grid GridReference;
    public Transform StartPosition;
    public Vector3 TargetPosition;
    GameManagerScript gameManager;

    public GameObject Player;
    public string GhostName;

    //Inky needs to know Blinkys position for targeting
    public GameObject blinky;

    public string State = "Scatter";

    public Material ThisGhostMaterial;
    public Material EatenMaterial;
    public Material FrightenedMaterial;

    //Scatter, Chase repeat, after final scatter it is chase until round over
    int[] level1_ScatterChaseTiming = new int[7] { 7, 20, 7, 20, 5, 20, 5 };
    int[] level2to4_ScatterChaseTiming = new int[7] { 7, 20, 7, 20, 5, 13, 1 };
    int[] level5_ScatterChaseTiming = new int[7] { 5, 20, 5, 20, 5, 17, 1 };

    //Scatter targets to loop around designated corner
    public GameObject[] ScatterTargets = new GameObject[3];

    //Current item in scatter chase timings
    int scatterChaseCount;
    public int currentLevel;

    //Current target in Scatter targets
    public int scatterTargetCount;

    public float speed = 5.0f;

    private void Start()
    {
        currentLevel = GameObject.Find("GameManager").GetComponent<GameManagerScript>().getCurrentLevel();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        State = "Scatter";
        int scatterChaseCount = 0;
        int scatterTargetCount = 0;

        StartCoroutine(changeState(false));
        InvokeRepeating("AddLocationOfGhost", 3.0f, 0.2f);

    }

    private void Update()
    {
        if (State == "Chase")
            TargetPosition = getTargetLocation();
        else if (State == "Scatter")
            TargetPosition = getScatterLocation();
        else if (State == "Eaten")
            TargetPosition = GridReference.NodeFromWorldPoint(GameObject.Find("ResetPoint").transform.position).vPosition;
        else if (State == "Frightened")
            TargetPosition = GridReference.GetRandomPosition();

        //Find a path to the goal
        FindPath(StartPosition.position, TargetPosition);
    }

    void FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        //Get the node closest to the starting and target position
        Node StartNode = GridReference.NodeFromWorldPoint(startPosition);
        Node TargetNode = GridReference.NodeFromWorldPoint(targetPosition);

        if (!TargetNode.bIsWall || TargetNode == null)
            TargetNode = GridReference.getNearestFloorNode(TargetNode);

        //Containers for the open and closed list
        List<Node> OpenList = new List<Node>();
        HashSet<Node> ClosedList = new HashSet<Node>();

        //Add the starting node to the open list to begin the program
        OpenList.Add(StartNode);

        //Whilst there is something in the open list
        while(OpenList.Count > 0)
        {
            Node CurrentNode = OpenList[0];

            //Loop through the open list starting from the second object
            for(int i = 1; i < OpenList.Count; i++)
            {
                //If the f cost of that object is less than or equal to the f cost of the current node
                //then set the current node to that object
                if (OpenList[i].FCost < CurrentNode.FCost || OpenList[i].FCost == CurrentNode.FCost && OpenList[i].ihCost < CurrentNode.ihCost)
                {
                    CurrentNode = OpenList[i];
                }
            }
            //Remove this node from the open list and add it to the closed list
            OpenList.Remove(CurrentNode);
            ClosedList.Add(CurrentNode);

            //If the current node is the same as the target node calculate the final path
            if (CurrentNode == TargetNode)
            {
                GetFinalPath(StartNode, TargetNode);
            }

            //Loop through each neighbor of the current node
            foreach (Node NeighborNode in GridReference.GetNeighboringNodes(CurrentNode))
            {
                Vector3 toTarget = (NeighborNode.vPosition - transform.position).normalized;

                //If the neighbor is a wall or has already been checked then skip it
                if (!NeighborNode.bIsWall || ClosedList.Contains(NeighborNode) || (ClosedList.Count < 6 && GridReference.prevVisitedNodes.Contains(NeighborNode)))
                {
                    continue;
                }

                //Get the F cost of the neighbor
                int MoveCost = CurrentNode.igCost + GetManhattenDistance(CurrentNode, NeighborNode);

                //If the f cost is greater than the g cost or it is not in the open list
                if (MoveCost < NeighborNode.igCost || !OpenList.Contains(NeighborNode))
                {   
                    //Set the g cost to the f cost
                    NeighborNode.igCost = MoveCost;
                    //Set the h cost
                    NeighborNode.ihCost = GetManhattenDistance(NeighborNode, TargetNode);
                    //Set the parent of the node for retracing steps
                    NeighborNode.ParentNode = CurrentNode;

                    if(!OpenList.Contains(NeighborNode))
                    {
                        OpenList.Add(NeighborNode);
                    }
                }
            }

        }
    }

   

    void GetFinalPath(Node startNode, Node endNode)
    {
        List<Node> FinalPath = new List<Node>();
        Node CurrentNode = endNode;

        //Work through each node going through the parents to the beginning of the path
        while(CurrentNode != startNode)
        {
            //Add that node to the final path and move onto its parent node
            FinalPath.Add(CurrentNode);
            CurrentNode = CurrentNode.ParentNode;
        }
        //Reverse the path to get the correct order
        FinalPath.Reverse();

        //Check to make sure It's not ontop of it's final destination
        if (FinalPath.Count >= 1)
        {
            //Set the final path
            GridReference.FinalPath = FinalPath;
            MoveToPosition(FinalPath[0]);
        }
        else
        {
            MoveToPosition(GridReference.getNearestFloorNode(GridReference.NodeFromWorldPoint(getTransformInFrontOfGhost(2))));
        }

    }

    void MoveToPosition(Node nodeToGoTo)
    {

        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, nodeToGoTo.vPosition, step);
        transform.LookAt(nodeToGoTo.vPosition);
    }


    int GetManhattenDistance(Node nodeA, Node nodeB)
    {
        int ix = Mathf.Abs(nodeA.iGridX - nodeB.iGridX);
        int iy = Mathf.Abs(nodeA.iGridY - nodeB.iGridY);

        return ix + iy;
    }


    Vector3 getScatterLocation()
    {
        if (Vector3.Distance(ScatterTargets[scatterTargetCount].transform.position, gameObject.transform.position) < 2)
        {
            scatterTargetCount++;
            if (scatterTargetCount > ScatterTargets.Length - 1)
                scatterTargetCount = 0;
        }

        return ScatterTargets[scatterTargetCount].transform.position;
    }

    Vector3 getTargetLocation()
    {
        //If the name is Blinky (or a wrong name) just target the player
        Vector3 TargetToReturn;

        if (GhostName == "Blinky")
            TargetToReturn = Player.transform.position;
        else if (GhostName == "Pinky")
        {
            //Target 4 tiles in front of player
            TargetToReturn = getTransformInfrontOfPlayer(16);
        }
        else if (GhostName == "Inky")
        {
            //Target (difference between 4 tiles infront and Blinky, rotated by 180 degrees),
            Vector3 frontPosition = getTransformInfrontOfPlayer(8);
            float differenceBetweenBlinky = Vector3.Distance(frontPosition, blinky.transform.position);

            Vector3 dir = (frontPosition - blinky.transform.position).normalized;
            Debug.DrawLine(frontPosition, blinky.transform.position, Color.red, 0.1f);
            Vector3 newPosition = new Vector3(frontPosition.x - (blinky.transform.position.x - frontPosition.x), frontPosition.y, frontPosition.z - (blinky.transform.position.z - frontPosition.z));
            Debug.DrawLine(frontPosition, newPosition, Color.green, 0.1f);
            TargetToReturn = newPosition;
        }
        else //Clyde
        {
            //Target The player unless the player is within 8 tiles, in which case go to scatter
            if(Vector3.Distance(gameObject.transform.position, Player.transform.position) < 8)
            {
                TargetToReturn = getScatterLocation();
            }
            else
            {
                TargetToReturn = Player.transform.position;
            }
        }

        return TargetToReturn;

    }

    public IEnumerator changeState(bool overrideFrightened)
    {
        if (!gameManager.isFrightened || overrideFrightened)
        {
            int[] currentChaseScatterTimings;
            int waitTime;

            if (currentLevel == 1)
                currentChaseScatterTimings = level1_ScatterChaseTiming;
            else if (currentLevel > 1 && currentLevel < 5)
                currentChaseScatterTimings = level2to4_ScatterChaseTiming;
            else
                currentChaseScatterTimings = level5_ScatterChaseTiming;


            if (currentChaseScatterTimings.Length <= scatterChaseCount)
                waitTime = 30;
            else
                waitTime = currentChaseScatterTimings[scatterChaseCount];



            yield return new WaitForSeconds(waitTime);
            if(!gameManager.isFrightened || overrideFrightened)
            {
                if (scatterChaseCount % 2 == 0)
                    State = "Chase";
                else
                    State = "Scatter";
                scatterChaseCount++;
                StartCoroutine(changeState(false));
            } 
        }
    }

    Vector3 getTransformInfrontOfPlayer(int distance)
    {
        Vector3 playerPos = Player.transform.position;
        Vector3 playerDirection = Player.transform.forward;
        Quaternion playerRotation = Player.transform.rotation;
        Vector3 positionInFront = playerPos + playerDirection * distance;

        return (playerPos + playerDirection * distance);
    }

    Vector3 getTransformInFrontOfGhost(int distance)
    {
        Vector3 ghostPos = gameObject.transform.position;
        Vector3 ghostDirection = gameObject.transform.forward;

        return (ghostPos + ghostDirection * distance);
    }
    
    void AddLocationOfGhost()
    {        
        Vector3 behind = gameObject.transform.TransformPoint(0, 0, -1);
        GridReference.AddLocationOfGhost(behind);
        behind = gameObject.transform.TransformPoint(1, 0, -1);
        GridReference.AddLocationOfGhost(behind);
        behind = gameObject.transform.TransformPoint(-1, 0, -1);
        GridReference.AddLocationOfGhost(behind);
        behind = gameObject.transform.TransformPoint(0, 0, 0);
        GridReference.AddLocationOfGhost(behind);

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.tag == "Player") {
            if(State == "Frightened")
            {
                BecomeEaten();
            }else if(State == "Chase" || State == "Scatter")
            {
                GameObject.Find("GameManager").GetComponent<GameManagerScript>().ResetGame();
            }
        }
    }
    public void ExitFrightened()
    {
        if(State == "Frightened")
        {
            this.gameObject.transform.GetChild(0).GetComponent<Renderer>().material = ThisGhostMaterial;
            StartCoroutine(changeState(false));
        }
            
    }

    public void EnterFrightened()
    {
        if(State != "Eaten")
        {
            State = "Frightened";
            this.gameObject.transform.GetChild(0).GetComponent<Renderer>().material = FrightenedMaterial;
        }

    }

    void BecomeEaten()
    {
        State = "Eaten";
        this.gameObject.transform.GetChild(0).GetComponent<Renderer>().material = EatenMaterial;
    }

    public void StopEaten()
    {   
        if(State == "Eaten")
        {
            this.gameObject.transform.GetChild(0).GetComponent<Renderer>().material = ThisGhostMaterial;
            StartCoroutine(changeState(true));
        }
    }
}
