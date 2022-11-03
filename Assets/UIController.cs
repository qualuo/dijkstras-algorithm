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

        playToggle.onValueChanged.AddListener(delegate { PlayToggleChange(); });
        gridSizeSlider.onValueChanged.AddListener(delegate { SliderGridSizeChange(); });
        iterationSpeedSlider.onValueChanged.AddListener(delegate { SliderIPSChange(); });
        visToggle.onValueChanged.AddListener(delegate { VisToggleChange(); });
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
    public void SliderGridSizeChange() {
        int v = (int) gridSizeSlider.value;
        gridSizeSliderEvent.Invoke(v);
        gridSizeTMP.text = v + "x" + v;
    }
    public void SliderIPSChange() {
        int v = (int) iterationSpeedSlider.value;
        iterationSpeedSliderEvent.Invoke(v);
        iterationsPSTMP.text = v.ToString();
    }
    public void VisToggleChange() {
        bool isOn = visToggle.isOn;
        visToggleEvent.Invoke(isOn);
        iterationSpeedSlider.gameObject.SetActive(isOn);
    }
    public void PlayToggleChange() {
        bool isOn = playToggle.isOn;
        if (isOn) {
            Time.timeScale = 1.0f;
            playTMP.text = "Playing";
        } else {
            Time.timeScale = 0.0f;
            playTMP.text = "Paused";
        }
    }
}
