using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DialogueController : MonoBehaviour {

    [HideInInspector]
    private bool isRunning;

    [SerializeField]
    Image leftSpeaker;
    [SerializeField]
    Image rightSpeaker;
    [SerializeField]
    Text speakerName;
    [SerializeField]
    Transform leftNameTransform;
    [SerializeField]
    Transform rightNameTransform;
    [SerializeField]
    Text dialogue;
    [SerializeField]
    GameObject speakerNameBox;
    [SerializeField]
    GameObject textBox;
    [SerializeField]
    Animator arrowAnimator;
    [SerializeField]
    float spriteScale;

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
    bool alwaysAutoSkip;
    bool autoSkip;
    int autoSkipDelay;
    bool noSprite;

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

    Vector3 leftSpeakerBoxPosition;
    Vector3 rightSpeakerBoxPosition;

    List<string> activeTags = new List<string>();

    public static DialogueController instance { get; private set; }

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
            useColoredHighlight = true;
            autoSkip = false;
			isTypewriting = false;
            autoSkipDelay = 0;
			typewriterPosition = 0;
            Show();
        }
        else {
            Debug.Log("DialogueController Error - Event " + eventName + " does not exist");
        }
    }

    void Awake() {
        isRunning = false;
        isReady = false;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        leftSpeakerBoxPosition = leftNameTransform.position;
        rightSpeakerBoxPosition = rightNameTransform.position;

        instance = this;
    }
	
	void FixedUpdate () {
        if (isRunning && isReady) {
            // Doesn't currently have text to print or printed
            if (!isTypewriting) {
                HideArrow();
                ParseLine();
            }
            // Is handling text, mouse button pressed
            else if (Input.GetMouseButtonDown(0)) {
                // If the text has been fully printed, go to next line
                if (typewriterPosition >= currentText.Length) {
                    AdvanceLine();
                }
                // If not, update the text skipping to the end of it
                else {
                    UpdateText(true);
                }
            }
            else {
                // If we're not at the end of a text, print some more of it
                if (!(typewriterPosition >= currentText.Length)) {
                    UpdateText(false);
                }
                // If we are, check if we're supposed to autoskip instead of waiting for the MB press
                else if (autoSkip) {
                    frameCounter--;
                    // Auto skip delay is over, parse next line
                    if (frameCounter <= 0) {
                        AdvanceLine();
                    }
                }
            }
        }
	}

    void UpdateText(bool skipToEnd) {
        // This function can be forced to skip to the end of a text line.
        // Setting a frame interval of zero achieves the same result, immediately.
        if (skipToEnd || frameInterval == 0) {
            typewriterPosition = currentText.Length;
            dialogue.text = currentText;
            ShowArrow();
        }

        frameCounter--;
        // Frame counter done - write a new character.
        if (frameCounter <= 0) {
            typewriterPosition += 1;
            CheckForRichText();
            // Are we at the end of the text?
            if (typewriterPosition >= currentText.Length) {
                dialogue.text = currentText;
                // If auto skip is set, wait autoSkipDelay frames before moving to the next line 
                if (autoSkip) {
                    frameCounter = autoSkipDelay;
                }
                // If not, prompt the user to advance
                else {
                    ShowArrow();
                }
            }
            // Text is not done. Add a new char to it.
            else {
                dialogue.text = currentText.Substring(0, typewriterPosition) + GetClosingTags();
                char currentChar = currentText[typewriterPosition - 1];
                char nextChar = (typewriterPosition + 1 >= currentText.Length ? currentChar : currentText[typewriterPosition]);
                // Find out if the character forces an interval or emits a sound.
                if (intervalChars.Contains(currentChar) && !intervalChars.Contains(nextChar)) {
                    frameCounter = intervalTime;
                }
                else if (halfIntervalChars.Contains(currentChar) && !halfIntervalChars.Contains(nextChar)) {
                    frameCounter = halfIntervalTime;
                }
                else {
                    frameCounter = frameInterval;
                }
                if (!muted && !soundlessChars.Contains(currentChar)) {
                    EmitSound();
                }
            }
        }
    }

    void CheckForRichText() {

        // Return immediately if we're at the end of the string
        if (typewriterPosition >= currentText.Length)
            return;

        char currentChar = currentText[typewriterPosition - 1];

        while (currentChar == '<') {
            int increment;
            try {
                for (increment = 1; currentText[typewriterPosition + increment - 1] != '>'; ++increment) ;
            }
            catch (System.ArgumentOutOfRangeException e) {
                // No closing brackets means it's not a tag.
                return;
            }

            char[] delims = { '=', ' ' };
            string[] validTags = { "b", "/b", "i", "/i", "size", "/size", "color", "/color" };
            string[] tokens = currentText.Substring(typewriterPosition, increment - 1).Split(delims);

            if (!validTags.Contains(tokens[0])) {
                // Not a valid tag, not a problem.
                return;
            }
            // Check if it's a closing tag - means we can stop kludging that tag into the text object
            if (tokens[0].Contains('/')) {
                activeTags.Remove(tokens[0].Replace("/", ""));
            }
            // If it's a new tag, kludge in the corresponding closer so that Unity doesn't go crazy.
            // Push it into the beginning of the list, since nested tags MUST be in reverse order.
            else {
                activeTags.Insert(0, tokens[0]);
            }
            // Advance the typewriter past the tag.
            typewriterPosition += increment + 1;
            
            // Are we at the end of the string? Our job's done.
            if (typewriterPosition >= currentText.Length) {
                return;
            }

            // Repeat the loop to check for a sequence of tags.
            currentChar = currentText[typewriterPosition - 1];
        }
    }

    void AdvanceLine() {
        isTypewriting = false; // will trigger parsing next line
        dialogue.text = "";

        // Restore hidden sprites
        if (noSprite) {
            leftSpeaker.gameObject.SetActive(true);
            rightSpeaker.gameObject.SetActive(true);
        }
        // Reset the autoskip value only if it's temporary
        if (!alwaysAutoSkip) {
            autoSkip = false;
        }
    }

    void EmitSound() {
        audioSource.Play();
    }

    string GetClosingTags() {
        string s = "";
        foreach (var tag in activeTags) {
            s += "</" + tag + ">";
        }
        return s;
    }

    void ParseLine() {
        // Check for the end of the script
        if (lineIndex >= currentEvent.script.Count) {
            HideArrow();
            Hide();
            return;
        }

        // Update current line
        currentText = currentEvent.script[lineIndex];

        // Lines that start with % contains either actions or assignments
        if (currentText.StartsWith("%")) {
            char[] delims = {'%', ','};
            string[] statements = currentText.ToLower().Split(delims);

            // These lines might also contain multiple statements.
            foreach (var statement in statements) {
                // An assignment is a statement like so: "xx=yyy"
                if (statement.Contains("=")) {
                    ParseAssignment(statement); 
                }
                // An action is a statement like so: "do:zzzzz"
                else if (statement.Contains("do:")) {
                    ParseAction(statement);
                }
                // Empty statement, ignore.
                else if (statement.Replace(" ", "").Length == 0) {
                    continue;
                }
                else {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid statement");
                    continue;
                }
            }
        }
        // Lines that don't start with % are text to be printed
        else {
            // Clear the list of active tags
            activeTags.Clear();

            isTypewriting = true;
            typewriterPosition = 0;
        }
        // Move to next line
        ++lineIndex;
    }

    void ParseAssignment(string statement) {
        char[] delims = { ' ', '=' };
        string[] arguments = statement.Split(delims);
        if (arguments.Length != 2) {
            Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid number of arguments");
            return;
        }
        // Evaluate the assignee
        switch (arguments[0]) {
            case "leftspeaker":
                leftCharacter = GameData.instance.GetCharacterData(arguments[1]);
                if (leftCharacter == null) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid speaker");
                    return;
                }
                leftSpeaker.sprite = leftCharacter.defaultSprite;
                speakerName.text = leftCharacter.name;
                speakerName.alignment = TextAnchor.MiddleLeft;
                leftSpeaker.SetNativeSize();
                leftSpeaker.GetComponent<RectTransform>().sizeDelta *= spriteScale;
                leftSpeaker.color = normalColor;
                speakerNameBox.transform.position = leftSpeakerBoxPosition;
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
                speakerName.alignment = TextAnchor.MiddleRight;
                rightSpeaker.SetNativeSize();
                rightSpeaker.GetComponent<RectTransform>().sizeDelta *= spriteScale;
                rightSpeaker.color = normalColor;
                speakerNameBox.transform.position = rightSpeakerBoxPosition;
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
            case "intervaltime":
                try {
                    int newIntervalTime = int.Parse(arguments[1]);
                    if (newIntervalTime < 0) {
                        Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Interval time cannot be negative");
                        return;
                    }
                    intervalTime = newIntervalTime;
                }
                catch (System.Exception e) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid interval time");
                }
                break;
            case "halfintervaltime":
                try {
                    int newHalfIntervalTime = int.Parse(arguments[1]);
                    if (newHalfIntervalTime < 0) {
                        Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Half interval time cannot be negative");
                        return;
                    }
                    halfIntervalTime = newHalfIntervalTime;
                }
                catch (System.Exception e) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid half interval time");
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
            case "alwaysautoskip":
                try {
                    bool newAlwaysAutoSkip = bool.Parse(arguments[1]);
                    alwaysAutoSkip = newAlwaysAutoSkip;
                    autoSkip = alwaysAutoSkip;
                }
                catch (System.Exception e) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid alwaysAutoSkip boolean value");
                }
                break;
            case "autoskipdelay":
                try {
                    int newAutoSkipDelay = int.Parse(arguments[1]);
                    if (newAutoSkipDelay < 0) {
                        Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - autoSkipDelay cannot be negative");
                        return;
                    }
                    autoSkipDelay = newAutoSkipDelay;
                }
                catch (System.Exception e) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid autoSkipDelay");
                }
                break;
            default:
                Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Invalid assignee");
                break;
        }
    }

    void ParseAction(string statement) {
        char[] delims = { ' ', ':' };
        string[] arguments = statement.Split(delims);
        if (arguments.Length != 2) {
            Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - %do must have only one argument");
            return;
        }
        // First token is 'do'. Evaluate the action
        switch (arguments[1]) {
            case "highlightleft":
                if (leftCharacter == null) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Trying to highlight an unset character");
                    return;
                }
                speakerName.text = leftCharacter.name;
                speakerName.alignment = TextAnchor.MiddleLeft;
                leftSpeaker.color = normalColor;
                speakerNameBox.transform.position = leftSpeakerBoxPosition;
                if (useColoredHighlight) {
                    rightSpeaker.color = shadowedColor;
                }
                break;
            case "highlightright":
                if (rightCharacter == null) {
                    Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Trying to highlight an unset character");
                    return;
                }
                speakerName.text = rightCharacter.name;
                speakerName.alignment = TextAnchor.MiddleRight;
                speakerNameBox.transform.position = rightSpeakerBoxPosition;
                rightSpeaker.color = normalColor;
                if (useColoredHighlight) {
                    leftSpeaker.color = shadowedColor;
                }
                break;
            case "hideleft":
                leftCharacter = null;
                leftSpeaker.sprite = null;
                leftSpeaker.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                break;
            case "hideright":
                rightCharacter = null;
                rightSpeaker.sprite = null;
                rightSpeaker.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                break;
            case "noleftsprite":
                noSprite = true;
                leftSpeaker.gameObject.SetActive(false);
                break;
            case "norightsprite":
                noSprite = true;
                rightSpeaker.gameObject.SetActive(false);
                break;
            case "autoskipnext":
                autoSkip = true;
                break;
            default:
                Debug.Log("DialogueController Warning - Parse error at statement \"" + statement + "\" - Unknown action");
                break;
        }
    }

    void Show() {
        isReady = false;
        animator.SetBool("running", true);
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    void Hide() {
        isRunning = false;
        isReady = false;
        animator.SetBool("running", false);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        
        leftCharacter = null;
        leftSpeaker.sprite = null;
        leftSpeaker.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        rightCharacter = null; 
        rightSpeaker.sprite = null;
        rightSpeaker.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);

        speakerName.text = "";
        dialogue.text = "";
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

    public static bool IsRunning() {
        if (instance == null) {
            return false;
        }
        else return instance.isRunning;
    }
}
