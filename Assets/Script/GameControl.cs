using System.Collections.Generic;
using UnityEngine;
using UOS;


[RequireComponent(typeof(uOS))]
public class GameControl : MonoBehaviour, UOSApplication, Logger
{
    /// <summary>
    /// Aux struct for pin event queue.
    /// </summary>
    private struct PinEvent
    {
        public UhpPin pin;
        public object newValue;
    }

    #region Unity

    private static bool created = false;

    void Awake() {
        // Prevent the creation of duplicate controllers
        if (!created) {
            DontDestroyOnLoad(transform.gameObject);
            created = true;
        }
        else {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Called before the first update.
    /// </summary>
    void Start()
    {
        // Starts uOS. PinDriver is declared at uOS settings menu on the editor.
        uOS.Init(this, this);

        // Declares the game input pin and registers for change events.
        var pin = new UhpPin();
        pin.name = "punch";
        pin.type = UhpType.Continuous(0, 1); // [-1, 1)
        PinDriver.instance.Add(pin);
        PinDriver.instance.PinChanged += OnPinChanged;
    }

    /// <summary>
    /// Called once each frame.
    /// </summary>
    void Update()
    {
        ProcessLog();
        ProcessPins();
    }

    /// <summary>
    /// Enqueues pin event to be processed at Unity thread.
    /// </summary>
    /// <param name="pin"></param>
    /// <param name="newValue"></param>
    private void OnPinChanged(UhpPin pin, object newValue)
    {
        lock (_pin_lock)
        {
            pinQueue.Enqueue(new PinEvent { pin = pin, newValue = newValue });
        }
    }

    private object _pin_lock = new object();
    private Queue<PinEvent> pinQueue = new Queue<PinEvent>();
    /// <summary>
    /// Flushes pin event queue.
    /// </summary>
    private void ProcessPins()
    {
        lock (_pin_lock)
        {
            while (pinQueue.Count > 0)
            {
                var pinEvent = pinQueue.Dequeue();
                Debug.Log("New pin value for " + pinEvent.pin.name + ": " + pinEvent.newValue);
            }
        }
    }
    #endregion

    #region UOS
    public void Init(IGateway gateway, uOSSettings settings)
    {
    }

    public void TearDown()
    {
    }

    public void Log(object message)
    {
        DoLog("INFO:" + message.ToString());
    }

    public void LogError(object message)
    {
        DoLog("ERROR: " + message);
    }

    public void LogException(System.Exception e)
    {
        DoLog("ERROR: " + e.ToString());
    }

    public void LogWarning(object message)
    {
        DoLog("WARNING: " + message);
    }

    private object _log_lock = new object();
    private Queue<string> logQueue = new Queue<string>();

    private void DoLog(string msg)
    {
        lock (_log_lock)
        {
            logQueue.Enqueue(msg);
        }
    }

    private void ProcessLog()
    {
        lock (_log_lock)
        {
            while (logQueue.Count > 0)
                Debug.Log(logQueue.Dequeue());
        }
    }
    #endregion
}
