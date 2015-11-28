using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    // Use this for initialization
    void Start () {
    }
    
    // Update is called once per frame
    void Update () {
    
    }

    public void ZoomIn() {
        if ((int)Camera.main.orthographicSize > 60)
            Camera.main.orthographicSize -= 15; 
    }

    public void ZoomOut() {
        if ((int)Camera.main.orthographicSize < 270)
            Camera.main.orthographicSize += 15; 
    }

    public void ResetZoom() {
        Camera.main.orthographicSize = 135; 
    }
}
