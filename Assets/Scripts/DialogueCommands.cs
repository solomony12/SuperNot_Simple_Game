using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Yarn.Unity;
using Yarn.Unity.Legacy;
using static Animations.AnimationType;

public class DialogueCommands : MonoBehaviour
{
    public static DialogueCommands Instance;
    public Animations AnimationsInstance;

    public static GameObject dialogueBoxPanel;

    public static DialogueRunner dialogueRunner;

    private string currentStoryRunning;

    public event Action OnSceneEndMid;
    public event Action OnSceneEnded;
    public event Action OnNextDialogueLinePlayed;

    bool isQuitting = false;

    private void Awake()
    {
        // Script instances
        Instance = this;
        AnimationsInstance = GameObject.FindWithTag(ScriptConstants.gameControllerString).GetComponent<Animations>();

        // Get the dialogueBoxPanel
        dialogueBoxPanel = GameObject.FindWithTag(ScriptConstants.dialogueBoxPanelString);

        // Get the dialogue runner
        dialogueRunner = GameObject.FindWithTag(ScriptConstants.dialogueSystemString).GetComponent<DialogueRunner>();
    }

    void Start()
    {
        // Listener(s)
        dialogueRunner.onDialogueComplete.AddListener(EndOfScene);
    }
    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    /// <summary>
    /// MVC method for Play animation on dialogue box
    /// </summary>
    /// <param name="animation">Type of animation to be played</param>
    /// <param name="duration">Duration of animation in seconds</param>
    /// <param name="onComplete">Action/method to run when animation is completed. Null by default</param>
    public void PlayAnimationOnDialogueBox(Animations.AnimationType animation, float duration = ScriptConstants.defaultAnimationDuration, Action onComplete = null)
    {
        AnimationsInstance.PlayAnimation(dialogueBoxPanel, animation, duration, onComplete);
    }

    /// <summary>
    /// Plays the next line of dialogue in the scene
    /// </summary>
    /// <returns>True if the last line was ran, indicating the end of the scene</returns>
    public bool AdvanceLine()
    {
        //Debug.Log("Start of AdvanceLine");
        OnNextDialogueLinePlayed?.Invoke();
        dialogueRunner.RequestNextLine();
        //Debug.Log("End of AdvanceLine");
        return dialogueRunner.IsDialogueRunning;
    }

    /// <summary>
    /// Start in a certain node of the yarn script based on the character and type of story
    /// </summary>
    /// <param name="objName">Name of the startingCharacter</param>
    /// <param name="markerId">Type of story</param>
    public void StartScene(string objName, string markerId = ScriptConstants.randomStoryID)
    {
        //Debug.Log($"StartScene: objName: {objName}; markerID: {markerId}");

        // Random Dialogue
        if (markerId.Equals(ScriptConstants.randomStoryID))
        {
            // Select a random dialogue from the given character/object
            currentStoryRunning = SelectRandomDialogueForObject(objName);
            //Debug.Log($"Rand currStoryRunning is {currentStoryRunning}");
        }
        // Main Story
        else if (markerId.Equals(ScriptConstants.mainStoryMarkerID))
        {
            string mainStoryName = DialogueProgressionManager.Instance.GetLatestMainStory();

            SceneOrganizer.Instance.StoreOriginalGameObjectsData();

            // Start yarn script scene with that name (mainStoryName)
            currentStoryRunning = mainStoryName;
            //Debug.Log($"Main currStoryRunning is {currentStoryRunning}");
        }
        // Character Arc Story
        else
        {
            string characterPartName = DialogueProgressionManager.Instance.GetLatestCharacterArc(objName);

            SceneOrganizer.Instance.StoreOriginalGameObjectsData();

            // Start yarn script scene with that name (characterPartName)
            currentStoryRunning = characterPartName;
            //Debug.Log($"Char currStoryRunning is {currentStoryRunning}");
        }

        // Play scene
        //Debug.Log($"Playing scene: {currentStoryRunning}");
        dialogueRunner.StartDialogue(currentStoryRunning);

        //Debug.Log("Scene Start");
    }

    /// <summary>
    /// When the yarn script node finishes running
    /// </summary>
    public void EndOfScene()
    {
        //Debug.Log("End of Scene");

        // Prevents saving if game quit abruptly (like stopping editor or closing app or crash)
        if (isQuitting) return;

        // SceneOrganizer needs to restore temp and update permanent data BEFORE ReachState is called to save progress
        OnSceneEndMid?.Invoke();

        // Mark story part as completed (if not random dialogue)
        if (!currentStoryRunning.StartsWith(ScriptConstants.randomStoryID))
        {
            //Debug.Log($"Marking {currentStoryRunning} as completed");
            SceneOrganizer.Instance.SaveCurrentSceneName();
            DialogueProgressionManager.Instance.ReachState(currentStoryRunning);
        }

        // Hide the dialogue box
        PlayAnimationOnDialogueBox(FadeOut);

        // TODO: Reset the line viewer to clear the dialogue after scene is finished
        //var lineView = FindObjectOfType<LineView>();
        //lineView.textComponent.text = "";

        // Launch any methods connected to this listener
        OnSceneEnded?.Invoke();
    }

    /// <summary>
    /// Get a random dialogue name to play
    /// </summary>
    /// <param name="objName">The character/object to get random dialogue for</param>
    /// <returns>Name of part/node</returns>
    private string SelectRandomDialogueForObject(string objName)
    {
        int dialogueNumberSelected = 0;

        // Grab all the unlocked R nodes with the given character/object name
        List<UnlockPart> unlockedRandCharParts = DialogueProgressionManager.Instance.GetUnlockedPartsByIDAndName(ScriptConstants.randomStoryID, objName);

        // Choose at random which one to play
        if (unlockedRandCharParts.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, unlockedRandCharParts.Count);
            UnlockPart randomPart = unlockedRandCharParts[randomIndex];
            DialogueProgressionManager.Instance.TryExtractNumber(randomPart.node, out int newNum);
            dialogueNumberSelected = newNum;
        }
        else
        {
            // TODO: This 'else' can be deleted later in the future
            // Nothing found
            Debug.Log("Playing default random of 0");
        }

        return $"{ScriptConstants.randomStoryID}{dialogueNumberSelected:D2}_{objName}";
    }
}
