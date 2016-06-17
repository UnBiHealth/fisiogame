using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EventTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        DialogueController.instance.DialogueOverEvent += OnEventEnded;
	}
	
    public void OnEventSelected() {
        string eventName = GetComponent<InputField>().text;
        GetComponent<InputField>().text = "";
        DialogueController.instance.Play(eventName);
    }

    public void OnEventEnded(GameData.EventData data) {
        if (data.charactersUnlocked.Count > 0) {
            string output = "Characters Unlocked: ";
            foreach (string character in data.charactersUnlocked)
                output += character + ", ";
        }
        if (data.questsUnlocked.Count > 0) {
            string output = "Quests Unlocked: ";
            foreach (int quest in data.questsUnlocked) 
                output += quest + ", ";
        }
    }
}
