using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuestPopup : MonoBehaviour {

    [SerializeField]
    Text title;
    [SerializeField]
    Text questGiverName;
    [SerializeField]
    Text flavorText;
    [SerializeField]
    Image questGiverSprite;
    [SerializeField]
    ListView requirementsListView;

    int questNumber;
    GameData.QuestData questData;

    public void SetUp(GameData.QuestData data, int questNumber) {
        GameData.BuildingData building = GameData.instance.GetBuildingData(data.builds);
        GameData.CharacterData character = GameData.instance.GetCharacterData(data.questGiver);
        this.questNumber = questNumber;
        questData = data;
        title.text = building.name;
        questGiverName.text = character.name;
        questGiverSprite.sprite = character.defaultSprite;
        questGiverSprite.SetNativeSize();
        flavorText.text = data.description;

        foreach (var pair in data.requirements) {
            requirementsListView.Add(pair.Key, pair.Value, GameState.instance.resources[pair.Key]);
        }
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
