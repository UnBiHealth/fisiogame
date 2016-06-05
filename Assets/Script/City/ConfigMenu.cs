using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ConfigMenu : MonoBehaviour {

    [SerializeField]
    Text muteButtonText;

    public void ToggleSound() {
        // Never trust floating point numbers.
        if (AudioListener.volume <= 0.1f) {
            AudioListener.volume = 1.0f;
            muteButtonText.text = "Desligar Som";
        }
        else {
            AudioListener.volume = 0.0f;
            muteButtonText.text = "Ligar Som";
        }
    }

    public void EndSession() {
        // TODO
        SceneManager.LoadScene("Initial");
    }
}
