using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCommands : MonoBehaviour
{
    public CharacterAnimations CharacterAnimationsInstance;

    private string characterImagePoseString = "CharacterImagePose";
    private string characterImageFaceString = "CharacterImageFace";
    private string gameControllerString = "GameController";

    public static GameObject characterImagePose;
    public static GameObject characterImageFace;

    private void Awake()
    {
        // CharacterAnimations script instance
        CharacterAnimationsInstance = GameObject.FindWithTag(gameControllerString).GetComponent<CharacterAnimations>();

        // Get the Character Image
        characterImagePose = GameObject.FindWithTag(characterImagePoseString);
        characterImageFace = GameObject.FindWithTag(characterImageFaceString);
        if (characterImagePose == null || characterImageFace == null)
        {
            throw new Exception("Character Image part could not be found");
        }
    }

    // Takes in characterName and poseNumberID to return charName_poseName
    public string CharacterPoseNumIdToStringName(string charName, int poseNum)
    {
        string poseStr;

        switch (poseNum)
        {
            case 0:
                poseStr = "Default";
                break;
            case 1:
                poseStr = "Confident";
                break;
            case 2:
                poseStr = "Reclusive";
                break;
            case 3:
                poseStr = "Thinking";
                break;
            default:
                poseStr = "Default";
                break;
        }

        return $"{charName}_{poseStr}";
    }

    // Takes in characterName and faceNumberID to return charName_faceName
    public string CharacterFaceNumIdToStringName(string charName, int faceNum)
    {
        string faceStr;

        switch (faceNum)
        {
            case 0:
                faceStr = "Default";
                break;
            case 1:
                faceStr = "Happy";
                break;
            case 2:
                faceStr = "Embarrassed";
                break;
            case 3:
                faceStr = "Angry";
                break;
            case 4:
                faceStr = "Sad";
                break;
            case 5:
                faceStr = "Surprised";
                break;
            case 6:
                faceStr = "Playful";
                break;
            case 7:
                faceStr = "Pouting";
                break;
            default:
                faceStr = "Default";
                break;
        }

        return $"{charName}_{faceStr}";
    }

    // Updates the character pose
    public void ChangeCharacterPose(string characterPoseName)
    {
        Sprite newSpritePose = Resources.Load<Sprite>($"Art/CharacterArt/Poses/{characterPoseName}");

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
        Sprite newSpriteFace = Resources.Load<Sprite>($"Art/CharacterArt/Faces/{characterFaceName}");

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

    // TODO: Have the same method like this but for Dialogue
    // MVC method for Play animation on character
    public void PlayAnimationOnCurrentCharacter(CharacterAnimations.AnimationType animation, float duration = 0.5f, Action onComplete = null)
    {
        CharacterAnimationsInstance.PlayAnimation(characterImagePose, animation, duration, onComplete);
    }
}
