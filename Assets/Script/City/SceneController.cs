using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour {

    [SerializeField]
    ListView questListView;
    [SerializeField]
    GameObject plazaContainer;
    [SerializeField]
    GameObject cityMap;
    [SerializeField]
    GameObject cityHUD;
    [SerializeField]
    DialogueController dialogueController;
    
    public static SceneController instance { get; private set; }


    void Start() {
        instance = this;
        GoToPlaza();
        dialogueController.Play("Demo");
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

    #region TestFunctions
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
