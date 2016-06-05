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

        foreach (string key in rewards.Keys) {
            if (rewards[key] > 0) {
                GameState.instance.resources[key] += rewards[key];
                Debug.Log("Add " + key + ", " + rewards[key]);
                resourcesListView.Add(key, rewards[key]);
            }
        }
    }

    public void EndMinigame() {
        SceneManager.LoadScene("City");
    }
}
