using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CityCursorOld : MonoBehaviour {
    
    RectTransform rectTransform;

    public float speed;
    public Image background;

	// Use this for initialization
	void Start () {
    //    UnityEngine.Cursor.visible = false;
        rectTransform = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
    void Update() {
        //Debug.Log(rectTransform.position.ToString());

        Vector3 position = rectTransform.position;
        Vector3 movement = new Vector3((Input.GetKey(KeyCode.LeftArrow ) ? - speed * Time.deltaTime : 0) +
                                       (Input.GetKey(KeyCode.RightArrow) ? speed * Time.deltaTime : 0),
                                       (Input.GetKey(KeyCode.UpArrow   ) ? + speed * Time.deltaTime : 0) +
                                       (Input.GetKey(KeyCode.DownArrow ) ? - speed * Time.deltaTime : 0),
                                       0);

        Debug.Log(position.x + movement.x < 0 || position.x + movement.x > Screen.width);
        Debug.Log((position.x + movement.x) + " " + (Screen.width));

        Rect boundary = background.rectTransform.rect;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(position + movement);

        Debug.Log(boundary.ToString());

        // If we're moving out of the background, do nothing
        if (worldPosition.x + 16 > boundary.x + boundary.width ||
            worldPosition.x - 16 < boundary.x) { }
        // If the position of the cursor with movement applied goes outside the screen, the camera should move instead of the cursor
        else if (position.x + movement.x < 0 || position.x + movement.x > Screen.width) {
            Camera.main.transform.Translate(movement.x, 0, 0);
        }
        // Neither happened, so move the cursor
        else {
            rectTransform.Translate(movement.x, 0, 0);
        }


        if (worldPosition.y + 16 > boundary.y + boundary.height ||
            worldPosition.y - 16 < boundary.y) { }
        else if (position.y + movement.y < 0 || position.y + movement.y > Screen.height) {
            Camera.main.transform.Translate(0, movement.y, 0);
        }
        else {
            rectTransform.Translate(0, movement.y, 0);
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            var pointer = new PointerEventData(EventSystem.current);
            var raycastResults = new List<RaycastResult>();

            pointer.position = rectTransform.position;
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0) {
                var topmost = raycastResults[0].gameObject;
                var b = topmost.GetComponent<Button>();
                if (b != null) {
                    b.onClick.Invoke();
                }
            }
        }
	}

    public void Hit() {
        Debug.Log("Hit!");
    }
}
