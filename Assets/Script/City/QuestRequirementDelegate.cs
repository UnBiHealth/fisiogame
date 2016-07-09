using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuestRequirementDelegate : ListView.Delegate {

    [SerializeField]
    Image icon;
    [SerializeField]
    Gauge gauge;
    [SerializeField]
    Text text;

    string resource;
    int required;
    int obtained;

    public override void Set(params object[] args) {
        this.resource = args[0] as string;
        required = (int) args[1];
        obtained = (int) args[2];

        Debug.Log("Quest Resource: " + resource);

        GameData.ResourceData resourceData = GameData.instance.GetResourceData(resource);
        if (obtained >= required) {
            icon.sprite = resourceData.highlightedIcon;
        }
        else {
            icon.sprite = resourceData.icon;
        }
        icon.SetNativeSize();

        gauge.minValue = 0;
        gauge.maxValue = required;
        gauge.currentValue = obtained;

        text.text = obtained + " de " + required;
    }
}
