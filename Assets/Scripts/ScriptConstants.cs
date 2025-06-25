using UnityEngine;

public static class ScriptConstants
{
    // Paths
    public const string characterArtPath = "Art/CharacterArt/";
    // Tag name strings
    public const string seatString = "Seat";
    public const string characterAndDialogueString = "CharacterAndDialogue";
    public const string characterImagePoseString = "CharacterImagePose";
    public const string characterImageFaceString = "CharacterImageFace";
    public const string dialogueBoxPanelString = "DialogueBoxPanel";
    public const string dialogueSystemString = "DialogueSystem";
    public const string gameControllerString = "GameController";

    // Animation values
    public const float defaultAnimationDuration = 0.5f;

    /// <summary>
    /// Character pose types
    /// </summary>
    public enum PoseTypes
    {
        Default,
        Confident,
        Reclusive,
        Thinking,
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
