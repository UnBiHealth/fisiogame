using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CharacterSpawner : MonoBehaviour {

    Image[] slots;
    List<int> availableSlots = new List<int>();
    List<string> used = new List<string>();

	// Use this for initialization
	void Start () {
  	}

    public void Spawn() {
        slots = GetComponentsInChildren<Image>();

        foreach (int i in Enumerable.Range(0, slots.Length)) {
            availableSlots.Add(i);
        }

        HashSet<string> characters = new HashSet<string>();

        foreach (string character in GameState.instance.unlockedCharacters) {
            characters.Add(character);
        }

        foreach (int quest in GameState.instance.unlockedQuests) {
            string questGiver = GameData.instance.GetQuestData(quest).questGiver;
            characters.Add(questGiver);
        }


        foreach (string character in characters) {
            int random = Random.Range(0, availableSlots.Count);
            int slot = availableSlots[random];
            slots[slot].sprite = GameData.instance.GetCharacterData(character).miniSprite;
            slots[slot].SetNativeSize();
            if (slots[slot].GetComponent<RectTransform>().anchoredPosition.x < 0) {
                slots[slot].transform.localScale = new Vector3(-1, 1, 0);
            }
            availableSlots.Remove(slot);
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
