using UnityEngine;
using UOS;

public class PinTest : MonoBehaviour {

    float newValue;

    void Update() {

        if (Input.GetKeyDown(KeyCode.Return)) {
            newValue = 1.0f;
            GenerateRepetitionsEvent();
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
        uOS.gateway.Notify(n, uOS.gateway.currentDevice);
    }

    void GenerateRepetitionsEvent() {
        var n = new Notify(PinDriver.UPDATE_EVENT_NAME, PinDriver.DRIVER_NAME);
        n.AddParameter(PinDriver.PIN_PARAM_NAME, "repetitions");
        n.AddParameter(PinDriver.VALUE_PARAM_NAME, 20.ToString());
        uOS.gateway.Notify(n, uOS.gateway.currentDevice);
        Debug.Log("Repetitions event");
    }
}
