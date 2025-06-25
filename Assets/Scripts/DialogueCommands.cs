using System;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

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
        return true;
    }
}
