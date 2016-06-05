using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour {

    [SerializeField]
	ListView questListView;
	[SerializeField]
	ListView resourcesListViewA;
	[SerializeField]
	ListView resourcesListViewB;
    [SerializeField]
    GameObject plazaContainer;
    [SerializeField]
    GameObject cityMap;
    [SerializeField]
    GameObject cityHUD;
    [SerializeField]
    QuestPopup questPopup;
    [SerializeField]
    MinigameLoader minigameLoader;
    [SerializeField]
    GameObject woodcuttingButton;
    [SerializeField]
    GameObject miningButton;
    [SerializeField]
    GameObject smithingButton;
    
    public static SceneController instance { get; private set; }

    void Awake() {
        instance = this;
    }

    void Start() {
        DialogueController.instance.DialogueOverEvent += OnDialogueEventOver;
        RefreshBuildings();
        RefreshMinigames();
        RefreshQuests();
        RefreshResources();

        if (GameState.instance.completedEvents.Contains("VillageArrival")) {
            CheckQuests();
            GoToPlaza();
            SaveGame();
            DialogueController.instance.Play("WelcomeBack");
        }
        else {
            GoToMap();
            cityHUD.SetActive(false);
            StartCoroutine("StartVillageArrival");
        }

	}
	
	// Update is called once per frame
	void Update () {

    }

    void OnDestroy() {
        instance = null;
    }

    public void GoToMap() {
        this.GetComponent<CameraController>().ResetZoom();
        plazaContainer.SetActive(false);
        cityHUD.SetActive(true);
        cityMap.SetActive(true);
    }
    public void GoToPlaza() {
        plazaContainer.SetActive(true);
        cityHUD.SetActive(false);
        cityMap.SetActive(false);
    }

    public void AttemptToStart(string minigame) {
        minigameLoader.AttemptConnection(minigame);
    }

    IEnumerator StartVillageArrival() {
        yield return new WaitForSeconds(2);
        DialogueController.instance.DialogueOverEvent += OnVillageArrivalDone;
        DialogueController.instance.Play("VillageArrival");
    }
    // Handled separately because it changes view and fires another event
    void OnVillageArrivalDone(GameData.EventData data) {
        StartCoroutine("StartTutorial");
    }

    IEnumerator StartTutorial() {
        GoToPlaza();
        yield return new WaitForSeconds(2);
        DialogueController.instance.DialogueOverEvent -= OnVillageArrivalDone;
        DialogueController.instance.Play("Tutorial");
    }

    IEnumerator StartQueuedEvent() {
        yield return new WaitForSeconds(1);
        if (GameState.instance.queuedEvents.Count <= 0)
            yield break;
        DialogueController.instance.Play(GameState.instance.queuedEvents[0]);
    }

    void OnDialogueEventOver(GameData.EventData data) {
        GameState.instance.CompleteEvent(data.name);
        if (data.charactersUnlocked.Count > 0) {
            foreach (var character in data.charactersUnlocked) {
                GameState.instance.UnlockCharacter(character);
                Debug.Log("Unlocked " + character);
            }
            RefreshMinigames();
        }
        if (data.questsUnlocked.Count > 0) {
            foreach (var quest in data.questsUnlocked) {
                GameState.instance.UnlockQuest(quest);
                Debug.Log("Unlocked Quest " + quest);
            }
            RefreshQuests();
        }
        SaveGame();
        StartCoroutine("StartQueuedEvent");
    }

    public void RefreshMinigames() {
        woodcuttingButton.SetActive(GameState.instance.unlockedCharacters.Contains("lumberjack"));
        miningButton.SetActive(GameState.instance.unlockedCharacters.Contains("stonemason"));
        smithingButton.SetActive(GameState.instance.unlockedCharacters.Contains("blacksmith"));
    }

    public void RefreshQuests() {
        questListView.Clear();
        foreach (var quest in GameState.instance.unlockedQuests) {
            questListView.Add(quest, questPopup);
        }
    }

    public void RefreshBuildings() {
        // TODO
    }

    public void RefreshResources() {
		int count = 0;

        foreach (var pair in GameState.instance.resources) {
			
			if (count <= GameState.instance.resources.Count / 2)
	            resourcesListViewA.Add(pair.Key, pair.Value);
			else
				resourcesListViewB.Add(pair.Key, pair.Value);
				
			count++;
        }
    }

    public void CompleteQuest(GameData.QuestData data, int questNumber) {
        questPopup.gameObject.SetActive(false);
        GameState.instance.UnlockBuilding(data.builds);
        GameState.instance.CompleteQuest(questNumber);
        RefreshQuests();
        if (data.questCompletedEvent.Length > 0) {
            GameState.instance.QueueEvent(data.questCompletedEvent);
        }
        StartCoroutine("StartQueuedEvent");
        SaveGame();
    }

    void CheckQuests() {
        foreach (var quest in GameData.instance.quests) {
            int questNumber = int.Parse(quest.Key);
            if (GameState.instance.completedQuests.Contains(questNumber)) // Quest is completed
				continue;
            if (GameState.instance.unlockedQuests.Contains(questNumber))  // Quest is already in progress
				continue;
			if (!GameState.instance.completedQuests.Contains(quest.Value.unlockedBy)) // Quest is missing requirements
				continue;
            GameState.instance.UnlockQuest(questNumber);
            string questEvent = quest.Value.questOpeningEvent;
            if (questEvent.Length > 0) 
                GameState.instance.QueueEvent(questEvent);
        }
    }

    public void SaveGame() {
        GameState.instance.Save();
        Debug.Log("Game saved.");
    }

    public void EndSession() {
        SaveGame();
        SceneManager.LoadScene("Initial");
    }


    #region Test Functions
    public void PopulateListView() {
        questListView.Add("Minha");
        questListView.Add("Própria");
        questListView.Add("ListView");
        questListView.Add("Tá");
        questListView.Add("Funfano");
        questListView.Add("Mano");
        questListView.Add("!!!!");
    }

    public void RemoveQuest(int index) {
        questListView.Remove(index);
    }

    public void RemoveQuest(ListView.DelegateTest isTarget) {
        questListView.Remove(isTarget);
    }

    public void SortView(ListView.DelegateCompare shouldSwap) {
        questListView.Sort(shouldSwap);
    }

    #endregion
}
