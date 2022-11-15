using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

    public bool isWeightRandom;
    public int weight;

    private List<GameObject> neighbors; // Connected nodes/vertices
    public GameObject prev;             // Previous node on path to target
    private int dist;                   // Cost/distance to reach this node
    private bool isColorable;
    private bool isFrontier;            // True if node is a neighbor but not yet visited
    private int weightMax;

    void Start() {
        if (isWeightRandom) {
            weightMax = 1000;
            weight = Random.Range(1, weightMax + 1);
        } else {
            weightMax = 1;
            weight = 1;
        }
        neighbors = new List<GameObject>();
        prev = null;
        dist = int.MaxValue;
        isColorable = true;
        isFrontier = false;

        if (GetComponent<Renderer>() == null) return;
        GetComponent<Renderer>().material.SetColor("_Color", new Color(0.8f, 0.8f, 0.8f, 1));
        if (weightMax != 1) {
            float v = MapRange(weight, 1, weightMax, 0.6f, 0.98f);
            transform.localScale = new Vector3(v, v, v);
        }
    }

    public static float MapRange(float val, float min1, float max1, float min2, float max2) {
        return ((val - min1) * (max2 - min2) / (max1 - min1)) + min2;
    }
    public void SetIsWeightRandom(bool b) {
        isWeightRandom = b;
    }

    public List<GameObject> GetNeighbors() {
        return neighbors;
    }
    public GameObject GetPrev() {
        return prev;
    }
    public int GetWeight() {
        return weight;
    }

    public int GetTotalDist() {
        return dist;
    }
    public bool GetIsFrontier() {
        return isFrontier;
    }

    public void AddNeighbor(GameObject n) {
        neighbors.Add(n);
    }
    public void SetDist(int d) {
        dist = d;
    }
    public void SetPrev(GameObject n) {
        prev = n;
    }
    public void SetIsFrontier(bool b) {
        isFrontier = b;
    }

    public void SetInitial() {
        if (GetComponent<Renderer>() == null) return;
        GetComponent<Renderer>().material.SetColor("_Color", new Color(0f, 0.8f, 0, 1));
        isColorable = false;
        transform.localScale = new Vector3(0.98f, 0.98f, 0.98f);
    }
    public void SetTarget() {
        if (GetComponent<Renderer>() == null) return;
        GetComponent<Renderer>().material.SetColor("_Color", new Color(0.8f, 0, 0, 1));
         isColorable = false;
    }
    public void SetImpassableNode() {
        if (GetComponent<Renderer>() == null) return;
        GetComponent<Renderer>().material.SetColor("_Color", new Color(0.1f, 0.1f, 0.1f, 1));
        isColorable = false;
        transform.localScale = new Vector3(0.98f, 0.98f, 0.98f);
    }
    public void SetTargetPathNode() {
        if (GetComponent<Renderer>() == null) return;
        GetComponent<Renderer>().material.SetColor("_Color", new Color(0.9f, 0.8f, 0, 1));  
        isColorable = false;
    }
    public void SetVisitedNode(int min) {
        if (!isColorable) return;
        if (GetTotalDist() == int.MaxValue) return;
        if (GetComponent<Renderer>() == null) return;
        Renderer r = GetComponent<Renderer>();
        r.material.SetColor("_Color", new Color(0, MapRange(GetTotalDist(), 0, min, 0.7f, 0.4f), 0, 1));
    }
    public void SetFrontierNode(int min) {
        if (!isColorable) return;
        if (GetTotalDist() == int.MaxValue) return;
        if (GetComponent<Renderer>() == null) return;
        Renderer r = GetComponent<Renderer>();
        r.material.SetColor("_Color", new Color(0, 0.4f, 0.6f, 1));
    }

}
