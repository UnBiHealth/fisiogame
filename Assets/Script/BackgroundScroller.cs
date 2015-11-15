using UnityEngine;
using System.Collections;

// Object that lists all its children and, when told to, scrolls them
public class BackgroundScroller : MonoBehaviour {

    // Set these through the editor
    public float begin;
    public float end;
    public float scrollSpeed;

    bool scrolling = false;

    ArrayList children = new ArrayList();

    private class Child {
        GameObject childObject;
        // The position FROM WHICH the object should be reset (to the left of the camera)
        public float resetPosition;
        // The position TO WHICH the object should be reset (to the right of the camera)
        public float resetDestination;
        // The higher the parallax layer, the faster the scroll speed for an object
        public float parallaxLayer;

        public Child(GameObject child, float begin, float end) {
            float spriteWidth = child.GetComponent<SpriteRenderer>().bounds.size.x;

            childObject = child;
            resetPosition = begin - spriteWidth / 2;
            resetDestination = end - spriteWidth / 2;
            parallaxLayer = child.GetComponent<SpriteRenderer>().sortingOrder;
        }

        public Transform GetTransform() {
            return childObject.transform;
        }

        public void ResetPosition() {
            Vector3 position = childObject.transform.localPosition;
            position.x = resetDestination;
            childObject.transform.localPosition = position;
        }
    }

	void Start () {
        // List every child as a Child object
        var list = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform t in list) {
            if (t.gameObject != gameObject) {
                children.Add(new Child(t.gameObject, begin, end));
            }
        }
	}
	
	void Update () {
        if (scrolling) {
            foreach (Child child in children) {
                child.GetTransform().Translate(-scrollSpeed * child.parallaxLayer * Time.deltaTime, 0, 0, Space.Self);
                if (child.GetTransform().localPosition.x < child.resetPosition) {
                    child.ResetPosition();
                }
            }
        }
	}

    public void Scroll() {
        scrolling = true;
    }

    public void StopScrolling() {
        scrolling = false;
    }
}
