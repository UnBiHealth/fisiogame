using UnityEngine;
using UOS;

public class PinTest : MonoBehaviour
{
    private float timer = 0;

    void Update()
    {
        // Every 3 or 4 seconds generates a new pin value...
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer += Random.Range(2, 3);
            float newValue = Random.Range(-1f, 1f);
            var n = new Notify(PinDriver.UPDATE_EVENT_NAME, PinDriver.DRIVER_NAME);
            n.AddParameter(PinDriver.PIN_PARAM_NAME, "punch");
            n.AddParameter(PinDriver.VALUE_PARAM_NAME, newValue.ToString());
            Debug.Log("uepa: " + newValue);
            uOS.gateway.Notify(n, uOS.gateway.currentDevice);
        }
    }
}
