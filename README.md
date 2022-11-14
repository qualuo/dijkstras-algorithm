# Pathfinding Visualization - Dijkstra's Algorithm

### See it in action: https://qualuo.github.io/dijkstras-algorithm/

Dijkstra's pathfinding algorithm implemented in Unity with C#. Graph consists of a weighted 8-connectivity grid. Red is the target; green is starting point. Blue nodes are the frontier/open/tentative nodes (that have a preliminary shortest distance). Node size indicates weight (smaller = shorter). Longer paths are darker, while short and easy paths are brighter. Random nodes are removed from the grid. New scenarios are procedurally generated.

<p align="center">
  <img src="./Images/pathfinding.gif" alt="Showcase"/>
</p>

## Algorithm

Dijkstra's algorithm is used in pathfinding to find the shortest path from one node to every other node. These are the steps:
1. Choose a starting node. This will be the first 'current' node. (Passed into function.)
2. Add all graph nodes to a set. This set contains all 'unvisited' nodes.
3. Set starting node's 'distance' to 0, and ∞ (represented with max value) for all others. This value ('distance') is the shortest distance to the node from starting node.
4. Set the 'current' node to the one with lowest 'distance' in the 'unvisited' set.
5. Remove 'current' node from set of 'unvisited' nodes.
6. For every neighbor (connected vertex) of 'current' node: if sum of 'distance' to the neighbor is less than the neighbor's current 'distance', save the smaller value (shorter distance), and set a pointer/reference to neighbor.
7. Repeat from 4 until unvisited set is empty.


Or in code: 

        // Dijkstra's Shortest Path Algorithm implementation (no priority queue)
        void FindPathsDijkstra(List<List<GameObject>> nodesGrid, GameObject nodeInitial) {
          HashSet<GameObject> unvisited = new HashSet<GameObject>();

          foreach (List<GameObject> row in nodesGrid) {
              foreach (GameObject node in row) {
                  unvisited.Add(node);
                  if (node.Equals(nodeInitial)) {
                      Node n = node.GetComponent<Node>();
                      n.SetDist(0);
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
                      nodeCurrent = o;
                  }
              }
              unvisited.Remove(nodeCurrent);
              
              // Optional stop after target is found
              if (nodeCurrent.Equals(nodeTarget)) { 
                  isTargetFound = true;
                  break;
              }

              Node node = nodeCurrent.GetComponent<Node>();
              foreach (GameObject neighbor in node.GetNeighbors()) {
                  Node nodeNeighbor = neighbor.GetComponent<Node>();
                  int nxtDist = node.GetTotalDist() + nodeNeighbor.GetWeight();
                  if (nxtDist < nodeNeighbor.GetTotalDist()) {
                      nodeNeighbor.SetDist(nxtDist);
                      nodeNeighbor.SetPrev(nodeCurrent);
                  }
              }
          }
       }
