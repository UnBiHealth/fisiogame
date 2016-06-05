using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResourceDelegate : ListView.Delegate {

    [SerializeField]
    Image icon;
    [SerializeField]
    Text resourceName;
    [SerializeField]
    Text resourceAmount;
    
    public override void Set(params object[] args) {
        GameData.ResourceData resource = GameData.instance.GetResourceData(args[0] as string);
        icon.sprite = resource.highlightedIcon;
		icon.SetNativeSize();
        resourceName.text = resource.name;
        resourceAmount.text = "" + args[1];
    }
}
