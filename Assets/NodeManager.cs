using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeManager : MonoBehaviour {
    public GameObject nodePrefab;           // Node used in graph

    public Slider gridSizeSlider;
    private int gridSize;                   // Square grid/graph
    private bool isGridSizeSliderInit;
    private bool isGridSizeChanged;

    public Toggle visToggle;
    private bool isVisToggleInit;
    private bool isVisualizing;

    public Slider iterationSpeedSlider;
    private bool isIPSListenerInit;


    private List<List<GameObject>> nodes;   // Graph of 2D grid
    private List<GameObject> lines;         // Collection of lines (so they can be destroyed)
    private GameObject nodeInitial;         // Starting node
    private GameObject nodeTarget;          // End node

    private float iterationsPerSecond; // Iteration delay
    private bool isRoundInProgress;         // A round is a whole beginning to end, then there is the reset phase
    private bool isFindingPaths;            // True while algorithm is running
    private bool isTargetFound;             // True when algorithm has found target
    private float colorUpdateTime;          // Color update interval
    private float inProgressTime;           // Duration of current search
    private int minFoundDist;               // Shortest distance of visited nodes so far (used for animation)
    private int iterations;

    void Start() {

        gridSizeSlider.interactable = false;
        if (gridSizeSlider && !isGridSizeSliderInit) {
            gridSizeSlider.onValueChanged.AddListener(delegate { SliderGridSizeChange(); });
            isGridSizeSliderInit = true;
        }
        if (iterationSpeedSlider && !isIPSListenerInit) { 
            iterationSpeedSlider.onValueChanged.AddListener(delegate { SliderIPSChange(); });
            isIPSListenerInit = true;
        }
        if (visToggle && !isVisToggleInit) {
            visToggle.onValueChanged.AddListener(delegate { VisToggleChange(); });
            isVisToggleInit = true;
        }

        nodes = new List<List<GameObject>>();
        lines = new List<GameObject>();

        if (gridSizeSlider) {
            gridSize = (int) gridSizeSlider.value;
        } else {
            gridSize = 12;
        }
        if (visToggle) {
            isVisualizing = visToggle.isOn;
        } else {
            isVisualizing = true;
        }
        if (iterationSpeedSlider) {
            iterationsPerSecond = iterationSpeedSlider.value; 
        } else {
            iterationsPerSecond = 16f;
        }
        isTargetFound = false;
        colorUpdateTime = 0;
        inProgressTime = 0;
        minFoundDist = 0;
        iterations = 0;

        isRoundInProgress = true;
        StartCoroutine(StartRound());
    }
    IEnumerator StartRound() {
        yield return new WaitUntil(SetupGrid);
        yield return new WaitUntil(PopulateNeighbors);
        yield return new WaitUntil(SetupStartEndNodes);
        StartCoroutine(FindPathsDijkstras(nodes, nodeInitial));
    }

    void Update() {

        float delta = Time.deltaTime;
        if (isVisualizing) {
            colorUpdateTime -= delta;
            if (colorUpdateTime <= 0) {
                StartCoroutine(UpdateNodeColors());
                colorUpdateTime = 0.05f;
            }
        }

        if (isRoundInProgress) {
            inProgressTime += delta;
        }
    }


    IEnumerator UpdateNodeColors() {
        foreach (List<GameObject> row in nodes) {
            foreach (GameObject node in row) {
                Node n = node.GetComponent<Node>();
                if (n.GetIsFrontier()) n.SetFrontierNode(minFoundDist);
                else n.SetVisitedNode(minFoundDist);
            }
        }
        yield return null;
    }

    IEnumerator RestartAfterSeconds(float time) {
        isRoundInProgress = false;
        isTargetFound = false;
        gridSizeSlider.interactable = true;
        StartCoroutine(UpdateNodeColors());
        yield return new WaitForSeconds(time);

        foreach (List<GameObject> row in nodes) {
            foreach (GameObject node in row) {
                Destroy(node);
            }
        }
        foreach(GameObject o in lines) {
            Destroy(o);
        }
        Start();
    }

    bool SetupGrid() {
        for (int col = 0; col < gridSize; col++) {
            nodes.Add(new List<GameObject>());
            for (int row = 0; row < gridSize; row++) {
                GameObject n = Instantiate(nodePrefab);
                n.transform.position = new Vector3(col, row, 0);
                nodes[col].Add(n);
            }
        }
        return true;
    }
    bool PopulateNeighbors() {
        for (int col = 0; col < gridSize; col++) {
            for (int row = 0; row < gridSize; row++) {
                Node n = nodes[col][row].GetComponent<Node>();
                // 8-connectivity
                if (col > 0 && row > 0) n.AddNeighbor(nodes[col - 1][row - 1]);   // Add left  below
                if (col > 0) n.AddNeighbor(nodes[col - 1][row]);   // Add left
                if (col > 0 && row < gridSize - 1) n.AddNeighbor(nodes[col - 1][row + 1]);   // Add left  above
                if (row > 0) n.AddNeighbor(nodes[col][row - 1]);   // Add       below
                if (row < gridSize - 1) n.AddNeighbor(nodes[col][row + 1]);   // Add       above
                if (col < gridSize - 1 && row > 0) n.AddNeighbor(nodes[col + 1][row - 1]);   // Add right below
                if (col < gridSize - 1) n.AddNeighbor(nodes[col + 1][row]);   // Add right
                if (col < gridSize - 1 && row < gridSize - 1) n.AddNeighbor(nodes[col + 1][row + 1]);   // Add right above
            }
        }
        return true;
    }
    bool SetupStartEndNodes() {
        nodeInitial = nodes[Random.Range(0, gridSize)][Random.Range(0, gridSize)];
        nodeInitial.GetComponent<Node>().SetInitial();
        nodeTarget = nodeInitial;
        if (nodeTarget.Equals(nodeInitial)) {
            while (nodeTarget.Equals(nodeInitial)) { // Guarantee target is not same as initial
                nodeTarget = nodes[Random.Range(0, gridSize)][Random.Range(0, gridSize)];
            }
        }
        nodeTarget.GetComponent<Node>().SetTarget();
        return true;
    }

    // Dijkstra's Shortest Path Algorithm without priority queue
    IEnumerator FindPathsDijkstras(List<List<GameObject>> nodesGrid, GameObject nodeInitial) {
        isFindingPaths = true;
        HashSet<GameObject> unvisited = new HashSet<GameObject>();

        foreach (List<GameObject> row in nodesGrid) {
            foreach (GameObject node in row) {
                if (node.Equals(nodeInitial)) {
                    unvisited.Add(node);
                    Node n = node.GetComponent<Node>();
                    n.SetDist(0);
                } else if (node.Equals(nodeTarget)) {
                    unvisited.Add(node);
                } else if (Random.Range(0, 10) < Random.Range(0, 9)) { // Chance to be excluded AND random exclusion threshold
                    node.GetComponent<Node>().SetImpassableNode();
                } else {
                    unvisited.Add(node);
                }
            }
        }

        StartCoroutine(IterateUnvisited(nodesGrid, nodeInitial, unvisited));
        yield return null;
    }

    IEnumerator IterateUnvisited(List<List<GameObject>> nodesGrid, GameObject nodeInitial, HashSet<GameObject> unvisited) {
        GameObject nodeCurrent = nodeInitial;
        while (unvisited.Count > 0) {

            if (nodeCurrent.GetComponent<Node>().GetPrev())
                if (isVisualizing) DrawLineVisitedPath(nodeCurrent.transform.position, nodeCurrent.GetComponent<Node>().GetPrev().transform.position); // Visual

            int min = int.MaxValue;
            foreach (GameObject o in unvisited) {
                Node n = o.GetComponent<Node>();
                int dist = n.GetTotalDist();
                if (dist <= min) {
                    min = n.GetTotalDist();
                    minFoundDist = min; // Visual purpose
                    nodeCurrent = o;
                }
            }
            unvisited.Remove(nodeCurrent);

            if (nodeCurrent.Equals(nodeTarget)) {
                isTargetFound = true;
                break;
            }

            if (isVisualizing) yield return new WaitForSeconds(1 / iterationsPerSecond); // Delay for visual purposes
            if (nodeCurrent == null) break; // Node can be destroyed during wait

            if (min == int.MaxValue) break; // Unsolvable graph! Dijkstra's algorithm expects path to target

            Node node = nodeCurrent.GetComponent<Node>();
            node.SetIsFrontier(false);
            foreach (GameObject neighbor in node.GetNeighbors()) {
                Node nodeNeighbor = neighbor.GetComponent<Node>();
                int nxtDist = node.GetTotalDist() + nodeNeighbor.GetWeight();
                if (nxtDist < nodeNeighbor.GetTotalDist()) {
                    nodeNeighbor.SetDist(nxtDist);
                    nodeNeighbor.SetPrev(nodeCurrent);
                    nodeNeighbor.SetIsFrontier(true); // Visual purpose
                    //if (isVisualizing) DrawLineTentativePath(nodeCurrent.transform.position, nodeNeighbor.transform.position); // Visual
                }
            }
            iterations++;
        }
        if (!isTargetFound) StartCoroutine(RestartAfterSeconds(5));
        isFindingPaths = false;

        if (isTargetFound) {
            List<GameObject> targetPath = new List<GameObject>();
            GameObject p = nodeTarget.GetComponent<Node>().GetPrev();
            DrawLineTargetPath(nodeTarget.transform.position, p.transform.position); // Vis
            while (!p.Equals(nodeInitial)) {
                targetPath.Add(p);
                p.GetComponent<Node>().SetTargetPathNode();
                DrawLineTargetPath(p.transform.position, p.GetComponent<Node>().GetPrev().transform.position); // Vis
                p = p.GetComponent<Node>().GetPrev();
            }
            StartCoroutine(RestartAfterSeconds(5));
        }
        yield return null;
    }

    private void DrawLineVisitedPath(Vector3 start, Vector3 end) {
        GameObject o = new GameObject("Line");
        lines.Add(o);
        LineRenderer lr = o.AddComponent<LineRenderer>();
        Material m = new Material(Shader.Find("Unlit/Color"));
        lr.material = m;
        //m.SetColor("_Color", new Color(0, 0.6f, 0, 1));
        lr.startColor = Color.black; lr.endColor = Color.black;
        lr.startWidth = 0.075f; lr.endWidth = 0.075f;
        lr.SetPosition(0, start); lr.SetPosition(1, end);
    }
    private void DrawLineTentativePath(Vector3 start, Vector3 end) {
        GameObject o = new GameObject("Line");
        lines.Add(o);
        LineRenderer lr = o.AddComponent<LineRenderer>();
        Material m = new Material(Shader.Find("Unlit/Color"));
        lr.material = m;
        lr.startColor = Color.blue; lr.endColor = Color.blue;
        lr.startWidth = 0.005f; lr.endWidth = 0.005f;
        lr.SetPosition(0, start); lr.SetPosition(1, end);
    }
    private void DrawLineTargetPath(Vector3 start, Vector3 end) {
        GameObject o = new GameObject("Line");
        lines.Add(o);
        LineRenderer lr = o.AddComponent<LineRenderer>();
        Material m = new Material(Shader.Find("Unlit/Color"));
        lr.material = m;
        //m.SetColor("_Color", new Color(0.8f, 0, 0, 1));
        lr.startColor = Color.white; lr.endColor = Color.white;
        lr.startWidth = 0.2f; lr.endWidth = 0.2f;
        lr.SetPosition(0, start); lr.SetPosition(1, end);
    }

    public float GetIterationsPerSecond() {
        return iterationsPerSecond;
    }
    public int GetMinFoundDist() {
        return minFoundDist;
    }
    public int GetGridSize() {
        return gridSize;
    }
    public bool GetIsRoundInProgress() {
        return isRoundInProgress;
    }
    public int GetIterations() {
        return iterations;
    }
    public bool GetIsVisualizing() {
        return isVisualizing;
    }

    public void SliderGridSizeChange() {
        gridSize = (int)gridSizeSlider.value;
    }
    public void SliderIPSChange() {
        iterationsPerSecond = iterationSpeedSlider.value;
    }
    public void VisToggleChange() {
        isVisualizing = visToggle.isOn;
        iterationSpeedSlider.gameObject.SetActive(isVisualizing);
    }
}
