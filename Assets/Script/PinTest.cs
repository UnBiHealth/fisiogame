using UnityEngine;
using UOS;

public class PinTest : MonoBehaviour {

    float newValue;

    void Update() {

        if (Input.GetKeyDown(KeyCode.Escape)) {
            newValue = 1.0f;
            GenerateExitEvent();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            newValue = Random.Range(0f, 1f);
            GenerateEvent();
        }
        else if (Input.GetKeyDown(KeyCode.H)) {
            newValue = 0.777777f;
            GenerateEvent();
        }
        else if (Input.GetKeyDown(KeyCode.M)) {
            newValue = 0.444444f;
            GenerateEvent();
        }
        else if (Input.GetKeyDown(KeyCode.L)) {
            newValue = 0.111111f;
            GenerateEvent();
        }
    }

    void GenerateEvent() {
        var n = new Notify(PinDriver.UPDATE_EVENT_NAME, PinDriver.DRIVER_NAME);
        n.AddParameter(PinDriver.PIN_PARAM_NAME, "punch");
        n.AddParameter(PinDriver.VALUE_PARAM_NAME, newValue.ToString());
        Debug.Log("Pin: " + newValue);
        uOS.gateway.Notify(n, uOS.gateway.currentDevice);
    }

    void GenerateExitEvent() {
        var n = new Notify(PinDriver.UPDATE_EVENT_NAME, PinDriver.DRIVER_NAME);
        n.AddParameter(PinDriver.PIN_PARAM_NAME, "exerciseEnd");
        n.AddParameter(PinDriver.VALUE_PARAM_NAME, newValue.ToString());
        Debug.Log("Fire exit pin");
        uOS.gateway.Notify(n, uOS.gateway.currentDevice);
    }
}
