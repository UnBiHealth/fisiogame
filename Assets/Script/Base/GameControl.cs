using System.Collections.Generic;
using UnityEngine;
using UOS;


[RequireComponent(typeof(uOS))]
public class GameControl : MonoBehaviour, UOSApplication, UOS.Logger
{
    // Aux struct for pin event queue.
    private struct PinEvent {
        public UhpPin pin;
        public object newValue;
    }

    #region Unity

    private static GameControl _instance = null;

    public static GameControl instance {
        get {
            return _instance;
        }
    }

    void Awake() {
        // Prevent the creation of duplicate controllers
        if (_instance == null) {
            _instance = this;
            DontDestroyOnLoad(transform.gameObject);
        }
        else {
            Destroy(this.gameObject);
        }
    }

    void Start() {

        if (instance == this) {
            // Starts uOS. PinDriver is declared at uOS settings menu on the editor.
            uOS.Init(this, this);
        }

        // Declares the game input pins and registers for change events.
        var punchPin = new UhpPin();
        punchPin.name = "punch";
        punchPin.type = UhpType.Continuous(0, 1); // [-1, 1)
        PinDriver.instance.Add(punchPin);
        PinDriver.instance.PinChanged += OnPinChanged;

        registeredEvents.Add(punchPin.name, new List<EventHandler>());
    }


    void Update()
    {
        ProcessLog();
        ProcessPins();
    }

    // Enqueues pin event to be processed at Unity thread.
    private void OnPinChanged(UhpPin pin, object newValue) {
        lock (_pin_lock) {
            pinQueue.Enqueue(new PinEvent { pin = pin, newValue = newValue });
        }
    }

    public delegate void EventHandler(object newValue);
    private object _pin_lock = new object();
    private Queue<PinEvent> pinQueue = new Queue<PinEvent>();
    private Dictionary<string, List<EventHandler>> registeredEvents = new Dictionary<string, List<EventHandler>>();

    // Flushes pin event queue.
    private void ProcessPins() {
        lock (_pin_lock) {
            while (pinQueue.Count > 0) {
                var pinEvent = pinQueue.Dequeue();
                Debug.Log("New pin value for " + pinEvent.pin.name + ": " + pinEvent.newValue);
                foreach (var e in registeredEvents[pinEvent.pin.name]) {
                    e(pinEvent.newValue);
                }
            }
        }
    }

    public void RegisterListener(EventHandler handler, string eventName) {
        if (registeredEvents[eventName].Find(e => e == handler) == null) {
            registeredEvents[eventName].Add(handler);
        }
        else {
            Debug.LogWarning("Attempt at double-registering an event for " + eventName);
        }
    }

    public void UnregisterListener(EventHandler handler, string eventName) {
        if (!registeredEvents[eventName].Remove(handler)) {
            Debug.LogWarning("Attempted to remove inexistent event for " + eventName);
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
