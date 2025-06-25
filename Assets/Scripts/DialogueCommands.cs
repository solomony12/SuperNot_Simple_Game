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

    // MVC method for Play animation on dialogue box
    public void PlayAnimationOnDialogueBox(Animations.AnimationType animation, float duration = ScriptConstants.defaultAnimationDuration, Action onComplete = null)
    {
        AnimationsInstance.PlayAnimation(dialogueBoxPanel, animation, duration, onComplete);
    }
}
