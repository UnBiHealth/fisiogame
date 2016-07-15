using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecIcon : MonoBehaviour {

    Image image;

    Color opaque = new Color(0xff, 0xff, 0xff, 0xff);
    Color faded = new Color(0xff, 0xff, 0xff, 0x0);

	void Awake () {
        image = GetComponent<Image>();
        GameControl.instance.RegisterListener(OnRecChanged, "rec");
	}

    public void OnRecChanged(object newValue) {
        if ((int.Parse(newValue.ToString())) == 0) {
            image.color = faded;
        }
        else {
            image.color = opaque;
        }
    }

    public void Hide() {
        image.color = faded;
    }
}
