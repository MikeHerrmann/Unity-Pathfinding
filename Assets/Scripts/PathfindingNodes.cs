using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class PathfindingNodes : MonoBehaviour {

    public class Node
    {
        public Vector3 position;
        public Vector2 index;
        public List<Node> walkableNeihgbors;
        public GameObject nodePoint;
        public bool isWalkable;
        public bool isVisited;
        public float distanceToTarget;
    }

    GameManager gameManager;

    public GameObject gimbal;
    public GameObject nodePrefab;
    public Camera cam;
    public GUIText textField;

    List<Node> nodes;
    List<Node> path;

    Node startNode;
    Node targetNode;
    Node nextNode;

    List<Vector2> neighbourParams;

    int rowCount = 20;
    int columnCount = 20;
    float gridWidth = 25f;
    float scanHeight = 50f;
    float maxHeightDiff = 20f;

    float newX;
    float newZ;

    float oldHitX;
    float oldHitY;

    //-----------------------------------------------------------

    void Start () {


        gameManager = FindObjectOfType<GameManager>();
        Quaternion gimbalRotation = new Quaternion(gameManager.currentData.recursiveCameraGimbalRotation[0], gameManager.currentData.recursiveCameraGimbalRotation[1], gameManager.currentData.recursiveCameraGimbalRotation[2], gameManager.currentData.recursiveCameraGimbalRotation[3]);
        gimbal.transform.rotation = gimbalRotation;
        Vector3 cameraLocalPosition = new Vector3(gameManager.currentData.recursiveCameraLocalPosition[0], gameManager.currentData.recursiveCameraLocalPosition[1], gameManager.currentData.recursiveCameraLocalPosition[2]);
        cam.transform.localPosition = cameraLocalPosition;

        nodes = new List<Node>();
        path = new List<Node>();
       
 
        neighbourParams = new List<Vector2> { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, -1), new Vector2(-1, -1), new Vector2(-1, 0), new Vector2(-1, 1) };

        CreateNodes();
        ClearAll();
        SetTarget(gameManager.currentData.recursiveTargetIndex[0], gameManager.currentData.recursiveTargetIndex[1]);
        SetStart(gameManager.currentData.recursiveStartIndex[0], gameManager.currentData.recursiveStartIndex[1]);
        SetWalkableNeighbours();
        Search(startNode);
    }

    //-------------------------------------------------------------

        void CreateNodes()
    {
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                RaycastHit hit;
                Vector3 newOrigin = new Vector3(j * gridWidth + gridWidth / 2, scanHeight, i * gridWidth + gridWidth / 2);
                if (Physics.Raycast(newOrigin, -Vector3.up, out hit))
                {

                    Node newNode = new Node();
                    newNode.index = new Vector2(j, i);
                    newNode.position = hit.point;
                    newNode.isWalkable = true;
                    newNode.isVisited = false;

                    newNode.nodePoint = Instantiate(nodePrefab);
                    newNode.nodePoint.transform.position = hit.point;
                    nodes.Add(newNode);

                }
            }
        }
    }

    //---------------------------------------------------------------

    void SetStart(float a, float b)
    {

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].index == new Vector2(a, b))
            {
                startNode = nodes[i];
            }
        }
        startNode.nodePoint.GetComponent<Renderer>().material.color = Color.green;
        startNode.nodePoint.GetComponent<Renderer>().enabled = true;
        startNode.isWalkable = true;
       

    }

    //--------------------------------------------------------------

    void SetTarget(float a, float b)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].index.x == a && nodes[i].index.y == b)
            {
                targetNode = nodes[i];
            }
        }
        targetNode.isWalkable = true;
        targetNode.nodePoint.GetComponent<Renderer>().material.color = Color.red;
        targetNode.nodePoint.GetComponent<Renderer>().enabled = true;

        for (int i = 0; i < nodes.Count; i++)
        {
            float diffX = Mathf.Abs(nodes[i].index.x - targetNode.index.x);
            float diffY = Mathf.Abs(nodes[i].index.y - targetNode.index.y);
            nodes[i].distanceToTarget = Mathf.Sqrt(diffX * diffX + diffY * diffY);
        }

    }

    //-------------------------------------------------------------------

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
        float checkX = checkNode.index.x;
        float checkZ = checkNode.index.y;
        Vector2 testInd;
        Node newNeighbour;
        float checkHeight = checkNode.position.y;
        for (int i = 0; i < neighbourParams.Count; i++)
        {
            int a = (int)neighbourParams[i].x;
            int b = (int)neighbourParams[i].y;
            newX = checkX + a;
            newZ = checkZ + b;
            testInd = new Vector2(newX, newZ);
            newNeighbour = nodes.Find(x => x.index == testInd);
            if (newNeighbour != null && newNeighbour.isWalkable == true)
            {
                float compareHeight = newNeighbour.position.y;
                float diffHeight = Mathf.Abs(checkHeight - compareHeight);
                if (diffHeight <= maxHeightDiff)
                {
                    neighbourList.Add(newNeighbour);
                }
            }
        }
        
        return neighbourList;
    }

    //-----------------------------------------------

    void Search(Node checkNode)
    {

        checkNode.isVisited = true;
        
        path.Add(checkNode);

        if (checkNode == targetNode)
        {
            ShowPath();
           // textField.text = "ZIEL! ";
            return;
        }

        checkNode.walkableNeihgbors = checkNode.walkableNeihgbors.OrderBy(item => item.distanceToTarget).ToList();
       // Node nextNode = checkNode.walkableNeihgbors[0];

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

    void ShowPath()
    {
       

        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] != startNode && path[i] != targetNode)
                path[i].nodePoint.GetComponent<Renderer>().enabled = true;

           
        }

    }

    //-------------------------------------------------------

    void ClearAll()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].isWalkable == true)
            {
                nodes[i].isVisited = false;
                nodes[i].nodePoint.GetComponent<Renderer>().material.color = Color.cyan;
                nodes[i].nodePoint.GetComponent<Renderer>().enabled = false;
            }

        }
       // textField.text = "Test";
        path.Clear();

    }

    //---------------------------------------------------------

        void StoreData()
    {
        gameManager.currentData.recursiveStartIndex = new float[] { startNode.index.x, startNode.index.y };
        gameManager.currentData.recursiveTargetIndex = new float[] { targetNode.index.x, targetNode.index.y };
        float[] gimbalRotation = { gimbal.transform.rotation.x, gimbal.transform.rotation.y, gimbal.transform.rotation.z, gimbal.transform.rotation.w };
        gameManager.currentData.recursiveCameraGimbalRotation = gimbalRotation;
        float[] cameraLocalPosition = new float[] { cam.transform.localPosition.x, cam.transform.localPosition.y, cam.transform.localPosition.z };
        gameManager.currentData.recursiveCameraLocalPosition = cameraLocalPosition;
    }

    //-----------------------------------------------------

    void Update()
    {

        if (Input.GetKeyDown("b"))
        {
            StoreData();
            gameManager.SaveDataBinary();
        }

        if (Input.GetKeyDown("x"))
        {
            StoreData();
            gameManager.SaveDataXML();
        }

        if (Input.GetMouseButton(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;


            if (Physics.Raycast(ray, out hit, 10000))
            {
                float hitX = Mathf.Round((hit.point.x - gridWidth/2)  / gridWidth);
                float hitY = Mathf.Round((hit.point.z - gridWidth/2) / gridWidth);

                if (hitX != oldHitX || hitY != oldHitY)
                {
                    oldHitX = hitX;
                    oldHitY = hitY;
                   // textField.text = hitX + "\n" + hitY;

                    for (int i = 0; i < nodes.Count; i++)
                    {

                        if (hitX == nodes[i].index.x && hitY == nodes[i].index.y)
                        {
                            ClearAll();
                            SetTarget(nodes[i].index.x, nodes[i].index.y);
                            SetStart(startNode.index.x, startNode.index.y);
                            Search(startNode);
                        }
                    }
                }
            }
        }
        //------------
        if (Input.GetMouseButton(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;


            if (Physics.Raycast(ray, out hit, 10000))
            {
                float hitX = Mathf.Round((hit.point.x - gridWidth / 2) / gridWidth);
                float hitY = Mathf.Round((hit.point.z - gridWidth / 2) / gridWidth);

                if (hitX != oldHitX || hitY != oldHitY)
                {
                    oldHitX = hitX;
                    oldHitY = hitY;
                    // textField.text = hitX + "\n" + hitY;

                    for (int i = 0; i < nodes.Count; i++)
                    {

                        if (hitX == nodes[i].index.x && hitY == nodes[i].index.y)
                        {
                            ClearAll();
                            SetTarget(targetNode.index.x, targetNode.index.y);
                            SetStart(nodes[i].index.x, nodes[i].index.y);
                            Search(startNode);
                        }
                    }
                }
            }
        }
    }

    //--------------------------------------------------

}
