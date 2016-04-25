using UnityEngine;
using System.Collections;

public class ExerciseEndPopup : MonoBehaviour {

    [SerializeField]
    WoodcuttingControl woodcuttingControl;

    public void EndExercise() {
        woodcuttingControl.OnExerciseEnd(null);
    }
}
