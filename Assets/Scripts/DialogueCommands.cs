using System;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using static Animations.AnimationType;

public class DialogueCommands : MonoBehaviour
{
    public Animations AnimationsInstance;

    public static GameObject dialogueBoxPanel;

    public static DialogueRunner dialogueRunner;

    private void Awake()
    {
        // Animations script instance
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

    private void SceneSelection()
    {
        Debug.Log("Selecting Scene");
    }

    public void StartScene()
    {
        SceneSelection();

        Debug.Log("Scene Start");
    }

    public void EndOfScene()
    {
        Debug.Log("End of Scene");

        // Hide the dialogue box
        PlayAnimationOnDialogueBox(FadeOut);
    }
}
