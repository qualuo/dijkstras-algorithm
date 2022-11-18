using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour {

    public NodeManager observee;
    private float targetCoord;

    private Camera _camera; // Unity will complain using 'camera'

    public Button minusZoomButton;
    public Button plusZoomButton;


    void Start() {
        _camera = GetComponent<Camera>();

        if (minusZoomButton) {
            minusZoomButton.onClick.AddListener(delegate { ZoomOut(); });
        }
        if (plusZoomButton) {
            plusZoomButton.onClick.AddListener(delegate { ZoomIn(); });
        }
    }

    void ZoomOut() {
        _camera.orthographicSize += 5;
    }
    void ZoomIn() {
        _camera.orthographicSize = Mathf.Max(5, _camera.orthographicSize -= 5);


    }

    // Update is called once per frame
    void Update()
    {
        if (observee.GetIsRoundInProgress()) return;
        targetCoord = observee.GetGridSize()/2;
        float lerp = Mathf.Lerp(transform.position.x, targetCoord, 0.1f);
        transform.position = new Vector3(lerp, lerp, -10);

    }
}
