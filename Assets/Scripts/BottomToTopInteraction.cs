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
    public GameObject charDialParent;
    public static GameObject characterImagePose;
    public GameObject dialogueBoxPanel;

    public GameObject mainStoryMarker;
    public GameObject characterArcStoryMarker;
    List<GameObject> charArcCloneMarkers = new List<GameObject>();

    public GameObject topPanel;
    public GameObject bottomPanel;

    private HashSet<string> charNameSet;

    private GameObject lastClickedObject;
    private string currentCharacterShownName;

    private void Awake()
    {
        // Commands scripts instances
        GameObject gameController = GameObject.FindWithTag(ScriptConstants.gameControllerString);

        // Get the characterDialogueParent
        charDialParent = GameObject.FindWithTag(ScriptConstants.characterAndDialogueString);

        // Markers
        mainStoryMarker = GameObject.FindWithTag(ScriptConstants.mainStoryMarkerString);
        characterArcStoryMarker = GameObject.FindWithTag(ScriptConstants.characterArcStoryMarkerString);

        // Panels
        topPanel = GameObject.FindWithTag(ScriptConstants.topPanelString);
        bottomPanel = GameObject.FindWithTag(ScriptConstants.bottomPanelString);

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

        // By default, we don't show it
        characterImagePose.SetActive(false);

        // Disable the top UIs
        charDialParent.SetActive(false);

        // By default, we don't show it
        dialogueBoxPanel.SetActive(false);

        // Markers are set for the scene once the listerner activates, which means the data has finished loading

        // When a scene ends
        DialogueCommands.Instance.OnSceneEnded += HandleSceneEnded;

        // Upon Unity Scene load, wait for data to come before setting markers
        DialogueProgressionManager.Instance.OnDataInitialized += SetMarkers;
    }

    void OnAnyButtonClicked(Button clickedButton)
    {
        Debug.Log($"GameObject: {clickedButton.name}; Image Name: {clickedButton.GetComponent<Image>().sprite.name}");

        // Upon clicking on a new character/object, we show the character but hide the dialogue box
        // (Example: A student seat with a character was clicked (bottom))
        if (clickedButton.gameObject.CompareTag(ScriptConstants.interactableObjectString))
        {
            lastClickedObject = clickedButton.gameObject;

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

                // Update the current character
                currentCharacterShownName = nameStr;

                // Hide the dialogue box for this new character
                if (dialogueBoxPanel.activeSelf)
                {
                    DialogueCommands.Instance.PlayAnimationOnDialogueBox(FadeOut);
                }
                else
                {
                    dialogueBoxPanel.SetActive(false);
                }

                // Get the matching image based on the seat data
                // (Example: KoumeMomone, 0, 1 -> KoumeMomone_Default (face) and KoumeMomone_Default02 (pose))

                // Pose
                string charImagePoseName = CharacterCommands.Instance.CharacterPoseNumIdToStringName(nameStr, poseNum);
                //Debug.Log($"charImagePoseName is: {charImagePoseName}");

                // Face
                string charImageFaceName = CharacterCommands.Instance.CharacterFaceNumIdToStringName(nameStr, faceNum);
                //Debug.Log($"charImageFaceName is: {charImageFaceName}");

                try
                {
                    charDialParent.SetActive(true);

                    // If current character exists, fade them out first
                    if (characterImagePose.activeSelf)
                    {
                        CharacterCommands.Instance.PlayAnimationOnCurrentCharacter(FadeOut, ScriptConstants.defaultAnimationDuration,
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
            // Check to see if it's enabled/already faded-in
            if (dialogueBoxPanel.activeSelf)
            {
                // Yarn Spinner here to go through the dialogue as well as different expressions/poses
                // This just calls 'next' in DialogueCommands. This script file should basically do NOTHING else
                DialogueCommands.Instance.AdvanceLine();
            }
            else
            {
                // Show the dialogue box
                // Select and play the scene after the animation is over
                dialogueBoxPanel.SetActive(true);
                DialogueCommands.Instance.PlayAnimationOnDialogueBox(FadeIn, ScriptConstants.defaultAnimationDuration,
                    () => SelectScene());

                // Disable (don't allow tapping on the) bottom panel when a scene starts and reenable it once the scene is done
                DisableBottomPanel();
            }
        }
    }

    /// <summary>
    /// Change the two characterImages to <paramref name="charImagePoseName"/> and <paramref name="charImageFaceName"/>.
    /// Can pass in "null" if only one of the two images should be changed
    /// </summary>
    /// <param name="charImagePoseName">Character name with the pose name (null is an option)</param>
    /// <param name="charImageFaceName">Character name with the face expression (null is an option)</param>
    [YarnCommand("ShowCharacter")]
    public static void ShowCharacter(string charImagePoseName, string charImageFaceName)
    {
        Debug.Log($"[YarnCommand] ShowCharacter called with: Pose = '{charImagePoseName}', Face = '{charImageFaceName}'");

        characterImagePose.SetActive(true);

        // Change pose
        if (!string.IsNullOrEmpty(charImagePoseName))
        {
            CharacterCommands.Instance.ChangeCharacterPose(charImagePoseName);
        }

        // Change face
        if (!string.IsNullOrEmpty(charImageFaceName))
        {
            CharacterCommands.Instance.ChangeCharacterFace(charImageFaceName);
        }

        CharacterCommands.Instance.PlayAnimationOnCurrentCharacter(FadeIn);
    }

    /// <summary>
    /// Set the markers as children to the objects/characters they represent to start a story part
    /// </summary>
    void SetMarkers()
    {
        ResetMarkers();

        // Get story parts
        List<UnlockPart> storyParts = DialogueProgressionManager.Instance.GetLatestStoryPartsInScene();

        // Check for main story
        if (DialogueProgressionManager.Instance.HasMainStory(storyParts))
        {
            UnlockPart mainStory = DialogueProgressionManager.Instance.GetLatestMainStoryPart();

            // Find the game object the marker will be a child of
            GameObject mainMarkerCharacter = GameObject.Find(mainStory.startingCharacter);

            // Assign the marker as a child of the startingCharacter (or object) (and position)
            mainStoryMarker.transform.SetParent(mainMarkerCharacter.transform);
            mainStoryMarker.transform.position = mainStoryMarker.transform.parent.position;
        }
        else
        {
            // Hide the marker
            mainStoryMarker.SetActive(false);
        }

        // Check for character arcs
        if (DialogueProgressionManager.Instance.HasCharacterArcStory(storyParts))
        {
            // Grab all the character arc parts
            List<UnlockPart> characterStoryParts = DialogueProgressionManager.Instance.GetAllLatestCharacterArcParts();

            foreach (UnlockPart charStory in characterStoryParts)
            {
                // Find the game object the marker will be a child of
                GameObject charMarkerCharacter = GameObject.Find(charStory.startingCharacter);

                // Make an instance of the character arc marker
                GameObject newCharMarker = Instantiate(characterArcStoryMarker);
                charArcCloneMarkers.Add(newCharMarker);

                // Assign the marker as a child of the startingCharacter (or object) (and position)
                newCharMarker.transform.SetParent(charMarkerCharacter.transform, false);
                newCharMarker.transform.position = newCharMarker.transform.parent.position;
            }
        }
        // Finally, disable the original (as we only use the instances)
        characterArcStoryMarker.SetActive(false);

        GameObject[] characterMarkers = GameObject.FindGameObjectsWithTag(ScriptConstants.characterArcStoryMarkerString);

        // A potential overlapping pair
        if (mainStoryMarker.activeSelf && characterMarkers.Length != 0)
        {
            Vector2 mainMarkerPos = mainStoryMarker.transform.position;

            float threshold = 0.01f;

            // Go through each character marker instance to see if there's a match overlap
            foreach (GameObject charMarkInstance in characterMarkers)
            {
                Vector2 charPos = charMarkInstance.transform.position;
                // Overlap is found if true
                if (Vector2.Distance(mainMarkerPos, charPos) < threshold)
                {
                    // Change position so that main is on right and character arc is on left
                    mainStoryMarker.transform.position += new Vector3(15f, 0f, 0f);
                    charMarkInstance.transform.position += new Vector3(-15f, 0f, 0f);

                    // There can only ever be one match
                    break;
                }
            }
        }
     }

    /// <summary>
    /// Reset the markers positions and clones
    /// </summary>
    void ResetMarkers()
    {
        // Let markers able to be used
        mainStoryMarker.SetActive(true);
        characterArcStoryMarker.SetActive(true);

        // First reset position in relation to child/parent for main
        mainStoryMarker.transform.SetParent(bottomPanel.transform);

        // Clear all instances of characterArc markers
        foreach (GameObject clone in charArcCloneMarkers)
        {
            Destroy(clone);
        }
        charArcCloneMarkers.Clear();
    }

    /// <summary>
    /// Figure out which scene to play based on tag and character name
    /// </summary>
    void SelectScene()
    {
        // Start scene
        // TODO: Currently, if there's overlap, it will always play the main story first
        // Main
        if (HelperMethods.HasChildWithTag(lastClickedObject, ScriptConstants.mainStoryMarkerString))
        {
            //Debug.Log("Main runs");
            DialogueCommands.Instance.StartScene(lastClickedObject.name, ScriptConstants.mainStoryMarkerID);
        }
        // Character Arc
        else if (HelperMethods.HasChildWithTag(lastClickedObject, ScriptConstants.characterArcStoryMarkerString))
        {
            //Debug.Log("Char runs");
            DialogueCommands.Instance.StartScene(lastClickedObject.name, ScriptConstants.characterArcStoryMarkerID);
        }
        // Random Dialogue
        else
        {
            //Debug.Log("Rand runs");
            DialogueCommands.Instance.StartScene(lastClickedObject.name);
        }
    }

    /// <summary>
    /// Callback for when the scene finishes playing from DialogueCommands
    /// </summary>
    void HandleSceneEnded()
    {
        //Debug.Log("HandleSceneEnded Ran");

        // Change characterImage back to the starting image default
        string defaultCurrCharPose = $"{currentCharacterShownName}_{ScriptConstants.PoseTypes.Default}";
        string defaultCurrCharFace = $"{currentCharacterShownName}_{ScriptConstants.FaceTypes.Default}";
        ShowCharacter(defaultCurrCharPose, defaultCurrCharFace);

        // Update markers
        SetMarkers();

        // Reenable the bottom panel
        EnableBottomPanel();
    }

    /// <summary>
    /// Prevents the player from interacting with the UI in the bottom panel
    /// </summary>
    void DisableBottomPanel()
    {
        CanvasGroup bottomCanvasGroup = bottomPanel.GetComponent<CanvasGroup>();
        bottomCanvasGroup.blocksRaycasts = false;
        bottomCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Allows the player to interact with the UI in the bottom panel
    /// </summary>
    void EnableBottomPanel()
{
        CanvasGroup bottomCanvasGroup = bottomPanel.GetComponent<CanvasGroup>();
        bottomCanvasGroup.blocksRaycasts = true;
        bottomCanvasGroup.alpha = 1f;
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
