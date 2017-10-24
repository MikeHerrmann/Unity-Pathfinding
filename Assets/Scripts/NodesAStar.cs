using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;


public class NodesAStar : MonoBehaviour
{

    public GUIText textField1;
    public GUIText textField2;
    //public GUIText textField3;

    public class Node
    {
        public Vector3 position;
        public Vector2 index;
        public List<Node> walkableNeihgbors;
        public GameObject nodeQuad;
        public bool isWalkable;
        //public float h;
        public float g = float.MaxValue;
        public float f = float.MaxValue;
        public Node parentNode;
    }

    public GameObject quadPrefab;

    Node startNode;
    Node targetNode;
    Node currentNode;
    Node tempNode;

    List<Vector2> neighbourParams;
    List<Vector2> obstacleParams;

    int rowCount = 9;
    int columnCount = 9;
    float gridWidth = 1.5f;
  
    float maxX;
    float maxZ;

    float newX;
    float newZ;

    //List<Color> colors;
    WaitForSeconds delay;
    List<Node> nodes;
    List<Node> openList;
    List<Node> closedList;

    bool metTarget;

    //Stopwatch watch;

    //---------------------------------------------------------------
   
    void Start()
    {
        

        //watch = new Stopwatch();

        nodes = new List<Node>();
        openList = new List<Node>();
        closedList = new List<Node>();
        delay = new WaitForSeconds(0.5f);
        neighbourParams = new List<Vector2> { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, -1), new Vector2(-1, -1), new Vector2(-1, 0), new Vector2(-1, 1) };
        obstacleParams = new List<Vector2> { new Vector2(0, 1), new Vector2(1, 1), new Vector2(2, 1), new Vector2(6, 1), new Vector2(7, 1), new Vector2(8, 1), new Vector2(2, 3), new Vector2(2, 4), new Vector2(2, 5), new Vector2(3, 5), new Vector2(4, 5), new Vector2(4, 3), new Vector2(5, 3), new Vector2(6, 3), new Vector2(6, 4), new Vector2(6, 5), new Vector2(1, 7), new Vector2(2, 7), new Vector2(3, 7), new Vector2(5, 7), new Vector2(6, 7), new Vector2(7, 7) };

