using UnityEngine;

public static class ScriptConstants
{
    // Paths
    public const string characterArtPath = "Art/CharacterArt/";

    // Tag name strings
    public const string gameControllerString = "GameController";
    public const string characterAndDialogueString = "CharacterAndDialogue";
    public const string characterImagePoseString = "CharacterImagePose";
    public const string characterImageFaceString = "CharacterImageFace";
    public const string dialogueBoxPanelString = "DialogueBoxPanel";
    public const string dialogueSystemString = "DialogueSystem";
    public const string mainStoryMarkerString = "MainStoryMarker";
    public const string characterArcStoryMarkerString = "CharacterArcStoryMarker";
    public const string interactableObjectString = "InteractableObject";
    public const string topPanelString = "TopPanel";
    public const string bottomPanelString = "BottomPanel";
    public const string mainCanvasString = "MainCanvas";
    public const string effectsImageGameObjectString = "EffectsImage";

    public const string mainStoryMarkerID = "M";
    public const string characterArcStoryMarkerID = "C";
    public const string randomStoryID = "R";

    public const string startingStoryID = "M00";
    // TODO: Change when we figure out which one is the first
    public const string newGameFirstScene = "5E_Classroom";

    // Animation values
    public const float defaultAnimationDuration = 0.5f;

    public const string defaultString = "Default";

    /// <summary>
    /// Character pose types
    /// </summary>
    public enum PoseTypes
    {
        Default,
        Confident,
        Reclusive,
        Thinking,
        Fighting
    }

    /// <summary>
    /// Character face/expression types
    /// </summary>
    public enum FaceTypes
    {
        Default,
        Happy,
        Embarrassed,
        Angry,
        Sad,
        Surprised,
        Playful,
        Pouting
    }

}
