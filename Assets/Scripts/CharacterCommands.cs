using System;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class CharacterCommands : MonoBehaviour
{
    public static CharacterCommands Instance;
    public Animations AnimationsInstance;

    public static GameObject characterImagePose;
    public static GameObject characterImageFace;

    private void Awake()
    {
        Instance = this;

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

    /// <summary>
    /// Converts name and pose ID to the respective string
    /// </summary>
    /// <param name="charName">Character name</param>
    /// <param name="poseNum">Pose ID</param>
    /// <returns>Takes in <paramref name="charName"/> and <paramref name="poseNum"/> to return charName_poseName</returns>
    public string CharacterPoseNumIdToStringName(string charName, int poseNum)
    {
        string poseStr;

        poseStr = Enum.IsDefined(typeof(ScriptConstants.PoseTypes), poseNum)
            ? ((ScriptConstants.PoseTypes)poseNum).ToString()
            : ScriptConstants.PoseTypes.Default.ToString();

        return $"{charName}_{poseStr}";
    }

    /// <summary>
    /// Converts name and face ID to the respective string
    /// </summary>
    /// <param name="charName">Character name</param>
    /// <param name="faceNum">Face ID</param>
    /// <returns>Takes in <paramref name="charName"/> and <paramref name="faceNum"/> to return charName_faceName</returns>
    public string CharacterFaceNumIdToStringName(string charName, int faceNum)
    {
        string faceStr;

        faceStr = Enum.IsDefined(typeof(ScriptConstants.FaceTypes), faceNum) 
            ? ((ScriptConstants.FaceTypes)faceNum).ToString()
            : ScriptConstants.FaceTypes.Default.ToString();

        return $"{charName}_{faceStr}";
    }

    /// <summary>
    /// Updates the character pose
    /// </summary>
    /// <param name="characterPoseName">Character name with pose name</param>
    public void ChangeCharacterPose(string characterPoseName)
    {
        string path = $"{ScriptConstants.characterArtPath}Poses/{characterPoseName}";
        Sprite newSpritePose = Resources.Load<Sprite>(path);

        if (newSpritePose != null)
        {
            //Debug.Log($"Loaded pose sprite: {characterPoseName}");
            characterImagePose.GetComponent<Image>().sprite = newSpritePose;
        }
        else
        {
            throw new Exception($"'{characterPoseName}' pose was not found.\nPath is: {path}");
        }
    }

    /// <summary>
    /// Updates the character face
    /// </summary>
    /// <param name="characterFaceName">Character name and face expression</param>
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
            throw new Exception($"'{characterFaceName}' face was not found.");
        }
    }

    /// <summary>
    /// MVC method for Play animation on character
    /// </summary>
    /// <param name="animation">Type of animation to be played</param>
    /// <param name="duration">Duration of animation in seconds</param>
    /// <param name="onComplete">Action/method to run when animation is completed. Null by default</param>
    //[YarnCommand("PlayAnimationOnCharacter")]
    public void PlayAnimationOnCurrentCharacter(Animations.AnimationType animation, float duration = ScriptConstants.defaultAnimationDuration, Action onComplete = null)
    {
        AnimationsInstance.PlayAnimation(characterImagePose, animation, duration, onComplete);
    }
}
