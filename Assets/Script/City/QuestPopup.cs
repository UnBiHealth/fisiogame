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
	Button buildButton;
    [SerializeField]
    ListView requirementsListView;

    int questNumber;
    GameData.QuestData questData;

	public void SetUp(GameData.QuestData data, int questNumber) {
		gameObject.SetActive(true);

        GameData.BuildingData building = GameData.instance.GetBuildingData(data.builds);
        GameData.CharacterData character = GameData.instance.GetCharacterData(data.questGiver);
        this.questNumber = questNumber;
        questData = data;
        title.text = building.name;
        questGiverName.text = character.name;
        questGiverSprite.sprite = character.defaultSprite;
        questGiverSprite.SetNativeSize();
        flavorText.text = data.description;

		bool requirementsMet = true;
        foreach (var pair in data.requirements) {
			string resource = pair.Key;
			int required = pair.Value;
			int obtained = GameState.instance.resources[resource];
			requirementsListView.Add(resource, required, obtained);
			if (required > obtained) {
				requirementsMet = false;
			}
        }
		buildButton.interactable = requirementsMet;
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
