using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCommands : MonoBehaviour
{
    public Animations AnimationsInstance;

    public static GameObject characterImagePose;
    public static GameObject characterImageFace;

    private void Awake()
    {
        // Animations script instance
        AnimationsInstance = GameObject.FindWithTag(ScriptConstants.gameControllerString).GetComponent<Animations>();

        // Get the Character Image
        characterImagePose = GameObject.FindWithTag(ScriptConstants.characterImagePoseString);
        characterImageFace = GameObject.FindWithTag(ScriptConstants.characterImageFaceString);
        if (characterImagePose == null || characterImageFace == null)
        {
            throw new Exception("Character Image part could not be found");
        }
    }

    // Takes in characterName and poseNumberID to return charName_poseName
    public string CharacterPoseNumIdToStringName(string charName, int poseNum)
    {
        string poseStr;

        poseStr = Enum.IsDefined(typeof(ScriptConstants.PoseTypes), poseNum)
            ? ((ScriptConstants.PoseTypes)poseNum).ToString()
            : ScriptConstants.PoseTypes.Default.ToString();

        return $"{charName}_{poseStr}";
    }

    // Takes in characterName and faceNumberID to return charName_faceName
    public string CharacterFaceNumIdToStringName(string charName, int faceNum)
    {
        string faceStr;

        faceStr = Enum.IsDefined(typeof(ScriptConstants.FaceTypes), faceNum) 
            ? ((ScriptConstants.FaceTypes)faceNum).ToString()
            : ScriptConstants.FaceTypes.Default.ToString();

        return $"{charName}_{faceStr}";
    }

    // Updates the character pose
    public void ChangeCharacterPose(string characterPoseName)
    {
        Sprite newSpritePose = Resources.Load<Sprite>($"{ScriptConstants.characterArtPath}Poses/{characterPoseName}");

        if (newSpritePose != null)
        {
            //Debug.Log($"Loaded pose sprite: {characterPoseName}");
            characterImagePose.GetComponent<Image>().sprite = newSpritePose;
        }
        else
        {
            throw new Exception($"{characterPoseName} pose was not found.");
        }
    }

    // Updates the character face
    public void ChangeCharacterFace(string characterFaceName)
    {
        Sprite newSpriteFace = Resources.Load<Sprite>($"{ScriptConstants.characterArtPath}Faces/{characterFaceName}");

        if (newSpriteFace != null)
        {
            //Debug.Log($"Loaded face sprite: {characterFaceName}");
            characterImageFace.GetComponent<Image>().sprite = newSpriteFace;
        }
        else
        {
            throw new Exception($"{characterFaceName} face was not found.");
        }
    }

    // MVC method for Play animation on character
    public void PlayAnimationOnCurrentCharacter(Animations.AnimationType animation, float duration = ScriptConstants.defaultAnimationDuration, Action onComplete = null)
    {
        AnimationsInstance.PlayAnimation(characterImagePose, animation, duration, onComplete);
    }
}
