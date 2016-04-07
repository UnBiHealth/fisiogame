using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CityCursor : MonoBehaviour {
    
    RectTransform rectTransform;

    public Image background;
    public float scrollSpeed;

    public int gridWidth;
    public int gridHeight;

	// Use this for initialization
	void Start () {
        rectTransform = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
    void Update() {

        Vector3 position = Input.mousePosition;

        Rect boundary = background.rectTransform.rect;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(position);

        // If we're moving out of the background, do nothing
        if ((worldPosition.x + gridWidth / 2) > (boundary.x + boundary.width) ||
            (worldPosition.x - gridWidth / 2) < (boundary.x)) {
        }
        // If the position of the cursor with movement applied goes outside the screen, the camera should move instead of the cursor
        else if (position.x <= 0) {
            Camera.main.transform.Translate(- scrollSpeed * Time.deltaTime, 0, 0);
        }
        else if (position.x >= Screen.width) {
            Camera.main.transform.Translate(+ scrollSpeed * Time.deltaTime, 0, 0);
        }
        // Neither happened, so move the cursor
        else {
            transform.position = position;
        }


        if (worldPosition.y + gridHeight / 2 > boundary.y + boundary.height ||
            worldPosition.y - gridHeight / 2 < boundary.y) {
        }
        else if (position.y <= 0) {
            Camera.main.transform.Translate(0, - scrollSpeed * Time.deltaTime, 0);
        }
        else if (position.y >= Screen.height) {
            Camera.main.transform.Translate(0, + scrollSpeed * Time.deltaTime, 0);
        }
        else {
            transform.position = position;
        }

	}
}
