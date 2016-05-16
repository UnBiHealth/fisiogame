using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Building : MonoBehaviour {

    public string uiName;
    public string yield;
    public int yieldAmount;

    void Start() {
        GameData.BuildingData data = GameData.instance.GetBuildingData(gameObject.name);
        uiName = data.name;
        yield = data.yield;
        yieldAmount = data.yieldAmount;
    }
}
