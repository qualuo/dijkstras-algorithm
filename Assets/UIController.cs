using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public NodeManager nodeManager;
    public TextMeshProUGUI distanceTMP;
    public TextMeshProUGUI iterationsTMP;
    public TextMeshProUGUI iterationsPSTMP;
    public TextMeshProUGUI gridSizeTMP;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (nodeManager.GetIsVisualizing()) UpdateUI();
        else if (!nodeManager.GetIsRoundInProgress()) UpdateUI();
    }
    void UpdateUI() {
        int distance = nodeManager.GetMinFoundDist();
        if (distance == int.MaxValue) distanceTMP.text = "∞";
        else distanceTMP.text = distance.ToString();

        iterationsTMP.text = nodeManager.GetIterations().ToString();

        iterationsPSTMP.text = nodeManager.GetIterationsPerSecond().ToString();

        int gridSize = nodeManager.GetGridSize();
        gridSizeTMP.text = gridSize + "X" + gridSize;
    }
}
