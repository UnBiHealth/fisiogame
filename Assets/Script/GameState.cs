using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using JSONObject = System.Collections.Generic.Dictionary<string, object>;

public class GameState : MonoBehaviour {

    JSONObject saveFiles = new JSONObject();

    public bool mute;

    public bool newSave { get; private set; }

    List<int> completedQuests         = new List<int>();
    List<int> unlockedQuests          = new List<int>();
    List<string> unlockedCharacters   = new List<string>();
    List<string> completedBuildings   = new List<string>();
    List<string> queuedEvents         = new List<string>();
    Dictionary<string, int> resources = new Dictionary<string, int>();

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
        newSave = true;
        completedQuests.Clear();
        unlockedQuests.Clear();
        unlockedCharacters.Clear();
        completedBuildings.Clear();
        queuedEvents.Clear();
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
        newSave = false;

        mute               = (bool)saveFile["mute"];
        completedQuests    = saveFile["completedQuests"]    as List<int>;
        unlockedQuests     = saveFile["unlockedQuests"]     as List<int>;
        unlockedCharacters = saveFile["unlockedCharacters"] as List<string>;
        completedBuildings = saveFile["completedBuildings"] as List<string>;
        queuedEvents       = saveFile["queuedEvents"]       as List<string>;
        resources          = saveFile["resources"]          as Dictionary<string, int>;

        if (mute) {
            AudioListener.volume = 0;
        }

        return true;
    }
}
