using System;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class DialogueCommands : MonoBehaviour
{
    public Animations AnimationsInstance;

    // Tag name strings
    private string dialogueBoxPanelString = "DialogueBoxPanel";
    private string dialogueSystemString = "DialogueSystem";

    public static GameObject dialogueBoxPanel;

    public static DialogueRunner dialogueRunner;

    private void Awake()
    {
        // Get the dialogueBoxPanel
        dialogueBoxPanel = GameObject.FindWithTag(dialogueBoxPanelString);

        // Get the dialogue runner
        dialogueRunner = GameObject.FindWithTag(dialogueSystemString).GetComponent<DialogueRunner>();
    }

    // MVC method for Play animation on dialogue box
    public void PlayAnimationOnDialogueBox(Animations.AnimationType animation, float duration = 0.5f, Action onComplete = null)
    {
        AnimationsInstance.PlayAnimation(dialogueBoxPanel, animation, duration, onComplete);
    }
}
