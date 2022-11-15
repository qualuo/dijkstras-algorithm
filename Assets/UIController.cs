using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIController : MonoBehaviour {

    public Toggle playToggle;

    public Slider gridSizeSlider;
    public UnityEvent<int> gridSizeSliderEvent;

    public Slider iterationSpeedSlider;
    public UnityEvent<int> iterationSpeedSliderEvent;

    public Toggle visToggle;
    public UnityEvent<bool> visToggleEvent;

    public TMP_Dropdown algoDropdown;
    public UnityEvent<int> algoDropdownEvent;

    public Toggle randomWeightsToggle;
    public UnityEvent<bool> randomWeightsToggleEvent;


    public NodeManager nodeManager;
    public TextMeshProUGUI playTMP;
    public TextMeshProUGUI distanceTMP;
    public TextMeshProUGUI iterationsTMP;
    public TextMeshProUGUI iterationsPSTMP;
    public TextMeshProUGUI gridSizeTMP;

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
