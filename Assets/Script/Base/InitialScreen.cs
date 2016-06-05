using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class InitialScreen : MonoBehaviour {

    [SerializeField]
    GameObject inputOverlay;
    [SerializeField]
    InputField inputField;
    [SerializeField]
    GameObject inputPopup;
    [SerializeField]
    Text inputText;
    [SerializeField]
    GameObject errorPopup;
    [SerializeField]
    Text errorText;
    [SerializeField]
    BackgroundScroller backgroundScroller;

    bool newGame;

	void Start () {
        backgroundScroller.Scroll();
	}

    public void OnNewGame() {
        inputOverlay.SetActive(true);
        inputPopup.SetActive(true);
        errorPopup.SetActive(false);
        inputField.ActivateInputField();
        inputField.Select();
        inputText.text = "Digite um nome de usuário. Anote para não esquecer!";
        newGame = true;
    }

    public void OnContinue() {
        inputOverlay.SetActive(true);
        inputPopup.SetActive(true);
        errorPopup.SetActive(false);
        inputField.ActivateInputField();
        inputField.Select();
        inputText.text = "Digite o seu nome de usuário.";
        newGame = false;
    }

    public void OnInput() {

        if (inputField.text.Length == 0)
            return;

        if (newGame) {
            if (!GameState.instance.LoadDefault(inputField.text)) {
                inputPopup.SetActive(false);
                errorPopup.SetActive(true);
                errorText.text = "Este usuário já existe.";
                inputField.text = "";
            }
            else {
                SceneManager.LoadScene("City");
            }
        }
        else {
            if (!GameState.instance.Load(inputField.text)) {
                inputPopup.SetActive(false);
                errorPopup.SetActive(true);
                errorText.text = "Usuário não encontrado";
                inputField.text = "";
            }
            else {
                SceneManager.LoadScene("City");
            }
        }
    }

    public void OnErrorAccepted() {
        errorPopup.SetActive(false);
        inputOverlay.SetActive(false);
    }

	public void OnCancel() {
		inputOverlay.SetActive(false);
		inputPopup.SetActive(false);
		errorPopup.SetActive(false);
		inputField.text = "";
	}

    public void OnExit() {
        Application.Quit();
    }
}
