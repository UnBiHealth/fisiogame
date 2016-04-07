using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

using JSONObject = System.Collections.Generic.Dictionary<string, object>;

public class GameData : MonoBehaviour {
    
    [SerializeField]
    string lumberjackName = "Lenhador";

    [SerializeField]
    Sprite lumberjackDefaultSprite;

    Dictionary<string, BuildingData> buildings = new Dictionary<string, BuildingData>();
    Dictionary<string, QuestData> quests = new Dictionary<string, QuestData>();
    Dictionary<string, EventData> events = new Dictionary<string, EventData>();
    Dictionary<string, CharacterData> characters = new Dictionary<string, CharacterData>();

    public static GameData instance { get; private set; }

    #region Internal

    void Start() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this) {
            Destroy(this.gameObject);
            return;
        }

        Dictionary<string, object> temp;

        TextAsset buildingFile = Resources.Load("Buildings") as TextAsset;
        string buildingJSON = System.Text.Encoding.UTF8.GetString(buildingFile.bytes);
        temp = MiniJSON.Json.Deserialize(buildingJSON) as JSONObject;
        foreach (var building in temp) {
            JSONObject obj = temp[building.Key] as JSONObject;
            buildings.Add(building.Key, new BuildingData(obj["name"] as string, obj["yield"] as string, (long)obj["yieldAmount"]));
        }

        TextAsset questFile = Resources.Load("Quests") as TextAsset;
        string questsJSON = System.Text.Encoding.UTF8.GetString(questFile.bytes);
        temp = MiniJSON.Json.Deserialize(questsJSON) as JSONObject;
        foreach (var quest in temp) {
            JSONObject obj = temp[quest.Key] as JSONObject;
            quests.Add(quest.Key, new QuestData(obj["name"] as string, obj["description"] as string, obj["requirements"] as Dictionary<string, object>,
                                                obj["builds"] as string, (int)((long)(obj["buildPosition"] as JSONObject)["x"]),
                                                (int)((long)(obj["buildPosition"] as JSONObject)["y"]), (int)((long)obj["unlockedBy"]), (int)((long)obj["minimumSession"]),
                                                obj["questOpeningEvent"] as string, obj["questCompletedEvent"] as string));
        }

        TextAsset eventFile = Resources.Load("Events") as TextAsset;
        string eventJSON = System.Text.Encoding.UTF8.GetString(eventFile.bytes);
        temp = MiniJSON.Json.Deserialize(eventJSON) as JSONObject;
        foreach (var evt in temp) {
            JSONObject obj = temp[evt.Key] as JSONObject;
            events.Add(evt.Key, new EventData(obj["questsUnlocked"] as List<object>, obj["charactersUnlocked"] as List<object>, obj["script"] as List<object>));
        }

        characters.Add("Lumberjack", new CharacterData(lumberjackName, lumberjackDefaultSprite));
        /*
        foreach (var v in buildings) {
            Debug.Log(v.Value);
        }
        foreach (var v in quests) {
            Debug.Log(v.Value);
        }
        foreach (var v in events) {
            Debug.Log(v.Value);
        }
        */
	}
    #endregion

    #region Buildings
    public class BuildingData {
        public BuildingData(string name, string yield, long yieldAmount) {
            this.name = name;
            this.yield = yield;
            this.yieldAmount = (int)yieldAmount; 
        }
        public string name { get; private set; }
        public string yield { get; private set; }
        public int yieldAmount { get; private set; }

        public override string ToString() {
            return name + " - " + yield + " - " + yieldAmount;
        }
    }

    public BuildingData GetBuildingData(string key) {
        if (buildings.ContainsKey(key)) {
            return buildings[key];
        }
        else return null;
    }

    #endregion

    #region Quests
    public class QuestData {
        public QuestData(string name, string description, Dictionary<string, object> requirements,
                         string builds, int buildX, int buildY, int unlockedBy, int minimumSession,
                         string questOpeningEvent, string questCompletedEvent) {
            this.name = name;
            this.description = description;
            this.requirements = new Dictionary<string, int>();
            this.builds = builds;
            this.buildPosition = new Vector2(buildX, buildY);
            this.unlockedBy = unlockedBy;
            this.minimumSession = minimumSession;
            this.questOpeningEvent = questOpeningEvent;
            this.questCompletedEvent = questCompletedEvent;

            foreach (var v in requirements) {
                this.requirements.Add(v.Key, (int)(long)v.Value);
            }
        }
        public string name { get; private set; }
        public string description { get; private set; }
        public Dictionary<string, int> requirements { get; private set; }
        public string builds { get; private set; }
        public Vector2 buildPosition { get; private set; }
        public int unlockedBy { get; private set; }
        public int minimumSession { get; private set; }
        public string questOpeningEvent { get; private set; }
        public string questCompletedEvent { get; private set; }

        public override string ToString() {
            string s = name + " - " + description + "\n";
            foreach (var v in requirements) {
                s += v.Key + " - " + v.Value + "\n";
            }
            s += "Builds " + builds + " @" + buildPosition + "\n";
            s += "Unlocked by Quest " + unlockedBy + " @session " + minimumSession + "\n";
            s += "Plays" + (questOpeningEvent != null ? questOpeningEvent : "<none>") + " and " + (questCompletedEvent != null ? questCompletedEvent : "<none>");
            return s;
        }
    }

    public QuestData GetQuestData(string key) {
        if (quests.ContainsKey(key)) {
            return quests[key];
        }
        else return null;
    }

    public QuestData GetQuestData(int questNumber) {
        string key = "quest" + questNumber;
        if (quests.ContainsKey(key)) {
            return quests[key];
        }
        else return null;
    }
    #endregion

    #region Events
    public class EventData {
        public EventData(List<object> questsUnlocked, List<object> charactersUnlocked, List<object> script) {
            this.questsUnlocked = new List<int>();
            this.charactersUnlocked = new List<string>();
            this.script = new List<string>();

            if (questsUnlocked != null) foreach (var v in questsUnlocked) {
                this.questsUnlocked.Add((int) v);
            }

            if (charactersUnlocked != null) foreach (var v in charactersUnlocked) {
                this.charactersUnlocked.Add(v as string);
            }

            foreach (var v in script) {
                this.script.Add(v as string);
            }
        }
        public List<int> questsUnlocked { get; private set; }
        public List<string> charactersUnlocked { get; private set; }
        public List<string> script { get; private set; }

        public override string ToString() {
            string s = "Unlocks ";

            foreach (int i in questsUnlocked) {
                s += i + ",";
            }

            s += " - ";

            foreach (string character in charactersUnlocked) {
                s += character + ",";
            }
            s += "\n";
            foreach (string line in script) {
                s += "\n" + line;
            }
            return s;
        }
    }

    public EventData GetEventData(string key) {
        if (events.ContainsKey(key)) {
            return events[key];
        }
        else return null;
    }
    #endregion

    #region Characters
    public class CharacterData {
        public CharacterData(string name, Sprite defaultSprite) {
            this.name = name;
            this.defaultSprite = defaultSprite;
        }
        public string name;
        public Sprite defaultSprite;
    }

    public CharacterData GetCharacterData(string key) {
        if (characters.ContainsKey(key)) {
            return characters[key];
        }
        else return null;
    }
    #endregion
}

