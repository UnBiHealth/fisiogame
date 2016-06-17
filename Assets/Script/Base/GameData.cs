using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

using JSONObject = System.Collections.Generic.Dictionary<string, object>;

public class GameData : MonoBehaviour {

    [HideInInspector]
    public SpriteIndex spriteIndex;

    public Dictionary<string, BuildingData> buildings = new Dictionary<string, BuildingData>();
    public Dictionary<string, QuestData> quests = new Dictionary<string, QuestData>();
    public Dictionary<string, EventData> events = new Dictionary<string, EventData>();
    public Dictionary<string, CharacterData> characters = new Dictionary<string, CharacterData>();
    public Dictionary<string, MinigameData> minigames = new Dictionary<string, MinigameData>();
    public Dictionary<string, ResourceData> resources = new Dictionary<string, ResourceData>();

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

        spriteIndex = GetComponent<SpriteIndex>();

        Dictionary<string, object> temp;

        TextAsset buildingFile = Resources.Load("Buildings") as TextAsset;
        string buildingJSON = System.Text.Encoding.UTF8.GetString(buildingFile.bytes);
        temp = MiniJSON.Json.Deserialize(buildingJSON) as JSONObject;
        foreach (var building in temp) {
            JSONObject obj = temp[building.Key] as JSONObject;
            buildings.Add(building.Key, new BuildingData(GetString(obj, "name"), GetString(obj, "yield"), GetDouble(obj, "yieldAmount"),
                                                         GetDouble(obj, "ratio"), GetString(obj, "fromResource"), GetString(obj, "toResource"),
                                                         GetDouble(obj, "multiplier"), GetString(obj, "multipliedYield")));
        }

        TextAsset questFile = Resources.Load("Quests") as TextAsset;
        string questsJSON = System.Text.Encoding.UTF8.GetString(questFile.bytes);
        temp = MiniJSON.Json.Deserialize(questsJSON) as JSONObject;
        foreach (var quest in temp) {
            JSONObject obj = temp[quest.Key] as JSONObject;
            quests.Add(quest.Key, new QuestData(obj["name"] as string, obj["description"] as string, obj["requirements"] as Dictionary<string, object>,
                                                obj["builds"] as string, (int)((long)obj["unlockedBy"]), (int)((long)obj["minimumSession"]),
                                                obj["questOpeningEvent"] as string, obj["questCompletedEvent"] as string, obj["questGiver"] as string));
        }

        TextAsset eventFile = Resources.Load("Events") as TextAsset;
        string eventJSON = System.Text.Encoding.UTF8.GetString(eventFile.bytes);
        temp = MiniJSON.Json.Deserialize(eventJSON) as JSONObject;
        foreach (var evt in temp) {
            JSONObject obj = temp[evt.Key] as JSONObject;
            events.Add(evt.Key, new EventData(evt.Key,
                                              (obj.ContainsKey("questsUnlocked") ? obj["questsUnlocked"] as List<object> : null),
                                              (obj.ContainsKey("charactersUnlocked") ? obj["charactersUnlocked"] as List<object> : null),
                                               obj["script"] as List<object>));
        }

        TextAsset minigameFile = Resources.Load("Minigames") as TextAsset;
        string minigameJSON = System.Text.Encoding.UTF8.GetString(minigameFile.bytes);
        temp = MiniJSON.Json.Deserialize(minigameJSON) as JSONObject;
        foreach (var minigame in temp) {
            JSONObject obj = temp[minigame.Key] as JSONObject;
            minigames.Add(minigame.Key, new MinigameData((int)(long)obj["highYield"], (int)(long)obj["midYield"], (int)(long)obj["lowYield"], obj["bonuses"] as JSONObject));
        }

