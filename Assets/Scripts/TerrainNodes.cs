using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainNodes : MonoBehaviour {

    public class Node
    {
        public Vector3 position;
        public Vector2 index;
        public List<Node> walkableNeihgbors;
        public GameObject nodePoint;
        public bool isWalkable;
        //public float h;
        public float g = float.MaxValue;
        public float f = float.MaxValue;
        public Node parentNode;
    }

    public GameObject gimbal;
    public Terrain terrain;
    public GameObject nodePrefab;
    public Camera cam;
    public GUIText textField;

    GameManager gameManager;

    List<Node> nodes;
    List<Node> openList;
    List<Node> closedList;

    Node startNode;
    Node targetNode;

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
        Quaternion gimbalRotation = new Quaternion(gameManager.currentData.aStarCameraGimbalRotation[0], gameManager.currentData.aStarCameraGimbalRotation[1], gameManager.currentData.aStarCameraGimbalRotation[2], gameManager.currentData.aStarCameraGimbalRotation[3]);
        gimbal.transform.rotation = gimbalRotation;
        Vector3 cameraLocalPosition = new Vector3(gameManager.currentData.aStarCameraLocalPosition[0], gameManager.currentData.aStarCameraLocalPosition[1], gameManager.currentData.aStarCameraLocalPosition[2]);
        cam.transform.localPosition = cameraLocalPosition;

        nodes = new List<Node>();
        openList = new List<Node>();
        closedList = new List<Node>();
 
        neighbourParams = new List<Vector2> { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, -1), new Vector2(-1, -1), new Vector2(-1, 0), new Vector2(-1, 1) };

        CreateNodes();
        SetWalkableNeighbours();
        ClearAll();
        SetTarget(gameManager.currentData.aStarTargetIndex[0], gameManager.currentData.aStarTargetIndex[1]);
        SetStart(gameManager.currentData.aStarStartIndex[0], gameManager.currentData.aStarStartIndex[1]);
        Search();
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
                    newNode.f = float.MaxValue;
                    newNode.g = float.MaxValue;
                    newNode.parentNode = null;
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
        startNode.g = 0f;
        startNode.isWalkable = true;
        float diffX = Mathf.Abs(startNode.index.x - targetNode.index.x);
        float diffY = Mathf.Abs(startNode.index.y - targetNode.index.y);
        startNode.f = Mathf.Sqrt(diffX * diffX + diffY * diffY);
        if (!openList.Contains(startNode))
        {
            openList.Add(startNode);
        }
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
        targetNode.f = 0;
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

    void Search()
    {
        //textField.text = "PATH!" + "\nOpen List: " + openList.Count;
        while (openList.Count > 0)

        {

            float tempF = float.MaxValue;
            int index = 0;

            for (int i = 0; i < openList.Count; i++)
            {
                Node tempNode = openList[i];
                if (tempNode.f < tempF)
                {
                    tempF = tempNode.f;
                    index = i;
                }
            }

            Node currentNode = openList[index];

            if (currentNode == targetNode)
            {
             
                showPath(targetNode);
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
                float diffH = Mathf.Abs(currentNode.position.y - neighbourNode.position.y) / 10;
                float tempG = currentNode.g + Mathf.Sqrt(diffX * diffX + diffY * diffY) + diffH;// Vector3.Distance(neighbourNode.position, currentNode.position);

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
    }

    //-------------------------------------------------------------------------------

    void showPath(Node pathNode)
    {

        if (pathNode.parentNode != null && pathNode.parentNode != startNode)
        {
            Node nextPathNode = pathNode.parentNode;
            nextPathNode.nodePoint.GetComponent<Renderer>().enabled = true;
            showPath(nextPathNode);
        }
       // textField.text = "PATH!" + "\nOpen List: " + openList.Count; ;
    }

    //---------------------------------------------

    void ClearAll()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].isWalkable == true)
            {
                nodes[i].g = float.MaxValue;
                nodes[i].f = float.MaxValue;
                nodes[i].nodePoint.GetComponent<Renderer>().material.color = Color.cyan;
                nodes[i].nodePoint.GetComponent<Renderer>().enabled = false;
            }

        }

        openList.Clear();
        closedList.Clear();

    }

    //---------------------------------------------------------

        void StoreData()
    {
        gameManager.currentData.aStarStartIndex = new float[] { startNode.index.x, startNode.index.y };
        gameManager.currentData.aStarTargetIndex = new float[] { targetNode.index.x, targetNode.index.y };
        float[] gimbalRotation = { gimbal.transform.rotation.x, gimbal.transform.rotation.y, gimbal.transform.rotation.z, gimbal.transform.rotation.w };
        gameManager.currentData.aStarCameraGimbalRotation = gimbalRotation;
        float[] cameraLocalPosition = new float[] { cam.transform.localPosition.x, cam.transform.localPosition.y, cam.transform.localPosition.z };
        gameManager.currentData.aStarCameraLocalPosition = cameraLocalPosition;
    }

    //-------------------------------------------------------------------

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
                            Search();
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
                            Search();
                        }
                    }
                }
            }
        }
    }

    //--------------------------------------------------

}
