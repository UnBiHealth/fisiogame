using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CameraController : MonoBehaviour {

    public float defaultSize;
    public int maxZoomIns;

    int currentZoomIns;
    // Use this for initialization
    void Start () {
        defaultSize = Camera.main.orthographicSize;
        ResetZoom();
    }

    public void ZoomIn() {
        if (currentZoomIns < maxZoomIns) {
            currentZoomIns++;
            Camera.main.orthographicSize = defaultSize / currentZoomIns; 
        }
    }

    public void ZoomOut() {
        if (currentZoomIns > 1) {
            currentZoomIns--;
            Camera.main.orthographicSize = defaultSize / currentZoomIns; 
        }
    }

    public void ResetZoom() {
        currentZoomIns = 2;
        Camera.main.orthographicSize = defaultSize / currentZoomIns;
        Camera.main.transform.position = new Vector3(0, 0, -10);
    }

    public void BackToPlaza() {
        SceneManager.LoadScene("Plaza");
    }
}