        characters.Add("lumberjack", new CharacterData("Araújo, o Lenhador", spriteIndex.lumberjackSprite, spriteIndex.lumberjackMiniSprite));
        characters.Add("farmer", new CharacterData("Jorge, o Fazendeiro", spriteIndex.farmerSprite, spriteIndex.farmerMiniSprite));
        characters.Add("mayor", new CharacterData("Efigênio, o Prefeito", spriteIndex.mayorSprite, spriteIndex.mayorMiniSprite));
        characters.Add("stonemason", new CharacterData("Pietro, o Pedreiro", spriteIndex.stonemasonSprite, spriteIndex.stonemasonMiniSprite));
        characters.Add("salesman", new CharacterData("Aurélio, o Comerciante", spriteIndex.salesmanSprite, spriteIndex.salesmanMiniSprite));
        characters.Add("blacksmith", new CharacterData("Ademir, o Ferreiro", spriteIndex.blacksmithSprite, spriteIndex.blacksmithMiniSprite));
        characters.Add("scholar", new CharacterData("Lúcio, o Estudioso", spriteIndex.scholarSprite, spriteIndex.scholarMiniSprite));
        characters.Add("artist", new CharacterData("Cândido, o Artista", spriteIndex.artistSprite, spriteIndex.artistMiniSprite));

