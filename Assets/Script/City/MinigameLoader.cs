using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MinigameLoader : MonoBehaviour {

    string minigame;
    int repetitions = -1;

    public void AttemptConnection(string minigame) {
        this.minigame = minigame;
        repetitions = -1;
        gameObject.SetActive(true);
        GameControl.instance.RegisterListener(OnExerciseReady, "repetitions");
    }

    void Update() {
        if (repetitions > 0) {
            //TODO: Set game state
            gameObject.SetActive(false);
            SceneManager.LoadScene(minigame);
        }
    }

    void OnEnable() {

    }

    void OnDisable() {
        GameControl.instance.UnregisterListener(OnExerciseReady, "repetitions");
    }

    public void OnExerciseReady(object newValue) {
        repetitions = int.Parse(newValue.ToString());
    }
}
