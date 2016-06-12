using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CameraController : MonoBehaviour {

    public int defaultZoomIns;
    public int currentZoomIns;

    public float minSize;
    public float maxSize;

    public float currentSize {
        get {
            return Camera.main.orthographicSize;
        }
        set {
            Camera.main.orthographicSize = value;
        }
    }

    // Use this for initialization
    void Start () {
        ResetZoom();
    }

    public void ZoomIn() {
        if (currentSize <= minSize) {
            currentSize = minSize;
        }
        if (maxSize / (currentZoomIns + 1) <= minSize) {
            currentZoomIns++;
            currentSize = minSize;
        }
        else {
            currentZoomIns++;
            currentSize = maxSize / currentZoomIns; 
        }
    }

    public void ZoomOut() {
        if (currentZoomIns == 1) {
            currentSize = maxSize;
        }
        else {
            currentZoomIns--;
            currentSize = maxSize / currentZoomIns; 
        }
    }

    public void ResetZoom() {
        currentZoomIns = defaultZoomIns;
        currentSize = maxSize / defaultZoomIns;
        Camera.main.transform.position = new Vector3(0, 0, -10);
    }

    public void BackToPlaza() {
        SceneManager.LoadScene("Plaza");
    }
}
