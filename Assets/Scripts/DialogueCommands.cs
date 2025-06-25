using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;
using static Animations.AnimationType;

public class DialogueCommands : MonoBehaviour
{
    public Animations AnimationsInstance;
    public DialogueProgressionManager DPMInstance;

    public static GameObject dialogueBoxPanel;

    public static DialogueRunner dialogueRunner;

    private void Awake()
    {
        // Script instances
        AnimationsInstance = GameObject.FindWithTag(ScriptConstants.gameControllerString).GetComponent<Animations>();
        DPMInstance = GameObject.FindWithTag(ScriptConstants.gameControllerString).
            GetComponent<DialogueProgressionManager>();

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

    private void SceneSelection(string characterName)
    {
        Debug.Log("Selecting Scene");

        /*List<UnlockPart> possibleStoryParts = DPMInstance.GetUnlockedPartsForCharacter(characterName);

        // Check if there are unlocked parts
        if (possibleStoryParts != null && possibleStoryParts.Count > 0)
        {

            if (DPMInstance.hasMainStory || DPMInstance.HasCharacterArcStory)
            {
                // Has both: player gets to choose
                if (hasMainStory && hasCharacterArc) { }
            }
            // Select a random dialogue for this character
            else
            {

            }

        }
        else
        {
            throw new Exception("No unlocked parts found, including random dialogue");
        }*/
    }

    public void StartScene(string characterName)
    {
        SceneSelection(characterName);

        Debug.Log("Scene Start");
    }

    public void EndOfScene()
    {
        Debug.Log("End of Scene");

        // Hide the dialogue box
        PlayAnimationOnDialogueBox(FadeOut);
    }
}
