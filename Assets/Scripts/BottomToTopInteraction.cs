using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class BottomToTopInteraction : MonoBehaviour
{
    // Tag name strings
    private string seatString = "Seat";
    private string characterAndDialogueString = "CharacterAndDialogue";
    private string characterImagePoseString = "CharacterImagePose";
    private string characterImageFaceString = "CharacterImageFace";
    private string dialogueBoxPanelString = "DialogueBoxPanel";

    public GameObject charDialParent;
    public GameObject characterImagePose;
    public GameObject characterImageFace;
    public GameObject dialogueBoxPanel;

    HashSet<string> charNameSet;
    void Start()
    {
        // Get the characterDialogueParent
        charDialParent = GameObject.FindWithTag(characterAndDialogueString);

        // Get the dialogueBoxPanel
        // By default, we don't show it
        // TODO: Change to invisible and disabled (so we can do fade-in/out)
        dialogueBoxPanel = GameObject.FindWithTag(dialogueBoxPanelString);
        dialogueBoxPanel.SetActive(false);

        // Get the Character Image
        characterImagePose = GameObject.FindWithTag(characterImagePoseString);
        characterImageFace = GameObject.FindWithTag(characterImageFaceString);
        if (characterImagePose == null || characterImageFace == null)
        {
            throw new Exception("Character Image part could not be found");
        }
        // By default, we don't show it
        // TODO: Change to invisible and disabled (so we can do fade-in/out)
        characterImagePose.SetActive(false);
        characterImageFace.SetActive(false);

        // Disable the top UIs
        charDialParent.SetActive(false);


        // Get the list of character names from the .txt file
        string path = $"{Application.streamingAssetsPath}/Files/names.txt";
        Debug.Log($"Path: {path}");
        if (File.Exists(path))
        {
            charNameSet = new HashSet<string>(File.ReadAllLines(path));
            Debug.Log($"Loaded {charNameSet.Count} names.");
        } else {
            throw new Exception("names.txt was not found");
        }

        // Get all buttons and add listeners
        Button[] buttons = GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => OnAnyButtonClicked(btn));
        }
    }

    void OnAnyButtonClicked(Button clickedButton)
    {
        Debug.Log($"GameObject: {clickedButton.name}; Image Name: {clickedButton.GetComponent<Image>().sprite.name}");

        // A student seat with a character was clicked (bottom)
        // Upon clicking on a new character, we show the character but hide the dialogue box
        if (clickedButton.gameObject.CompareTag(seatString))
        {
            // 0: Name, 1: Face, 2: Pose, 3: "seat" (redundant)
            // (Example: KoumeMomone_0_1_seat)
            string[] splitArray = clickedButton.GetComponent<Image>().sprite.name.Split(char.Parse("_"));
            string nameStr = splitArray[0];
            int faceNum;
            int.TryParse(splitArray[1], out faceNum);
            int poseNum;
            int.TryParse(splitArray[2], out poseNum);
            Debug.Log($"{nameStr}, {faceNum}, {poseNum}");

            // If it's the same character that's currently showing, don't change the top at all
            // (the "if" statement below will not run)
            string[] topSplitArray = characterImagePose.GetComponent<Image>().sprite.name.Split(char.Parse("_"));
            string topNameStr = topSplitArray[0];
            Debug.Log($"Top character's name is {topNameStr}");
            bool isSameCharacter = nameStr.Equals(topNameStr, StringComparison.OrdinalIgnoreCase);

            Debug.Log($"Name in set is {charNameSet.Contains(nameStr)}");
            Debug.Log($"Is same character is {isSameCharacter}");

            // Make sure it's a proper character, and not the same character that's currently showing
            if (charNameSet.Contains(nameStr) && !isSameCharacter)
            {
                Debug.Log($"{nameStr} found in charNameSet (names.txt)");

                // Hide the dialogue box for this new character
                // TODO: Change to invisible and disabled(so we can do fade -in/out)
                dialogueBoxPanel.SetActive(false);

                // Get the matching image based on the seat data
                // (Example: KoumeMomone, 0, 1 -> KoumeMomone_Default (face) and KoumeMomone_Default02 (pose))

                // Pose
                string charImagePoseName = CharacterPoseNumIdToStringName(nameStr, poseNum);
                Debug.Log($"charImagePoseName is: {charImagePoseName}");

                // Face
                string charImageFaceName = CharacterFaceNumIdToStringName(nameStr, faceNum);
                Debug.Log($"charImageFaceName is: {charImageFaceName}");

                try
                {
                    charDialParent.SetActive(true);

                    // Change the characterImage to that character
                    // TODO: Change to fade-in and enabled (so we can do fade-in/out)
                    characterImagePose.SetActive(true);
                    ChangeCharacterPose(charImagePoseName);
                    
                    characterImageFace.SetActive(true);
                    ChangeCharacterFace(charImageFaceName);
                    
                } catch (Exception e) { 
                    Debug.Log($"Failed to switch character pose/face.\nError: {e.ToString()}");
                }
            }
        }

        // The character/dialogue box was clicked on. Advance dialogue (top)
        if (clickedButton.gameObject.CompareTag(characterAndDialogueString))
        {
            // TODO: Change to check to see if it's enabled/already faded-in
            if (dialogueBoxPanel.activeSelf)
            {
                Debug.Log("The dialogue will advance.");

                // TODO: yarn here to go through the dialogue as well as different expressions/poses
            }
            else {
                // Show the dialogue box
                // TODO: Change to fade-in and enabled (so we can do fade-in/out)
                dialogueBoxPanel.SetActive(true);
            }
        }
    }

    // Takes in characterName and poseNumberID to return charName_poseName
    string CharacterPoseNumIdToStringName(string charName, int poseNum)
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
    string CharacterFaceNumIdToStringName(string charName, int faceNum)
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
    void ChangeCharacterPose(string characterPoseName)
    {
        Sprite newSpritePose = Resources.Load<Sprite>($"Art/CharacterArt/Poses/{characterPoseName}");

        if (newSpritePose != null)
        {
            Debug.Log($"Loaded pose sprite: {characterPoseName}");
            characterImagePose.GetComponent<Image>().sprite = newSpritePose;
        }
        else
        {
            throw new Exception($"{characterPoseName} pose was not found.");
        }
    }

    // Updates the character face
    void ChangeCharacterFace(string characterFaceName)
    {
        Sprite newSpriteFace = Resources.Load<Sprite>($"Art/CharacterArt/Faces/{characterFaceName}");

        if (newSpriteFace != null)
        {
            Debug.Log($"Loaded face sprite: {characterFaceName}");
            characterImageFace.GetComponent<Image>().sprite = newSpriteFace;
        }
        else
        {
            throw new Exception($"{characterFaceName} face was not found.");
        }
    }

    /*void Update()
    {
        #if UNITY_EDITOR
            // In editor, simulate touch with mouse click
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Input.mousePosition;
                OnTouch(mousePos);
            }
        #else
            // On device, handle real touch
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                OnTouch(Input.GetTouch(0).position);
            }
        #endif
      }

        void OnTouch(Vector3 screenPos)
        {
            Debug.Log("Touch or click at: " + screenPos);
            // Your touch handling code here
        }*/
}
