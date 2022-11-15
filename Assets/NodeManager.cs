using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class NodeManager : MonoBehaviour {
    public GameObject nodePrefab;           // Node used in graph

    private int gridSize;                   // Square grid/graph
    private bool isVisualizing;             // 
    private float iterationsPerSecond;      // Iteration delay
    private int chosenAlgorithm;            // Which algorithm is used to search (index starts at 0)

    private List<List<GameObject>> nodes;   // Graph of 2D grid
    private List<GameObject> lines;         // Collection of lines (so they can be destroyed)
    private GameObject nodeInitial;         // Starting node
    private GameObject nodeTarget;          // End node

    private bool isRoundInProgress;         // A round is a whole beginning to end, after is the reset phase
    private bool isFindingPaths;            // True while algorithm is running
    private bool isTargetFound;             // True when algorithm has found target
    private float colorUpdateTime;          // Color update interval
    private float inProgressTime;           // Duration of current search
    private int minFoundDist;               // Shortest distance of visited nodes so far (used for animation)
    private int iterations;             

    void Start() {
        if (gridSize == 0) gridSize = 12;
        if (iterationsPerSecond == 0) iterationsPerSecond = 10;

        nodes = new List<List<GameObject>>();
        lines = new List<GameObject>();

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
        yield return new WaitUntil(DeleteRandomNeighbors);
        if (chosenAlgorithm == 0)
            StartCoroutine(FindPathsDijkstrasPriorityQueue(nodes, nodeInitial));
        else if (chosenAlgorithm == 1)
            StartCoroutine(FindPathsAStar(nodes, nodeInitial));
        while (isFindingPaths) {
            yield return null;
        }
        PostSearch();
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
    bool DeleteRandomNeighbors() {
        int difficulty = Random.Range(4,10); // Random exclusion threshold
        for (int col = gridSize - 1; col >= 0; col--) {
            for (int row = gridSize - 1; row >= 0; row--) {
                Node n = nodes[col][row].GetComponent<Node>();
                if (n.gameObject.Equals(nodeInitial) || n.gameObject.Equals(nodeTarget)) continue;
                bool willKeep = difficulty > Random.Range(0,10) ? true : false; // Chance to be excluded 
                if (!willKeep) {
                    n.GetComponent<Node>().SetImpassableNode();
                    foreach (GameObject neighbor in n.GetNeighbors()) {
                        neighbor.GetComponent<Node>().GetNeighbors().Remove(n.gameObject);
                    }
                    nodes[col].Remove(n.gameObject);
                    Destroy(n.gameObject);
                }
            }
        }
        return true;
    }
    void PostSearch() {
        if (!isTargetFound) {
            //minFoundDist = int.MaxValue;
        } else if (isTargetFound) {
            List<GameObject> targetPath = new List<GameObject>();
            GameObject p = nodeTarget.GetComponent<Node>().GetPrev();
            DrawLineTargetPath(nodeTarget.transform.position, p.transform.position); // Vis
            while (!p.Equals(nodeInitial)) {
                targetPath.Add(p);
                p.GetComponent<Node>().SetTargetPathNode();
                DrawLineTargetPath(p.transform.position, p.GetComponent<Node>().GetPrev().transform.position); // Vis
                p = p.GetComponent<Node>().GetPrev();
            }
        }
        StartCoroutine(RestartAfterSeconds(5));
    }

    // Dijkstra's Shortest Path Algorithm
    IEnumerator FindPathsDijkstras(List<List<GameObject>> nodesGrid, GameObject nodeInitial) {
        isFindingPaths = true;
        HashSet<GameObject> unvisited = new HashSet<GameObject>();

        foreach (List<GameObject> row in nodesGrid) {
            foreach (GameObject node in row) {
                if (node.Equals(nodeInitial)) {
                    node.GetComponent<Node>().SetDist(0);
                }
                unvisited.Add(node);
            }
        }

        GameObject nodeCurrent = nodeInitial;
        while (unvisited.Count > 0) {
            int min = int.MaxValue;
            foreach (GameObject o in unvisited) {
                Node n = o.GetComponent<Node>();
                int dist = n.GetTotalDist();
                if (dist < min) {
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

            if (min == int.MaxValue) break; // Unsolvable graph! Dijkstra's algorithm requires path to target (Or graph is super massive, overflow)

            if (nodeCurrent.GetComponent<Node>().GetPrev() && isVisualizing) DrawLineVisitedPath(nodeCurrent.transform.position, nodeCurrent.GetComponent<Node>().GetPrev().transform.position); // Visual
            Node node = nodeCurrent.GetComponent<Node>();
            node.SetIsFrontier(false);
            foreach (GameObject neighbor in node.GetNeighbors()) {
                Node nodeNeighbor = neighbor.GetComponent<Node>();
                int nxtDist = node.GetTotalDist() + nodeNeighbor.GetWeight();
                if (nxtDist < nodeNeighbor.GetTotalDist()) {
                    nodeNeighbor.SetDist(nxtDist);
                    nodeNeighbor.SetPrev(nodeCurrent);
                    nodeNeighbor.SetIsFrontier(true); // Visual purpose
                }
            }
            iterations++;
        }
        isFindingPaths = false;
        yield return null;
    }


    // Dijkstra's Shortest Path Algorithm using Priority Queue
    IEnumerator FindPathsDijkstrasPriorityQueue(List<List<GameObject>> nodesGrid, GameObject nodeInitial) {
        isFindingPaths = true;
        SimplePriorityQueue<GameObject, int> unvisited = new SimplePriorityQueue<GameObject, int>();

        foreach (List<GameObject> row in nodesGrid) {
            foreach (GameObject node in row) {
                if (node.Equals(nodeInitial)) {
                    node.GetComponent<Node>().SetDist(0);
                }
                unvisited.Enqueue(node, node.GetComponent<Node>().GetTotalDist());
            }
        }

        GameObject nodeCurrent;
        while (unvisited.Count > 0) {
            nodeCurrent = unvisited.Dequeue();
            minFoundDist = nodeCurrent.GetComponent<Node>().GetTotalDist();

            if (nodeCurrent.Equals(nodeTarget)) {
                isTargetFound = true;
                break;
            }

            if (isVisualizing) yield return new WaitForSeconds(1 / iterationsPerSecond); // Delay for visual purposes
            if (nodeCurrent == null) break; // Node can be destroyed during wait

            if (minFoundDist == int.MaxValue) break; // Unsolvable graph! Dijkstra's algorithm requires path to target (Or graph is super massive, overflow)

            if (nodeCurrent.GetComponent<Node>().GetPrev() && isVisualizing) DrawLineVisitedPath(nodeCurrent.transform.position, nodeCurrent.GetComponent<Node>().GetPrev().transform.position); // Visual
            Node node = nodeCurrent.GetComponent<Node>();
            node.SetIsFrontier(false);
            foreach (GameObject neighbor in node.GetNeighbors()) {
                Node nodeNeighbor = neighbor.GetComponent<Node>();
                int nxtDist = node.GetTotalDist() + nodeNeighbor.GetWeight();
                if (nxtDist < nodeNeighbor.GetTotalDist()) {
                    nodeNeighbor.SetDist(nxtDist);
                    nodeNeighbor.SetPrev(nodeCurrent);
                    unvisited.UpdatePriority(neighbor, nxtDist);
                    nodeNeighbor.SetIsFrontier(true); // Visual purpose
                }
            }
            iterations++;
        }
        isFindingPaths = false;
        yield return null;
    }

    // A* Search Algorithm (Informed Dijkstra's)
    IEnumerator FindPathsAStar(List<List<GameObject>> nodesGrid, GameObject nodeInitial) {
        isFindingPaths = true;
        SimplePriorityQueue<GameObject, int> unvisited = new SimplePriorityQueue<GameObject, int>();

        foreach (List<GameObject> row in nodesGrid) {
            foreach (GameObject node in row) {
                if (node.Equals(nodeInitial)) {
                    node.GetComponent<Node>().SetDist(0);
                }
                unvisited.Enqueue(node, node.GetComponent<Node>().GetTotalDist());
            }
        }

        GameObject nodeCurrent;
        while (unvisited.Count > 0) {
            nodeCurrent = unvisited.Dequeue();
            minFoundDist = nodeCurrent.GetComponent<Node>().GetTotalDist();

            if (nodeCurrent.Equals(nodeTarget)) {
                isTargetFound = true;
                break;
            }

            if (isVisualizing) yield return new WaitForSeconds(1 / iterationsPerSecond); // Delay for visual purposes
            if (nodeCurrent == null) break; // Node can be destroyed during wait

            if (minFoundDist == int.MaxValue) break; // Unsolvable graph! Dijkstra's algorithm requires path to target (Or graph is super massive, overflow)

            if (nodeCurrent.GetComponent<Node>().GetPrev() && isVisualizing) DrawLineVisitedPath(nodeCurrent.transform.position, nodeCurrent.GetComponent<Node>().GetPrev().transform.position); // Visual
            Node node = nodeCurrent.GetComponent<Node>();
            node.SetIsFrontier(false);
            foreach (GameObject neighbor in node.GetNeighbors()) {
                Node nodeNeighbor = neighbor.GetComponent<Node>();
                int nxtDist = node.GetTotalDist() + nodeNeighbor.GetWeight();
                if (nxtDist < nodeNeighbor.GetTotalDist()) {
                    nodeNeighbor.SetDist(nxtDist);
                    nodeNeighbor.SetPrev(nodeCurrent);

                    int priority = nxtDist + CalculateAStarHeuristic(neighbor.transform, nodeTarget.transform);
                    unvisited.UpdatePriority(neighbor, priority);

                    nodeNeighbor.SetIsFrontier(true); // Visual purpose
                }
            }
            iterations++;
        }
        isFindingPaths = false;
        yield return null;
    }

    int CalculateAStarHeuristic(Transform pos1, Transform pos2) {
        //float md = Mathf.Abs(pos2.position.x - pos1.position.x) + Mathf.Abs(pos2.position.y - pos1.position.y); // Manhattan distance, good for 4 directions, no obstacles
        float ed = Mathf.Sqrt(Mathf.Pow(pos2.position.x - pos1.position.x, 2) + Mathf.Pow(pos2.position.y - pos1.position.y, 2)); // Euclidean distance, good all-round
        return (int) ed;
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
    public void SetGridSize(int v) {
        if (isRoundInProgress) return;
        gridSize = v;
    }
    public void SetIteratonsPerSecond(int v) {
        iterationsPerSecond = v;
    }
    public void SetIsVisualizing(bool v) {
        isVisualizing = v;
    }
    public void SetChosenAlgorithm(int v) {
        chosenAlgorithm = v;
    }

}