        maxX = rowCount / 2 * gridWidth;
        maxZ = columnCount / 2 * gridWidth;

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                Vector3 newPosition = new Vector3(j * gridWidth - maxX, 0, maxZ - i * gridWidth);
                Node newNode = new Node();
                newNode.index = new Vector2(j, i);
                newNode.position = newPosition;
                newNode.isWalkable = true;
                newNode.f = float.MaxValue;
                newNode.g = float.MaxValue; 
                newNode.parentNode = null;
                newNode.nodeQuad = Instantiate(quadPrefab);
                newNode.nodeQuad.transform.position = newPosition;
                nodes.Add(newNode);
               
            }
        }

        SetObstacles();
        SetWalkableNeighbours();
        StartNewSearch();
    }

    //----------------------------------------------------------------

    void SetObstacles()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < obstacleParams.Count; j++)
            {
                if(nodes[i].index == obstacleParams[j]) {
                nodes[i].isWalkable = false;
                nodes[i].nodeQuad.GetComponent<Renderer>().material.color = Color.black;
                }
            }
        }
    }

    //------------------------------------------------------------

    void SetStartAndTarget()
    {
      
        startNode = nodes[(int)Mathf.Round(Random.Range(0, 8))];
        startNode.nodeQuad.GetComponent<Renderer>().material.color = Color.green;
        startNode.g = 0f;
        openList.Add(startNode);

        targetNode = nodes[(int)Mathf.Round(Random.Range(72, 80))];
        targetNode.nodeQuad.GetComponent<Renderer>().material.color = Color.red;

        float diffX = Mathf.Abs(startNode.index.x - startNode.index.x);
        float diffY = Mathf.Abs(startNode.index.y - startNode.index.y);
        startNode.f = Mathf.Sqrt(diffX * diffX + diffY * diffY);
        targetNode.f = 0;
    }

    //--------------------------------------------------------------

    void SetWalkableNeighbours()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].isWalkable == true)
            {
                nodes[i].walkableNeihgbors = DetectNeighbours(nodes[i]);
            }
        }
    }

    //-------------------------------------------------------------

    List<Node> DetectNeighbours(Node checkNode)
    {
        List<Node> neighbourList = new List<Node>();
        float checkX = checkNode.position.x;
        float checkZ = checkNode.position.z;
        Vector3 testPos;
        Node newNeighbour;
        for (int i = 0; i < neighbourParams.Count; i++)
        {
            int a = (int)neighbourParams[i].x;
            int b = (int)neighbourParams[i].y;
            newX = checkX + gridWidth * a;
            newZ = checkZ + gridWidth * b;
            testPos = new Vector3(newX, 0, newZ);
            newNeighbour = nodes.Find(x => x.position == testPos);
            if (newNeighbour != null && newNeighbour.isWalkable == true)
            {
                neighbourList.Add(newNeighbour);
            }
        }
        return neighbourList;
    }

    //-----------------------------------------------

    void Search()
    {
        
        //watch.Start();
        //while (openList.Count > 0)
       if(openList.Count > 0)
        {

            float tempF = float.MaxValue;
            int index = 0;

            for (int i = 0; i < openList.Count; i++)
            {
                tempNode = openList[i];
                if (tempNode.f < tempF)
                {
                    tempF = tempNode.f;
                    index = i;
                }
            }

            currentNode = openList[index];

            if(currentNode == targetNode)
            {
                showPath(targetNode);
                //watch.Stop();
                //print("Ready! Duration: " + watch.ElapsedMilliseconds);
                return;
            }
           
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (Node neighbourNode in currentNode.walkableNeihgbors)

            { 
                if (closedList.Contains(neighbourNode))
                {
                    continue;
                }

                if (!openList.Contains(neighbourNode))
                {
                    openList.Add(neighbourNode);
                }

                float diffX = Mathf.Abs(currentNode.index.x - neighbourNode.index.x);
                float diffY = Mathf.Abs(currentNode.index.y - neighbourNode.index.y);
                float tempG = currentNode.g + Mathf.Sqrt(diffX * diffX + diffY * diffY);// Vector3.Distance(neighbourNode.position, currentNode.position);

                if (tempG > neighbourNode.g)
                {
                    continue;
                }

                neighbourNode.g = tempG;
                diffX = Mathf.Abs(targetNode.index.x - neighbourNode.index.x);
                diffY = Mathf.Abs(targetNode.index.y - neighbourNode.index.y);
                neighbourNode.f = neighbourNode.g + Mathf.Sqrt(diffX * diffX + diffY * diffY); //Vector3.Distance(neighbourNode.position, targetNode.position);
                neighbourNode.parentNode = currentNode;
           
            }

        }
        ShowListElements();
        if(currentNode != startNode)
        currentNode.nodeQuad.GetComponent<Renderer>().material.color = Color.blue;
    }

    //-------------------------------------------------------------------------------

    void showPath(Node pathNode)
    {
     
        if (pathNode.parentNode != null && pathNode.parentNode != startNode)
        {
            Node nextPathNode = pathNode.parentNode;
            nextPathNode.nodeQuad.GetComponent<Renderer>().material.color = Color.yellow;
            showPath(nextPathNode);
        }
        
    }

    //---------------------------------------------

        void ClearAll()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if(nodes[i].isWalkable == true) { 
            nodes[i].parentNode = null;
            nodes[i].g = float.MaxValue;
            nodes[i].f = float.MaxValue;
            nodes[i].nodeQuad.GetComponent<Renderer>().material.color = Color.white;
            }
        }

        openList.Clear();
        closedList.Clear();
       
        StopAllCoroutines();
    }

    //---------------------------------------------------------

    void StartNewSearch()
    {
       
        //SetObstacles();
        //SetWalkableNeighbours();
        SetStartAndTarget();
        //Search();
        StartCoroutine(SearchSteps());
    }

    //--------------------------------------------------------------- 

   void ShowListElements()
    {
        foreach (Node node in nodes)
        {
            if (node != targetNode && node != startNode && node.isWalkable == true)
            {
                node.nodeQuad.GetComponent<Renderer>().material.color = Color.white;
                if (openList.Contains(node))
                {
                    node.nodeQuad.GetComponent<Renderer>().material.color = Color.cyan;
                }
                if (closedList.Contains(node))
                {
                    node.nodeQuad.GetComponent<Renderer>().material.color = Color.gray;
                }
            }
        }
    }

    //-----------------------------------------------------------------

        IEnumerator SearchSteps()
    {
        while (metTarget == false)
        {
            Search();
            yield return delay;
        }
       
    }

    //---------------------------------------------------------------------------

    void Update()
    {


        textField1.text = "Closed List: " + closedList.Count.ToString();
        textField2.text = "Open List: " + openList.Count.ToString();



        if (Input.GetKeyDown("space"))
        {
            ClearAll();
            StartNewSearch();
        }

    }
     
}
