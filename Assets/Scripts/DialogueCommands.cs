using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;
using static Animations.AnimationType;

public class DialogueCommands : MonoBehaviour
{
    public static DialogueCommands Instance;
    public Animations AnimationsInstance;

    public static GameObject dialogueBoxPanel;

    public static DialogueRunner dialogueRunner;

    private string currentStoryRunning;

    public event Action OnSceneEnded;

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
        dialogueRunner.RequestNextLine();
        return true;
    }

    public void StartScene(string objName, string markerId = ScriptConstants.randomID)
    {
        // Random Dialogue
        if (markerId.Equals(ScriptConstants.randomID))
        {
            // TODO: Select a random dialogue from the given character/object
            currentStoryRunning = "random";
            dialogueRunner.StartDialogue("random");
        }
        // Main Story
        else if (markerId.Equals(ScriptConstants.mainStoryMarkerID))
        {
            string mainStoryName = DialogueProgressionManager.Instance.GetLatestMainStory();

            // Start yarn script scene with that name (mainStoryName)
            currentStoryRunning = mainStoryName;
            dialogueRunner.StartDialogue(mainStoryName);
        }
        // Character Arc Story
        else
        {
            string characterPartName = DialogueProgressionManager.Instance.GetLatestCharacterArc(objName);

            // Start yarn script scene with that name (characterPartName)
            currentStoryRunning = characterPartName;
            dialogueRunner.StartDialogue(characterPartName);
        }

        Debug.Log("Scene Start");
    }

    public void EndOfScene()
    {
        Debug.Log("End of Scene");

        // Mark story part as completed
        DialogueProgressionManager.Instance.ReachState(currentStoryRunning);

        // Hide the dialogue box
        PlayAnimationOnDialogueBox(FadeOut);

        // Launch any methods connected to this listener
        OnSceneEnded?.Invoke();
    }
}
