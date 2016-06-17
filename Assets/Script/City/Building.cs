using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.SceneManagement;

public class Building : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField]
    Text labelText;
    [SerializeField]
    Image label;

    Image sprite;

    public GameData.BuildingData data;

    void Awake() { 
        data = GameData.instance.GetBuildingData(gameObject.name);
        Debug.Log(data.ToString());
        sprite = GetComponent<Image>();
        labelText.text = data.name;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(0, sprite.GetComponent<RectTransform>().sizeDelta.y / 4);
        labelRect.sizeDelta = new Vector2(labelText.preferredWidth + 30, labelRect.sizeDelta.y);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        label.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        label.gameObject.SetActive(false);
    }
}
