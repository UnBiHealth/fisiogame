using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonDelegate : ListView.Delegate {
    
    SceneController sceneController;

    [SerializeField]
    private string name;

    public void Start() {
        sceneController = SceneController.instance;
    }

    public override void Set(params object[] parameters) {
        name = parameters[0] as string;
        GetComponentInChildren<Text>().text = name;
    }

    public void OnClick() {
        //sceneController.RemoveQuest(index);

        ListView.DelegateTest func = (object[] obj) => {
            string name = obj[0] as string;
            return name.Length % 2 == 1;
        };

        sceneController.RemoveQuest(func);

        //ListView.DelegateCompare func = (object[] dataA, object[] dataB) => {
        //    string nameA = dataA[0] as string;
        //    string nameB = dataB[0] as string;
        //    return (string.Compare(nameA, nameB) > 0);
        //};

        //sceneController.SortView(func);
    }
}
