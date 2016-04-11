using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

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

    char[] intervalChars = { '?', '!', '.' };
    char[] halfIntervalChars = { ',', ':', ';' };
    char[] soundlessChars = { ' ', ',', ':', ';' };
    Color normalColor = new Color(1f, 1f, 1f, 1f);
    Color shadowedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    int frameInterval;
    int frameCounter;
    int halfIntervalTime;
    int intervalTime;
    bool muted;
    bool useColoredHighlight;

    Animator animator;
    AudioSource audioSource;
    bool isReady;
    bool isTypewriting;

    GameData.EventData currentEvent;
    GameData.CharacterData leftCharacter;
    GameData.CharacterData rightCharacter;
    int lineIndex;
    string currentText;
    int typewriterPosition;

    public void Play(string eventName) {
        currentEvent = GameData.instance.GetEventData(eventName);
        if (currentEvent != null) {
            isRunning = true;
            lineIndex = 0;
            frameInterval = 1;
            halfIntervalTime = 30;
            intervalTime = 60;
            frameCounter = 1;
            muted = false;
            useColoredHighlight = false;
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
        audioSource = GetComponent<AudioSource>();
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
                    dialogue.text = "";
                }
                else {
                    UpdateText(true);
                }
            }
            else {
                UpdateText(false);
            }
        }
	}

    void UpdateText(bool skipToEnd) {
        if (skipToEnd || frameInterval == 0) {
            typewriterPosition = currentText.Length;
            dialogue.text = currentText;
            ShowArrow();
        }
        frameCounter--;
        if (frameCounter <= 0) {
            typewriterPosition += 1;
            if (typewriterPosition >= currentText.Length) {
                dialogue.text = currentText;
                ShowArrow();
            }
            else {
                dialogue.text = currentText.Substring(0, typewriterPosition);
                char currentChar = currentText[typewriterPosition - 1];
                char nextChar = (typewriterPosition + 1 >= currentText.Length ? '\0' : currentText[typewriterPosition]);
                if (intervalChars.Contains(currentChar) && !intervalChars.Contains(nextChar)) {
                    frameCounter = intervalTime;
                }
                else if (halfIntervalChars.Contains(currentChar) && !halfIntervalChars.Contains(nextChar)) {
                    frameCounter = halfIntervalTime;
                }
                else {
                    frameCounter = frameInterval;
                }
                if (!muted && !soundlessChars.Contains(currentText[typewriterPosition - 1])) {
                    EmitSound();
                }
            }
        }
    }

    void EmitSound() {
        audioSource.Play();
    }

    void ParseLine() {
        if (lineIndex >= currentEvent.script.Count) {
            HideArrow();
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
                else if (statement.Contains("do:")) {
                    ParseDo(statement);
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
            Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid number of arguments");
            return;
        }
        switch (arguments[0]) {
            case "leftspeaker":
                leftCharacter = GameData.instance.GetCharacterData(arguments[1]);
                if (leftCharacter == null) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid speaker");
                    return;
                }
                leftSpeaker.sprite = leftCharacter.defaultSprite;
                speakerName.text = leftCharacter.name;
                leftSpeaker.SetNativeSize();
                leftSpeaker.GetComponent<RectTransform>().sizeDelta *= 2.0f;
                leftSpeaker.color = normalColor;
                if (useColoredHighlight) {
                    rightSpeaker.color = shadowedColor;
                }
                break;
            case "rightspeaker":
                rightCharacter = GameData.instance.GetCharacterData(arguments[1]);
                if (rightCharacter == null) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid speaker");
                    return;
                }
                rightSpeaker.sprite = rightCharacter.defaultSprite;
                speakerName.text = rightCharacter.name;
                rightSpeaker.SetNativeSize();
                rightSpeaker.GetComponent<RectTransform>().sizeDelta *= 2.0f;
                rightSpeaker.color = normalColor;
                if (useColoredHighlight) {
                    leftSpeaker.color = shadowedColor;
                }
                break;
            case "frameinterval":
                try {
                    int newFrameInterval = int.Parse(arguments[1]);
                    if (newFrameInterval < 0) {
                        Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Frame interval cannot be negative");
                        return;
                    }
                    frameInterval = newFrameInterval;
                }
                catch (System.Exception e) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid frame interval");
                }
                break;
            case "delay":
                try {
                    int newDelay = int.Parse(arguments[1]);
                    if (newDelay < 0) {
                        Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Delay cannot be negative");
                        return;
                    }
                    frameCounter = newDelay;
                }
                catch (System.Exception e) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid delay");
                }
                break;
            case "mute":
                try {
                    bool newMuted = bool.Parse(arguments[1]);
                    muted = newMuted;
                }
                catch (System.Exception e) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid mute boolean value");
                }
                break;
            case "usecoloredhighlight":
                try {
                    bool newUseColoredHighlight = bool.Parse(arguments[1]);
                    useColoredHighlight = newUseColoredHighlight;
                    if (!useColoredHighlight) {
                        leftSpeaker.color = normalColor;
                        rightSpeaker.color = normalColor;
                    }
                }
                catch (System.Exception e) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid useColoredHighlight boolean value");
                }
                break;
            default:
                Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid assignee");
                break;
        }
    }

    void ParseDo(string statement) {
        char[] delims = { ' ', ':' };
        string[] arguments = statement.Split(delims);
        if (arguments.Length != 2) {
            Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - %do must have only one argument");
            return;
        }
        switch (arguments[1]) {
            case "highlightleft":
                speakerName.text = leftCharacter.name;
                leftSpeaker.color = normalColor;
                rightSpeaker.color = shadowedColor;
                break;
            case "highlightright":
                speakerName.text = rightCharacter.name;
                leftSpeaker.color = shadowedColor;
                rightSpeaker.color = normalColor;
                break;
            case "hideleft":
                leftSpeaker.sprite = null;
                leftSpeaker.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                break;
            case "hideright":
                rightSpeaker.sprite = null;
                rightSpeaker.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                break;
            default:
                Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Unknown action");
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
        arrowAnimator.SetBool("running", false);
    }

    public void AnimatorReady() {
        isReady = true;
    }
}
