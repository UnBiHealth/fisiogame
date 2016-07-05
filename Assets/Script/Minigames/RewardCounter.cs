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
        ++GameState.instance.lastGameRepetitions;
    }

    public void AddMidHit() {
        rewards[resource] += minigame.midYield;
        ++GameState.instance.lastGameRepetitions;
    }

    public void AddLowHit() {
        rewards[resource] += minigame.lowYield;
        ++GameState.instance.lastGameRepetitions;
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
                int reward = (int)(rewards[key] * GameState.instance.activeMultipliers[key]);
                GameState.instance.resources[key] += reward;
                Debug.Log("Add " + key + ", " + reward);
                resourcesListView.Add(key, reward);
            }
        }
    }

    public void EndMinigame() {
        SceneManager.LoadScene("City");
    }
}
