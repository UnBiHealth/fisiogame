using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Gauge : MonoBehaviour {

    public int minValue { 
        get {
            return _minValue;
        }
        set {
            _minValue = value;
            _currentValue = Mathf.Clamp(_currentValue, _minValue, _maxValue);
            Refresh();
        }
    }
    public int maxValue {
        get {
            return _maxValue;
        }
        set {
            _maxValue = (value != 0 ? value : 1);
            _currentValue = Mathf.Clamp(_currentValue, _minValue, _maxValue);
            Refresh();
        }
    }

    public int currentValue {
        get {
            return _currentValue;
        }
        set {
            _currentValue = Mathf.Clamp(value, _minValue, _maxValue);
            Refresh();
        }
    }

    int _minValue = 0;
    int _maxValue = 1;
    int _currentValue = 1;

    [SerializeField]
    GameObject fill;
    [SerializeField]
    Sprite fullFill;
    [SerializeField]
    Sprite mediumFill;
    [SerializeField]
    Sprite lowFill;


	// Use this for initialization
	void Start () {
        Refresh();
	}

    public void Refresh() {
        Vector3 scale = fill.transform.localScale;
        scale.x = (_currentValue / (float)(_maxValue - _minValue));
        fill.transform.localScale = scale;
        if (_currentValue < _maxValue / 2) {
            fill.GetComponent<Image>().overrideSprite = lowFill;
        }
        else if (_currentValue < _maxValue) {
            fill.GetComponent<Image>().overrideSprite = mediumFill;
        }
        else {
            fill.GetComponent<Image>().overrideSprite = fullFill;
        }
    }
}
