using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CameraController : MonoBehaviour {

    public float defaultSize;
    public float zoomIncrement;
    public float maxZoomIns;
    // Use this for initialization
    void Start () {
        defaultSize = Camera.main.orthographicSize;
    }
    
    // Update is called once per frame
    void Update () {
    
    }

    public void ZoomIn() {
        if ((int)Camera.main.orthographicSize > defaultSize - (maxZoomIns * zoomIncrement))
            Camera.main.orthographicSize -= zoomIncrement; 
    }

    public void ZoomOut() {
        if ((int)Camera.main.orthographicSize < defaultSize)
            Camera.main.orthographicSize += zoomIncrement; 
    }

    public void ResetZoom() {
        Camera.main.orthographicSize = defaultSize;
        Camera.main.transform.position = new Vector3(0, 0, -10);
    }

    public void BackToPlaza() {
        SceneManager.LoadScene("Plaza");
    }
}
