using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ListView : MonoBehaviour {

    [SerializeField]
    GameObject delegatePrefab;

    [SerializeField]
    int spacing;

    [SerializeField]
    bool resizeView;

    [SerializeField]
    int maxViewHeight;

    [SerializeField]
    GameObject viewport;
    [SerializeField]
    GameObject content;

    private float viewHeight;

    public abstract class Delegate : MonoBehaviour {
        public int index;
        public abstract void Set(params object[] args);
    }

    public delegate bool DelegateTest(object[] data);
    public delegate bool DelegateCompare(object[] dataA, object[] dataB);

    private List<object[]> listModel = new List<object[]>();
    private List<GameObject> childList = new List<GameObject>();

    public void Add(params object[] delegateParams) {

        GameObject instance = Instantiate(delegatePrefab);
        instance.transform.SetParent(content.transform, false);
        instance.GetComponent<Delegate>().Set(delegateParams);
        instance.GetComponent<Delegate>().index = listModel.Count;
        instance.transform.SetAsLastSibling();

        if (listModel.Count > 0) {
            viewHeight += spacing;
        }

        RectTransform rectTransform = instance.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
        rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMax.y);
        rectTransform.pivot = new Vector2(0f, 1f);

        float itemHeight = rectTransform.sizeDelta.y;
        instance.transform.localPosition = new Vector3(0, -viewHeight, transform.localPosition.z);

        viewHeight += itemHeight;
        
        ResizeView();

        listModel.Add(delegateParams);
        childList.Add(instance);
    }

    public void Remove(int index) {
        Transform deletee = childList[index].transform;

        Transform prev = null;
        Transform next = childList[childList.Count - 1].transform;
        next.GetComponent<Delegate>().index--;

        for (int i = Count() - 2; i >= index; --i) {
            prev = next;
            next = childList[i].transform;
            prev.localPosition = next.localPosition;
            next.GetComponent<Delegate>().index--;
        }

        viewHeight -= (deletee.GetComponent<RectTransform>().sizeDelta.y + spacing);
        ResizeView();

        listModel.RemoveAt(index);
        childList.RemoveAt(index);
        deletee.SetParent(null, false);
        Destroy(deletee.gameObject);
    }
    
    public void Remove(DelegateTest isTarget) {

        for (int i = 0; i < Count(); ++i) {
            object[] data = listModel[i];
            if (isTarget(data)) {
                Remove(i);
                --i;
            }
        }
    }

    void Swap(int indexA, int indexB) {
        Debug.Log("Swap " + indexA + " and " + indexB);
        var temp = listModel[indexA];
        listModel[indexA] = listModel[indexB];
        listModel[indexB] = temp;

        var temp2 = childList[indexA];
        childList[indexA] = childList[indexB];
        childList[indexB] = temp2;

        int temp3 = childList[indexA].GetComponent<Delegate>().index;
        childList[indexA].GetComponent<Delegate>().index = childList[indexB].GetComponent<Delegate>().index;
        childList[indexB].GetComponent<Delegate>().index = temp3;
    }

    public void Sort(DelegateCompare shouldSwap) {
        bool swapped;
        int iterationCount = 0;
        do {
            iterationCount++;
            swapped = false;
            for (int i = 0; i < listModel.Count - 1; ++i) {
                if (shouldSwap(listModel[i], listModel[i + 1])) {
                    Swap(i, i + 1);
                    swapped = true;
                }
            }
        } while (swapped && iterationCount < listModel.Count);

        RemakeView();
    }

    public void Clear() {
        foreach (GameObject child in childList) {
            Destroy(child);
        }
        childList.Clear();
        listModel.Clear();

        RemakeView();
    }

    public void ResizeView() {
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, viewHeight);
        if (resizeView) {
            RectTransform rt = GetComponent<RectTransform>();
            if (viewHeight > maxViewHeight) {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, maxViewHeight);
            }
            else {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, viewHeight);
            }
        }
    }

    public void RemakeView() {
        viewHeight = 0;

        foreach (GameObject child in childList) {
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            float itemHeight = rectTransform.sizeDelta.y;
            child.transform.localPosition = new Vector3(0, -viewHeight, transform.localPosition.z);

            if (listModel.Count > 0) {
                viewHeight += spacing;
            }

            viewHeight += itemHeight;
        }

        ResizeView();
    }

    public int Count() {
        return listModel.Count;
    }
}
