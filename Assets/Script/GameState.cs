using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using JSONObject = System.Collections.Generic.Dictionary<string, object>;

public class GameState : MonoBehaviour {

    JSONObject saveFiles = new JSONObject();

    public bool mute;

    public List<int> completedQuests = new List<int>();
    public List<int> unlockedQuests = new List<int>();
    public List<string> unlockedCharacters = new List<string>();
    public List<string> completedBuildings = new List<string>();
    public List<string> queuedEvents = new List<string>();
    public List<string> completedEvents = new List<string>();
    public Dictionary<string, int> resources = new Dictionary<string, int>();

    string saveName;
    string saveFilePath;
    public static GameState instance { get; private set; }

    void Awake() {
        if (instance != null) {
            Destroy(this.gameObject);
        }
        else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

	void Start () {
        LoadDefault("");

        string saveFileFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/fisiogame";
        try {
            Directory.CreateDirectory(saveFileFolder);
        }
        catch (System.Exception e) {
            Debug.Log("Failed to create directory: " + e.GetType());
        }

        saveFilePath = saveFileFolder + "/save";

        string serialized = "";
        try {
            serialized = File.ReadAllText(saveFilePath);
            saveFiles = MiniJSON.Json.Deserialize(serialized) as JSONObject;
        }
        catch (FileNotFoundException e) {
            saveFiles = new JSONObject();
        }
        catch (System.Exception e) {
            Debug.Log("Failed to open file " + e.GetType());
        }
        if (serialized.Length == 0) {
            saveFiles = new JSONObject();
        }
	}

    public void Save() {
        if (saveName.Length == 0) return;
        JSONObject saveFile = new JSONObject();
        saveFile.Add("mute",               mute);
        saveFile.Add("completedQuests",    completedQuests);
        saveFile.Add("unlockedQuests",     unlockedQuests);
        saveFile.Add("unlockedCharacters", unlockedCharacters);
        saveFile.Add("completedBuildings", completedBuildings);
        saveFile.Add("queuedEvents",       queuedEvents);
        saveFile.Add("completedEvents",    completedEvents);
        saveFile.Add("resources",          resources);

        saveFiles[saveName] = saveFile;

        File.WriteAllText(saveFilePath, MiniJSON.Json.Serialize(saveFiles));
    }

    public bool LoadDefault(string saveName) {
        if (saveFiles.ContainsKey(saveName))
            return false;

        this.saveName = saveName;
        mute = false;
        AudioListener.volume = 1.0f;
        completedQuests.Clear();
        unlockedQuests.Clear();
        unlockedCharacters.Clear();
        completedBuildings.Clear();
        queuedEvents.Clear();
        completedEvents.Clear();
        resources.Clear();
        resources.Add("Wood",   0);
        resources.Add("Stone",  0);
        resources.Add("Metal",  0);
        resources.Add("Wool",   0);
        resources.Add("Food",   0);
        resources.Add("Gold",   0);
        resources.Add("Paper",  0);
        resources.Add("Fabric", 0);
        resources.Add("Brick",  0);
        resources.Add("Marble", 0);
        resources.Add("Coal",   0);

        return true;
    }

    public bool Load(string saveName) {
        if (!saveFiles.ContainsKey(saveName)) {
            return false;
        }
       
        JSONObject saveFile = saveFiles[saveName] as JSONObject;

        this.saveName = saveName;

        // MiniJSON makes me sad.
        mute               = (bool)saveFile["mute"];
        completedQuests    = ExtractInts   (saveFile["completedQuests"] as List<object>);
        unlockedQuests     = ExtractInts   (saveFile["unlockedQuests"] as List<object>);
        unlockedCharacters = ExtractStrings(saveFile["unlockedCharacters"] as List<object>);
        completedBuildings = ExtractStrings(saveFile["completedBuildings"] as List<object>);
        queuedEvents       = ExtractStrings(saveFile["queuedEvents"] as List<object>);
        completedEvents    = ExtractStrings(saveFile["completedEvents"] as List<object>);

        resources = new Dictionary<string, int>();
        var tempResources = saveFile["resources"] as Dictionary <string, object>;

        foreach (var pair in tempResources) {
            resources.Add(pair.Key, int.Parse(pair.Value.ToString()));
        }

        if (mute) {
            AudioListener.volume = 0;
        }

        return true;
    }

    public void UnlockQuest(int number) {
        unlockedQuests.Add(number);
    }

    public void CompleteQuest(int number) {
        completedQuests.Add(number);
        unlockedQuests.Remove(number);
    }

    public void QueueEvent(string name) {
        queuedEvents.Add(name);
    }

    public void CompleteEvent(string name) {
        queuedEvents.Remove(name);
        completedEvents.Add(name);
    }

    public void UnlockCharacter(string name) {
        unlockedCharacters.Add(name);
    }

    public void UnlockBuilding(string name) {
        completedBuildings.Add(name);
    }

    List<int> ExtractInts(List<object> list) {
        List<int> newList = new List<int>();
        foreach (var obj in list) {
            newList.Add(int.Parse(obj.ToString()));
        }
        return newList;
    }

    List<string> ExtractStrings(List<object> list) {
        List<string> newList = new List<string>();
        foreach (var obj in list) {
            newList.Add(obj.ToString());
        }
        return newList;
    }
}

