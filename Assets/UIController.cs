using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIController : MonoBehaviour {

    [SerializeField]
    private Toggle playToggle;

    [SerializeField]
    private Slider gridSizeSlider;
    [SerializeField]
    private UnityEvent<int> gridSizeSliderEvent;

    [SerializeField]
    private Slider iterationSpeedSlider;
    [SerializeField]
    private UnityEvent<int> iterationSpeedSliderEvent;

    [SerializeField]
    private Toggle visToggle;
    [SerializeField]
    private UnityEvent<bool> visToggleEvent;

    [SerializeField]
    private TMP_Dropdown algoDropdown;
    [SerializeField]
    private UnityEvent<int> algoDropdownEvent;

    [SerializeField]
    private Toggle randomWeightsToggle;
    [SerializeField]
    private UnityEvent<bool> randomWeightsToggleEvent;


    [SerializeField]
    private NodeManager nodeManager;
    [SerializeField]
    private TextMeshProUGUI playTMP;
    [SerializeField]
    private TextMeshProUGUI distanceTMP;
    [SerializeField]
    private TextMeshProUGUI iterationsTMP;
    [SerializeField]
    private TextMeshProUGUI iterationsPSTMP;
    [SerializeField]
    private TextMeshProUGUI gridSizeTMP;

    void Start() {

        gridSizeSliderEvent.Invoke((int)gridSizeSlider.value);
        iterationSpeedSliderEvent.Invoke((int)iterationSpeedSlider.value);
        visToggleEvent.Invoke(visToggle.isOn);
        algoDropdownEvent.Invoke((int)algoDropdown.value);
        randomWeightsToggleEvent.Invoke(randomWeightsToggle.isOn);

        playToggle.onValueChanged.AddListener(delegate { OnPlayToggleChange(); });
        gridSizeSlider.onValueChanged.AddListener(delegate { OnSliderGridSizeChange(); });
        iterationSpeedSlider.onValueChanged.AddListener(delegate { OnSliderIPSChange(); });
        visToggle.onValueChanged.AddListener(delegate { OnVisToggleChange(); });
        algoDropdown.onValueChanged.AddListener(delegate { OnAlgoDropdownChange(); });
        randomWeightsToggle.onValueChanged.AddListener(delegate { OnRandomWeightsToggleChange(); });

    }

    void Update() {
        if (nodeManager.GetIsRoundInProgress()) gridSizeSlider.interactable = false;
        else gridSizeSlider.interactable = true;

        if (nodeManager.GetIsVisualizing()) UpdateUI();
        else if (!nodeManager.GetIsRoundInProgress()) UpdateUI();
    }
    void UpdateUI() {
        int distance = nodeManager.GetMinFoundDist();
        if (distance == int.MaxValue) distanceTMP.text = "∞";
        else distanceTMP.text = distance.ToString();

        iterationsTMP.text = nodeManager.GetIterations().ToString();
    }
    public void OnPlayToggleChange() {
        bool isOn = playToggle.isOn;
        if (isOn) {
            Time.timeScale = 1.0f;
            playTMP.text = "Playing";
        } else {
            Time.timeScale = 0.0f;
            playTMP.text = "Paused";
        }
    }
    public void OnSliderGridSizeChange() {
        int v = (int) gridSizeSlider.value;
        gridSizeSliderEvent.Invoke(v);
        gridSizeTMP.text = v + "x" + v;
    }
    public void OnSliderIPSChange() {
        int v = (int) iterationSpeedSlider.value;
        iterationSpeedSliderEvent.Invoke(v);
        iterationsPSTMP.text = v.ToString();
    }
    public void OnVisToggleChange() {
        bool isOn = visToggle.isOn;
        visToggleEvent.Invoke(isOn);
        iterationSpeedSlider.gameObject.SetActive(isOn);
    }
    public void OnAlgoDropdownChange() {
        int v = algoDropdown.value;
        algoDropdownEvent.Invoke(v);
    }
    public void OnRandomWeightsToggleChange() {
        bool isOn = randomWeightsToggle.isOn;
        randomWeightsToggleEvent.Invoke(isOn);
    }
}