        resources.Add("Wood"  , new ResourceData("Madeira", spriteIndex.woodIcon, spriteIndex.woodIconHighlight));
        resources.Add("Stone", new ResourceData("Pedra", spriteIndex.stoneIcon, spriteIndex.stoneIconHighlight));
        resources.Add("Metal", new ResourceData("Metal", spriteIndex.metalIcon, spriteIndex.metalIconHighlight));
        resources.Add("Wool", new ResourceData("Lã", spriteIndex.woolIcon, spriteIndex.woolIconHighlight));
        resources.Add("Food", new ResourceData("Comida", spriteIndex.foodIcon, spriteIndex.foodIconHighlight));
        resources.Add("Gold", new ResourceData("Ouro", spriteIndex.goldIcon, spriteIndex.goldIconHighlight));
        resources.Add("Paper", new ResourceData("Papel", spriteIndex.paperIcon, spriteIndex.paperIconHighlight));
        resources.Add("Fabric", new ResourceData("Tecido", spriteIndex.fabricIcon, spriteIndex.fabricIconHighlight));
        resources.Add("Brick", new ResourceData("Tijolo", spriteIndex.brickIcon, spriteIndex.brickIconHighlight));
        resources.Add("Marble", new ResourceData("Mármore", spriteIndex.marbleIcon, spriteIndex.marbleIconHighlight));
        resources.Add("Coal", new ResourceData("Carvão", spriteIndex.coalIcon, spriteIndex.coalIconHighlight));
	}

    // C# GENERICS ARE BAD AND MICROSOFT SHOULD BE ASHAMED
    long GetLong(JSONObject obj, string key) {
        try {
            long value = (long)obj[key];
            return value;
        }
        catch (System.Exception e) {
            long value = 0;
            return value;
        }
    }

    double GetDouble(JSONObject obj, string key) {
        try {
            double value = (double)obj[key];
            return value;
        }
        catch (System.Exception e) {
            double value = 0;
            return value;
        }
    }

    string GetString(JSONObject obj, string key) {
        try {
            string value = obj[key] as string;
            return value;
        }
        catch (System.Exception e) {
            string value = "";
            return value;
        }
    }

    #endregion

    #region Buildings
    public class BuildingData {
        public BuildingData(string name, string yield, double yieldAmount,
                            double exchangeRatio, string fromResource, string toResource,
                            double multiplier, string multipliedYield) {
            this.name = name;
            this.yield = yield;
            this.yieldAmount = (float)yieldAmount;
            this.triggersYield = yield.Length > 0;
            this.exchangeRatio = (float)exchangeRatio;
            this.fromResource = fromResource;
            this.toResource = toResource;
            this.triggersExchange = exchangeRatio > 0;
            this.multiplier = (float)multiplier;
            this.multipliedYield = multipliedYield;
            this.triggersMultiplier = multiplier > 0;
        }
        public string name { get; private set; }
        public string yield { get; private set; }
        public float yieldAmount { get; private set; }
        public string fromResource { get; private set; }
        public string toResource { get; private set; }
        public float exchangeRatio { get; private set; }
        public float multiplier { get; private set; }
        public string multipliedYield { get; private set; }
        public bool triggersYield { get; private set; }
        public bool triggersExchange { get; private set; }
        public bool triggersMultiplier { get; private set; }

        public override string ToString() {
            return name + " - " + yield + " " + yieldAmount + " - " + fromResource + " " + toResource + " " + exchangeRatio + " - " + multipliedYield + " " + multiplier;
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
                         string builds, int unlockedBy, int minimumSession, string questOpeningEvent,
                         string questCompletedEvent, string questGiver) {
            this.name = name;
            this.description = description;
            this.requirements = new Dictionary<string, int>();
            this.builds = builds;
            this.unlockedBy = unlockedBy;
            this.minimumSession = minimumSession;
            this.questOpeningEvent = questOpeningEvent;
            this.questCompletedEvent = questCompletedEvent;
            this.questGiver = questGiver;

            foreach (var v in requirements) {
                this.requirements.Add(v.Key, (int)(long)v.Value);
            }
        }
        public string name { get; private set; }
        public string description { get; private set; }
        public Dictionary<string, int> requirements { get; private set; }
        public string builds { get; private set; }
        public int unlockedBy { get; private set; }
        public int minimumSession { get; private set; }
        public string questOpeningEvent { get; private set; }
        public string questCompletedEvent { get; private set; }
        public string questGiver { get; private set; }

        public override string ToString() {
            string s = name + " - " + description + "\n";
            foreach (var v in requirements) {
                s += "Quest " + v.Key + " - " + v.Value + "\n";
            }
            s += "Builds " + builds + '\n';
            s += "Unlocked by Quest " + unlockedBy + " @session " + minimumSession + "\n";
            s += "Plays" + (questOpeningEvent != null ? questOpeningEvent : "<none>") + " and " + (questCompletedEvent != null ? questCompletedEvent : "<none>");
            return s;
        }
    }


    public QuestData GetQuestData(int questNumber) {
        string key = questNumber.ToString();
        if (quests.ContainsKey(key)) {
            return quests[key];
        }
        else return null;
    }
    #endregion

    #region Events
    public class EventData {
        public EventData(string name, List<object> questsUnlocked, List<object> charactersUnlocked, List<object> script) {
            this.name = name;
            this.questsUnlocked = new List<int>();
            this.charactersUnlocked = new List<string>();
            this.script = new List<string>();

            if (questsUnlocked != null) foreach (var v in questsUnlocked) {
                this.questsUnlocked.Add((int)(long) v);
            }

            if (charactersUnlocked != null) foreach (var v in charactersUnlocked) {
                this.charactersUnlocked.Add(v as string);
            }

            foreach (var v in script) {
                this.script.Add(v as string);
            }
        }
        public string name { get; private set; }
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
        public CharacterData(string name, Sprite defaultSprite, Sprite miniSprite) {
            this.name = name;
            this.defaultSprite = defaultSprite;
            this.miniSprite = miniSprite;
        }
        public string name;
        public Sprite defaultSprite;
        public Sprite miniSprite;
    }

    public CharacterData GetCharacterData(string key) {
        if (characters.ContainsKey(key)) {
            return characters[key];
        }
        else return null;
    }
    #endregion

    #region Minigames
    public class MinigameData {
        public MinigameData(int highYield, int midYield, int lowYield, JSONObject bonuses) {
            this.highYield = highYield;
            this.midYield = midYield;
            this.lowYield = lowYield;
            this.bonuses = new Dictionary<string, Bonus>();
            foreach (var pair in bonuses) {
                JSONObject data = pair.Value as JSONObject;
                this.bonuses.Add(pair.Key, new Bonus((float)(double)data["odds"], int.Parse(data["yield"].ToString())));
            }
        }
        public int highYield;
        public int midYield;
        public int lowYield;
        public Dictionary<string, Bonus> bonuses;

        public class Bonus {
            public Bonus(float odds, int yield) {
                this.odds = odds;
                this.yield = yield;
            }
            public float odds;
            public int yield;
        }
    }

    public MinigameData GetMinigameData(string resource) {
        if (minigames.ContainsKey(resource)) {
            return minigames[resource];
        }
        else {
            return null;
        }
    }
    #endregion

    #region Resources
    public class ResourceData {
        public ResourceData(string name, Sprite icon, Sprite highlightedIcon) {
            this.name = name;
            this.icon = icon;
            this.highlightedIcon = highlightedIcon;
        }
        public string name;
        public Sprite icon;
        public Sprite highlightedIcon;
    }

    public ResourceData GetResourceData(string key) {
        if (resources.ContainsKey(key)) {
            return resources[key];
        }
        else return null;
    }
    #endregion
}

