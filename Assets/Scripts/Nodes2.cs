using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;


public class Nodes2 : MonoBehaviour
{
    public class Node
    {
        public Vector3 position;
        public Vector2 index;
        public List<Node> walkableNeihgbors;
        public GameObject nodeQuad;
        public bool isWalkable;
        public bool isVisited;
        public float distanceToTarget;
    }


    public GameObject quadPrefab;
    [SerializeField]
    public List<Node> nodes = new List<Node>();

    public GUIText textField;

    Node startNode;
    Node targetNode;
    Node nextNode;

    List<Vector2> neighbourParams;
    List<Vector2> obstacleParams;

    int rowCount = 9;
    int columnCount = 9;
    float gridWidth = 1.5f;
  
    float maxX;
    float maxZ;

    float newX;
    float newZ;

    List<Color> colors;
    WaitForSeconds delay;
    List<Node> path;

    Stopwatch watch;

    //---------------------------------------------------------------

    void Start()
    {
        watch = new Stopwatch();

        path = new List<Node>();
        colors = new List<Color>{ Color.blue, Color.cyan, Color.magenta, Color.yellow };
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
                newNode.position = newPosition;
                newNode.index = new Vector2(j, i);
                newNode.isWalkable = true;
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




    void Search(Node checkNode)
    {
        

        checkNode.isVisited = true;

        path.Add(checkNode);

        if (checkNode == targetNode)
        {
            StartCoroutine(ShowPath());
           
            //ShowPath();
            //watch.Stop();
            //print("Ready! Duration: " + watch.ElapsedMilliseconds);
            return;
        }

        checkNode.walkableNeihgbors = checkNode.walkableNeihgbors.OrderBy(item => item.distanceToTarget).ToList();

        for (int i = 0; i < checkNode.walkableNeihgbors.Count; i++)
        {
            nextNode = checkNode.walkableNeihgbors[i];
            if (nextNode.isVisited == true || nextNode.walkableNeihgbors.Count == 0)
            {
                continue;
            }
            Search(nextNode);
        }
        
    }


    //------------------------------------------------------

    IEnumerator ShowPath()
   //void ShowPath()
    {
        int test = path.Count;
        for (int i = 0; i < test; i++)
        {
            if (path[i] != startNode && path[i] != targetNode)
                path[i].nodeQuad.GetComponent<Renderer>().material.color = Color.yellow;
          
            yield return delay;
        }
       
    }

    //-------------------------------------------------------

    void ClearAll()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            Node node = nodes[i];
            if(node.isWalkable == true) {
            node.isVisited = false;
           
            node.nodeQuad.GetComponent<Renderer>().material.color = Color.white;
            }
        }
       
        StopAllCoroutines();
        path.Clear();
    }

    //----------------------------------------------------------

    void SetObstacles()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < obstacleParams.Count; j++)
            {
                if (nodes[i].index == obstacleParams[j])
                {
                    nodes[i].isWalkable = false;
                    nodes[i].nodeQuad.GetComponent<Renderer>().material.color = Color.black;
                }
            }
        }
    }

    //------------------------------------------------------------

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

    //----------------------------------------------------

    void SetStart()
    {

        startNode = nodes[(int)Mathf.Round(Random.Range(0, 8))];
        startNode.nodeQuad.GetComponent<Renderer>().material.color = Color.green;
        startNode.isWalkable = true;


    }

    //--------------------------------------------------------------

    void SetTarget()
    {
        targetNode = nodes[(int)Mathf.Round(Random.Range(72, nodes.Count - 1))];
        targetNode.isWalkable = true;
        targetNode.nodeQuad.GetComponent<Renderer>().material.color = Color.red;

        for (int i = 0; i < nodes.Count; i++)
        {
            float diffX = Mathf.Abs(nodes[i].index.x - targetNode.index.x);
            float diffY = Mathf.Abs(nodes[i].index.y - targetNode.index.y);
            nodes[i].distanceToTarget = Mathf.Sqrt(diffX * diffX + diffY * diffY);
        }

    }

    //-------------------------------------------------------------------

    void StartNewSearch()
    {
        ClearAll();
        SetTarget();
        SetStart();
        watch.Start();
        Search(startNode);
    }

    //---------------------------------------------------------------

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
           
            StartNewSearch();
        }
    }

    //----------------------------------------------------------------
}
