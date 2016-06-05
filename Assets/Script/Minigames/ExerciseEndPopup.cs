using UnityEngine;
using System.Collections;

public class ExerciseEndPopup : MonoBehaviour {

    [SerializeField]
    MinigameControllerA minigameControl;

    public void EndExercise() {
        minigameControl.OnExerciseEnd(null);
    }
}
