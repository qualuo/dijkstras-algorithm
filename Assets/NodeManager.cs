using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour {
    public GameObject nodePrefab;
    public int gridSize = 15;

    private List<List<GameObject>> nodes;
    private int distMinFound;
    private bool isInProgress;
    private bool isTargetFound;
    private GameObject nodeInitial;
    private GameObject nodeTarget;
    private List<GameObject> lines;

    void Start() {
        nodes = new List<List<GameObject>>();
        distMinFound = 0;
        isTargetFound = false;
        isInProgress = false;
        lines = new List<GameObject>();

        // Set up grid (graph)
        for (int col = 0; col < gridSize; col++) {
            nodes.Add(new List<GameObject>());
            for (int row = 0; row < gridSize; row++) {
                GameObject n = Instantiate(nodePrefab);
                n.transform.position = new Vector3(col, row, 0);
                nodes[col].Add(n);
            }
        }
    }

    // Update is called once per frame
    void Update() {

        if (!isInProgress) {
            isInProgress = true;
            StartVisualization();
        } else if (isInProgress) {
            foreach (List<GameObject> row in nodes) {
                foreach (GameObject node in row) {
                    node.GetComponent<Node>().AddVisColor(distMinFound);
                }
            }
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
        }
    }

    IEnumerator RestartAfterSeconds(float time) {
        isTargetFound = false;
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
    void StartVisualization() {
        
        // Populate grid neighbors (8-connectivity)
        for (int col = 0; col < gridSize; col++) {
            for (int row = 0; row < gridSize; row++) {
                Node n = nodes[col][row].GetComponent<Node>();
                if (col > 0            && row > 0           ) n.AddNeighbor(nodes[col - 1][row - 1]);   // Add left  below
                if (col > 0                                 ) n.AddNeighbor(nodes[col - 1][row    ]);   // Add left
                if (col > 0            && row < gridSize - 1) n.AddNeighbor(nodes[col - 1][row + 1]);   // Add left  above
                if (                      row > 0           ) n.AddNeighbor(nodes[col    ][row - 1]);   // Add       below
                if (                      row < gridSize - 1) n.AddNeighbor(nodes[col    ][row + 1]);   // Add       above
                if (col < gridSize - 1 && row > 0           ) n.AddNeighbor(nodes[col + 1][row - 1]);   // Add right below
                if (col < gridSize - 1                      ) n.AddNeighbor(nodes[col + 1][row    ]);   // Add right
                if (col < gridSize - 1 && row < gridSize - 1) n.AddNeighbor(nodes[col + 1][row + 1]);   // Add right above
            }
        }

        // Choose start and end points
        nodeInitial = nodes[Random.Range(0, gridSize)][Random.Range(0, gridSize)];
        nodeInitial.GetComponent<Node>().SetInitial();
        nodeTarget = nodes[Random.Range(0, gridSize)][Random.Range(0, gridSize)];
        if (nodeTarget.Equals(nodeInitial)) {
            while (nodeTarget.Equals(nodeInitial)) {
                nodeTarget = nodes[Random.Range(0, gridSize)][Random.Range(0, gridSize)];
            }
        } 
        nodeTarget.GetComponent<Node>().SetTarget();

        StartCoroutine(DSPA(nodes, nodeInitial));
    }

    /*
     * Dijkstra's Shortest Path Algorithm
     *
     * Dijkstra's algorithm is used to find the shortest path from one node to every other node. These are the steps:
     * 1. Choose a starting node. This will be the first 'current' node. (Passed into function.)
     * 2. Add all nodes to a set. This set contains all 'unvisited' nodes.
     * 3. Make an array, set starting node's distance value to 0, and INFINITY (max value) for all others. 
     *    This value ('distance') is the shortest distance to the node from starting node.
     * 4. Set the 'current' node to the one with lowest 'distance' in the unvisited set.
     * 5. Remove 'current' node from set of unvisited nodes.
     * 6. For every neighbor of current node: if sum of 'distance' to the neighbor is less than the neighbor's current 'distance', 
     *    save the smaller value (shorter distance), and set a pointer/reference to neighbor.
     * 7. Repeat from 4 until unvisited set is empty.
     * 
     */
    IEnumerator DSPA(List<List<GameObject>> nodesGrid, GameObject nodeInitial) { // without Priority Queue
        HashSet<GameObject> unvisited = new HashSet<GameObject>();

        foreach (List<GameObject> row in nodesGrid) {
            foreach (GameObject node in row) {
                unvisited.Add(node);
                if (node.Equals(nodeInitial)) {
                    Node n = node.GetComponent<Node>();
                    n.AddDist(0);
                }
            }
        }

        GameObject nodeCurrent = nodeInitial;
        while (unvisited.Count > 0) {
            int min = int.MaxValue;
            foreach (GameObject o in unvisited) {
                int d = o.GetComponent<Node>().GetTotalDist();
                if (d <= min) {
                    min = o.GetComponent<Node>().GetTotalDist();
                    distMinFound = min; // Visual purpose only
                    nodeCurrent = o;
                }
            }
            unvisited.Remove(nodeCurrent);
            if (nodeCurrent.Equals(nodeTarget)) { 
                isTargetFound = true;
                break;
            }

            yield return new WaitForSeconds(0.05f); // Delay for visual purposes
            if (nodeCurrent == null) break;

            Node node = nodeCurrent.GetComponent<Node>();
            foreach (GameObject neighbor in node.GetNeighbors()) {
                Node nodeNeighbor = neighbor.GetComponent<Node>();
                int nxtDist = node.GetTotalDist() + nodeNeighbor.GetWeight();
                if (nxtDist < nodeNeighbor.GetTotalDist()) {
                    nodeNeighbor.AddDist(nxtDist);
                    nodeNeighbor.SetPrev(nodeCurrent);
                    DrawLineTentative(nodeCurrent.transform.position, nodeNeighbor.transform.position); // Visual
                }
            }
        }
        yield return null;
    }

    private void DrawLineTentative(Vector3 start, Vector3 end) {
        GameObject o = new GameObject("Line");
        lines.Add(o);
        LineRenderer lr = o.AddComponent<LineRenderer>();
        Material m = new Material(Shader.Find("Standard"));
        lr.material = m;
        lr.startColor = Color.black; lr.endColor = Color.black;
        lr.startWidth = 0.05f; lr.endWidth = 0.05f;
        lr.SetPosition(0, start); lr.SetPosition(1, end);
    }

    private void DrawLineTargetPath(Vector3 start, Vector3 end) {
        GameObject o = new GameObject("Line");
        lines.Add(o);
        LineRenderer lr = o.AddComponent<LineRenderer>();
        Material m = new Material(Shader.Find("Standard"));
        lr.material = m;
        lr.startColor = Color.green; lr.endColor = Color.green;
        lr.startWidth = 0.2f; lr.endWidth = 0.2f;
        lr.SetPosition(0, start); lr.SetPosition(1, end);
    }
}
