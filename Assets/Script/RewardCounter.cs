using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class RewardCounter : MonoBehaviour {

    [SerializeField]
    Text title;
    [SerializeField]
    Text text1;
    [SerializeField]
    ResourceDelegate mainResource;
    [SerializeField]
    Text text2;
    [SerializeField]
    ListView resourcesListView;
    [SerializeField]
    Button continueButton;

    GameData.MinigameData minigame;
    string resource;
    Dictionary<string, int> rewards = new Dictionary<string, int>();

	public void SetUp (string resource, string character) {
        this.resource = resource;
        minigame = GameData.instance.GetMinigameData(resource);
        rewards.Add(resource, 0);
        foreach (string key in minigame.bonuses.Keys) {
            rewards.Add(key, 0);
        }
        text1.text = text1.text.Replace("$character", character);
        text2.text = text2.text.Replace("$character", character);
	}

    public void AddHighHit() {
        rewards[resource] += minigame.highYield;
    }

    public void AddMidHit() {
        rewards[resource] += minigame.midYield;
    }

    public void AddLowHit() {
        rewards[resource] += minigame.lowYield;
    }

    public void AddRandomBonus() {
        float rand = Random.Range(0.0f, 1.0f);
        foreach (var pair in minigame.bonuses) {
            rand -= pair.Value.odds;
            if (rand < 0) {
                rewards[pair.Key] += pair.Value.yield;
                break;
            }
        }
    }

    public void Finish() {
        this.gameObject.SetActive(true);

        int nonZeroResources = 0;
        GameObject[] array;

        foreach (string key in rewards.Keys) {
            if (rewards[key] > 0) {
                GameState.instance.resources[key] += rewards[key];
                nonZeroResources++;
            }  
        }

        if (nonZeroResources <= 1) {
            array = new GameObject[] { title.gameObject, text1.gameObject, mainResource.gameObject, continueButton.gameObject };
            text2.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            resourcesListView.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        }
        else {
            array = new GameObject[] { title.gameObject, text1.gameObject, mainResource.gameObject, text2.gameObject, resourcesListView.gameObject, continueButton.gameObject };
            foreach (string key in rewards.Keys) {
                if (key != resource) {
                    Debug.Log("Add " + key + ", " + rewards[key]);
                    resourcesListView.Add(key, rewards[key]);
                }
            }
        }

        mainResource.Set(resource, rewards[resource]);

        GameObject popupBody = title.transform.parent.gameObject;
        float popupHeight = 0;
        foreach (GameObject child in array) {
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            float itemHeight = rectTransform.sizeDelta.y;
            child.transform.localPosition = new Vector3(0, -popupHeight, transform.localPosition.z);

            if (popupHeight > 0) {
                popupHeight += 10 + itemHeight;
            }
            else {
                popupHeight += itemHeight;
            }
        }
        
        RectTransform t = popupBody.GetComponent<RectTransform>();
        t.sizeDelta = new Vector2(t.sizeDelta.x, popupHeight + 30);

        foreach (GameObject child in array) {
            child.transform.localPosition -= new Vector3(0, -popupHeight/2, 0);
        }

    }

    public void EndMinigame() {
        SceneManager.LoadScene("City");
    }
}
