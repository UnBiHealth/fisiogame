using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuestDelegate : ListView.Delegate {

    QuestPopup questPopup;

    int questNumber;
    GameData.QuestData questData;

    public override void Set(params object[] args) {
        questNumber = (int)args[0];
        questData = GameData.instance.GetQuestData(questNumber);
        questPopup = args[1] as QuestPopup;
        string buildingName = GameData.instance.GetBuildingData(questData.builds).name;
        Text txt = GetComponentInChildren<Text>();
        if (buildingName.Contains(" "))
            txt.text = buildingName.Split(' ')[0];
        else
            txt.text = buildingName;
    }

    public void OpenQuestPopup() {
        questPopup.SetUp(questData, questNumber);
    }
}
