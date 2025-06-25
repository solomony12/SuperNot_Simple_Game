using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Yarn.Unity;
using static Animations.AnimationType;

public class BottomToTopInteraction : MonoBehaviour
{
    public CharacterCommands CharacterCommandsInstance;
    public DialogueCommands DialogueCommandsInstance;
    public DialogueProgressionManager DPMInstance;

    public GameObject charDialParent;
    public GameObject characterImagePose;
    public GameObject dialogueBoxPanel;

    public GameObject mainStoryMarker;
    public GameObject characterArcStoryMarker;

    public DialogueRunner dialogueRunner;

    private HashSet<string> charNameSet;

    private string currentCharacterShownName;

    private void Awake()
    {
        // Commands scripts instances
        GameObject gameController = GameObject.FindWithTag(ScriptConstants.gameControllerString);
        CharacterCommandsInstance = gameController.GetComponent<CharacterCommands>();
        DialogueCommandsInstance = gameController.GetComponent<DialogueCommands>();
        DPMInstance = GameObject.FindWithTag(ScriptConstants.gameControllerString).
            GetComponent<DialogueProgressionManager>();

        // Get the characterDialogueParent
        charDialParent = GameObject.FindWithTag(ScriptConstants.characterAndDialogueString);

        // Markers
        mainStoryMarker = GameObject.FindWithTag(ScriptConstants.mainStoryMarkerString);
        characterArcStoryMarker = GameObject.FindWithTag(ScriptConstants.characterArcStoryMarkerString);

        // Get the list of character names from the .txt file
        string path = $"{Application.streamingAssetsPath}/Files/names.txt";
        //Debug.Log($"Path: {path}");
        if (File.Exists(path))
        {
            charNameSet = new HashSet<string>(File.ReadAllLines(path));
            Debug.Log($"Loaded {charNameSet.Count} names.");
        }
        else
        {
            throw new Exception("names.txt was not found");
        }
    }
    void Start()
    {
        // Get all buttons and add listeners
        Button[] buttons = GetComponentsInChildren<Button>(); // Debug Note: it grabs all CHILDREN so this script has to be on a parent
        //Debug.Log($"Number of buttons: {buttons.Length}");
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => OnAnyButtonClicked(btn));
            //Debug.Log($"Lisener added on {btn.gameObject.name}");
        }

        // Get the characterPose
        characterImagePose = CharacterCommands.characterImagePose;
        // Get the dialogueBoxPanel
        dialogueBoxPanel = DialogueCommands.dialogueBoxPanel;
        // Get the dialogue runner
        dialogueRunner = DialogueCommands.dialogueRunner;

        // By default, we don't show it
        characterImagePose.SetActive(false);

        // Disable the top UIs
        charDialParent.SetActive(false);

        // By default, we don't show it
        dialogueBoxPanel.SetActive(false);

        // Set markers for the scene
        SetMarkers();
    }

    void OnAnyButtonClicked(Button clickedButton)
    {
        Debug.Log($"GameObject: {clickedButton.name}; Image Name: {clickedButton.GetComponent<Image>().sprite.name}");

        // A student seat with a character was clicked (bottom)
        // Upon clicking on a new character, we show the character but hide the dialogue box
        if (clickedButton.gameObject.CompareTag(ScriptConstants.seatString))
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
            currentCharacterShownName = topSplitArray[0];
            //Debug.Log($"Top character's name is {currentCharacterShownName}");
            bool isSameCharacter = nameStr.Equals(currentCharacterShownName, StringComparison.OrdinalIgnoreCase);

            //Debug.Log($"Name in set is {charNameSet.Contains(nameStr)}");
            //Debug.Log($"Is same character is {isSameCharacter}");

            // Make sure it's a proper character, and not the same character that's currently showing
            if (charNameSet.Contains(nameStr) && !isSameCharacter)
            {
                //Debug.Log($"{nameStr} found in charNameSet (names.txt)");

                // Hide the dialogue box for this new character
                if (dialogueBoxPanel.activeSelf)
                {
                    DialogueCommandsInstance.PlayAnimationOnDialogueBox(FadeOut);
                }
                else
                {
                    dialogueBoxPanel.SetActive(false);
                }

                // Get the matching image based on the seat data
                // (Example: KoumeMomone, 0, 1 -> KoumeMomone_Default (face) and KoumeMomone_Default02 (pose))

                // Pose
                string charImagePoseName = CharacterCommandsInstance.CharacterPoseNumIdToStringName(nameStr, poseNum);
                //Debug.Log($"charImagePoseName is: {charImagePoseName}");

                // Face
                string charImageFaceName = CharacterCommandsInstance.CharacterFaceNumIdToStringName(nameStr, faceNum);
                //Debug.Log($"charImageFaceName is: {charImageFaceName}");

                try
                {
                    charDialParent.SetActive(true);

                    // If current character exists, fade them out first
                    if (characterImagePose.activeSelf)
                    {
                        CharacterCommandsInstance.PlayAnimationOnCurrentCharacter(FadeOut, ScriptConstants.defaultAnimationDuration,
                                () => ShowCharacter(charImagePoseName, charImageFaceName));
                    }
                    else
                    {
                        // Change the characterImage to that character
                        ShowCharacter(charImagePoseName, charImageFaceName);
                    }
                    
                } catch (Exception e) { 
                    Debug.Log($"Failed to switch character pose/face.\nError: {e.ToString()}");
                }
            }
        }


        // The character/dialogue box was clicked on. Advance dialogue (top)
        if (clickedButton.gameObject.CompareTag(ScriptConstants.characterAndDialogueString))
        {
            // TCheck to see if it's enabled/already faded-in
            if (dialogueBoxPanel.activeSelf)
            {
                // Yarn Spinner here to go through the dialogue as well as different expressions/poses
                // This just calls 'next' in DialogueCommands. This script file should basically do NOTHING else
                DialogueCommandsInstance.AdvanceLine();
            }
            else
            {
                /// TODO: Based on DialogueCommands, we need to see if there's a scene able to be played.
                /// We'll probably be using the idea of Main, Character Relation, and Random so it wlll always play
                /// a 'scene;' in most cases, Random.
                /// HOWEVER this does bring up the use case where if a Main or Character Relation scene just ended,
                /// it should repeat a new last line so that it always helps direct the player where they should go/tap
                /// to progress the Main/Character Relation story:
                /// What we'll do for that last line repeat is we'll just have that as its own scene that is set after the main scene
                /// is finished playing. That way, it'll run that yarn script over and over again.
                /// If Main and Character Relation overlap on the same character, allow the player to choose which one they want to start
                /// 
                /// So now, if there's a marker as a child, we pass in the marker ID
                DialogueCommandsInstance.StartScene(currentCharacterShownName);

                // Show the dialogue box
                //Debug.Log("Showing dialogue box");
                dialogueBoxPanel.SetActive(true);
                DialogueCommandsInstance.PlayAnimationOnDialogueBox(FadeIn);
            }
        }
    }

    /// <summary>
    /// Change the two characterImages to <paramref name="charImagePoseName"/> and <paramref name="charImageFaceName"/>
    /// </summary>
    /// <param name="charImagePoseName">Character name with the pose name</param>
    /// <param name="charImageFaceName">Character name with the face expression</param>
    void ShowCharacter(string charImagePoseName, string charImageFaceName)
    {
        characterImagePose.SetActive(true);
        CharacterCommandsInstance.ChangeCharacterPose(charImagePoseName);
        CharacterCommandsInstance.ChangeCharacterFace(charImageFaceName);
        CharacterCommandsInstance.PlayAnimationOnCurrentCharacter(FadeIn);
    }

    /// <summary>
    /// Set the markers as children to the objects/characters they represent to start a story part
    /// </summary>
    void SetMarkers()
    {
        // Get story parts
        List<UnlockRule> storyParts = DPMInstance.GetLatestStoryPartsInScene();

        // Check for main story
        if (DPMInstance.HasMainStory(storyParts))
        {
            UnlockRule mainStory = DPMInstance.GetLatestMainStoryRule();

            // Find the game object the marker will be a child of
            GameObject mainMarkerCharacter = GameObject.Find(mainStory.startingCharacter);

            // Assign the marker as a child of the startingCharacter (or object)
            mainStoryMarker.transform.SetParent(mainMarkerCharacter.transform);
        }
        else
        {
            // Hide the marker
            mainStoryMarker.SetActive(false);
        }

        // Check for character arcs
        if (DPMInstance.HasCharacterArcStory(storyParts))
        {
            // Grab all the character arc parts
            List<UnlockRule> characterStoryParts = DPMInstance.GetAllLatestCharacterArcRules();

            foreach (UnlockRule charStory in characterStoryParts)
            {
                // Find the game object the marker will be a child of
                GameObject charMarkerCharacter = GameObject.Find(charStory.startingCharacter);

                // Make an instance of the character arc marker
                GameObject newCharMarker = Instantiate(characterArcStoryMarker);

                // Assign the marker as a child of the startingCharacter (or object)
                newCharMarker.transform.SetParent(charMarkerCharacter.transform);
            }
        }
        // Finally, disable the original (as we only use the instances)
        characterArcStoryMarker.SetActive(false);
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
