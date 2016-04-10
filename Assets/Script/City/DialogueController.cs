using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueController : MonoBehaviour {

    [HideInInspector]
    public bool isRunning { get; private set; }

    [SerializeField]
    Image leftSpeaker;
    [SerializeField]
    Image rightSpeaker;
    [SerializeField]
    Text speakerName;
    [SerializeField]
    Text dialogue;
    [SerializeField]
    Animator arrowAnimator;

    float typewriterSpeed;

    Animator animator;
    bool isReady;
    bool isTypewriting;

    GameData.EventData currentEvent;
    int lineIndex;
    string currentText;
    float typewriterPosition;

    public void Play(string eventName) {
        currentEvent = GameData.instance.GetEventData(eventName);
        if (currentEvent != null) {
            isRunning = true;
            lineIndex = 0;
            typewriterSpeed = 0.5f;
            Show();
        }
        else {
            Debug.Log("DialogueController Error - Event " + eventName + " does not exist");
        }
    }

    void Awake() {
        isRunning = false;
        isReady = false;
        isTypewriting = false;
        animator = GetComponent<Animator>();
        typewriterPosition = 0;
    }

	void Start () {
        
	}
	
	void Update () {
        if (isRunning && isReady) {
            if (!isTypewriting) {
                HideArrow();
                ParseLine();
            }
            else if (Input.GetMouseButtonDown(0)) {
                if (typewriterPosition >= currentText.Length) {
                    isTypewriting = false; // will trigger parsing next line
                }
                else {
                    UpdateText((float)currentText.Length);
                }
            }
            else {
                UpdateText(typewriterSpeed);
            }
        }
	}

    void UpdateText(float increment) {
        typewriterPosition += increment;
        if ((int)typewriterPosition >= currentText.Length) {
            dialogue.text = currentText;
            ShowArrow();
        }
        else {
            dialogue.text = currentText.Substring(0, (int)typewriterPosition);
        }
    }

    void ParseLine() {
        if (lineIndex >= currentEvent.script.Count) {
            Hide();
            return;
        }

        currentText = currentEvent.script[lineIndex];

        if (currentText.StartsWith("%")) {
            char[] delims = {'%', ','};
            string[] statements = currentText.ToLower().Split(delims);

            foreach (var statement in statements) {
                if (statement.Contains("=")) {
                    ParseAssignment(statement);
                }
                else if (statement.Replace(" ", "").Length == 0) {
                    continue;
                }
                else {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid statement");
                    continue;
                }
            }
        }
        else {
            isTypewriting = true;
            typewriterPosition = 0;
        }
        ++lineIndex;
    }
    void ParseAssignment(string statement) {
        char[] delims = { ' ', '=' };
        string[] arguments = statement.Split(delims);
        if (arguments.Length != 2) {
            Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Too many arguments");
            return;
        }
        switch (arguments[0]) {
            case "leftspeaker":
                GameData.CharacterData leftCharacter = GameData.instance.GetCharacterData(arguments[1]);
                if (leftCharacter == null) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid speaker");
                    return;
                }
                leftSpeaker.sprite = leftCharacter.defaultSprite;
                speakerName.text = leftCharacter.name;
                leftSpeaker.SetNativeSize();
                leftSpeaker.GetComponent<RectTransform>().sizeDelta *= 2.0f;
                break;
            case "rightspeaker":
                GameData.CharacterData rightCharacter = GameData.instance.GetCharacterData(arguments[1]);
                if (rightCharacter == null) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid speaker");
                    return;
                }
                rightSpeaker.sprite = rightCharacter.defaultSprite;
                speakerName.text = rightCharacter.name;
                rightSpeaker.SetNativeSize();
                rightSpeaker.GetComponent<RectTransform>().sizeDelta *= 2.0f;
                break;
            case "speed":
                try {
                    float newSpeed = float.Parse(arguments[1], System.Globalization.CultureInfo.InvariantCulture);
                    if (newSpeed <= 0.0f) {
                        Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Speed must be higher than zero");
                        return;
                    }
                    typewriterSpeed = newSpeed;
                }
                catch (System.Exception e) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid speed");
                }
                break;
            default:
                Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid assignee");
                break;
        }
    }

    void Show() {
        isReady = false;
        animator.SetBool("running", true);
    }

    void Hide() {
        isRunning = false;
        isReady = false;
        animator.SetBool("running", false);
    }

    void ShowArrow() {
        arrowAnimator.SetBool("running", true);
    }

    void HideArrow() {
        arrowAnimator.SetBool("running", true);
    }

    public void AnimatorReady() {
        isReady = true;
    }
}
