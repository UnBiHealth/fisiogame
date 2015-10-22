using System.Collections.Generic;
using UnityEngine;
using UOS;

public class PinDriver : UOSEventDriver, UOSEventListener
{
    public const string DRIVER_NAME = "unbihealth.PinDriver";
    public const string LIST_SERVICE_NAME = "list";
    public const string PINS_FIELD_NAME = "pins";
    public const string CONNECT_SERVICE_NAME = "connect";
    public const string DISCONNECT_SERVICE_NAME = "disconnect";
    public const string UPDATE_EVENT_NAME = "update";
    public const string PIN_PARAM_NAME = "pin";
    public const string VALUE_PARAM_NAME = "value";
    public const string DESTROYED_EVENT_NAME = "destroyed";

    /// <summary>
    /// Handler for pin change events.
    /// </summary>
    /// <param name="pin">The changed pin.</param>
    /// <param name="newValue">The new value.</param>
    public delegate void PinChangedHandler(UhpPin pin, object newValue);

    /// <summary>
    /// Event to listen internally for pin changes.
    /// </summary>
    public event PinChangedHandler PinChanged;

    private static PinDriver _instance = null;
    /// <summary>
    /// This is Unity, there's no beautiful way to make a singleton...
    /// </summary>
    public static PinDriver instance {
        get {
            return _instance;
        }
    }

    private UnityGateway gateway;
    private string instanceId;
    private Dictionary<string, UhpPin> pins = new Dictionary<string, UhpPin>();
    private Dictionary<string, HashSet<UpDevice>> driverListeners = new Dictionary<string, HashSet<UpDevice>>();

    /// <summary>
    /// This "singleton" constructor is public so uOS can instantiate it.
    /// </summary>
    public PinDriver()
    {
        _instance = this;
    }


    #region Internal pin handling methods.
    private object _lock = new object();
    public void Add(UhpPin pin)
    {
        lock (_lock)
        {
            pins[pin.name] = pin;
        }
    }

    public void Remove(string pinName)
    {
        lock (_lock)
        {
            pins.Remove(pinName);
            FireDriverEvent(pinName, DESTROYED_EVENT_NAME, null);
            driverListeners.Remove(pinName);
        }
    }

    public void PinValueChanged(string pinName, object newValue)
    {
        FireDriverEvent(pinName, UPDATE_EVENT_NAME, newValue);
    }
    #endregion

    #region UOSEventDriver
    private static UpDriver _driver = null;
    private static UpDriver driver
    {
        get
        {
            if (_driver == null)
            {
                _driver = new UpDriver(DRIVER_NAME);
                _driver.AddService(LIST_SERVICE_NAME);
                _driver.AddService(CONNECT_SERVICE_NAME)
                    .AddParameter(PIN_PARAM_NAME, UpService.ParameterType.MANDATORY);
                _driver.AddService(DISCONNECT_SERVICE_NAME)
                    .AddParameter(PIN_PARAM_NAME, UpService.ParameterType.MANDATORY);
                _driver.AddEvent(UPDATE_EVENT_NAME)
                    .AddParameter(PIN_PARAM_NAME, UpService.ParameterType.MANDATORY)
                    .AddParameter(VALUE_PARAM_NAME, UpService.ParameterType.MANDATORY);
            }
            return _driver;
        }
    }

    public UpDriver GetDriver()
    {
        return driver;
    }

    public List<UpDriver> GetParent()
    {
        return null;
    }

    public void Init(IGateway gateway, uOSSettings settings, string instanceId)
    {
        this.gateway = (UnityGateway)gateway;
        this.instanceId = instanceId;
        pins.Clear();
        driverListeners.Clear();
        gateway.Register(this, null, DRIVER_NAME, null, UPDATE_EVENT_NAME);
    }

    public void Destroy()
    {
        gateway.Unregister(this);
    }

    public void RegisterListener(Call call, Response response, CallContext context)
    {
        throw new System.InvalidOperationException("Not implemented yet.");
    }

    public void UnregisterListener(Call call, Response response, CallContext context)
    {
        throw new System.InvalidOperationException("Not implemented yet.");
    }
    #endregion


    #region Driver's services.
    public void List(Call call, Response response, CallContext context)
    {
        lock (_lock)
        {
            var list = new List<UhpPin>(pins.Values);
            response.AddParameter(PINS_FIELD_NAME, list.ConvertAll<string>(System.Convert.ToString));
        }
    }

    public void Connect(Call call, Response response, CallContext context)
    {
        UpDevice device = context.callerDevice;
        if (device == null)
            throw new System.ArgumentNullException("device");

        string pinName = call.GetParameterString(PIN_PARAM_NAME);
        if (string.IsNullOrEmpty(pinName))
        {
            response.error = "no pin informed";
            return;
        }
        UhpPin pin;
        if (!pins.TryGetValue(pinName, out pin))
        {
            response.error = "informed pin does not exist";
            return;
        }

        lock (_lock)
        {
            HashSet<UpDevice> devices;
            if (!driverListeners.TryGetValue(pinName, out devices))
                devices = new HashSet<UpDevice>();
            devices.Add(device);
            driverListeners[pinName] = devices;
            response.AddParameter("result", "ok");
        }
    }

    public void HandleEvent(Notify evt)
    {
        UhpPin pin;
        lock (_lock)
        {
            if (!pins.TryGetValue((string)evt.GetParameter(PIN_PARAM_NAME), out pin))
                return;
        }

        object newValue = pin.type.ParseValue(evt.GetParameter(VALUE_PARAM_NAME).ToString());
        if (PinChanged != null)
            PinChanged(pin, newValue);
    }

    private void FireDriverEvent(string pinName, string eventKey, object newValue)
    {
        HashSet<UpDevice> listeners;
        lock (_lock)
        {
            if (!driverListeners.TryGetValue(pinName, out listeners))
                return;
        }
        Notify n = new Notify(eventKey, DRIVER_NAME, instanceId);
        n.AddParameter(PIN_PARAM_NAME, pinName);
        if (UPDATE_EVENT_NAME.Equals(eventKey))
            n.AddParameter(VALUE_PARAM_NAME, newValue.ToString());
        foreach (var device in listeners)
            gateway.Notify(n, device);
    }
    #endregion
}
